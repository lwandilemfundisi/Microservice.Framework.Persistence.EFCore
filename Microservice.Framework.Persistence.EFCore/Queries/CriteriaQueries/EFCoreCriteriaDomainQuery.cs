﻿using Microservice.Framework.Persistence.Extensions;
using Microservice.Framework.Persistence.Queries;
using Microservice.Framework.Persistence.Queries.Filtering;
using Microservice.Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microservice.Framework.Persistence.EFCore.Queries.Filtering;
using Microservice.Framework.Persistence.Queries.CriteriaQueries;

namespace Microservice.Framework.Persistence.EFCore.Queries.CriteriaQueries
{
    public abstract class EFCoreCriteriaDomainQuery<TDomain> : CriteriaDomainQuery<TDomain, EFCoreDomainCriteria>
        where TDomain : class
    {
        public EFCoreCriteriaDomainQuery()
            : base()
        {
        }
    }
}
