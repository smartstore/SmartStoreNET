using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Customers;
using SmartStore.Rules;

namespace SmartStore.Services.Cart.Rules.Impl
{
    public class CustomerRoleRule : IRule
    {
        public bool Match(CartRuleContext context, RuleExpression expression)
        {
            var list = expression.Value as List<int>;
            if (list == null || list.Count == 0)
            {
                return true;
            }

            var currentRoleIds = context.Customer.CustomerRoles.Select(x => x.Id);
            return currentRoleIds.All(x => expression.Operator.Match(x, list));
        }
    }
}
