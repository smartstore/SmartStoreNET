namespace SmartStore.Services.Cart.Rules.Impl
{
    public class BillingCountryRule : ListRuleBase<int>
    {
        protected override int GetValue(CartRuleContext context)
        {
            return context.Customer?.BillingAddress?.CountryId ?? 0;
        }
    }
}
