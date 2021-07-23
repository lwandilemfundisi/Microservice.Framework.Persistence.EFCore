using Microservice.Framework.Persistence.EFCore.Queries.Filtering;
using Microservice.Framework.Persistence.EFCore.Queries.NamedQueries;
using NUnit.Framework;
using System.Collections.Generic;

namespace Microservice.Framework.Persistence.EFCore.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var newQuery = new TestNamedProc();
            var namedCriteria = newQuery.BuildNamedCriteria();
            var test = namedCriteria.BuildQueryString();
        }
    }

    public class TestNamedProc : EFCoreNamedQuery
    {
        public override string Name => "StoredProc";

        public override void OnBuildParameters(IDictionary<string, object> parameters)
        {
            parameters.Add("value1", 1);
            parameters.Add("value2", 2);
            parameters.Add("value3", 3);
        }
    }
}