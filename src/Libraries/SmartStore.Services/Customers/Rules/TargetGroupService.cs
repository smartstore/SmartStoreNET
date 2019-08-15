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
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Customers
{
    public class TargetGroupService : RuleProviderBase, ITargetGroupService
    {
        private readonly IRuleFactory _ruleFactory;
        private readonly IRepository<Customer> _rsCustomer;
        private readonly ICommonServices _services;

        public TargetGroupService(
            IRuleFactory ruleFactory, 
            IRepository<Customer> rsCustomer,
            ICommonServices services) : base(RuleScope.Customer)
        {
            _ruleFactory = ruleFactory;
            _rsCustomer = rsCustomer;
            _services = services;
        }

        public FilterExpressionGroup CreateExpressionGroup(int ruleSetId)
        {
            return _ruleFactory.CreateExpressionGroup(ruleSetId, this) as FilterExpressionGroup;
        }

        public override IRuleExpression VisitRule(RuleEntity rule)
        {
            var expression = new FilterExpression();
            base.ConvertRule(rule, expression);
            expression.Descriptor = ((RuleExpression)expression).Descriptor as FilterDescriptor;
            return expression;
        }

        public override IRuleExpressionGroup VisitRuleSet(RuleSetEntity ruleSet)
        {
            var group = new FilterExpressionGroup(typeof(Customer))
            {
                Id = ruleSet.Id,
                LogicalOperator = ruleSet.LogicalOperator,
                IsSubGroup = ruleSet.IsSubGroup,
                Value = ruleSet.Id,
                RawValue = ruleSet.Id.ToString(),
                Provider = this
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
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.TaxExempt"),
                    RuleType = RuleType.Boolean,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int?>(x => x.BillingAddress.CountryId)
                {
                    Name = "BillingCountry",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.BillingCountry"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new FilterDescriptor<Customer, int?>(x => x.ShippingAddress.CountryId)
                {
                    Name = "ShippingCountry",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ShippingCountry"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastActivityDateUtc, DateTime.UtcNow))
                {
                    Name = "LastActivityDays",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.LastActivityDays"),
                    RuleType = RuleType.NullableInt,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 30))
                {
                    Name = "CompletedOrderCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.CompletedOrderCount"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 40))
                {
                    Name = "CancelledOrderCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.CancelledOrderCount"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(y => y.OrderStatusId == 10 || y.OrderStatusId == 20))
                {
                    Name = "NewOrderCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.NewOrderCount"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new AnyFilterDescriptor<Customer, OrderItem, int>(x => x.Orders.SelectMany(o => o.OrderItems), oi => oi.ProductId)
                {
                    Name = "HasPurchasedProduct",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.HasPurchasedProduct"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0]
                },
                new AllFilterDescriptor<Customer, OrderItem, int>(x => x.Orders.SelectMany(o => o.OrderItems), oi => oi.ProductId)
                {
                    Name = "HasPurchasedAllProducts",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.HasPurchasedAllProducts"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0]
                },

                new TargetGroupFilterDescriptor(_ruleFactory, this)
                {
                    Name = "RuleSet",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.RuleSet"),
                    RuleType = RuleType.Int,
                    Operators = new[] { RuleOperator.IsEqualTo, RuleOperator.IsNotEqualTo },
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("TargetGroup")
                },
                // TODO: more ...

                new FilterDescriptor<Customer, bool>(x => x.Active)
                {
                    Name = "Active",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Active"),
                    RuleType = RuleType.Boolean,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastLoginDateUtc, DateTime.UtcNow))
                {
                    Name = "LastLoginDays",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.LastLoginDays"),
                    RuleType = RuleType.NullableInt,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.CreatedOnUtc, DateTime.UtcNow))
                {
                    Name = "CreatedDays",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.CreatedDays"),
                    RuleType = RuleType.NullableInt,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, string>(x => x.Salutation)
                {
                    Name = "Salutation",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Salutation"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, string>(x => x.Title)
                {
                    Name = "Title",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Title"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, string>(x => x.Company)
                {
                    Name = "Company",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Company"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, string>(x => x.CustomerNumber)
                {
                    Name = "CustomerNumber",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.CustomerNumber"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },

                // TODO
                //new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.BirthDate, DateTime.UtcNow))
                //{
                //    Name = "BirthDate",
                //    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.BirthDate"),
                //    RuleType = RuleType.NullableInt,
                //    Constraints = new IRuleConstraint[0]
                //},

                // Moved from Customer attrs > Gender, VatNumberStatusId, TimeZoneId, TaxDisplayTypeId
                new FilterDescriptor<Customer, string>(x => x.Gender)
                {
                    Name = "Gender",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Gender"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },

                // TODO: handle status
                new FilterDescriptor<Customer, int>(x => x.VatNumberStatusId)
                {
                    Name = "VatNumberStatusId",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.VatNumberStatusId"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, string>(x => x.TimeZoneId)
                {
                    Name = "TimeZoneId",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.TimeZoneId"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.TaxDisplayTypeId)
                {
                    Name = "TaxDisplayTypeId",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.TaxDisplayTypeId"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },

                // TODO: later
                //new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastForumVisit, DateTime.UtcNow))
                //{
                //    Name = "LastForumVisit",
                //    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.LastForumVisit"),
                //    RuleType = RuleType.NullableInt,
                //    Constraints = new IRuleConstraint[0]
                //},

                // TODO: must be in order table
                //new FilterDescriptor<Customer, string>(x => x.LastUserAgent)
                //{
                //    Name = "LastUserAgent",
                //    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.LastUserAgent"),
                //    RuleType = RuleType.String,
                //    Constraints = new IRuleConstraint[0]
                //},

                // customer roles
                new AllFilterDescriptor<Customer, CustomerRole, int>(x => x.CustomerRoles, cr => cr.Id)
                {
                    Name = "IsInCustomerRole",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.IsInCustomerRole"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0]
                },

                // Orders > StoreId, LastOrderDate (CreatedOnUtc), AcceptThirdPartyEmailHandOver, OrderTotal, OrderSubtotalInclTax, OrderSubtotalExclTax
                //          ShippingStatusId, PaymentStatusId, (TODO ???)
                //          PaymentMethodSystemName, ShippingRateComputationMethodSystemName
                //          CustomerCurrencyCode, CustomerLanguageId, CustomerTaxDisplayTypeId (TODO: Really??? from customer, order or both?)

                new AnyFilterDescriptor<Customer, Order, int>(x => x.Orders, o => o.StoreId)
                {
                    Name = "OrderCountInStore",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OrderCountInStore"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.Orders.Max(y => y.CreatedOnUtc), DateTime.UtcNow))
                {
                    Name = "LastOrderDateDays",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.LastOrderDateDays"),
                    RuleType = RuleType.NullableInt,
                    Constraints = new IRuleConstraint[0]
                },
                new AnyFilterDescriptor<Customer, Order, bool>(x => x.Orders, o => o.AcceptThirdPartyEmailHandOver)
                {
                    Name = "AcceptThirdPartyEmailHandOver",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.AcceptThirdPartyEmailHandOver"),
                    RuleType = RuleType.Boolean,
                    Constraints = new IRuleConstraint[0]
                },

                // TODO: customer
                new FilterDescriptor<Order, decimal>(x => x.OrderTotal)
                {
                    Name = "OrderTotal",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OrderTotal"),
                    RuleType = RuleType.Money,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Order, decimal>(x => x.OrderSubtotalInclTax)
                {
                    Name = "OrderSubtotalInclTax",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OrderSubtotalInclTax"),
                    RuleType = RuleType.Money,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Order, decimal>(x => x.OrderSubtotalExclTax)
                {
                    Name = "OrderSubtotalExclTax",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OrderSubtotalExclTax"),
                    RuleType = RuleType.Money,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Order, string>(x => x.PaymentMethodSystemName)
                {
                    Name = "PaymentMethodSystemName",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.PaymentMethodSystemName"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Order, string>(x => x.ShippingRateComputationMethodSystemName)
                {
                    Name = "ShippingRateComputationMethodSystemName",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ShippingRateComputationMethodSystemName"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },
            };
        }
    }
}
