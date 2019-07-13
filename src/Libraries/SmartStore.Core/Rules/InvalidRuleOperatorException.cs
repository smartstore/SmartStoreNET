using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public sealed class InvalidRuleOperatorException : NotSupportedException
    {
        public InvalidRuleOperatorException(IRule rule) 
            : base("The rule type '{0}' does not support the rule operator '{1}'.".FormatInvariant(rule.GetType().FullName, rule.Expression.Operator))
        {
        }
    }
}
