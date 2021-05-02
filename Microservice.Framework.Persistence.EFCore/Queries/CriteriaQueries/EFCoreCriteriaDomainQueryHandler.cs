using Microservice.Framework.Persistence.Queries;
using Microservice.Framework.Persistence.Queries.Filtering;
using Microservice.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microservice.Framework.Persistence.EFCore.Queries.Filtering;
using Microservice.Framework.Persistence.Queries.CriteriaQueries;

namespace Microservice.Framework.Persistence.EFCore.Queries.CriteriaQueries
{
    public abstract class EFCoreCriteriaDomainQueryHandler<TDomain> : CriteriaDomainQueryHandler<TDomain, EFCoreDomainCriteria> 
        where TDomain : class
    {
        public EFCoreCriteriaDomainQueryHandler(IPersistenceFactory persistenceFactory)
            : base(persistenceFactory)
        {
        }
    }
}
