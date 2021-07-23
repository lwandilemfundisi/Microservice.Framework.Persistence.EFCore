using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microservice.Framework.Common;
using Microservice.Framework.Persistence.EFCore.Queries.Filtering;
using Microservice.Framework.Persistence.Queries.Filtering;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microservice.Framework.Persistence.EFCore.Queries.NamedQueries;
using System.Data.Common;
using Microsoft.Extensions.Configuration;

namespace Microservice.Framework.Persistence.EFCore
{
    public class EntityFrameworkPersistence<TDbContext> : IPersistence
        where TDbContext : DbContext
    {
        private readonly ILogger<EntityFrameworkPersistence<TDbContext>> _logger;
        private readonly IUniqueConstraintDetectionStrategy _strategy;
        private readonly TDbContext _context;
        private readonly IConfiguration _configuration;

        private readonly AsyncLock _asyncLock = new AsyncLock();

        public EntityFrameworkPersistence(
            ILogger<EntityFrameworkPersistence<TDbContext>> logger,
            IDbContextProvider<TDbContext> contextProvider,
            IUniqueConstraintDetectionStrategy strategy,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _context = contextProvider.CreateContext();
            _strategy = strategy;
            _configuration = configuration;
        }

        public Task Dispose(CancellationToken cancellationToken)
        {
            if (_context.IsNotNull())
                _context.Dispose();

            return Task.FromResult(0);
        }

        public async Task Delete<TDomain>(TDomain domain, CancellationToken cancellationToken) where TDomain : class
        {
            try
            {
                _context.Remove(domain);
                await _context.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation(_strategy))
            {
                _logger.LogError(
                    "Entity Framework delete detected an optimistic concurrency " +
                    "exception for entity with ID '{0}'", domain.GetType().GetProperty("Id").GetValue(domain));
                throw new OptimisticConcurrencyException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Entity Framework delete detected an " +
                    "exception for entity with ID '{0}', message : {1}", domain.GetType().GetProperty("Id").GetValue(domain), ex);
                throw;
            }
        }

        public async Task<IEnumerable<TModel>> ExecuteStoredProcedure<TModel>(NamedCriteria namedCriteria, CancellationToken cancellationToken)
        {
            try
            {
                return await _context
                    .GetStoredProcedure(namedCriteria.Name)
                    .WithSqlParams(namedCriteria.Parameters)
                    .ExecuteStoredProc<TModel>(int.Parse(_configuration["Timeout"].OrDefault("180")));
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    "Entity Framework execute stored procedure detected an " +
                    "exception for stored procedure with name '{0}', message : {1}", namedCriteria.Name, ex);
                throw;
            }
        }

        public async Task<TDomain> Get<TDomain, TDomainCriteria>(TDomainCriteria domainCriteria, CancellationToken cancellationToken)
            where TDomain : class
            where TDomainCriteria : DomainCriteria
        {
            using (await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                return await EntityFrameworkExpressionBuilder
                    .BuildCriteria(
                    _context.Set<TDomain>(), 
                    domainCriteria).FirstOrDefaultAsync();
            }
        }

        public async Task<IList<TDomain>> GetAll<TDomain, TDomainCriteria>(TDomainCriteria domainCriteria, CancellationToken cancellationToken)
            where TDomain : class
            where TDomainCriteria : DomainCriteria
        {
            using (await _asyncLock.WaitAsync(cancellationToken).ConfigureAwait(false))
            {
                return await EntityFrameworkExpressionBuilder
                    .BuildCriteria(
                    _context.Set<TDomain>(), 
                    domainCriteria).ToListAsync();
            }
        }

        public async Task Save<TDomain>(TDomain domain, CancellationToken cancellationToken) where TDomain : class
        {
            try
            {
                _context.Add(domain);
                await _context.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation(_strategy))
            {
                _logger.LogError(
                    "Entity Framework save detected an optimistic concurrency " +
                    "exception for entity with ID '{0}'", domain.GetType().GetProperty("Id").GetValue(domain));
                throw new OptimisticConcurrencyException(ex.Message, ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Entity Framework save detected an " +
                    "exception for entity with ID '{0}', message : {1}", domain.GetType().GetProperty("Id").GetValue(domain), ex);
                throw;
            }
        }

        public async Task Update<TDomain>(TDomain domain, CancellationToken cancellationToken) where TDomain : class
        {
            try
            {
                _context.Update(domain);
                await _context.SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation(_strategy))
            {
                _logger.LogError(
                    "Entity Framework event update detected an optimistic concurrency " +
                    "exception for entity with ID '{0}'", domain.GetType().GetProperty("Id").GetValue(domain));
                throw new OptimisticConcurrencyException(ex.Message, ex);
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    "Entity Framework update detected an " +
                    "exception for entity with ID '{0}', message : {1}", domain.GetType().GetProperty("Id").GetValue(domain), ex);
                throw;
            }
        }
    }
}
