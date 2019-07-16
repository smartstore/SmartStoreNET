using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public class RuleDescriptor
    {
        private RuleOperation[] _operators;

        public RuleType Type { get; set; }
        public bool IsOptional { get; set; }
        public IEnumerable<IRuleConstraint> Constraints { get; set; }
        public string Editor { get; set; }
        public IDictionary<string, object> Metadata { get; }

        public RuleOperation[] Operators
        {
            get => _operators ?? (_operators = Type.GetValidOperators().ToArray());
            set => _operators = value;
        }
    }
}
