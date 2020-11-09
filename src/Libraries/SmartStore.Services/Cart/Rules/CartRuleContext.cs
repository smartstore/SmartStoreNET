using System;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Stores;

namespace SmartStore.Services.Cart.Rules
{
    public class CartRuleContext
    {
        private readonly Func<object> _sessionKeyBuilder;
        private object _sessionKey;

        internal CartRuleContext(Func<object> sessionKeyBuilder)
        {
            _sessionKeyBuilder = sessionKeyBuilder;
        }

        public Customer Customer { get; set; }
        public Store Store { get; set; }
        public IWorkContext WorkContext { get; set; }

        public object SessionKey => _sessionKey ?? (_sessionKey = _sessionKeyBuilder?.Invoke() ?? 0);
    }
}
