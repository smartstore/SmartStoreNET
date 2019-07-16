using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public enum LogicalRuleOperator
    {
        And,
        Or
    }

    public class CompositeRule : RuleBase
    {
        private readonly List<IRule> _rules = new List<IRule>();

        public LogicalRuleOperator LogicalOperator { get; set; }
        public IReadOnlyCollection<IRule> Rules
        {
            get => _rules;
        }

        public void AddRule(IRule rule)
        {
            Guard.NotNull(rule, nameof(rule));
            _rules.Add(rule);
        }

        public override bool Match(RuleContext context)
        {
            bool match = false;

            foreach (var rule in Rules)
            {
                match = rule.Match(context);

                if (!match && LogicalOperator == LogicalRuleOperator.And)
                    break;

                if (match && LogicalOperator == LogicalRuleOperator.Or)
                    break;
            }

            return match;
        }

        public override void ApplyToQuery(QueryRuleContext context)
        {
            foreach (var rule in Rules)
            {
                rule.ApplyToQuery(context); // TODO: Apply LINQ expression AND/OR
            }
        }

        protected override RuleDescriptor GetRuleMetadata()
        {
            throw new NotSupportedException();
        }
    }
}
