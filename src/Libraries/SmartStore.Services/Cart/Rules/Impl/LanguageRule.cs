namespace SmartStore.Services.Cart.Rules.Impl
{
    public class LanguageRule : ListRuleBase<int>
    {
        protected override int GetValue(CartRuleContext context)
        {
            return context.WorkContext.WorkingLanguage.Id;
        }
    }
}
