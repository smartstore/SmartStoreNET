using System;
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
        public Func<object> GetRuleHashCode { get; set; }
    }
}
