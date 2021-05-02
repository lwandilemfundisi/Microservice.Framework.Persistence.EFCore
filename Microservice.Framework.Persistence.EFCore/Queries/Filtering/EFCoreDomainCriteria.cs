using Microservice.Framework.Persistence.Queries.Filtering;

namespace Microservice.Framework.Persistence.EFCore.Queries.Filtering
{
    public class EFCoreDomainCriteria : DomainCriteria
    {
        public Include Include { get; set; }
    }
}
