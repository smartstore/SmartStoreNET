using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Rules.Cart.Impl;
using SmartStore.Rules.Filters;
using SmartStore.Rules.Domain;
using SmartStore.Core.Domain.Customers;
using System.Data.Entity;

namespace SmartStore.Rules.Customers
{
    public class CustomerRuleService : RuleServiceBase
    {
        public CustomerRuleService()
            : base(RuleScope.Customer)
        {
        }

        public override IRuleExpression VisitRule(RuleEntity rule)
        {
            var expression = new FilterExpression();
            base.ConvertRule(rule, expression);
            return expression;
        }

        public override IRuleExpressionGroup VisitRuleSet(RuleSetEntity ruleSet)
        {
            var expression = new RuleExpressionGroup
            {
                LogicalOperator = ruleSet.LogicalOperator
            };

            return expression;
        }

        protected override IEnumerable<RuleDescriptor> LoadDescriptors()
        {
            return new List<FilterDescriptor>
            {
                new FilterDescriptor<Customer, bool>(x => x.IsTaxExempt)
                {
                    RuleType = RuleType.Boolean,
                    Name = "TaxExempt"
                },
                new FilterDescriptor<Customer, int?>(x => x.BillingAddress.CountryId)
                {
                    RuleType = RuleType.NullableInt,
                    Name = "BillingCountry"
                },
                new FilterDescriptor<Customer, int?>(x => x.ShippingAddress.CountryId)
                {
                    RuleType = RuleType.NullableInt,
                    Name = "ShippingCountry"
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastActivityDateUtc, DateTime.UtcNow))
                {
                    RuleType = RuleType.NullableInt,
                    Name = "LastActivityDays"
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 30))
                {
                    RuleType = RuleType.Int,
                    Name = "CompletedOrderCount"
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 40))
                {
                    RuleType = RuleType.Int,
                    Name = "CancelledOrderCount"
                },
                // TODO: more ...
            };
        }
    }
}
