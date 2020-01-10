namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CurrencyRule : ListRuleBase<int>
    {
        protected override object GetValue(CartRuleContext context)
        {
            return context.WorkContext.WorkingCurrency.Id;
        }
    }
}
