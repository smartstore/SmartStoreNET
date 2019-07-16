using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Domain.Customers;

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

        public override void ApplyToQuery(QueryRuleContext context)
        {
            if (context.Query is IQueryable<Customer> query)
            {
                var arr = Expression.Value.Convert<IEnumerable<int>>();

                // TODO
                query = query.Where(c => c.CustomerRoles.Any(r => r.Id == arr.First()));
            }
        }

        protected override RuleDescriptor GetRuleMetadata()
        {
            return new RuleDescriptor
            {
                Type = RuleType.IntArray,
                Editor = "CustomerRole",
                Constraints = new IRuleConstraint[0]
            };
        }
    }
}
