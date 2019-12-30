using System.Collections.Generic;

namespace SmartStore.Rules
{
    public interface IRuleExpression
    {
        int Id { get; }
        RuleDescriptor Descriptor { get; }
        RuleOperator Operator { get; }
        object Value { get; }
        string RawValue { get; }
        IDictionary<string, object> Metadata { get; }
    }

    public class RuleExpression : IRuleExpression
    {
        public RuleExpression()
        {
            Metadata = new Dictionary<string, object>();
        }

        public int Id { get; set; }
        public RuleDescriptor Descriptor { get; set; }
        public RuleOperator Operator { get; set; }
        public object Value { get; set; }
        public string RawValue { get; set; }
        public IDictionary<string, object> Metadata { get; set; }
    }
}
