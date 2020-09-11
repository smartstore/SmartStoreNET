using System;

namespace SmartStore.Rules
{
    public sealed class InvalidRuleOperatorException : NotSupportedException
    {
        public InvalidRuleOperatorException(RuleExpression expression)
            : base("The rule type '{0}' does not support the rule operator '{1}'.".FormatInvariant(expression.Descriptor.Name, expression.Operator))
        {
            // TODO: expression.Descriptor.Name could be null or empty (?)
        }
    }
}
