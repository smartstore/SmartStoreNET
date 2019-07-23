using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Cart.Impl
{
    public class CartTotalRule : IRule
    {
        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            // INFO/TODO: get cart total from somewhere
            var cartTotal = 1000d;

            return expression.Operator.Match(cartTotal, expression.Value);
        }
    }
}
