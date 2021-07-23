using Microservice.Framework.Common;
using Microservice.Framework.Persistence.Queries.Filtering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microservice.Framework.Persistence.EFCore.Queries.Filtering
{
    public class EFCoreNamedCriteria : NamedCriteria
    {
        public override string ToString()
        {
            var result = "{0} ".FormatInvariantCulture(Name);

            foreach(var param in Parameters.Select((v, i) => new { i, v }))
            {
                if (param.i == 0)
                    result += $"@{param.v.Key}";
                else
                    result += $",@{param.v.Key}";
            }

            return result;
        }

    }
}
