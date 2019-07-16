using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Impl
{
    public class RuleRule : RuleBase
    {
        protected IRule GetOtherRule()
        {
            var ruleId = Expression.Value.Convert<int>();

            // TODO: get other rule from service
            IRule rule = null;

            return rule;
        }

        public override bool Match(RuleContext context)
        {
            var rule = GetOtherRule();
            if (rule == null)
                return false; // TBD: really?!

            if (Expression.Operator == RuleOperation.EqualTo)
            {
                return rule.Match(context);
            }
            if (Expression.Operator == RuleOperation.NotEqualTo)
            {
                return !rule.Match(context);
            }

            throw new InvalidRuleOperatorException(this);
        }

        public override void ApplyToQuery(QueryRuleContext context)
        {
            throw new NotSupportedException();
        }

        protected override RuleDescriptor GetRuleMetadata()
        {
            return new RuleDescriptor
            {
                Type = RuleType.Int,
                Editor = "Rules",
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
