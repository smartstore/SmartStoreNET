using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules
{
    public abstract class RuleBase : IRule
    {
        private RuleDescriptor _descriptor;

        public RuleExpression Expression { get; set; }

        public abstract bool Match(RuleContext context);

        public RuleDescriptor Descriptor
        {
            get
            {
                return _descriptor ?? (_descriptor = GetRuleDescriptor());
            }
        }

        protected abstract RuleDescriptor GetRuleDescriptor();
    }
}
