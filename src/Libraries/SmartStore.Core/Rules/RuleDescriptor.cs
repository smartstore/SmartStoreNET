using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public class RuleDescriptor
    {
        private RuleOperator[] _operators;

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public RuleType RuleType { get; set; }
        public RuleValueSelectList SelectList { get; set; }
        public IEnumerable<IRuleConstraint> Constraints { get; set; }
        public IDictionary<string, object> Metadata { get; }

        public RuleOperator[] Operators
        {
            get => _operators ?? (_operators = RuleType.GetValidOperators().ToArray());
            set => _operators = value;
        }
    }
}
