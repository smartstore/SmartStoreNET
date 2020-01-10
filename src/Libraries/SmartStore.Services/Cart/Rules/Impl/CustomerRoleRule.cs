using System.Collections.Generic;
using System.Linq;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CustomerRoleRule : ListRuleBase<int>
    {
        protected override IEnumerable<int> GetValues(CartRuleContext context)
        {
            var roleIds = context.Customer.CustomerRoles
                .Where(x => x.Active)
                .Select(x => x.Id);

            return roleIds;
        }
    }
}
