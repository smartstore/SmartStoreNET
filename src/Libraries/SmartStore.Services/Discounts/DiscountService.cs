using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Events;
using SmartStore.Core.Plugins;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Customers;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Collections;

namespace SmartStore.Services.Discounts
{
    public partial class DiscountService : IDiscountService
    {
        private const string DISCOUNTS_ALL_KEY = "SmartStore.discount.all-{0}-{1}";
        private const string DISCOUNTS_PATTERN_KEY = "SmartStore.discount.*";

        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountRequirement> _discountRequirementRepository;
        private readonly IRepository<DiscountUsageHistory> _discountUsageHistoryRepository;
        private readonly IRequestCache _requestCache;
		private readonly IStoreContext _storeContext;
		private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IEventPublisher _eventPublisher;
		private readonly ISettingService _settingService;
		private readonly IProviderManager _providerManager;

        public DiscountService(IRequestCache requestCache,
            IRepository<Discount> discountRepository,
            IRepository<DiscountRequirement> discountRequirementRepository,
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository,
			IStoreContext storeContext,
			IGenericAttributeService genericAttributeService,
            IPluginFinder pluginFinder,
            IEventPublisher eventPublisher,
			ISettingService settingService,
			IProviderManager providerManager)
        {
            _requestCache = requestCache;
            _discountRepository = discountRepository;
            _discountRequirementRepository = discountRequirementRepository;
            _discountUsageHistoryRepository = discountUsageHistoryRepository;
			_storeContext = storeContext;
			_genericAttributeService = genericAttributeService;
            _pluginFinder = pluginFinder;
            _eventPublisher = eventPublisher;
			_settingService = settingService;
			_providerManager = providerManager;
        }

        /// <summary>
        /// Checks discount limitation for customer
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <returns>Value indicating whether discount can be used</returns>
        protected virtual bool CheckDiscountLimitations(Discount discount, Customer customer)
        {
			Guard.NotNull(discount, nameof(discount));

            switch (discount.DiscountLimitation)
            {
                case DiscountLimitationType.Unlimited:
                    {
                        return true;
                    }
                case DiscountLimitationType.NTimesOnly:
                    {
                        var totalDuh = GetAllDiscountUsageHistory(discount.Id, null, 0, 1).TotalCount;
                        return totalDuh < discount.LimitationTimes;
                    }
                case DiscountLimitationType.NTimesPerCustomer:
                    {
                        if (customer != null && !customer.IsGuest())
                        {
                            //registered customer
                            var totalDuh = GetAllDiscountUsageHistory(discount.Id, customer.Id, 0, 1).TotalCount;
                            return totalDuh < discount.LimitationTimes;
                        }
                        else
                        {
                            //guest
                            return true;
                        }
                    }
                default:
                    break;
            }
            return false;
        }

        public virtual void DeleteDiscount(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            _discountRepository.Delete(discount);

            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }

        public virtual Discount GetDiscountById(int discountId)
        {
            if (discountId == 0)
                return null;

            return _discountRepository.GetById(discountId);
        }

        public virtual IEnumerable<Discount> GetAllDiscounts(DiscountType? discountType, string couponCode = "", bool showHidden = false)
        {
            int? discountTypeId = null;
            if (discountType.HasValue)
                discountTypeId = (int)discountType.Value;

            // we load all discounts, and filter them by passed "discountType" parameter later
            // we do it because we know that this method is invoked several times per HTTP request with distinct "discountType" parameter
            // that's why we access the database only once
            string key = string.Format(DISCOUNTS_ALL_KEY, showHidden, couponCode);
            var result = _requestCache.Get(key, () =>
            {
                var query = _discountRepository.Table;

                if (!showHidden)
                {
                    // The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                    // That's why we pass the date value
                    var nowUtc = DateTime.UtcNow.Date;
                    query = query.Where(d =>
                        (!d.StartDateUtc.HasValue || d.StartDateUtc <= nowUtc)
                        && (!d.EndDateUtc.HasValue || d.EndDateUtc >= nowUtc));
                }

                if (!String.IsNullOrWhiteSpace(couponCode))
                {
                    couponCode = couponCode.Trim();
                    query = query.Where(d => d.CouponCode == couponCode);
                }

                query = query.OrderByDescending(d => d.Id);

                var discounts = query.ToList();

				var map = new Multimap<int, Discount>();
				discounts.Each(x => map.Add(x.DiscountTypeId, x));

				return map;
            });

            if (discountTypeId > 0)
            {
				return result[discountTypeId.Value];
            }

            return result.SelectMany(x => x.Value);
        }

        public virtual void InsertDiscount(Discount discount)
        {
			Guard.NotNull(discount, nameof(discount));

            _discountRepository.Insert(discount);

            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }

        public virtual void UpdateDiscount(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            _discountRepository.Update(discount);

            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }

        public virtual void DeleteDiscountRequirement(DiscountRequirement discountRequirement)
        {
            if (discountRequirement == null)
                throw new ArgumentNullException("discountRequirement");

            _discountRequirementRepository.Delete(discountRequirement);

            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }

		public virtual Provider<IDiscountRequirementRule> LoadDiscountRequirementRuleBySystemName(string systemName, int storeId = 0)
        {
			return _providerManager.GetProvider<IDiscountRequirementRule>(systemName, storeId);
        }

		public virtual IEnumerable<Provider<IDiscountRequirementRule>> LoadAllDiscountRequirementRules(int storeId = 0)
        {
			return _providerManager.GetAllProviders<IDiscountRequirementRule>(storeId);
        }

        public virtual Discount GetDiscountByCouponCode(string couponCode, bool showHidden = false)
        {
            if (String.IsNullOrWhiteSpace(couponCode))
                return null;

            var discount = GetAllDiscounts(null, couponCode, showHidden).FirstOrDefault();
            return discount;
        }

        public virtual bool IsDiscountValid(Discount discount, Customer customer)
        {
			Guard.NotNull(discount, nameof(discount));

            var couponCodeToValidate = "";
            if (customer != null)
			{
				couponCodeToValidate = customer.GetAttribute<string>(SystemCustomerAttributeNames.DiscountCouponCode, _genericAttributeService);
			}			

            return IsDiscountValid(discount, customer, couponCodeToValidate);
        }

        public virtual bool IsDiscountValid(Discount discount, Customer customer, string couponCodeToValidate)
        {
			Guard.NotNull(discount, nameof(discount));

			// Check coupon code
			if (discount.RequiresCouponCode)
            {
                if (discount.CouponCode.IsEmpty())
                    return false;

                if (!discount.CouponCode.Equals(couponCodeToValidate, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            // Check date range
            var now = DateTime.UtcNow;
			var store = _storeContext.CurrentStore;

            if (discount.StartDateUtc.HasValue)
            {
                var startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                    return false;
            }

            if (discount.EndDateUtc.HasValue)
            {
                var endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                    return false;
            }

            if (!CheckDiscountLimitations(discount, customer))
                return false;

			// better not to apply discounts if there are gift cards in the cart cause the customer could "earn" money through that.
			if (discount.DiscountType == DiscountType.AssignedToOrderTotal || discount.DiscountType == DiscountType.AssignedToOrderSubTotal)
			{
				var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);
				if (cart.Any(x => x.Item?.Product != null && x.Item.Product.IsGiftCard))
					return false;
			}

			// discount requirements
			var requirements = discount.DiscountRequirements;
            foreach (var req in requirements)
            {
				var requirementRule = LoadDiscountRequirementRuleBySystemName(req.DiscountRequirementRuleSystemName, store.Id);
                if (requirementRule == null)
                    continue;
				
				var request = new CheckDiscountRequirementRequest
                {
                    DiscountRequirement = req,
                    Customer = customer,
					Store = store
                };

				// TODO: cache result... CheckRequirement is called very often
				if (!requirementRule.Value.CheckRequirement(request))
                    return false;
            }

            return true;
        }

        public virtual DiscountUsageHistory GetDiscountUsageHistoryById(int discountUsageHistoryId)
        {
            if (discountUsageHistoryId == 0)
                return null;

            var duh = _discountUsageHistoryRepository.GetById(discountUsageHistoryId);
            return duh;
        }

        public virtual IPagedList<DiscountUsageHistory> GetAllDiscountUsageHistory(int? discountId, int? customerId, int pageIndex, int pageSize)
        {
            var query = _discountUsageHistoryRepository.Table;

            if (discountId.HasValue && discountId.Value > 0)
                query = query.Where(duh => duh.DiscountId == discountId.Value);

            if (customerId.HasValue && customerId.Value > 0)
                query = query.Where(duh => duh.Order != null && duh.Order.CustomerId == customerId.Value);

            query = query.OrderByDescending(c => c.CreatedOnUtc);
            return new PagedList<DiscountUsageHistory>(query, pageIndex, pageSize);
        }

        public virtual void InsertDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException("discountUsageHistory");

            _discountUsageHistoryRepository.Insert(discountUsageHistory);

            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }

        public virtual void UpdateDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException("discountUsageHistory");

            _discountUsageHistoryRepository.Update(discountUsageHistory);

            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }

        public virtual void DeleteDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException("discountUsageHistory");

            _discountUsageHistoryRepository.Delete(discountUsageHistory);

            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }
    }
}
