using Microsoft.EntityFrameworkCore;
using Microservice.Framework.Common;
using Microservice.Framework.Persistence.EFCore.Queries.Filtering;
using Microservice.Framework.Persistence.Queries.Filtering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microservice.Framework.Persistence.EFCore
{
    internal static class EntityFrameworkExpressionBuilder
    {
        public static IQueryable<TDomain> BuildCriteria<TDomain, TDomainCriteria>(DbSet<TDomain> dbSet,
            TDomainCriteria domainCriteria)
            where TDomain : class
            where TDomainCriteria : DomainCriteria
        {
            IQueryable<TDomain> queryableData = dbSet.AsQueryable<TDomain>();

            Expression expression = null;
            ParameterExpression parameter = Expression.Parameter(typeof(TDomain), "pe");

            if (domainCriteria.Filter.IsNotNull())
            {
                expression = BuildExpression<TDomain>(domainCriteria.Filter, parameter);
            }

            if (expression != null)
            {
                expression = Expression.Call(
                typeof(Queryable),
                "Where",
                new Type[] { queryableData.ElementType },
                queryableData.Expression,
                Expression.Lambda<Func<TDomain, bool>>(expression, new ParameterExpression[] { parameter }));
            }

            expression = AddOrder(expression ?? queryableData.Expression, domainCriteria, queryableData, parameter);

            expression = Include(expression ?? queryableData.Expression, domainCriteria, queryableData, parameter);

            return dbSet.AsQueryable<TDomain>().Provider.CreateQuery<TDomain>(expression);
        }

        private static Expression Include<TDomain, TDomainCriteria>(Expression filterExpression,
            TDomainCriteria domainCriteria,
            IQueryable<TDomain> queryableData,
            ParameterExpression parameter) 
            where TDomain : class
            where TDomainCriteria : DomainCriteria
        {
            var efdomainCriteria = domainCriteria as EFCoreDomainCriteria;

            if(efdomainCriteria.IsNotNull())
            {
                if (efdomainCriteria.Include.IsNotNull())
                {
                    var includeExpression = Expression.Lambda(
                        Expression.PropertyOrField(parameter, efdomainCriteria.Include.PropertyName),
                        parameter);

                    return Expression.Call(
                        typeof(EntityFrameworkQueryableExtensions),
                        "Include",
                        new Type[] { queryableData.ElementType, includeExpression.ReturnType },
                        filterExpression,
                        Expression.Quote(includeExpression));
                }
            }

            return filterExpression;
        }

        private static Expression AddOrder<TDomain>(Expression filterExpression,
            DomainCriteria domainCriteria,
            IQueryable<TDomain> queryableData,
            ParameterExpression parameter) where TDomain : class
        {
            if (domainCriteria.SortOrder.IsNotNull())
            {
                Expression propertyToOrderByExpression = Expression.Convert(Expression.Property(parameter, domainCriteria.SortOrder.PropertyName), typeof(object));

                switch (domainCriteria.SortOrder.SortOrderType)
                {
                    case SortOrderType.Ascending:
                        {
                            var orderByExpression = Expression.Lambda(propertyToOrderByExpression, parameter);

                            return Expression.Call(
                                typeof(Queryable),
                                "OrderBy",
                                new Type[] { queryableData.ElementType, orderByExpression.ReturnType },
                                filterExpression,
                                Expression.Quote(orderByExpression));
                        }
                    case SortOrderType.Descending:
                        {
                            var orderByExpression = Expression.Lambda(propertyToOrderByExpression, parameter);

                            return Expression.Call(
                                typeof(Queryable),
                                "OrderByDescending",
                                new Type[] { queryableData.ElementType, orderByExpression.ReturnType },
                                filterExpression,
                                Expression.Quote(orderByExpression));
                        }
                }
            }

            return filterExpression;
        }

        private static Expression BuildExpression<TDomain>(BaseFilter filter, ParameterExpression parameter) where TDomain : class
        {
            var equalityFilter = filter as EqualityFilter;
            if (equalityFilter.IsNotNull())
            {
                return ResolveEqualityFilter(equalityFilter, parameter);
            }

            var nullFilter = filter as NullFilter;
            if (nullFilter.IsNotNull())
            {
                return Expression.Equal(Expression.Property(parameter, nullFilter.Property), Expression.Constant(null));
            }

            return null;
        }

        private static Expression ResolveEqualityFilter(EqualityFilter equalityFilter, ParameterExpression parameter)
        {
            switch (equalityFilter.Filter)
            {
                case FilterType.Equal:
                    {
                        return Expression.Equal(
                            Expression.Property(parameter, equalityFilter.Property), 
                            Expression.Constant(equalityFilter.Value));
                    }
                case FilterType.NotNull:
                    {
                        return Expression.NotEqual(
                            Expression.Property(parameter, equalityFilter.Property), 
                            Expression.Constant(null));
                    }
                case FilterType.NotEmpty:
                    {
                        return Expression.Equal(
                            Expression.Property(parameter, equalityFilter.Property), 
                            Expression.Constant(string.Empty));
                    }
                case FilterType.Null:
                    {
                        return Expression.Equal(
                            Expression.Property(parameter, equalityFilter.Property), 
                            Expression.Constant(null));
                    }
                case FilterType.Like:
                    {
                        return Expression.Equal(
                            Expression.Call(
                                Expression.Property(parameter, equalityFilter.Property), 
                                typeof(string).GetMethod("Contains", new[] { typeof(string) }), 
                                new Expression[] { Expression.Constant(equalityFilter.Value as string) }), 
                            Expression.Constant(true));
                    }
                case FilterType.GreaterThanOrEqualTo:
                    {
                        return Expression.GreaterThanOrEqual(
                            Expression.Property(parameter, equalityFilter.Property), 
                            Expression.Constant(equalityFilter.Value));
                    }
                case FilterType.GreaterThan:
                    {
                        return Expression.GreaterThan(
                            Expression.Property(parameter, equalityFilter.Property), 
                            Expression.Constant(equalityFilter.Value));
                    }
                case FilterType.LessThanOrEqualTo:
                    {
                        return Expression.LessThanOrEqual(
                            Expression.Property(parameter, equalityFilter.Property), 
                            Expression.Constant(equalityFilter.Value));
                    }
                case FilterType.LessThan:
                    {
                        return Expression.LessThan(
                            Expression.Property(parameter, equalityFilter.Property), 
                            Expression.Constant(equalityFilter.Value));
                    }
                case FilterType.NotEqual:
                    {
                        return Expression.NotEqual(
                            Expression.Property(parameter, equalityFilter.Property), 
                            Expression.Constant(equalityFilter.Value));
                    }
                case FilterType.In:
                    {
                        var valueType = equalityFilter.Value.GetType();

                        return Expression.Equal(
                            Expression.Call(
                                Expression.Constant(equalityFilter.Value),
                                valueType.GetMethod("Contains", new[] { valueType.GetGenericArguments()[0] }),
                                new Expression[] { Expression.Property(parameter, equalityFilter.Property) }),
                            Expression.Constant(true));
                    }
                case FilterType.NotIn:
                    {
                        var valueType = equalityFilter.Value.GetType();

                        return Expression.NotEqual(
                            Expression.Call(
                                Expression.Constant(equalityFilter.Value),
                                valueType.GetMethod("Contains", new[] { valueType.GetGenericArguments()[0] }),
                                new Expression[] { Expression.Property(parameter, equalityFilter.Property) }),
                            Expression.Constant(true));
                    }
                case FilterType.StartsWith:
                    {
                        return Expression.Equal(
                            Expression.Call(
                                Expression.Property(parameter, equalityFilter.Property),
                                typeof(string).GetMethod("StartsWith", new[] { typeof(string) }),
                                new Expression[] { Expression.Constant(equalityFilter.Value) }),
                            Expression.Constant(true));
                    }
                default:
                    {
                        return null;
                    }
            }
        }
    }
}
