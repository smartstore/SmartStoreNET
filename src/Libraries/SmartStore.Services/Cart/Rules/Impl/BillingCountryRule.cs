namespace SmartStore.Services.Cart.Rules.Impl
{
    public class BillingCountryRule : ListRuleBase<int>
    {
        protected override object GetValue(CartRuleContext context)
        {
            return context.Customer?.BillingAddress?.CountryId ?? 0;
        }
    }
}
