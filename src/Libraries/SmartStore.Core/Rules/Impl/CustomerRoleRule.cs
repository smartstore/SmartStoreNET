using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Rules.Impl
{
    [Rule("CustomerRole", FriendlyName = "Customer role", Scope = RuleScope.Cart | RuleScope.Customer, DisplayOrder = 0)]
    public class CustomerRoleRule : RuleBase
    {
        public override bool Match(RuleContext context)
        {
            var list = Expression.Value.Convert<List<int>>();
            if (list == null || list.Count == 0)
            {
                return true;
            }

            var currentRoleIds = context.Customer.CustomerRoles.Select(x => x.Id);
            return currentRoleIds.All(x => Expression.Operator.Match(x, list));
        }

        protected override RuleDescriptor GetRuleDescriptor()
        {
            return new RuleDescriptor
            {
                RuleType = RuleType.IntArray,
                SelectList = new RemoteRuleValueSelectList("CustomerRole") { Multiple = true },
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
