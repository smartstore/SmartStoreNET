using System.Collections.Generic;
using System.Linq;
using SmartStore.Utilities;

namespace SmartStore.Rules
{
    public abstract class RuleDescriptor
    {
        private RuleOperator[] _operators;
        private string _controlId;

        protected RuleDescriptor(RuleScope scope)
        {
            Scope = scope;
            Metadata = new Dictionary<string, object>();
        }

        public RuleScope Scope { get; protected set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public string ControlId
        {
            get
            {
                if (_controlId == null)
                {
                    _controlId = string.Concat("rule-value-", CommonHelper.GenerateRandomInteger());
                }

                return _controlId;
            }
        }

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
