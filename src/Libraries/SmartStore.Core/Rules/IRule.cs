using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Plugins;

namespace SmartStore.Rules
{
    [Flags]
    public enum RuleScope
    {
        Cart = 1,
        LineItem = 2,
        Customer = 4,
        Product = 8,
    }

    public interface IRule : IRuleDescriptorAccessor
    {
        RuleExpression Expression { get; set; }
        bool Match(RuleContext context);
    }
}
