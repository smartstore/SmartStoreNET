using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Rules;
using SmartStore.Rules.Domain;
using SmartStore.Rules.Filters;
using SmartStore.Services.Localization;

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
                .Where(x => x != null)
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
            query = query
                .Where(predicate)
                .Cast<Customer>()
                .OrderByDescending(c => c.CreatedOnUtc);

            return new PagedList<Customer>(query, pageIndex, pageSize);
        }

        protected override IEnumerable<RuleDescriptor> LoadDescriptors()
        {
            var stores = _services.StoreService.GetAllStores()
                .Select(x => new RuleValueSelectListOption { Value = x.Id.ToString(), Text = x.Name })
                .ToArray();

            var vatNumberStatus = ((VatNumberStatus[])Enum.GetValues(typeof(VatNumberStatus)))
                .Select(x => new RuleValueSelectListOption { Value = ((int)x).ToString(), Text = x.GetLocalizedEnum(_services.Localization) })
                .ToArray();

            var taxDisplayTypes = ((TaxDisplayType[])Enum.GetValues(typeof(TaxDisplayType)))
                .Select(x => new RuleValueSelectListOption { Value = ((int)x).ToString(), Text = x.GetLocalizedEnum(_services.Localization) })
                .ToArray();

            var shippingStatus = ((ShippingStatus[])Enum.GetValues(typeof(ShippingStatus)))
                .Select(x => new RuleValueSelectListOption { Value = ((int)x).ToString(), Text = x.GetLocalizedEnum(_services.Localization) })
                .ToArray();

            var paymentStatus = ((PaymentStatus[])Enum.GetValues(typeof(PaymentStatus)))
                .Select(x => new RuleValueSelectListOption { Value = ((int)x).ToString(), Text = x.GetLocalizedEnum(_services.Localization) })
                .ToArray();

            var descriptors = new List<FilterDescriptor>
            {
                new FilterDescriptor<Customer, bool>(x => x.Active)
                {
                    Name = "Active",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Active"),
                    RuleType = RuleType.Boolean,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, bool>(x => x.IsTaxExempt)
                {
                    Name = "TaxExempt",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.TaxExempt"),
                    RuleType = RuleType.Boolean,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastActivityDateUtc, DateTime.UtcNow))
                {
                    Name = "LastActivityDays",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.LastActivityDays"),
                    RuleType = RuleType.NullableInt,
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
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.BirthDate, DateTime.UtcNow))
                {
                    Name = "BirthDateDays",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.BirthDate"),
                    RuleType = RuleType.NullableInt,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int?>(x => x.BillingAddress != null ? x.BillingAddress.CountryId : 0)
                {
                    Name = "BillingCountry",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.BillingCountry"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new FilterDescriptor<Customer, int?>(x => x.ShippingAddress != null ? x.ShippingAddress.CountryId : 0)
                {
                    Name = "ShippingCountry",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ShippingCountry"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
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
                new FilterDescriptor<Customer, string>(x => x.Gender)
                {
                    Name = "Gender",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Gender"),
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
                new FilterDescriptor<Customer, string>(x => x.TimeZoneId)
                {
                    Name = "TimeZone",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.TimeZone"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.VatNumberStatusId)
                {
                    Name = "VatNumberStatus",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.VatNumberStatus"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new LocalRuleValueSelectList(vatNumberStatus)
                },
                new FilterDescriptor<Customer, int>(x => x.TaxDisplayTypeId)
                {
                    Name = "TaxDisplayType",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.TaxDisplayType"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new LocalRuleValueSelectList(taxDisplayTypes)
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastForumVisit, DateTime.UtcNow))
                {
                    Name = "LastForumVisitDays",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.LastForumVisit"),
                    RuleType = RuleType.NullableInt,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, string>(x => x.LastUserAgent)
                {
                    Name = "LastUserAgent",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.LastUserAgent"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0]
                },

                new AnyFilterDescriptor<Customer, CustomerRoleMapping, int>(x => x.CustomerRoleMappings, rm => rm.CustomerRoleId)
                {
                    Name = "IsInCustomerRole",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.IsInCustomerRole"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("CustomerRole") { Multiple = true }
                },
                new FilterDescriptor<Customer, int>(x => x.ReturnRequests.Count())
                {
                    Name = "ReturnRequestCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ReturnRequestCount"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },

                new FilterDescriptor<Customer, int>(x => x.Orders.Count(o => !o.Deleted && o.OrderStatusId == 30))
                {
                    Name = "CompletedOrderCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.CompletedOrderCount"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(o => !o.Deleted && o.OrderStatusId == 40))
                {
                    Name = "CancelledOrderCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.CancelledOrderCount"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(o => !o.Deleted && (o.OrderStatusId == 10 || o.OrderStatusId == 20)))
                {
                    Name = "NewOrderCount",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.NewOrderCount"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0]
                },
                new AnyFilterDescriptor<Customer, Order, int>(x => x.Orders.Where(o => !o.Deleted), o => o.StoreId)
                {
                    Name = "OrderInStore",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OrderInStore"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new LocalRuleValueSelectList(stores) { Multiple = true }
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.Orders.Where(o => !o.Deleted).Max(o => o.CreatedOnUtc), DateTime.UtcNow))
                {
                    Name = "LastOrderDateDays",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.LastOrderDateDays"),
                    RuleType = RuleType.NullableInt,
                    Constraints = new IRuleConstraint[0]
                },
                new AnyFilterDescriptor<Customer, Order, decimal>(x => x.Orders.Where(o => !o.Deleted), o => o.OrderTotal)
                {
                    Name = "OrderTotal",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OrderTotal"),
                    RuleType = RuleType.Money,
                    Constraints = new IRuleConstraint[0]
                },
                new AnyFilterDescriptor<Customer, Order, decimal>(x => x.Orders.Where(o => !o.Deleted), o => o.OrderSubtotalInclTax)
                {
                    Name = "OrderSubtotalInclTax",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OrderSubtotalInclTax"),
                    RuleType = RuleType.Money,
                    Constraints = new IRuleConstraint[0]
                },
                new AnyFilterDescriptor<Customer, Order, decimal>(x => x.Orders.Where(o => !o.Deleted), o => o.OrderSubtotalExclTax)
                {
                    Name = "OrderSubtotalExclTax",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.OrderSubtotalExclTax"),
                    RuleType = RuleType.Money,
                    Constraints = new IRuleConstraint[0]
                },
                new AnyFilterDescriptor<Customer, OrderItem, int>(x => x.Orders.Where(o => !o.Deleted).SelectMany(o => o.OrderItems), oi => oi.ProductId)
                {
                    Name = "HasPurchasedProduct",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.HasPurchasedProduct"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Product") { Multiple = true }
                },
                new AllFilterDescriptor<Customer, OrderItem, int>(x => x.Orders.Where(o => !o.Deleted).SelectMany(o => o.OrderItems), oi => oi.ProductId)
                {
                    Name = "HasPurchasedAllProducts",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.HasPurchasedAllProducts"),
                    RuleType = RuleType.IntArray,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Product") { Multiple = true }
                },
                new AnyFilterDescriptor<Customer, Order, bool>(x => x.Orders.Where(o => !o.Deleted), o => o.AcceptThirdPartyEmailHandOver)
                {
                    Name = "AcceptThirdPartyEmailHandOver",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.AcceptThirdPartyEmailHandOver"),
                    RuleType = RuleType.Boolean,
                    Constraints = new IRuleConstraint[0]
                },
                new AnyFilterDescriptor<Customer, Order, string>(x => x.Orders.Where(o => !o.Deleted), o => o.CustomerCurrencyCode)
                {
                    Name = "CurrencyCode",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Currency"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Currency")
                },
                new AnyFilterDescriptor<Customer, Order, int>(x => x.Orders.Where(o => !o.Deleted), o => o.CustomerLanguageId)
                {
                    Name = "OrderLanguage",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.Language"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("Language")
                },
                new AnyFilterDescriptor<Customer, Order, string>(x => x.Orders.Where(o => !o.Deleted), o => o.PaymentMethodSystemName)
                {
                    Name = "PaymentMethod",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.PaidBy"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("PaymentMethod")
                },
                new AnyFilterDescriptor<Customer, Order, int>(x => x.Orders.Where(o => !o.Deleted), o => o.PaymentStatusId)
                {
                    Name = "PaymentStatus",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.PaymentStatus"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new LocalRuleValueSelectList(paymentStatus)
                },
                new AnyFilterDescriptor<Customer, Order, string>(x => x.Orders.Where(o => !o.Deleted), o => o.ShippingMethod)
                {
                    Name = "ShippingMethod",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ShippingMethod"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("ShippingMethod")
                },
                new AnyFilterDescriptor<Customer, Order, string>(x => x.Orders.Where(o => !o.Deleted), o => o.ShippingRateComputationMethodSystemName)
                {
                    Name = "ShippingRateComputationMethod",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ShippingRateComputationMethod"),
                    RuleType = RuleType.String,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("ShippingRateComputationMethod")
                },
                new AnyFilterDescriptor<Customer, Order, int>(x => x.Orders.Where(o => !o.Deleted), o => o.ShippingStatusId)
                {
                    Name = "ShippingStatus",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.ShippingStatus"),
                    RuleType = RuleType.Int,
                    Constraints = new IRuleConstraint[0],
                    SelectList = new LocalRuleValueSelectList(shippingStatus)
                },
                new TargetGroupFilterDescriptor(_ruleFactory, this)
                {
                    Name = "RuleSet",
                    DisplayName = _services.Localization.GetResource("Admin.Rules.FilterDescriptor.RuleSet"),
                    RuleType = RuleType.Int,
                    Operators = new[] { RuleOperator.IsEqualTo, RuleOperator.IsNotEqualTo },
                    Constraints = new IRuleConstraint[0],
                    SelectList = new RemoteRuleValueSelectList("TargetGroup")
                }
            };

            descriptors
                .Where(x => x.RuleType == RuleType.Money)
                .Each(x => x.Metadata["postfix"] = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode);

            return descriptors;
        }
    }
}
