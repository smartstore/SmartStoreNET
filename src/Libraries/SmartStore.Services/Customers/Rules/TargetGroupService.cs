using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Rules;
using SmartStore.Rules.Filters;
using SmartStore.Rules.Domain;
using SmartStore.Core.Domain.Customers;

namespace SmartStore.Services.Customers
{
    public class TargetGroupService : RuleProviderBase, ITargetGroupService
    {
        private readonly IRepository<Customer> _rsCustomer;

        public TargetGroupService(IRepository<Customer> rsCustomer)
            : base(RuleScope.Customer)
        {
            _rsCustomer = rsCustomer;
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

        public IPagedList<Customer> ProcessFilter(FilterExpression filter, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            Guard.NotNull(filter, nameof(filter));

            return ProcessFilter(
                LogicalRuleOperator.And, 
                new[] { filter }, 
                pageIndex, 
                pageSize);
        }

        public IPagedList<Customer> ProcessFilter(
            LogicalRuleOperator logicalOperator,
            FilterExpression[] filters,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            Guard.NotNull(filters, nameof(filters));

            if (filters.Length == 0)
            {
                return new PagedList<Customer>(Enumerable.Empty<Customer>(), 0, int.MaxValue);
            }

            // TODO: really untracked?
            var query = _rsCustomer.TableUntracked.Where(x => !x.Deleted);

            FilterExpressionGroup group = null;

            if (filters.Length == 1 && filters[0] is FilterExpressionGroup group2)
            {
                group = group2;
            }
            else
            {
                group = new FilterExpressionGroup(typeof(Customer)) { LogicalOperator = logicalOperator };
                group.AddExpressions(filters);
            }

            // Create lambda predicate
            var predicate = group.ToPredicate(false);

            // Apply predicate to query
            query = query.Where(predicate).Cast<Customer>();

            return new PagedList<Customer>(query, pageIndex, pageSize);
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
