using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Cart.Impl
{
    public class CompositeRule : IRule
    {
        private readonly RuleExpressionGroup _group;
        private readonly CartRuleService _cartRuleService;

        public CompositeRule(RuleExpressionGroup group, CartRuleService cartRuleService)
        {
            _group = group;
            _cartRuleService = cartRuleService;
        }

        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            bool match = false;

            foreach (var expr in _group.Expressions.Cast<RuleExpression>())
            {
                var descriptor = expr.Descriptor as CartRuleDescriptor;
                if (descriptor == null)
                    continue;

                var processor = _cartRuleService.GetProcessor(expr);

                match = processor.Match(context, expr);

                if (!match && _group.LogicalOperator == LogicalRuleOperator.And)
                    break;

                if (match && _group.LogicalOperator == LogicalRuleOperator.Or)
                    break;
            }

            return match;
        }
    }
}