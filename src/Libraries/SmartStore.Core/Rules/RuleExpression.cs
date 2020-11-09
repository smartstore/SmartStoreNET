using System.Collections.Generic;
using SmartStore.Utilities;

namespace SmartStore.Rules
{
    public interface IRuleExpression
    {
        int Id { get; }
        int RuleSetId { get; }
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
        public int RuleSetId { get; set; }
        public RuleDescriptor Descriptor { get; set; }
        public RuleOperator Operator { get; set; }
        public object Value { get; set; }
        public string RawValue { get; set; }
        public IDictionary<string, object> Metadata { get; set; }

        public override int GetHashCode()
        {
            var combiner = HashCodeCombiner
                .Start()
                .Add(RuleSetId)
                .Add(Id)
                .Add(Operator)
                .Add(RawValue);

            return combiner.CombinedHash;
        }
    }
}
