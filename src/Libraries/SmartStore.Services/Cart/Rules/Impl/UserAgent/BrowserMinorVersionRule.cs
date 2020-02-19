using SmartStore.Rules;
using SmartStore.Services.Common;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class BrowserMinorVersionRule : IRule
    {
        private readonly IUserAgent _userAgent;

        public BrowserMinorVersionRule(IUserAgent userAgent)
        {
            _userAgent = userAgent;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            if (_userAgent.UserAgent.Minor.HasValue() && int.TryParse(_userAgent.UserAgent.Minor, out var minorVersion))
            {
                return expression.Operator.Match(minorVersion, expression.Value);
            }

            return false;
        }
    }
}
