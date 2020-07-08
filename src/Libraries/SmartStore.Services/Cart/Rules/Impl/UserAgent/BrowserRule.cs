using System.Linq;
using SmartStore.Rules;
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

        public static RuleValueSelectListOption[] GetDefaultValues()
        {
            return new[]
            {
                "Chrome",
                "Chrome Mobile",
                "Edge",
                "Firefox",
                "Firefox Mobile",
                "IE",
                "IE Mobile",
                "Mobile Safari",
                "Opera",
                "Opera Mobile",
                "Opera Mini",
                "Safari",
                "Samsung Internet"
            }
            .Select(x => new RuleValueSelectListOption { Value = x, Text = x })
            .ToArray();
        }

        protected override string GetValue(CartRuleContext context)
        {
            return _userAgent.UserAgent.Family.NullEmpty();
        }
    }
}
