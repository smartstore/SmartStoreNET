using System;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services.Common;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class PaymentMethodRule : ListRuleBase<string>
    {
        public PaymentMethodRule()
            : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        protected override string GetValue(CartRuleContext context)
        {
            var paymentMethod = context.Customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, context.Store.Id);

            return paymentMethod.NullEmpty();
        }
    }
}
