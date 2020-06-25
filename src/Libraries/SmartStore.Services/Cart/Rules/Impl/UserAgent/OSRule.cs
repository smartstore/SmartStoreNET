using SmartStore.Services.Common;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class OSRule : ListRuleBase<string>
    {
        private readonly IUserAgent _userAgent;

        public OSRule(IUserAgent userAgent)
        {
            _userAgent = userAgent;
        }

        protected override string GetValue(CartRuleContext context)
        {
            return _userAgent.OS.Family.NullEmpty();
        }
    }
}
