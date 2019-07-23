using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Plugins;

namespace SmartStore.Rules.Cart
{
    public interface IRule
    {
        bool Match(CartRuleContext context, RuleExpression expression);
    }
}
