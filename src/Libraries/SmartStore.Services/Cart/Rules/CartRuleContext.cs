using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Cart.Rules
{
    public class CartRuleContext
    {
        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public IWorkContext WorkContext { get; set; }
    }
}
