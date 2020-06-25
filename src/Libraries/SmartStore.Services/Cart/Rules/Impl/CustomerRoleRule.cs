using System.Collections.Generic;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CustomerRoleRule : ListRuleBase<int>
    {
        protected override IEnumerable<int> GetValues(CartRuleContext context)
        {
            var roleIds = context.Customer.GetRoleIds();
            return roleIds;
        }
    }
}
