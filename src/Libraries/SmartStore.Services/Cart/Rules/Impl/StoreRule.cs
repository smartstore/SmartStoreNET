namespace SmartStore.Services.Cart.Rules.Impl
{
    public class StoreRule : ListRuleBase<int>
    {
        protected override int GetValue(CartRuleContext context)
        {
            return context.Store.Id;
        }
    }
}
