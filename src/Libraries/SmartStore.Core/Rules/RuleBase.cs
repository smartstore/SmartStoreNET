using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public abstract class RuleBase : IRule
    {
        private RuleDescriptor _metadata;

        public RuleExpression Expression { get; set; }

        public abstract bool Match(RuleContext context);

        public abstract void ApplyToQuery(QueryRuleContext context);

        public RuleDescriptor Descriptor
        {
            get
            {
                return _metadata ?? (_metadata = GetRuleMetadata());
            }
        }

        protected abstract RuleDescriptor GetRuleMetadata();
    }
}
