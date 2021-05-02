using Microsoft.Extensions.DependencyInjection;
using Microservice.Framework.Ioc;
using System;

namespace Microservice.Framework.Persistence.EFCore
{
    public interface IEntityFrameworkConfiguration
    {
        void Apply(IServiceCollection serviceCollection);
    }
}