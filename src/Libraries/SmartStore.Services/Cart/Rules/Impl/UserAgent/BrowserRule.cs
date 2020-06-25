using SmartStore.Services.Common;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class BrowserRule : ListRuleBase<string>
    {
        private readonly IUserAgent _userAgent;

        public BrowserRule(IUserAgent userAgent)
        {
            _userAgent = userAgent;
        }

        protected override string GetValue(CartRuleContext context)
        {
            return _userAgent.UserAgent.Family.NullEmpty();
        }
    }
}
