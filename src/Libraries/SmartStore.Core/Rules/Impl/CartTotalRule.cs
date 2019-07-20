using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Impl
{
    public class CartTotalRule : RuleBase
    {
        public override bool Match(RuleContext context)
        {
            // INFO/TODO: get cart total from somewhere
            var cartTotal = 1000d;

            return Expression.Operator.Match(cartTotal, Expression.Value);
        }

        protected override RuleDescriptor GetRuleDescriptor()
        {
            return new RuleDescriptor
            {
                RuleType = RuleType.Float,
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
