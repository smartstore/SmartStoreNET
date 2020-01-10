namespace SmartStore.Services.Cart.Rules.Impl
{
    public class StoreRule : ListRuleBase<int>
    {
        protected override object GetValue(CartRuleContext context)
        {
            return context.Store.Id;
        }
    }
}
