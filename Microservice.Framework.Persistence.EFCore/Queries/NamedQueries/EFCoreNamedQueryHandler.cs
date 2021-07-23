using Microservice.Framework.Persistence.EFCore.Queries.Filtering;
using Microservice.Framework.Persistence.Queries.NamedQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Framework.Persistence.EFCore.Queries.NamedQueries
{
    public abstract class EFCoreNamedQueryHandler<TModel> 
        : StoredProcedureQueryHandler<TModel, EFCoreNamedCriteria> where TModel : class
    {
        public EFCoreNamedQueryHandler(IPersistenceFactory persistenceFactory)
            : base(persistenceFactory)
        {
        }
    }
}
