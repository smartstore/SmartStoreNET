using SmartStore.Rules;
using SmartStore.Services.Common;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class UserAgentRule : IRule
    {
        private readonly IUserAgent _userAgent;

        public UserAgentRule(IUserAgent userAgent)
        {
            _userAgent = userAgent;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            return expression.Operator.Match(_userAgent.RawValue, expression.Value);
        }
    }
}
