using Microsoft.Extensions.DependencyInjection;
using Microservice.Framework.Ioc;
using System;

namespace Microservice.Framework.Persistence.EFCore
{
    public class EntityFrameworkConfiguration : IEntityFrameworkConfiguration
    {
        private Action<IServiceCollection> _registerUniqueConstraintDetectionStrategy;
        private Action<IServiceCollection> _registerBulkOperationConfiguration;

        public static EntityFrameworkConfiguration New => new EntityFrameworkConfiguration();

        private EntityFrameworkConfiguration()
        {
            UseUniqueConstraintDetectionStrategy<DefaultUniqueConstraintDetectionStrategy>();
            UseBulkOperationConfiguration<DefaultBulkOperationConfiguration>();
        }

        void IEntityFrameworkConfiguration.Apply(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IEntityFrameworkConfiguration>(s => this);
            _registerUniqueConstraintDetectionStrategy(serviceCollection);
            _registerBulkOperationConfiguration(serviceCollection);
        }

        public EntityFrameworkConfiguration UseBulkOperationConfiguration<T>()
            where T : class, IBulkOperationConfiguration
        {
            _registerBulkOperationConfiguration = s => s.AddTransient<IBulkOperationConfiguration, T>();
            return this;
        }

        public EntityFrameworkConfiguration UseUniqueConstraintDetectionStrategy<T>()
            where T : class, IUniqueConstraintDetectionStrategy
        {
            _registerUniqueConstraintDetectionStrategy = s => s.AddTransient<IUniqueConstraintDetectionStrategy, T>();
            return this;
        }
    }
}