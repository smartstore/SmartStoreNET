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

    public interface IRuleExpressionGroup : IRuleExpression
    {
        int RefRuleId { get; set; }
        LogicalRuleOperator LogicalOperator { get; }
        bool IsSubGroup { get; }
        IRuleProvider Provider { get; }
        IEnumerable<IRuleExpression> Expressions { get; }
        void AddExpressions(params IRuleExpression[] expressions);
    }

    public class RuleExpressionGroup : RuleExpression, IRuleExpressionGroup
    {
        private readonly List<IRuleExpression> _expressions = new List<IRuleExpression>();

        public int RefRuleId { get; set; }
        public LogicalRuleOperator LogicalOperator { get; set; }
        public bool IsSubGroup { get; set; }
        public IRuleProvider Provider { get; set; }

        public IEnumerable<IRuleExpression> Expressions
        {
            get => _expressions;
        }

        public virtual void AddExpressions(params IRuleExpression[] expressions)
        {
            Guard.NotNull(expressions, nameof(expressions));
            _expressions.AddRange(expressions);
        }
    }
}