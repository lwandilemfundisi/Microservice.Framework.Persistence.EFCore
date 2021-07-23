using Microservice.Framework.Persistence.EFCore.Queries.CriteriaQueries;
using Microservice.Framework.Persistence.EFCore.Queries.Filtering;
using Microservice.Framework.Persistence.EFCore.Queries.NamedQueries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microservice.Framework.Persistence.EFCore.Test
{
    public class Tests
    {
        private IPersistenceFactory _factory;

        [SetUp]
        public void Setup()
        {
            var myConfiguration = new Dictionary<string, string>
            {
                { "Timeout", "180" }
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(myConfiguration)
                .Build();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            EntityFrameworkConfiguration.New.Apply(serviceCollection);
            serviceCollection.AddTransient<IDbContextProvider<TestDbContext>, TestDbContextProvider>();
            serviceCollection.AddTransient<IPersistenceFactory, EntityFrameworkPersistenceFactory<TestDbContext>>();
            serviceCollection.AddSingleton<IConfiguration>(rctx => { return configuration; });
            var provicer = serviceCollection.BuildServiceProvider();

            _factory = provicer.GetService<IPersistenceFactory>();
        }

        [Test]
        public async Task Test1()
        {
            //var h = new TestCriteriaHandler(_factory);

            //var all = await h.FindAll(new TestProductQuery());

            
            var h = new TestNamedHandler(_factory);

            var all = await h.FindAll(new TestNamedProc());
        }
    }

    public class TestProductQuery : EFCoreCriteriaDomainQuery<Product>
    {

    }

    public class TestNamedProc : EFCoreNamedQuery
    {
        public override string Name => "[SalesLT].[Product_GetAll]";

        public override void OnBuildParameters(IDictionary<string, object> parameters)
        {
        }
    }

    public class TestNamedHandler : EFCoreNamedQueryHandler<Product>
    {
        public TestNamedHandler(IPersistenceFactory p)
            : base(p)
        {

        }
    }

    public class TestCriteriaHandler : EFCoreCriteriaDomainQueryHandler<Product>
    {
        public TestCriteriaHandler(IPersistenceFactory p)
            : base(p)
        {

        }
    }
}