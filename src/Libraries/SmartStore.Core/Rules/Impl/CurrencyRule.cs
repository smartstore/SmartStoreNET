using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Impl
{
    [Rule("Currency", FriendlyName = "Currency", Scope = RuleScope.Cart, DisplayOrder = 0)]
    public class CurrencyRule : ArrayRuleBase<int>
    {
        protected override int GetValue(RuleContext context)
        {
            return context.WorkContext.WorkingCurrency.Id;
        }

        protected override RuleMetadata GetRuleMetadata()
        {
            return new RuleMetadata
            {
                TypeCode = RuleTypeCode.IntArray,
                Operators = RuleOperators.ArrayOperators,
                Editor = "Currency",
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
