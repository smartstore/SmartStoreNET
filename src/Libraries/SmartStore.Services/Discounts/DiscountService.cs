using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Cart.Rules;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;

namespace SmartStore.Services.Discounts
{
    public partial class DiscountService : IDiscountService
    {
        private const string DISCOUNTS_ALL_KEY = "SmartStore.discount.all-{0}-{1}";
        private const string DISCOUNTS_PATTERN_KEY = "SmartStore.discount.*";

        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountUsageHistory> _discountUsageHistoryRepository;
        private readonly IRequestCache _requestCache;
        private readonly IStoreContext _storeContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICartRuleProvider _cartRuleProvider;
        private readonly IDictionary<DiscountKey, bool> _discountValidityCache;

        public DiscountService(
            IRequestCache requestCache,
            IRepository<Discount> discountRepository,
            IRepository<DiscountUsageHistory> discountUsageHistoryRepository,
            IStoreContext storeContext,
            IGenericAttributeService genericAttributeService,
            ICartRuleProvider cartRuleProvider)
        {
            _requestCache = requestCache;
            _discountRepository = discountRepository;
            _discountUsageHistoryRepository = discountUsageHistoryRepository;
            _storeContext = storeContext;
            _genericAttributeService = genericAttributeService;
            _cartRuleProvider = cartRuleProvider;
            _discountValidityCache = new Dictionary<DiscountKey, bool>();
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
            Guard.NotNull(discount, nameof(discount));

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
            var discountTypeId = discountType.HasValue ? (int)discountType.Value : 0;

            // We load all discounts and filter them by passed "discountType" parameter later because
            // this method is invoked several times per HTTP request with distinct "discountType" parameter.
            var key = string.Format(DISCOUNTS_ALL_KEY, showHidden, couponCode);
            var result = _requestCache.Get(key, () =>
            {
                var query = _discountRepository.Table;

                if (!showHidden)
                {
                    var utcNow = DateTime.UtcNow;

                    query = query.Where(d =>
                        (!d.StartDateUtc.HasValue || d.StartDateUtc <= utcNow) &&
                        (!d.EndDateUtc.HasValue || d.EndDateUtc >= utcNow));
                }

                if (!string.IsNullOrWhiteSpace(couponCode))
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
                return result[discountTypeId];
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
            Guard.NotNull(discount, nameof(discount));

            _discountRepository.Update(discount);
            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
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
            var couponCodeToValidate = "";
            if (customer != null)
            {
                couponCodeToValidate = customer.GetAttribute<string>(SystemCustomerAttributeNames.DiscountCouponCode, _genericAttributeService);
            }

            var valid = IsDiscountValid(discount, customer, couponCodeToValidate);
            return valid;
        }

        public virtual bool IsDiscountValid(Discount discount, Customer customer, string couponCodeToValidate)
        {
            Guard.NotNull(discount, nameof(discount));

            var cacheKey = new DiscountKey(discount, customer, couponCodeToValidate);
            if (_discountValidityCache.TryGetValue(cacheKey, out var result))
            {
                return result;
            }

            // Check coupon code.
            if (discount.RequiresCouponCode)
            {
                if (discount.CouponCode.IsEmpty())
                    return Cached(false);

                if (!discount.CouponCode.Equals(couponCodeToValidate, StringComparison.InvariantCultureIgnoreCase))
                    return Cached(false);
            }

            // Check date range.
            var now = DateTime.UtcNow;

            if (discount.StartDateUtc.HasValue)
            {
                var startDate = DateTime.SpecifyKind(discount.StartDateUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                    return Cached(false);
            }

            if (discount.EndDateUtc.HasValue)
            {
                var endDate = DateTime.SpecifyKind(discount.EndDateUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                    return Cached(false);
            }

            if (!CheckDiscountLimitations(discount, customer))
                return Cached(false);

            // Better not to apply discounts if there are gift cards in the cart cause the customer could "earn" money through that.
            if (discount.DiscountType == DiscountType.AssignedToOrderTotal || discount.DiscountType == DiscountType.AssignedToOrderSubTotal)
            {
                var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
                if (cart.Any(x => x.Item?.Product != null && x.Item.Product.IsGiftCard))
                    return Cached(false);
            }

            // Rule sets.
            if (!_cartRuleProvider.RuleMatches(discount))
            {
                return Cached(false);
            }

            return Cached(true);

            bool Cached(bool value)
            {
                _discountValidityCache[cacheKey] = value;
                return value;
            }
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
            Guard.NotNull(discountUsageHistory, nameof(discountUsageHistory));

            _discountUsageHistoryRepository.Insert(discountUsageHistory);
            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }

        public virtual void UpdateDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            Guard.NotNull(discountUsageHistory, nameof(discountUsageHistory));

            _discountUsageHistoryRepository.Update(discountUsageHistory);
            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }

        public virtual void DeleteDiscountUsageHistory(DiscountUsageHistory discountUsageHistory)
        {
            Guard.NotNull(discountUsageHistory, nameof(discountUsageHistory));

            _discountUsageHistoryRepository.Delete(discountUsageHistory);
            _requestCache.RemoveByPattern(DISCOUNTS_PATTERN_KEY);
        }

        class DiscountKey : Tuple<Discount, Customer, string>
        {
            public DiscountKey(Discount discount, Customer customer, string customerCouponCode)
                : base(discount, customer, customerCouponCode)
            {
            }
        }
    }
}
