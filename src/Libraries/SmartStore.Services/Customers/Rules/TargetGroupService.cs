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
using SmartStore.Core.Localization;
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

        public Localizer T { get; set; } = NullLocalizer.Instance;

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
            var query = _rsCustomer.TableUntracked.Where(x => !x.Deleted && !x.IsSystemAccount);

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
                    DisplayName = T("Admin.Rules.FilterDescriptor.Active"),
                    RuleType = RuleType.Boolean
                },
                new FilterDescriptor<Customer, string>(x => x.Salutation)
                {
                    Name = "Salutation",
                    DisplayName = T("Admin.Rules.FilterDescriptor.Salutation"),
                    RuleType = RuleType.String
                },
                new FilterDescriptor<Customer, string>(x => x.Title)
                {
                    Name = "Title",
                    DisplayName = T("Admin.Rules.FilterDescriptor.Title"),
                    RuleType = RuleType.String
                },
                new FilterDescriptor<Customer, string>(x => x.Company)
                {
                    Name = "Company",
                    DisplayName = T("Admin.Rules.FilterDescriptor.Company"),
                    RuleType = RuleType.String
                },
                new FilterDescriptor<Customer, string>(x => x.Gender)
                {
                    Name = "Gender",
                    DisplayName = T("Admin.Rules.FilterDescriptor.Gender"),
                    RuleType = RuleType.String
                },
                new FilterDescriptor<Customer, string>(x => x.CustomerNumber)
                {
                    Name = "CustomerNumber",
                    DisplayName = T("Admin.Rules.FilterDescriptor.CustomerNumber"),
                    RuleType = RuleType.String
                },
                new AnyFilterDescriptor<Customer, CustomerRoleMapping, int>(x => x.CustomerRoleMappings, rm => rm.CustomerRoleId)
                {
                    Name = "IsInCustomerRole",
                    DisplayName = T("Admin.Rules.FilterDescriptor.IsInCustomerRole"),
                    RuleType = RuleType.IntArray,
                    SelectList = new RemoteRuleValueSelectList("CustomerRole") { Multiple = true }
                },
                new FilterDescriptor<Customer, bool>(x => x.IsTaxExempt)
                {
                    Name = "TaxExempt",
                    DisplayName = T("Admin.Rules.FilterDescriptor.TaxExempt"),
                    RuleType = RuleType.Boolean
                },
                new FilterDescriptor<Customer, int>(x => x.VatNumberStatusId)
                {
                    Name = "VatNumberStatus",
                    DisplayName = T("Admin.Rules.FilterDescriptor.VatNumberStatus"),
                    RuleType = RuleType.Int,
                    SelectList = new LocalRuleValueSelectList(vatNumberStatus)
                },
                new FilterDescriptor<Customer, int>(x => x.TaxDisplayTypeId)
                {
                    Name = "TaxDisplayType",
                    DisplayName = T("Admin.Rules.FilterDescriptor.TaxDisplayType"),
                    RuleType = RuleType.Int,
                    SelectList = new LocalRuleValueSelectList(taxDisplayTypes)
                },
                new FilterDescriptor<Customer, string>(x => x.TimeZoneId)
                {
                    Name = "TimeZone",
                    DisplayName = T("Admin.Rules.FilterDescriptor.TimeZone"),
                    RuleType = RuleType.String
                },
                new FilterDescriptor<Customer, string>(x => x.LastUserAgent)
                {
                    Name = "LastUserAgent",
                    DisplayName = T("Admin.Rules.FilterDescriptor.LastUserAgent"),
                    RuleType = RuleType.String
                },
                new FilterDescriptor<Customer, int?>(x => x.BillingAddress != null ? x.BillingAddress.CountryId : 0)
                {
                    Name = "BillingCountry",
                    DisplayName = T("Admin.Rules.FilterDescriptor.BillingCountry"),
                    RuleType = RuleType.IntArray,
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new FilterDescriptor<Customer, int?>(x => x.ShippingAddress != null ? x.ShippingAddress.CountryId : 0)
                {
                    Name = "ShippingCountry",
                    DisplayName = T("Admin.Rules.FilterDescriptor.ShippingCountry"),
                    RuleType = RuleType.IntArray,
                    SelectList = new RemoteRuleValueSelectList("Country") { Multiple = true }
                },
                new FilterDescriptor<Customer, int>(x => x.ReturnRequests.Count())
                {
                    Name = "ReturnRequestCount",
                    DisplayName = T("Admin.Rules.FilterDescriptor.ReturnRequestCount"),
                    RuleType = RuleType.Int
                },

                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastActivityDateUtc, DateTime.UtcNow))
                {
                    Name = "LastActivityDays",
                    DisplayName = T("Admin.Rules.FilterDescriptor.LastActivityDays"),
                    RuleType = RuleType.NullableInt
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastLoginDateUtc, DateTime.UtcNow))
                {
                    Name = "LastLoginDays",
                    DisplayName = T("Admin.Rules.FilterDescriptor.LastLoginDays"),
                    RuleType = RuleType.NullableInt
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.LastForumVisit, DateTime.UtcNow))
                {
                    Name = "LastForumVisitDays",
                    DisplayName = T("Admin.Rules.FilterDescriptor.LastForumVisit"),
                    RuleType = RuleType.NullableInt
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.CreatedOnUtc, DateTime.UtcNow))
                {
                    Name = "CreatedDays",
                    DisplayName = T("Admin.Rules.FilterDescriptor.CreatedDays"),
                    RuleType = RuleType.NullableInt
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.BirthDate, DateTime.UtcNow))
                {
                    Name = "BirthDateDays",
                    DisplayName = T("Admin.Rules.FilterDescriptor.BirthDate"),
                    RuleType = RuleType.NullableInt
                },
                new TargetGroupFilterDescriptor(_ruleFactory, this)
                {
                    Name = "RuleSet",
                    DisplayName = T("Admin.Rules.FilterDescriptor.RuleSet"),
                    RuleType = RuleType.Int,
                    Operators = new[] { RuleOperator.IsEqualTo, RuleOperator.IsNotEqualTo },
                    SelectList = new RemoteRuleValueSelectList("TargetGroup")
                },

                new AnyFilterDescriptor<Customer, Order, int>(x => x.Orders.Where(o => !o.Deleted), o => o.StoreId)
                {
                    Name = "OrderInStore",
                    DisplayName = T("Admin.Rules.FilterDescriptor.OrderInStore"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.IntArray,
                    SelectList = new LocalRuleValueSelectList(stores) { Multiple = true }
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(o => !o.Deleted && (o.OrderStatusId == 10 || o.OrderStatusId == 20)))
                {
                    Name = "NewOrderCount",
                    DisplayName = T("Admin.Rules.FilterDescriptor.NewOrderCount"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Int
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(o => !o.Deleted && o.OrderStatusId == 30))
                {
                    Name = "CompletedOrderCount",
                    DisplayName = T("Admin.Rules.FilterDescriptor.CompletedOrderCount"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Int
                },
                new FilterDescriptor<Customer, int>(x => x.Orders.Count(o => !o.Deleted && o.OrderStatusId == 40))
                {
                    Name = "CancelledOrderCount",
                    DisplayName = T("Admin.Rules.FilterDescriptor.CancelledOrderCount"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Int
                },
                new FilterDescriptor<Customer, int?>(x => DbFunctions.DiffDays(x.Orders.Where(o => !o.Deleted).Max(o => o.CreatedOnUtc), DateTime.UtcNow))
                {
                    Name = "LastOrderDateDays",
                    DisplayName = T("Admin.Rules.FilterDescriptor.LastOrderDateDays"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.NullableInt
                },
                new AnyFilterDescriptor<Customer, Order, decimal>(x => x.Orders.Where(o => !o.Deleted), o => o.OrderTotal)
                {
                    Name = "OrderTotal",
                    DisplayName = T("Admin.Rules.FilterDescriptor.OrderTotal"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Money
                },
                new AnyFilterDescriptor<Customer, Order, decimal>(x => x.Orders.Where(o => !o.Deleted), o => o.OrderSubtotalInclTax)
                {
                    Name = "OrderSubtotalInclTax",
                    DisplayName = T("Admin.Rules.FilterDescriptor.OrderSubtotalInclTax"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Money
                },
                new AnyFilterDescriptor<Customer, Order, decimal>(x => x.Orders.Where(o => !o.Deleted), o => o.OrderSubtotalExclTax)
                {
                    Name = "OrderSubtotalExclTax",
                    DisplayName = T("Admin.Rules.FilterDescriptor.OrderSubtotalExclTax"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Money
                },
                new AnyFilterDescriptor<Customer, Order, int>(x => x.Orders.Where(o => !o.Deleted), o => o.ShippingStatusId)
                {
                    Name = "ShippingStatus",
                    DisplayName = T("Admin.Rules.FilterDescriptor.ShippingStatus"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Int,
                    SelectList = new LocalRuleValueSelectList(shippingStatus)
                },
                new AnyFilterDescriptor<Customer, Order, int>(x => x.Orders.Where(o => !o.Deleted), o => o.PaymentStatusId)
                {
                    Name = "PaymentStatus",
                    DisplayName = T("Admin.Rules.FilterDescriptor.PaymentStatus"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Int,
                    SelectList = new LocalRuleValueSelectList(paymentStatus)
                },
                new AnyFilterDescriptor<Customer, OrderItem, int>(x => x.Orders.Where(o => !o.Deleted).SelectMany(o => o.OrderItems), oi => oi.ProductId)
                {
                    Name = "HasPurchasedProduct",
                    DisplayName = T("Admin.Rules.FilterDescriptor.HasPurchasedProduct"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.IntArray,
                    SelectList = new RemoteRuleValueSelectList("Product") { Multiple = true }
                },
                new AllFilterDescriptor<Customer, OrderItem, int>(x => x.Orders.Where(o => !o.Deleted).SelectMany(o => o.OrderItems), oi => oi.ProductId)
                {
                    Name = "HasPurchasedAllProducts",
                    DisplayName = T("Admin.Rules.FilterDescriptor.HasPurchasedAllProducts"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.IntArray,
                    SelectList = new RemoteRuleValueSelectList("Product") { Multiple = true }
                },
                new AnyFilterDescriptor<Customer, Order, bool>(x => x.Orders.Where(o => !o.Deleted), o => o.AcceptThirdPartyEmailHandOver)
                {
                    Name = "AcceptThirdPartyEmailHandOver",
                    DisplayName = T("Admin.Rules.FilterDescriptor.AcceptThirdPartyEmailHandOver"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Boolean
                },
                new AnyFilterDescriptor<Customer, Order, string>(x => x.Orders.Where(o => !o.Deleted), o => o.CustomerCurrencyCode)
                {
                    Name = "CurrencyCode",
                    DisplayName = T("Admin.Rules.FilterDescriptor.Currency"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.String,
                    SelectList = new RemoteRuleValueSelectList("Currency")
                },
                new AnyFilterDescriptor<Customer, Order, int>(x => x.Orders.Where(o => !o.Deleted), o => o.CustomerLanguageId)
                {
                    Name = "OrderLanguage",
                    DisplayName = T("Admin.Rules.FilterDescriptor.Language"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.Int,
                    SelectList = new RemoteRuleValueSelectList("Language")
                },
                new AnyFilterDescriptor<Customer, Order, string>(x => x.Orders.Where(o => !o.Deleted), o => o.PaymentMethodSystemName)
                {
                    Name = "PaymentMethod",
                    DisplayName = T("Admin.Rules.FilterDescriptor.PaidBy"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.String,
                    SelectList = new RemoteRuleValueSelectList("PaymentMethod")
                },
                new AnyFilterDescriptor<Customer, Order, string>(x => x.Orders.Where(o => !o.Deleted), o => o.ShippingMethod)
                {
                    Name = "ShippingMethod",
                    DisplayName = T("Admin.Rules.FilterDescriptor.ShippingMethod"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.String,
                    SelectList = new RemoteRuleValueSelectList("ShippingMethod")
                },
                new AnyFilterDescriptor<Customer, Order, string>(x => x.Orders.Where(o => !o.Deleted), o => o.ShippingRateComputationMethodSystemName)
                {
                    Name = "ShippingRateComputationMethod",
                    DisplayName = T("Admin.Rules.FilterDescriptor.ShippingRateComputationMethod"),
                    GroupKey = "Admin.Orders",
                    RuleType = RuleType.String,
                    SelectList = new RemoteRuleValueSelectList("ShippingRateComputationMethod")
                },
            };

            descriptors
                .Where(x => x.RuleType == RuleType.Money)
                .Each(x => x.Metadata["postfix"] = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode);

            return descriptors;
        }
    }
}
