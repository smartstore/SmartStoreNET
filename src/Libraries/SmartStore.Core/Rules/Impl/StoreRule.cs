using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Impl
{
    [Rule("Store", FriendlyName = "Store", Scope = RuleScope.Cart, DisplayOrder = 0)]
    public class StoreRule : ArrayRuleBase<int>
    {
        protected override int GetValue(RuleContext context)
        {
            return context.Store.Id;
        }

        protected override RuleMetadata GetRuleMetadata()
        {
            return new RuleMetadata
            {
                TypeCode = RuleTypeCode.IntArray,
                Operators = RuleOperators.ArrayOperators,
                Editor = "Store",
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
