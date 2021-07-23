using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microservice.Framework.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Microservice.Framework.Persistence.EFCore
{
    public static class DbContextExtensions
    {
        public static async Task<int> Delete<TContext, TEntity, TProjection>(IDbContextProvider<TContext> contextProvider,
            int batchSize,
            CancellationToken cancellationToken,
            Expression<Func<TEntity, TProjection>> projection,
            Expression<Func<TEntity, bool>> condition = null,
            Action<TProjection, EntityEntry<TEntity>> setProperties = null) 
            where TContext : DbContext 
            where TEntity : class, new()
        {
            int rowsAffected = 0;

            while (!cancellationToken.IsCancellationRequested)
                using (var dbContext = contextProvider.CreateContext())
                {
                    IQueryable<TEntity> query = dbContext
                        .Set<TEntity>()
                        .AsNoTracking();

                    if (condition != null)
                    {
                        query = query.Where(condition);
                    }

                    IEnumerable<TProjection> items = await query
                        .Take(batchSize)
                        .Select(projection)
                        .ToArrayAsync(cancellationToken)
                        .ConfigureAwait(false);

                    if (!items.Any())
                        return rowsAffected;

                    if (setProperties == null)
                    {
                        dbContext.RemoveRange((IEnumerable<object>) items);
                    }
                    else
                    {
                        foreach (var item in items)
                        {
                            var entity = new TEntity();
                            var entry = dbContext.Attach(entity);
                            setProperties.Invoke(item, entry);
                            entry.State = EntityState.Deleted;
                        }
                    }

                    rowsAffected += await dbContext.SaveChangesAsync(cancellationToken)
                        .ConfigureAwait(false);
                }

            return rowsAffected;
        }

        public static DbCommand GetStoredProcedure(this DbContext context, string storedProcName)
        {
            var cmd = context.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = storedProcName;
            cmd.CommandType = System.Data.CommandType.StoredProcedure;
            return cmd;
        }

        public static DbCommand WithSqlParams(this DbCommand cmd, IDictionary<string, object> parameters)
        {
            if (string.IsNullOrEmpty(cmd.CommandText))
                throw new InvalidOperationException(
                  "Call GetStoredProcedure before using this method");

            foreach (var item in parameters)
            {
                var param = cmd.CreateParameter();

                if (item.Value.IsNull())
                {
                    param.ParameterName = item.Key;
                    param.Value = null;
                }
                else
                {
                    var valuleType = item.Value.GetType();
                    var isString = typeof(string).Equals(valuleType);
                    if (!isString && typeof(IEnumerable).IsAssignableFrom(valuleType))
                    {
                        var values = new List<object>();
                        var enumerator = ((IEnumerable)item.Value).GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            values.Add(enumerator.Current);
                        }
                        param.ParameterName = item.Key;
                        param.Value = values.ToArray();
                    }
                    else
                    {
                        if (isString)
                        {
                            var stringValue = item.Value.AsString();

                            param.ParameterName = item.Key;
                            param.Value = stringValue;
                        }
                        else
                        {
                            param.ParameterName = item.Key;
                            param.Value = item.Value;
                        }
                    }
                }

                cmd.Parameters.Add(param);
            }

            return cmd;
        }

        public static IEnumerable<T> MapToList<T>(this DbDataReader dr)
        {
            var objList = new List<T>();
            var props = typeof(T).GetRuntimeProperties();

            var colMapping = dr.GetColumnSchema()
              .Where(x => props.Any(y => y.Name.ToLower() == x.ColumnName.ToLower()))
              .ToDictionary(key => key.ColumnName.ToLower());

            if (dr.HasRows)
            {
                while (dr.Read())
                {
                    T obj = Activator.CreateInstance<T>();
                    foreach (var prop in props)
                    {
                        var val =
                          dr.GetValue(colMapping[prop.Name.ToLower()].ColumnOrdinal.Value);
                        prop.SetValue(obj, val == DBNull.Value ? null : val);
                    }
                    objList.Add(obj);
                }
            }
            return objList;
        }

        public static async Task<IEnumerable<T>> ExecuteStoredProc<T>(this DbCommand command, int commandTimeOut)
        {
            using (command)
            {
                command.CommandTimeout = commandTimeOut;

                if (command.Connection.State == System.Data.ConnectionState.Closed)
                    command.Connection.Open();
                try
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        return reader.MapToList<T>();
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    command.Connection.Close();
                }
            }
        }
    }
}
