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
            var ruleId = Expression.Comparand.Convert<int>();

            // TODO: get other rule from service
            IRule rule = null;

            return rule;
        }

        public override bool Match(RuleContext context)
        {
            var rule = GetOtherRule();
            if (rule == null)
                return false; // TBD: really?!

            if (Expression.Operator == RuleOperators.Equal)
            {
                return rule.Match(context);
            }
            if (Expression.Operator == RuleOperators.NotEqual)
            {
                return !rule.Match(context);
            }

            throw new InvalidRuleOperatorException(this);
        }

        public override void ApplyToQuery(QueryRuleContext context)
        {
            throw new NotSupportedException();
        }

        protected override RuleMetadata GetRuleMetadata()
        {
            return new RuleMetadata
            {
                TypeCode = RuleTypeCode.Int,
                Operators = new string[] { RuleOperators.Equal, RuleOperators.NotEqual },
                Editor = "Rules",
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
