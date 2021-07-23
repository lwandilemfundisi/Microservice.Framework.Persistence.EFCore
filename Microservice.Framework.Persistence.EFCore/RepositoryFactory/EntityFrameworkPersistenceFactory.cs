using Microservice.Framework.Common;
using Microservice.Framework.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microservice.Framework.Persistence.Queries.Filtering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Microservice.Framework.Persistence.EFCore
{
    public class EntityFrameworkPersistenceFactory<TDbContext> : IPersistenceFactory
        where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContextProvider<TDbContext> contextProvider;
        private readonly IUniqueConstraintDetectionStrategy strategy;

        public EntityFrameworkPersistenceFactory(
            IServiceProvider serviceProvider,
            IDbContextProvider<TDbContext> contextProvider,
            IUniqueConstraintDetectionStrategy strategy)
        {
            _serviceProvider = serviceProvider;
            this.contextProvider = contextProvider;
            this.strategy = strategy;
        }

        public IPersistence GetPersistence<TDomain>()
            where TDomain : class
        {
            return new EntityFrameworkPersistence<TDbContext>(
                _serviceProvider.GetRequiredService<ILogger<EntityFrameworkPersistence<TDbContext>>>(), 
                contextProvider, 
                strategy,
                _serviceProvider.GetService<IConfiguration>());
        }

        public IPersistence GetPersistence(Type type)
        {
            return new EntityFrameworkPersistence<TDbContext>(
                _serviceProvider.GetRequiredService<ILogger<EntityFrameworkPersistence<TDbContext>>>(),
                contextProvider, 
                strategy,
                _serviceProvider.GetService<IConfiguration>());
        }
    }
}
