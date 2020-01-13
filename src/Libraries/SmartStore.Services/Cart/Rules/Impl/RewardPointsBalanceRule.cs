using SmartStore.Rules;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class RewardPointsBalanceRule : IRule
    {
        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var rewardPointsBalance = context.Customer.GetRewardPointsBalance();

            return expression.Operator.Match(rewardPointsBalance, expression.Value);
        }
    }
}
