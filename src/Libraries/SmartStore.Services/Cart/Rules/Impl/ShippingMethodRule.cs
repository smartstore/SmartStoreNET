using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Common;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class ShippingMethodRule : ListRuleBase<int>
    {
        protected override int GetValue(CartRuleContext context)
        {
            var shippingMethod = context.Customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, context.Store.Id);

            return shippingMethod?.ShippingMethodId ?? 0;
        }
    }
}
