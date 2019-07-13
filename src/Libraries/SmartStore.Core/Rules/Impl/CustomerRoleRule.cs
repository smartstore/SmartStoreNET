using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Rules.Impl
{
    [Rule("CustomerRole", FriendlyName = "Customer role", Scope = RuleScope.Cart | RuleScope.Customer, DisplayOrder = 0)]
    public class CustomerRoleRule : ArrayRuleBase<int>
    {
        protected override int GetValue(RuleContext context)
        {
            // TODO: Allow many values
            return context.Customer.CustomerRoles.FirstOrDefault()?.Id ?? 0;
        }

        protected override RuleMetadata GetRuleMetadata()
        {
            return new RuleMetadata
            {
                TypeCode = RuleTypeCode.IntArray,
                Operators = RuleOperators.ArrayOperators,
                Editor = "CustomerRole",
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
