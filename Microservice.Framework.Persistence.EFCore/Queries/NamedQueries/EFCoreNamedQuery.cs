using Microservice.Framework.Persistence.EFCore.Queries.Filtering;
using Microservice.Framework.Persistence.Queries.NamedQueries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Framework.Persistence.EFCore.Queries.NamedQueries
{
    public abstract class EFCoreNamedQuery<TModel> 
        : StoredProcedureQuery<TModel, EFCoreNamedCriteria>
        where TModel : class
    {
    }
}
