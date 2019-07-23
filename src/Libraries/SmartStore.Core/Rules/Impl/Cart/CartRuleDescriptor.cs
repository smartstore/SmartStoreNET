using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Rules.Cart.Impl;

namespace SmartStore.Rules.Cart
{
    public class CartRuleDescriptor : RuleDescriptor
    {
        public CartRuleDescriptor() : base(RuleScope.Cart)
        {
        }

        public Type ProcessorType { get; set; }
        public IRule ProcessorInstance { get; set; }
    }
}
