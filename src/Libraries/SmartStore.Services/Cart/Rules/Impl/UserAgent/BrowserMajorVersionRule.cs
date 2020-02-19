using SmartStore.Rules;
using SmartStore.Services.Common;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class BrowserMajorVersionRule : IRule
    {
        private readonly IUserAgent _userAgent;

        public BrowserMajorVersionRule(IUserAgent userAgent)
        {
            _userAgent = userAgent;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            if (_userAgent.UserAgent.Major.HasValue() && int.TryParse(_userAgent.UserAgent.Major, out var majorVersion))
            {
                return expression.Operator.Match(majorVersion, expression.Value);
            }

            return false;
        }
    }
}
