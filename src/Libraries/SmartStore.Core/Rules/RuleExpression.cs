using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public interface IRuleExpression
    {
        int Id { get; }
        RuleDescriptor Descriptor { get; }
        RuleOperator Operator { get; }
        object Value { get; }
        string RawValue { get; }
    }

    public class RuleExpression : IRuleExpression
    {
        public int Id { get; set; }
        public RuleDescriptor Descriptor { get; set; }
        public RuleOperator Operator { get; set; }
        public object Value { get; set; }
        public string RawValue { get; set; }
    }
}
