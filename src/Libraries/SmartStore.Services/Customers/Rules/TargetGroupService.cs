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
        private readonly IRuleFactory _ruleFactory;
        private readonly IRepository<Customer> _rsCustomer;

        public TargetGroupService(IRuleFactory ruleFactory, IRepository<Customer> rsCustomer)
            : base(RuleScope.Customer)
        {
            _ruleFactory = ruleFactory;
            _rsCustomer = rsCustomer;
        }

        public FilterExpressionGroup CreateExpressionGroup(int ruleSetId)
        {
            return _ruleFactory.CreateExpressionGroup(ruleSetId, this) as FilterExpressionGroup;
        }

        public override IRuleExpression VisitRule(RuleEntity rule)
        {
            var expression = new FilterExpression();
            base.ConvertRule(rule, expression);
            return expression;
        }

        public override IRuleExpressionGroup VisitRuleSet(RuleSetEntity ruleSet)
        {
            var group = new FilterExpressionGroup(typeof(Customer))
            {
                Id = ruleSet.Id,
                LogicalOperator = ruleSet.LogicalOperator,
                Value = ruleSet.Id,
                RawValue = ruleSet.Id.ToString(),
                // INFO: filter group does NOT access any descriptor
            };

            return group;
        }

        public IPagedList<Customer> ProcessFilter(
            FilterExpression filter, 
            int pageIndex = 0, 
            int pageSize = int.MaxValue)
        {
            Guard.NotNull(filter, nameof(filter));

            return ProcessFilter(
                new[] { filter },
                LogicalRuleOperator.And, 
                pageIndex, 
                pageSize);
        }


        public IPagedList<Customer> ProcessFilter(
            int[] ruleSetIds, 
            LogicalRuleOperator logicalOperator,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            Guard.NotNull(ruleSetIds, nameof(ruleSetIds));

            var filters = ruleSetIds
                .Select(id => _ruleFactory.CreateExpressionGroup(id, this))
                .Cast<FilterExpression>()
                .ToArray();

            return ProcessFilter(filters, logicalOperator, pageIndex, pageSize);
        }

        public IPagedList<Customer> ProcessFilter(
            FilterExpression[] filters,
            LogicalRuleOperator logicalOperator,
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
                    Name = "TaxExempt",
                    RuleType = RuleType.Boolean,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int?>(x => x.BillingAddress.CountryId)
                {
                    Name = "BillingCountry",
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new FilterDescriptor<Customer, int?>(x => x.ShippingAddress.CountryId)
                {
                    Name = "ShippingCountry",
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastActivityDateUtc, DateTime.UtcNow))
                {
                    Name = "LastActivityDays",
                    RuleType = RuleType.NullableInt,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 30))
                {
                    Name = "CompletedOrderCount",
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 40))
                {
                    Name = "CancelledOrderCount",
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 30))
                {
                    Name = "NewOrderCount",
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new TargetGroupFilterDescriptor(_ruleFactory, this)
                {
                    Name = "RuleSet",
                    RuleType = RuleType.Int,
                    Operators = new[] { RuleOperator.IsEqualTo, RuleOperator.IsNotEqualTo },
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("TargetGroup")
                },
                // TODO: more ...
            };
        }
    }
}
