using System.Linq;
using SmartStore.Rules;
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

        public static RuleValueSelectListOption[] GetDefaultValues()
        {
            return new[]
            {
                "Android",
                "BlackBerry OS",
                "BlackBerry Tablet OS",
                "Chrome OS",
                "Firefox OS",
                "iOS",
                "Kindle",
                "Linux",
                "Mac OS X",
                "Symbian OS",
                "Ubuntu",
                "webOS",
                "Windows",
                "Windows Mobile",
                "Windows Phone",
                "Windows CE"
            }
            .Select(x => new RuleValueSelectListOption { Value = x, Text = x })
            .ToArray();
        }

        protected override string GetValue(CartRuleContext context)
        {
            return _userAgent.OS.Family.NullEmpty();
        }
    }
}
