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

namespace SmartStore.Services.Discounts
{
    /// <summary>
    /// Discount service
    /// </summary>
    public partial class DiscountService : IDiscountService
    {
        #region Constants
        private const string DISCOUNTS_ALL_KEY = "SmartStore.discount.all-{0}-{1}";
        private const string DISCOUNTS_PATTERN_KEY = "SmartStore.discount.";
        #endregion

        #region Fields

        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountRequirement> _discountRequirementRepository;
        private readonly IRepository<DiscountUsageHistory> _discountUsageHistoryRepository;
        private readonly ICacheManager _cacheManager;
		private readonly IStoreContext _storeContext;
		private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPluginFinder _pluginFinder;
        private readonly IEventPublisher _eventPublisher;
		private readonly ISettingService _settingService;
		private readonly IProviderManager _providerManager;

        #endregion

        #region Ctor

        public DiscountService(ICacheManager cacheManager,
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
            this._cacheManager = cacheManager;
            this._discountRepository = discountRepository;
            this._discountRequirementRepository = discountRequirementRepository;
            this._discountUsageHistoryRepository = discountUsageHistoryRepository;
			this._storeContext = storeContext;
			this._genericAttributeService = genericAttributeService;
            this._pluginFinder = pluginFinder;
            this._eventPublisher = eventPublisher;
			this._settingService = settingService;
			this._providerManager = providerManager;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Checks discount limitation for customer
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <returns>Value indicating whether discount can be used</returns>
        protected virtual bool CheckDiscountLimitations(Discount discount, Customer customer)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

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

        #endregion

        #region Methods

        /// <summary>
        /// Delete discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public virtual void DeleteDiscount(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            _discountRepository.Delete(discount);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(discount);
        }

        /// <summary>
        /// Gets a discount
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <returns>Discount</returns>
        public virtual Discount GetDiscountById(int discountId)
        {
            if (discountId == 0)
                return null;

            return _discountRepository.GetById(discountId);
        }

        /// <summary>
        /// Gets all discounts
        /// </summary>
        /// <param name="discountType">Discount type; null to load all discount</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Discount collection</returns>
        public virtual IList<Discount> GetAllDiscounts(DiscountType? discountType, string couponCode = "", bool showHidden = false)
        {
            int? discountTypeId = null;
            if (discountType.HasValue)
                discountTypeId = (int)discountType.Value;

            //we load all discounts, and filter them by passed "discountType" parameter later
            //we do it because we know that this method is invoked several times per HTTP request with distinct "discountType" parameter
            //that's why let's access the database only once
            string key = string.Format(DISCOUNTS_ALL_KEY, showHidden, couponCode);
            var result = _cacheManager.Get(key, () =>
            {
                var query = _discountRepository.Table;
                if (!showHidden)
                {
                    //The function 'CurrentUtcDateTime' is not supported by SQL Server Compact. 
                    //That's why we pass the date value
                    var nowUtc = DateTime.UtcNow;
                    query = query.Where(d =>
                        (!d.StartDateUtc.HasValue || d.StartDateUtc <= nowUtc)
                        && (!d.EndDateUtc.HasValue || d.EndDateUtc >= nowUtc)
                        );
                }
                if (!String.IsNullOrWhiteSpace(couponCode))
                {
                    couponCode = couponCode.Trim();

                    query = query.Where(d => d.CouponCode == couponCode);
                }
                query = query.OrderByDescending(d => d.Id);

                var discounts = query.ToList();
                return discounts;
            });
            if (discountTypeId.HasValue && discountTypeId.Value > 0)
            {
                result = result.Where(d => d.DiscountTypeId == discountTypeId).ToList();
            }
            return result;
        }

        /// <summary>
        /// Inserts a discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public virtual void InsertDiscount(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            _discountRepository.Insert(discount);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(discount);
        }

        /// <summary>
        /// Updates the discount
        /// </summary>
        /// <param name="discount">Discount</param>
        public virtual void UpdateDiscount(Discount discount)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            _discountRepository.Update(discount);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(discount);
        }

        /// <summary>
        /// Delete discount requirement
        /// </summary>
        /// <param name="discountRequirement">Discount requirement</param>
        public virtual void DeleteDiscountRequirement(DiscountRequirement discountRequirement)
        {
            if (discountRequirement == null)
                throw new ArgumentNullException("discountRequirement");

            _discountRequirementRepository.Delete(discountRequirement);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(discountRequirement);
        }

        /// <summary>
        /// Load discount requirement rule by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <returns>Found discount requirement rule</returns>
		public virtual Provider<IDiscountRequirementRule> LoadDiscountRequirementRuleBySystemName(string systemName, int storeId = 0)
        {
			return _providerManager.GetProvider<IDiscountRequirementRule>(systemName, storeId);
        }

        /// <summary>
        /// Load all discount requirement rules
        /// </summary>
        /// <returns>Discount requirement rules</returns>
		public virtual IEnumerable<Provider<IDiscountRequirementRule>> LoadAllDiscountRequirementRules(int storeId = 0)
        {
			return _providerManager.GetAllProviders<IDiscountRequirementRule>(storeId);
        }

        /// <summary>
        /// Get discount by coupon code
        /// </summary>
        /// <param name="couponCode">CouponCode</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Discount</returns>
        public virtual Discount GetDiscountByCouponCode(string couponCode, bool showHidden = false)
        {
            if (String.IsNullOrWhiteSpace(couponCode))
                return null;

            var discount = GetAllDiscounts(null, couponCode, showHidden).FirstOrDefault();
            return discount;
        }

        /// <summary>
        /// Check discount requirements
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        public virtual bool IsDiscountValid(Discount discount, Customer customer)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            var couponCodeToValidate = "";
            if (customer != null)
				couponCodeToValidate = customer.GetAttribute<string>(SystemCustomerAttributeNames.DiscountCouponCode, _genericAttributeService);

            return IsDiscountValid(discount, customer, couponCodeToValidate);
        }

        /// <summary>
        /// Check discount requirements
        /// </summary>
        /// <param name="discount">Discount</param>
        /// <param name="customer">Customer</param>
        /// <param name="couponCodeToValidate">Coupon code to validate</param>
        /// <returns>true - requirement is met; otherwise, false</returns>
        public virtual bool IsDiscountValid(Discount discount, Customer customer, string couponCodeToValidate)
        {
            if (discount == null)
                throw new ArgumentNullException("discount");

            //check coupon code
            if (discount.RequiresCouponCode)
            {
                if (String.IsNullOrEmpty(discount.CouponCode))
                    return false;
                if (!discount.CouponCode.Equals(couponCodeToValidate, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            //check date range
            DateTime now = DateTime.UtcNow;
			int storeId = _storeContext.CurrentStore.Id;

            if (discount.StartDateUtc.HasValue)
            {
                DateTime startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                    return false;
            }
            if (discount.EndDateUtc.HasValue)
            {
                DateTime endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                    return false;
            }

            if (!CheckDiscountLimitations(discount, customer))
                return false;

            // discount requirements
            var requirements = discount.DiscountRequirements;
            foreach (var req in requirements)
            {
				var requirementRule = LoadDiscountRequirementRuleBySystemName(req.DiscountRequirementRuleSystemName, storeId);
                if (requirementRule == null)
                    continue;

                var request = new CheckDiscountRequirementRequest()
                {
                    DiscountRequirement = req,
                    Customer = customer,
					Store = _storeContext.CurrentStore
                };
                if (!requirementRule.Value.CheckRequirement(request))
                    return false;
            }

			// better not to apply discounts if there are gift cards in the cart cause the customer could "earn" money through that.
			if (discount.DiscountType == DiscountType.AssignedToOrderTotal || discount.DiscountType == DiscountType.AssignedToOrderSubTotal)
			{
				var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, storeId);

				if (cart.Any(x => x.Item.Product.IsGiftCard))
					return false;
			}

            return true;
        }

        /// <summary>
        /// Gets a discount usage history record
        /// </summary>
        /// <param name="discountUsageHistoryId">Discount usage history record identifier</param>
        /// <returns>Discount usage history</returns>
        public virtual DiscountUsageHistory GetDiscountUsageHistoryById(int discountUsageHistoryId)
        {
            if (discountUsageHistoryId == 0)
                return null;

            var duh = _discountUsageHistoryRepository.GetById(discountUsageHistoryId);
            return duh;
        }

        /// <summary>
        /// Gets all discount usage history records
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="customerId">Customer identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Discount usage history records</returns>
        public virtual IPagedList<DiscountUsageHistory> GetAllDiscountUsageHistory(int? discountId,
            int? customerId, int pageIndex, int pageSize)
        {
            var query = _discountUsageHistoryRepository.Table;
            if (discountId.HasValue && discountId.Value > 0)
                query = query.Where(duh => duh.DiscountId == discountId.Value);
            if (customerId.HasValue && customerId.Value > 0)
                query = query.Where(duh => duh.Order != null && duh.Order.CustomerId == customerId.Value);
            query = query.OrderByDescending(c => c.CreatedOnUtc);
            return new PagedList<DiscountUsageHistory>(query, pageIndex, pageSize);
        }

        /// <summary>
        /// Insert discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        public virtual void InsertDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException("discountUsageHistory");

            _discountUsageHistoryRepository.Insert(discountUsageHistory);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(discountUsageHistory);
        }


        /// <summary>
        /// Update discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        public virtual void UpdateDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException("discountUsageHistory");

            _discountUsageHistoryRepository.Update(discountUsageHistory);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(discountUsageHistory);
        }

        /// <summary>
        /// Delete discount usage history record
        /// </summary>
        /// <param name="discountUsageHistory">Discount usage history record</param>
        public virtual void DeleteDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            if (discountUsageHistory == null)
                throw new ArgumentNullException("discountUsageHistory");

            _discountUsageHistoryRepository.Delete(discountUsageHistory);

            _cacheManager.RemoveByPattern(DISCOUNTS_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(discountUsageHistory);
        }

        #endregion
    }
}
