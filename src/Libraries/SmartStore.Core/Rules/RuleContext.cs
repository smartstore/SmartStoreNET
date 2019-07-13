using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Rules
{
    public class RuleContext
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public IWorkContext WorkContext { get; set; }
    }

    public class QueryRuleContext
    {
        public IQueryable Query { get; set; }
    }
}
