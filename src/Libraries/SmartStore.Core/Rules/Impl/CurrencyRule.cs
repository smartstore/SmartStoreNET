using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Impl
{
    [Rule("Currency", FriendlyName = "Currency", Scope = RuleScope.Cart, DisplayOrder = 0)]
    public class CurrencyRule : ListRuleBase<int>
    {
        protected override int GetValue(RuleContext context)
        {
            return context.WorkContext.WorkingCurrency.Id;
        }

        protected override RuleDescriptor GetRuleDescriptor()
        {
            return new RuleDescriptor
            {
                RuleType = RuleType.IntArray,
                SelectList = new RemoteRuleValueSelectList("Currency") { Multiple = true },
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
