using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Plugins;
using SmartStore.Rules;

namespace SmartStore.Services.Cart.Rules
{
    public interface IRule
    {
        bool Match(CartRuleContext context, RuleExpression expression);
    }
}
