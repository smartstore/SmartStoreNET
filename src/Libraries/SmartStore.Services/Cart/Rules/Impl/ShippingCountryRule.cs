namespace SmartStore.Services.Cart.Rules.Impl
{
    public class ShippingCountryRule : ListRuleBase<int>
    {
        protected override object GetValue(CartRuleContext context)
        {
            return context.Customer?.ShippingAddress?.CountryId ?? 0;
        }
    }
}
