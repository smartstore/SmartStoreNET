namespace SmartStore.Services.Cart.Rules.Impl
{
    public class ShippingCountryRule : ListRuleBase<int>
    {
        protected override int GetValue(CartRuleContext context)
        {
            return context.Customer?.ShippingAddress?.CountryId ?? 0;
        }
    }
}
