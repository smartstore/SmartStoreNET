using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Payments;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Order service
    /// </summary>
    public partial class OrderTotalCalculationService : IOrderTotalCalculationService
    {
        private const string CART_TAXING_INFO_KEY = "CartTaxingInfos";

        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ITaxService _taxService;
        private readonly IShippingService _shippingService;
        private readonly IProviderManager _providerManager;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IDiscountService _discountService;
        private readonly IGiftCardService _giftCardService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly ICurrencyService _currencyService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly TaxSettings _taxSettings;
        private readonly RewardPointsSettings _rewardPointsSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="workContext">Work context</param>
		/// <param name="storeContext">Store context</param>
        /// <param name="priceCalculationService">Price calculation service</param>
        /// <param name="taxService">Tax service</param>
        /// <param name="shippingService">Shipping service</param>
        /// <param name="paymentService">Payment service</param>
        /// <param name="checkoutAttributeParser">Checkout attribute parser</param>
        /// <param name="discountService">Discount service</param>
        /// <param name="giftCardService">Gift card service</param>
        /// <param name="genericAttributeService">Generic attribute service</param>
        /// <param name="taxSettings">Tax settings</param>
        /// <param name="rewardPointsSettings">Reward points settings</param>
        /// <param name="shippingSettings">Shipping settings</param>
        /// <param name="shoppingCartSettings">Shopping cart settings</param>
        /// <param name="catalogSettings">Catalog settings</param>
        public OrderTotalCalculationService(
            IWorkContext workContext,
            IStoreContext storeContext,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IShippingService shippingService,
            IProviderManager providerManager,
            ICheckoutAttributeParser checkoutAttributeParser,
            IDiscountService discountService,
            IGiftCardService giftCardService,
            IGenericAttributeService genericAttributeService,
            IPaymentService paymentService,
            ICurrencyService currencyService,
            IProductAttributeParser productAttributeParser,
            TaxSettings taxSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings)
        {
            _workContext = workContext;
            _storeContext = storeContext;
            _priceCalculationService = priceCalculationService;
            _taxService = taxService;
            _shippingService = shippingService;
            _providerManager = providerManager;
            _checkoutAttributeParser = checkoutAttributeParser;
            _discountService = discountService;
            _giftCardService = giftCardService;
            _genericAttributeService = genericAttributeService;
            _paymentService = paymentService;
            _currencyService = currencyService;
            _productAttributeParser = productAttributeParser;
            _taxSettings = taxSettings;
            _rewardPointsSettings = rewardPointsSettings;
            _shippingSettings = shippingSettings;
            _shoppingCartSettings = shoppingCartSettings;
            _catalogSettings = catalogSettings;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        #endregion

        #region Utilities

        private Func<OrganizedShoppingCartItem, CartTaxingInfo> GetTaxingInfo = cartItem => (CartTaxingInfo)cartItem.CustomProperties[CART_TAXING_INFO_KEY];

        protected virtual void PrepareAuxiliaryServicesTaxingInfos(IList<OrganizedShoppingCartItem> cart)
        {
            // no additional infos required
            if (!cart.Any() || _taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.SpecifiedTaxCategory)
                return;

            // additional infos already collected
            if (cart.First().CustomProperties.ContainsKey(CART_TAXING_INFO_KEY))
                return;

            // instance taxing info objects
            cart.Each(x => x.CustomProperties[CART_TAXING_INFO_KEY] = new CartTaxingInfo());

            // collect infos
            if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.HighestCartAmount)
            {
                // calculate all subtotals
                cart.Each(x => GetTaxingInfo(x).SubTotalWithoutDiscount = _priceCalculationService.GetSubTotal(x, false));

                // items with the highest subtotal
                var highestAmountItems = cart
                    .GroupBy(x => x.Item.Product.TaxCategoryId)
                    .OrderByDescending(x => x.Sum(y => GetTaxingInfo(y).SubTotalWithoutDiscount))
                    .First();

                // mark items
                highestAmountItems.Each(x => GetTaxingInfo(x).HasHighestCartAmount = true);
            }
            else if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.HighestTaxRate)
            {
                var customer = cart.GetCustomer();
                var maxTaxRate = decimal.Zero;
                var maxTaxCategoryId = 0;

                // get tax category id with the highest rate
                cart.Each(x =>
                {
                    var taxRate = _taxService.GetTaxRate(x.Item.Product, x.Item.Product.TaxCategoryId, customer);
                    if (taxRate > maxTaxRate)
                    {
                        maxTaxRate = taxRate;
                        maxTaxCategoryId = x.Item.Product.TaxCategoryId;
                    }
                });

                // mark items
                cart.Where(x => x.Item.Product.TaxCategoryId == maxTaxCategoryId)
                    .Each(x => GetTaxingInfo(x).HasHighestTaxRate = true);
            }
            //else if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.ProRata)
            //{
            //	// calculate all subtotals
            //	cart.Each(x => GetTaxingInfo(x).SubTotalWithoutDiscount = _priceCalculationService.GetSubTotal(x, false));

            //	// sum over all subtotals
            //	var subTotalSum = cart.Sum(x => GetTaxingInfo(x).SubTotalWithoutDiscount);

            //	// calculate pro rata weightings
            //	cart.Each(x =>
            //	{
            //		var taxingInfo = GetTaxingInfo(x);
            //		taxingInfo.ProRataWeighting = taxingInfo.SubTotalWithoutDiscount / subTotalSum;
            //	});
            //}
        }

        protected virtual decimal? GetAdjustedShippingTotal(IList<OrganizedShoppingCartItem> cart, out Discount appliedDiscount)
        {
            appliedDiscount = null;

            decimal? shippingTotal = null;
            ShippingOption shippingOption = null;
            var customer = cart.GetCustomer();
            var storeId = _storeContext.CurrentStore.Id;

            if (customer != null)
            {
                shippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _genericAttributeService, storeId);
            }

            if (shippingOption != null)
            {
                // use last shipping option (get from cache)
                var shippingMethods = _shippingService.GetAllShippingMethods(null, storeId);
                shippingTotal = AdjustShippingRate(shippingOption.Rate, cart, shippingOption, shippingMethods, out appliedDiscount);
            }
            else
            {
                // use fixed rate (if possible)
                Address shippingAddress = null;
                if (customer != null)
                    shippingAddress = customer.ShippingAddress;

                var shippingRateComputationMethods = _shippingService.LoadActiveShippingRateComputationMethods(storeId);
                if (!shippingRateComputationMethods.Any())
                    throw new SmartException(T("Shipping.CouldNotLoadMethod"));

                if (shippingRateComputationMethods.Count() == 1)
                {
                    var shippingRateComputationMethod = shippingRateComputationMethods.First();
                    var getShippingOptionRequest = _shippingService.CreateShippingOptionRequest(cart, shippingAddress, storeId);
                    var fixedRate = shippingRateComputationMethod.Value.GetFixedRate(getShippingOptionRequest);

                    if (fixedRate.HasValue)
                    {
                        shippingTotal = AdjustShippingRate(fixedRate.Value, cart, null, null, out appliedDiscount);
                    }
                }
            }

            return shippingTotal;
        }

        protected virtual decimal GetShippingTaxAmount(
            decimal shipping,
            Customer customer,
            int taxCategoryId,
            SortedDictionary<decimal, decimal> taxRates)
        {
            var taxRate = decimal.Zero;
            var shippingExclTax = _taxService.GetShippingPrice(shipping, false, customer, taxCategoryId, out taxRate);
            var shippingInclTax = _taxService.GetShippingPrice(shipping, true, customer, taxCategoryId, out taxRate);

            var shippingTax = shippingInclTax - shippingExclTax;

            if (shippingTax < decimal.Zero)
                shippingTax = decimal.Zero;

            if (taxRate > decimal.Zero && shippingTax > decimal.Zero)
            {
                if (taxRates.ContainsKey(taxRate))
                    taxRates[taxRate] = taxRates[taxRate] + shippingTax;
                else
                    taxRates.Add(taxRate, shippingTax);
            }

            return shippingTax;
        }

        protected virtual decimal GetPaymentFeeTaxAmount(
            decimal paymentFee,
            Customer customer,
            int taxCategoryId,
            SortedDictionary<decimal, decimal> taxRates)
        {
            var taxRate = decimal.Zero;
            var paymentFeeExclTax = _taxService.GetPaymentMethodAdditionalFee(paymentFee, false, customer, taxCategoryId, out taxRate);
            var paymentFeeInclTax = _taxService.GetPaymentMethodAdditionalFee(paymentFee, true, customer, taxCategoryId, out taxRate);

            var paymentFeeTax = paymentFeeInclTax - paymentFeeExclTax;

            if (taxRate > decimal.Zero && paymentFeeTax != decimal.Zero)
            {
                if (taxRates.ContainsKey(taxRate))
                    taxRates[taxRate] = taxRates[taxRate] + paymentFeeTax;
                else
                    taxRates.Add(taxRate, paymentFeeTax);
            }

            return paymentFeeTax;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
        public virtual void GetShoppingCartSubTotal(IList<OrganizedShoppingCartItem> cart,
            out decimal discountAmount, out Discount appliedDiscount,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount)
        {
            bool includingTax = false;

            switch (_workContext.TaxDisplayType)
            {
                case TaxDisplayType.ExcludingTax:
                    includingTax = false;
                    break;
                case TaxDisplayType.IncludingTax:
                    includingTax = true;
                    break;
            }

            GetShoppingCartSubTotal(cart,
                includingTax,
                out discountAmount,
                out appliedDiscount,
                out subTotalWithoutDiscount,
                out subTotalWithDiscount);
        }

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
		public virtual void GetShoppingCartSubTotal(IList<OrganizedShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount, out Discount appliedDiscount,
            out decimal subTotalWithoutDiscount, out decimal subTotalWithDiscount)
        {
            SortedDictionary<decimal, decimal> taxRates = null;

            GetShoppingCartSubTotal(cart,
                includingTax,
                out discountAmount,
                out appliedDiscount,
                out subTotalWithoutDiscount,
                out subTotalWithDiscount,
                out taxRates);
        }

        /// <summary>
        /// Gets shopping cart subtotal
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="subTotalWithoutDiscount">Sub total (without discount)</param>
        /// <param name="subTotalWithDiscount">Sub total (with discount)</param>
        /// <param name="taxRates">Tax rates (of order sub total)</param>
        public virtual void GetShoppingCartSubTotal(IList<OrganizedShoppingCartItem> cart,
            bool includingTax,
            out decimal discountAmount,
            out Discount appliedDiscount,
            out decimal subTotalWithoutDiscount,
            out decimal subTotalWithDiscount,
            out SortedDictionary<decimal, decimal> taxRates)
        {
            discountAmount = decimal.Zero;
            appliedDiscount = null;
            subTotalWithoutDiscount = decimal.Zero;
            subTotalWithDiscount = decimal.Zero;
            taxRates = new SortedDictionary<decimal, decimal>();

            if (cart.Count == 0)
                return;

            // get the customer 
            var customer = cart.GetCustomer();
            var currency = _workContext.WorkingCurrency;

            // sub totals
            decimal subTotalExclTaxWithoutDiscount = decimal.Zero;
            decimal subTotalInclTaxWithoutDiscount = decimal.Zero;

            foreach (var shoppingCartItem in cart)
            {
                if (shoppingCartItem.Item.Product == null)
                    continue;

                decimal taxRate, sciSubTotal, sciExclTax, sciInclTax = decimal.Zero;

                shoppingCartItem.Item.Product.MergeWithCombination(shoppingCartItem.Item.AttributesXml, _productAttributeParser);

                if (currency.RoundOrderItemsEnabled)
                {
                    // Gross > Net RoundFix
                    int qty = shoppingCartItem.Item.Quantity;

                    sciSubTotal = _priceCalculationService.GetUnitPrice(shoppingCartItem, true);

                    // Adaption to eliminate rounding issues
                    sciExclTax = _taxService.GetProductPrice(shoppingCartItem.Item.Product, sciSubTotal, false, customer, out taxRate);
                    sciExclTax = sciExclTax.RoundIfEnabledFor(currency) * qty;
                    sciInclTax = _taxService.GetProductPrice(shoppingCartItem.Item.Product, sciSubTotal, true, customer, out taxRate);
                    sciInclTax = sciInclTax.RoundIfEnabledFor(currency) * qty;
                }
                else
                {
                    sciSubTotal = _priceCalculationService.GetSubTotal(shoppingCartItem, true);
                    sciExclTax = _taxService.GetProductPrice(shoppingCartItem.Item.Product, sciSubTotal, false, customer, out taxRate);
                    sciInclTax = _taxService.GetProductPrice(shoppingCartItem.Item.Product, sciSubTotal, true, customer, out taxRate);
                }

                subTotalExclTaxWithoutDiscount += sciExclTax;
                subTotalInclTaxWithoutDiscount += sciInclTax;

                // tax rates
                decimal sciTax = sciInclTax - sciExclTax;
                if (taxRate > decimal.Zero && sciTax > decimal.Zero)
                {
                    if (!taxRates.ContainsKey(taxRate))
                    {
                        taxRates.Add(taxRate, sciTax);
                    }
                    else
                    {
                        taxRates[taxRate] = taxRates[taxRate] + sciTax;
                    }
                }
            }

            // checkout attributes
            if (customer != null)
            {
                var checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
                var caValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(checkoutAttributesXml);
                if (caValues != null)
                {
                    foreach (var caValue in caValues)
                    {
                        decimal taxRate = decimal.Zero;

                        decimal caExclTax = _taxService.GetCheckoutAttributePrice(caValue, false, customer, out taxRate);
                        decimal caInclTax = _taxService.GetCheckoutAttributePrice(caValue, true, customer, out taxRate);
                        subTotalExclTaxWithoutDiscount += caExclTax;
                        subTotalInclTaxWithoutDiscount += caInclTax;

                        //tax rates
                        decimal caTax = caInclTax - caExclTax;
                        if (taxRate > decimal.Zero && caTax > decimal.Zero)
                        {
                            if (!taxRates.ContainsKey(taxRate))
                            {
                                taxRates.Add(taxRate, caTax);
                            }
                            else
                            {
                                taxRates[taxRate] = taxRates[taxRate] + caTax;
                            }
                        }
                    }
                }
            }

            //subtotal without discount
            if (includingTax)
            {
                subTotalWithoutDiscount = subTotalInclTaxWithoutDiscount;
            }
            else
            {
                subTotalWithoutDiscount = subTotalExclTaxWithoutDiscount;
            }

            if (subTotalWithoutDiscount < decimal.Zero)
            {
                subTotalWithoutDiscount = decimal.Zero;
            }

            subTotalWithoutDiscount = subTotalWithoutDiscount.RoundIfEnabledFor(currency);

            /*We calculate discount amount on order subtotal excl tax (discount first)*/
            //calculate discount amount ('Applied to order subtotal' discount)
            decimal discountAmountExclTax = GetOrderSubtotalDiscount(customer, subTotalExclTaxWithoutDiscount, out appliedDiscount);

            if (subTotalExclTaxWithoutDiscount < discountAmountExclTax)
                discountAmountExclTax = subTotalExclTaxWithoutDiscount;

            decimal discountAmountInclTax = discountAmountExclTax;
            //subtotal with discount (excl tax)
            decimal subTotalExclTaxWithDiscount = subTotalExclTaxWithoutDiscount - discountAmountExclTax;
            decimal subTotalInclTaxWithDiscount = subTotalExclTaxWithDiscount;

            //add tax for shopping items & checkout attributes
            Dictionary<decimal, decimal> tempTaxRates = new Dictionary<decimal, decimal>(taxRates);
            foreach (KeyValuePair<decimal, decimal> kvp in tempTaxRates)
            {
                decimal taxRate = kvp.Key;
                decimal taxValue = kvp.Value;

                if (taxValue != decimal.Zero)
                {
                    //discount the tax amount that applies to subtotal items
                    if (subTotalExclTaxWithoutDiscount > decimal.Zero)
                    {
                        decimal discountTax = taxRates[taxRate] * (discountAmountExclTax / subTotalExclTaxWithoutDiscount);
                        discountAmountInclTax += discountTax;
                        taxValue = taxRates[taxRate] - discountTax;
                        taxValue = taxValue.RoundIfEnabledFor(currency);
                        taxRates[taxRate] = taxValue;
                    }

                    //subtotal with discount (incl tax)
                    subTotalInclTaxWithDiscount += taxValue;
                }
            }

            discountAmountInclTax = discountAmountInclTax.RoundIfEnabledFor(currency);

            if (includingTax)
            {
                subTotalWithDiscount = subTotalInclTaxWithDiscount;
                discountAmount = discountAmountInclTax;
            }
            else
            {
                subTotalWithDiscount = subTotalExclTaxWithDiscount;
                discountAmount = discountAmountExclTax;
            }

            //round
            if (subTotalWithDiscount < decimal.Zero)
            {
                subTotalWithDiscount = decimal.Zero;
            }

            subTotalWithDiscount = subTotalWithDiscount.RoundIfEnabledFor(currency);
        }

        /// <summary>
        /// Gets an order discount (applied to order subtotal)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderSubTotal">Order subtotal</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Order discount</returns>
        public virtual decimal GetOrderSubtotalDiscount(Customer customer, decimal orderSubTotal, out Discount appliedDiscount)
        {
            appliedDiscount = null;
            decimal discountAmount = decimal.Zero;
            if (_catalogSettings.IgnoreDiscounts)
                return discountAmount;

            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToOrderSubTotal);
            var allowedDiscounts = new List<Discount>();
            if (allDiscounts != null)
            {
                foreach (var discount in allDiscounts)
                {
                    if (discount.DiscountType == DiscountType.AssignedToOrderSubTotal && !allowedDiscounts.Any(x => x.Id == discount.Id) && _discountService.IsDiscountValid(discount, customer))
                    {
                        allowedDiscounts.Add(discount);
                    }
                }
            }

            appliedDiscount = allowedDiscounts.GetPreferredDiscount(orderSubTotal);
            if (appliedDiscount != null)
                discountAmount = appliedDiscount.GetDiscountAmount(orderSubTotal);

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            return discountAmount;
        }


        /// <summary>
        /// Gets shopping cart additional shipping charge
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Additional shipping charge</returns>
        public virtual decimal GetShoppingCartAdditionalShippingCharge(IList<OrganizedShoppingCartItem> cart)
        {
            if (IsFreeShipping(cart))
                return decimal.Zero;

            decimal additionalShippingCharge = decimal.Zero;

            foreach (var sci in cart)
            {

                if (_shippingSettings.ChargeOnlyHighestProductShippingSurcharge)
                {
                    if (additionalShippingCharge < sci.Item.Product.AdditionalShippingCharge)
                    {
                        additionalShippingCharge = sci.Item.Product.AdditionalShippingCharge;
                    }
                }
                else
                {
                    if (sci.Item.IsShipEnabled && !sci.Item.IsFreeShipping && sci.Item.Product != null)
                    {
                        if (sci.Item.Product.ProductType == ProductType.BundledProduct && sci.Item.Product.BundlePerItemShipping)
                        {
                            sci.ChildItems.Each(x => additionalShippingCharge += (x.Item.Product.AdditionalShippingCharge * x.Item.Quantity));
                        }
                        else
                        {
                            additionalShippingCharge += sci.Item.Product.AdditionalShippingCharge * sci.Item.Quantity;
                        }
                    }
                }
            }
            return additionalShippingCharge;
        }

        /// <summary>
        /// Gets a value indicating whether shipping is free
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>A value indicating whether shipping is free</returns>
		public virtual bool IsFreeShipping(IList<OrganizedShoppingCartItem> cart)
        {
            Customer customer = cart.GetCustomer();
            if (customer != null)
            {
                // Check whether customer is in a customer role with free shipping applied.
                var customerRoles = customer.CustomerRoleMappings
                    .Select(x => x.CustomerRole)
                    .Where(x => x.Active);

                foreach (var customerRole in customerRoles)
                {
                    if (customerRole.FreeShipping)
                    {
                        return true;
                    }
                }
            }

            bool shoppingCartRequiresShipping = cart.RequiresShipping();
            if (!shoppingCartRequiresShipping)
                return true;

            //check whether all shopping cart items are marked as free shipping
            bool allItemsAreFreeShipping = true;
            foreach (var sc in cart)
            {
                if (sc.Item.IsShipEnabled && !sc.Item.IsFreeShipping)
                {
                    allItemsAreFreeShipping = false;
                    break;
                }
            }
            if (allItemsAreFreeShipping)
                return true;

            //free shipping over $X
            if (_shippingSettings.FreeShippingOverXEnabled)
            {
                //check whether we have subtotal enough to have free shipping
                decimal subTotalDiscountAmount = decimal.Zero;
                Discount subTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;
                GetShoppingCartSubTotal(cart, _shippingSettings.FreeShippingOverXIncludingTax, out subTotalDiscountAmount,
                    out subTotalAppliedDiscount, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

                if (subTotalWithDiscountBase > _shippingSettings.FreeShippingOverXValue)
                    return true;
            }

            //otherwise, return false
            return false;
        }

        /// <summary>
        /// Adjust shipping rate (free shipping, additional charges, discounts)
        /// </summary>
        /// <param name="shippingRate">Shipping rate to adjust</param>
        /// <param name="cart">Cart</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Adjusted shipping rate</returns>
		public virtual decimal AdjustShippingRate(decimal shippingRate, IList<OrganizedShoppingCartItem> cart,
            ShippingOption shippingOption, IList<ShippingMethod> shippingMethods, out Discount appliedDiscount)
        {
            appliedDiscount = null;

            if (IsFreeShipping(cart))
            {
                return decimal.Zero;
            }

            var adjustedRate = decimal.Zero;
            var bundlePerItemShipping = decimal.Zero;
            var ignoreAdditionalShippingCharge = false;

            foreach (var sci in cart)
            {
                if (sci.Item.Product != null && sci.Item.Product.ProductType == ProductType.BundledProduct && sci.Item.Product.BundlePerItemShipping)
                {
                    if (sci.ChildItems != null)
                    {
                        foreach (var childItem in sci.ChildItems.Where(x => x.Item.IsShipEnabled && !x.Item.IsFreeShipping))
                        {
                            bundlePerItemShipping += shippingRate;
                        }
                    }
                }
                else if (adjustedRate == decimal.Zero)
                {
                    adjustedRate = shippingRate;
                }
            }

            adjustedRate += bundlePerItemShipping;

            if (shippingOption != null && shippingMethods != null)
            {
                var shippingMethod = shippingMethods.FirstOrDefault(x => x.Id == shippingOption.ShippingMethodId);
                if (shippingMethod != null)
                {
                    ignoreAdditionalShippingCharge = shippingMethod.IgnoreCharges;
                }
            }

            // Additional shipping charges.
            if (!ignoreAdditionalShippingCharge)
            {
                decimal additionalShippingCharge = GetShoppingCartAdditionalShippingCharge(cart);
                adjustedRate += additionalShippingCharge;
            }

            // Discount.
            var customer = cart.GetCustomer();
            var discountAmount = GetShippingDiscount(customer, adjustedRate, out appliedDiscount);
            adjustedRate = adjustedRate - discountAmount;

            if (adjustedRate < decimal.Zero)
            {
                adjustedRate = decimal.Zero;
            }

            adjustedRate = adjustedRate.RoundIfEnabledFor(_workContext.WorkingCurrency);
            return adjustedRate;
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <returns>Shipping total</returns>
		public virtual decimal? GetShoppingCartShippingTotal(IList<OrganizedShoppingCartItem> cart)
        {
            bool includingTax = false;
            switch (_workContext.TaxDisplayType)
            {
                case TaxDisplayType.ExcludingTax:
                    includingTax = false;
                    break;
                case TaxDisplayType.IncludingTax:
                    includingTax = true;
                    break;
            }
            return GetShoppingCartShippingTotal(cart, includingTax);
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <returns>Shipping total</returns>
		public virtual decimal? GetShoppingCartShippingTotal(IList<OrganizedShoppingCartItem> cart, bool includingTax)
        {
            var taxRate = decimal.Zero;
            Discount appliedDiscount = null;

            return GetShoppingCartShippingTotal(cart, includingTax, out taxRate, out appliedDiscount);
        }

        /// <summary>
        /// Gets shopping cart shipping total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="includingTax">A value indicating whether calculated price should include tax</param>
        /// <param name="taxRate">Applied tax rate</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shipping total</returns>
		public virtual decimal? GetShoppingCartShippingTotal(
            IList<OrganizedShoppingCartItem> cart,
            bool includingTax,
            out decimal taxRate,
            out Discount appliedDiscount)
        {
            appliedDiscount = null;
            taxRate = decimal.Zero;

            if (IsFreeShipping(cart))
                return decimal.Zero;

            decimal? shippingTotalTaxed = null;
            var taxCategoryId = 0;
            var customer = cart.GetCustomer();
            var currency = _workContext.WorkingCurrency;

            var shippingTotal = GetAdjustedShippingTotal(cart, out appliedDiscount);

            if (!shippingTotal.HasValue)
                return null;

            if (shippingTotal.Value < decimal.Zero)
            {
                shippingTotal = decimal.Zero;
            }

            shippingTotal = shippingTotal.Value.RoundIfEnabledFor(currency);

            PrepareAuxiliaryServicesTaxingInfos(cart);

            // commented out cause requires several plugins to be updated and migration of Order.OrderShippingTaxRate and Order.PaymentMethodAdditionalFeeTaxRate
            //if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.ProRata)
            //{
            //	// calculate proRataShipping: get product weightings for cart and multiply them with the shipping amount
            //	shippingTotalTaxed = decimal.Zero;

            //	var tmpTaxRate = decimal.Zero;
            //	var taxRates = new List<decimal>();

            //	foreach (var item in cart)
            //	{
            //		var proRataShipping = shippingTotal.Value * GetTaxingInfo(item).ProRataWeighting;
            //		shippingTotalTaxed += _taxService.GetShippingPrice(proRataShipping, includingTax, customer, item.Item.Product.TaxCategoryId, out tmpTaxRate);

            //		taxRates.Add(tmpTaxRate);
            //	}

            //	// a tax rate is only defined if all rates are equal. return zero tax rate in all other cases.
            //	if (taxRates.Any() && taxRates.Distinct().Count() == 1)
            //	{
            //		taxRate = taxRates.First();
            //	}
            //}
            //else
            //{

            if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.HighestCartAmount)
            {
                var cartItem = cart.FirstOrDefault(x => GetTaxingInfo(x).HasHighestCartAmount);
                taxCategoryId = (cartItem != null ? cartItem.Item.Product.TaxCategoryId : 0);
            }
            else if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.HighestTaxRate)
            {
                var cartItem = cart.FirstOrDefault(x => GetTaxingInfo(x).HasHighestTaxRate);
                taxCategoryId = (cartItem != null ? cartItem.Item.Product.TaxCategoryId : 0);
            }

            // fallback to setting
            if (taxCategoryId == 0)
            {
                taxCategoryId = _taxSettings.ShippingTaxClassId;
            }

            shippingTotalTaxed = _taxService.GetShippingPrice(shippingTotal.Value, includingTax, customer, taxCategoryId, out taxRate);
            shippingTotalTaxed = shippingTotalTaxed.Value.RoundIfEnabledFor(currency);

            return shippingTotalTaxed;
        }

        /// <summary>
        /// Gets a shipping discount
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shippingTotal">Shipping total</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Shipping discount</returns>
        public virtual decimal GetShippingDiscount(Customer customer, decimal shippingTotal, out Discount appliedDiscount)
        {
            appliedDiscount = null;
            decimal shippingDiscountAmount = decimal.Zero;
            if (_catalogSettings.IgnoreDiscounts)
                return shippingDiscountAmount;

            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToShipping);
            var allowedDiscounts = new List<Discount>();

            if (allDiscounts != null)
            {
                foreach (var discount in allDiscounts)
                {
                    if (discount.DiscountType == DiscountType.AssignedToShipping && !allowedDiscounts.Any(x => x.Id == discount.Id) && _discountService.IsDiscountValid(discount, customer))
                    {
                        allowedDiscounts.Add(discount);
                    }
                }
            }

            appliedDiscount = allowedDiscounts.GetPreferredDiscount(shippingTotal);
            if (appliedDiscount != null)
            {
                shippingDiscountAmount = appliedDiscount.GetDiscountAmount(shippingTotal);
            }

            if (shippingDiscountAmount < decimal.Zero)
            {
                shippingDiscountAmount = decimal.Zero;
            }

            shippingDiscountAmount = shippingDiscountAmount.RoundIfEnabledFor(_workContext.WorkingCurrency);
            return shippingDiscountAmount;
        }





        /// <summary>
        /// Gets tax
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        /// <returns>Tax total</returns>
		public virtual decimal GetTaxTotal(IList<OrganizedShoppingCartItem> cart, bool usePaymentMethodAdditionalFee = true)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            SortedDictionary<decimal, decimal> taxRates = null;
            return GetTaxTotal(cart, out taxRates, usePaymentMethodAdditionalFee);
        }

        /// <summary>
        /// Gets tax
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <param name="taxRates">Tax rates</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating tax</param>
        /// <returns>Tax total</returns>
		public virtual decimal GetTaxTotal(IList<OrganizedShoppingCartItem> cart, out SortedDictionary<decimal, decimal> taxRates, bool usePaymentMethodAdditionalFee = true)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            taxRates = new SortedDictionary<decimal, decimal>();

            var subTotalTax = decimal.Zero;
            var shippingTax = decimal.Zero;
            var paymentFeeTax = decimal.Zero;
            var customer = cart.GetCustomer();
            var currency = _workContext.WorkingCurrency;

            //// (VATFIX)
            if (_taxService.IsVatExempt(null, customer))
            {
                taxRates.Add(decimal.Zero, decimal.Zero);
                return decimal.Zero;
            }
            //// (VATFIX)

            #region order sub total (items + checkout attributes)

            var orderSubTotalDiscountAmount = decimal.Zero;
            var subTotalWithoutDiscountBase = decimal.Zero;
            var subTotalWithDiscountBase = decimal.Zero;
            Discount appliedDiscount = null;
            SortedDictionary<decimal, decimal> orderSubTotalTaxRates = null;

            GetShoppingCartSubTotal(cart, false,
                out orderSubTotalDiscountAmount, out appliedDiscount,
                out subTotalWithoutDiscountBase, out subTotalWithDiscountBase,
                out orderSubTotalTaxRates);

            foreach (KeyValuePair<decimal, decimal> kvp in orderSubTotalTaxRates)
            {
                var taxRate = kvp.Key;
                var taxValue = kvp.Value;
                subTotalTax += taxValue;

                if (taxRate > decimal.Zero && taxValue > decimal.Zero)
                {
                    if (!taxRates.ContainsKey(taxRate))
                        taxRates.Add(taxRate, taxValue);
                    else
                        taxRates[taxRate] = taxRates[taxRate] + taxValue;
                }
            }

            #endregion

            #region shipping tax amount

            if (_taxSettings.ShippingIsTaxable && !IsFreeShipping(cart))
            {
                var taxCategoryId = 0;
                var shippingTotal = GetAdjustedShippingTotal(cart, out appliedDiscount);

                if (shippingTotal.HasValue)
                {
                    if (shippingTotal.Value < decimal.Zero)
                    {
                        shippingTotal = decimal.Zero;
                    }

                    shippingTotal = shippingTotal.Value.RoundIfEnabledFor(currency);

                    PrepareAuxiliaryServicesTaxingInfos(cart);

                    // commented out cause requires several plugins to be updated and migration of Order.OrderShippingTaxRate and Order.PaymentMethodAdditionalFeeTaxRate
                    //if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.ProRata)
                    //{
                    //	// calculate proRataShipping: get product weightings for cart and multiply them with the shipping amount
                    //	foreach (var item in cart)
                    //	{
                    //		var proRataShipping = shippingTotal.Value * GetTaxingInfo(item).ProRataWeighting;
                    //		shippingTax += GetShippingTaxAmount(proRataShipping, customer, item.Item.Product.TaxCategoryId, taxRates);
                    //	}
                    //}
                    //else
                    //{

                    if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.HighestCartAmount)
                    {
                        var cartItem = cart.FirstOrDefault(x => GetTaxingInfo(x).HasHighestCartAmount);
                        taxCategoryId = (cartItem != null ? cartItem.Item.Product.TaxCategoryId : 0);
                    }
                    else if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.HighestTaxRate)
                    {
                        var cartItem = cart.FirstOrDefault(x => GetTaxingInfo(x).HasHighestTaxRate);
                        taxCategoryId = (cartItem != null ? cartItem.Item.Product.TaxCategoryId : 0);
                    }

                    // fallback to setting
                    if (taxCategoryId == 0)
                    {
                        taxCategoryId = _taxSettings.ShippingTaxClassId;
                    }

                    shippingTax = GetShippingTaxAmount(shippingTotal.Value, customer, taxCategoryId, taxRates);
                    shippingTax = shippingTax.RoundIfEnabledFor(currency);
                }
            }

            #endregion

            #region payment fee tax amount

            if (usePaymentMethodAdditionalFee && _taxSettings.PaymentMethodAdditionalFeeIsTaxable && customer != null)
            {
                var paymentMethodSystemName = customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _genericAttributeService, _storeContext.CurrentStore.Id);
                var provider = _providerManager.GetProvider<IPaymentMethod>(paymentMethodSystemName);

                if (provider != null)
                {
                    var taxCategoryId = 0;
                    var paymentFee = provider.Value.GetAdditionalHandlingFee(cart);
                    paymentFee = paymentFee.RoundIfEnabledFor(currency);

                    PrepareAuxiliaryServicesTaxingInfos(cart);

                    // commented out cause requires several plugins to be updated and migration of Order.OrderShippingTaxRate and Order.PaymentMethodAdditionalFeeTaxRate
                    //if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.ProRata)
                    //{
                    //	// calculate proRataShipping: get product weightings for cart and multiply them with the shipping amount
                    //	foreach (var item in cart)
                    //	{
                    //		var proRataPaymentFees = paymentFee * GetTaxingInfo(item).ProRataWeighting;
                    //		paymentFeeTax += GetPaymentFeeTaxAmount(proRataPaymentFees, customer, item.Item.Product.TaxCategoryId, taxRates);
                    //	}
                    //}
                    //else
                    //{

                    if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.HighestCartAmount)
                    {
                        var cartItem = cart.FirstOrDefault(x => GetTaxingInfo(x).HasHighestCartAmount);
                        taxCategoryId = (cartItem != null ? cartItem.Item.Product.TaxCategoryId : 0);
                    }
                    else if (_taxSettings.AuxiliaryServicesTaxingType == AuxiliaryServicesTaxType.HighestTaxRate)
                    {
                        var cartItem = cart.FirstOrDefault(x => GetTaxingInfo(x).HasHighestTaxRate);
                        taxCategoryId = (cartItem != null ? cartItem.Item.Product.TaxCategoryId : 0);
                    }

                    // fallback to setting
                    if (taxCategoryId == 0)
                        taxCategoryId = _taxSettings.PaymentMethodAdditionalFeeTaxClassId;

                    paymentFeeTax = GetPaymentFeeTaxAmount(paymentFee, customer, taxCategoryId, taxRates);
                }
            }

            #endregion

            // add at least one tax rate (0%)
            if (taxRates.Count == 0)
                taxRates.Add(decimal.Zero, decimal.Zero);

            // summarize taxes
            var taxTotal = subTotalTax + shippingTax + paymentFeeTax;

            // ensure that tax is equal or greater than zero
            if (taxTotal < decimal.Zero)
                taxTotal = decimal.Zero;

            // round tax
            taxTotal = taxTotal.RoundIfEnabledFor(currency);
            return taxTotal;
        }





        public virtual ShoppingCartTotal GetShoppingCartTotal(
            IList<OrganizedShoppingCartItem> cart,
            bool ignoreRewardPoints = false,
            bool usePaymentMethodAdditionalFee = true,
            bool ignoreCreditBalance = false)
        {
            var customer = cart.GetCustomer();
            var store = _storeContext.CurrentStore;
            var currency = _workContext.WorkingCurrency;
            var paymentMethodSystemName = "";

            if (customer != null)
            {
                paymentMethodSystemName = customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _genericAttributeService, store.Id);
            }

            // Subtotal without tax
            var subtotalBase = decimal.Zero;
            var orderSubTotalDiscountAmount = decimal.Zero;
            Discount orderSubTotalAppliedDiscount = null;
            var subTotalWithoutDiscountBase = decimal.Zero;
            var subTotalWithDiscountBase = decimal.Zero;

            GetShoppingCartSubTotal(cart, false, out orderSubTotalDiscountAmount, out orderSubTotalAppliedDiscount, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

            // Subtotal with discount
            subtotalBase = subTotalWithDiscountBase;

            // Shipping without tax
            decimal? shoppingCartShipping = GetShoppingCartShippingTotal(cart, false);

            // Payment method additional fee without tax
            var paymentMethodAdditionalFeeWithoutTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && !string.IsNullOrEmpty(paymentMethodSystemName))
            {
                var provider = _providerManager.GetProvider<IPaymentMethod>(paymentMethodSystemName);
                var paymentMethodAdditionalFee = (provider != null ? provider.Value.GetAdditionalHandlingFee(cart) : decimal.Zero);

                paymentMethodAdditionalFee = paymentMethodAdditionalFee.RoundIfEnabledFor(currency);
                paymentMethodAdditionalFeeWithoutTax = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, false, customer);
            }

            // Tax
            var shoppingCartTax = GetTaxTotal(cart, usePaymentMethodAdditionalFee);

            // Order total
            var resultTemp = decimal.Zero;
            resultTemp += subtotalBase;
            if (shoppingCartShipping.HasValue)
            {
                resultTemp += shoppingCartShipping.Value;
            }

            resultTemp += paymentMethodAdditionalFeeWithoutTax;
            resultTemp += shoppingCartTax;
            resultTemp = resultTemp.RoundIfEnabledFor(currency);

            #region Order total discount

            Discount appliedDiscount = null;
            var discountAmount = GetOrderTotalDiscount(customer, resultTemp, out appliedDiscount);

            // Sub totals with discount        
            if (resultTemp < discountAmount)
                discountAmount = resultTemp;

            // Reduce subtotal
            resultTemp -= discountAmount;

            if (resultTemp < decimal.Zero)
            {
                resultTemp = decimal.Zero;
            }

            resultTemp = resultTemp.RoundIfEnabledFor(currency);

            #endregion

            #region Applied gift cards

            // Let's apply gift cards now (gift cards that can be used)
            var appliedGiftCards = new List<AppliedGiftCard>();
            if (!cart.IsRecurring())
            {
                // We don't apply gift cards for recurring products
                var giftCards = _giftCardService.GetActiveGiftCardsAppliedByCustomer(customer, store.Id);
                if (giftCards != null)
                {
                    foreach (var gc in giftCards)
                    {
                        if (resultTemp > decimal.Zero)
                        {
                            var remainingAmount = gc.GetGiftCardRemainingAmount();
                            var amountCanBeUsed = resultTemp > remainingAmount ? remainingAmount : resultTemp;

                            // Reduce subtotal
                            resultTemp -= amountCanBeUsed;

                            appliedGiftCards.Add(new AppliedGiftCard
                            {
                                GiftCard = gc,
                                AmountCanBeUsed = amountCanBeUsed
                            });
                        }
                    }
                }
            }

            #endregion

            #region Reward points

            var redeemedRewardPoints = 0;
            var redeemedRewardPointsAmount = decimal.Zero;

            if (_rewardPointsSettings.Enabled &&
                !ignoreRewardPoints && customer != null &&
                customer.GetAttribute<bool>(SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, _genericAttributeService, store.Id))
            {
                var rewardPointsBalance = customer.GetRewardPointsBalance();
                var rewardPointsBalanceAmount = ConvertRewardPointsToAmount(rewardPointsBalance);

                if (resultTemp > decimal.Zero)
                {
                    if (resultTemp > rewardPointsBalanceAmount)
                    {
                        redeemedRewardPoints = rewardPointsBalance;
                        redeemedRewardPointsAmount = rewardPointsBalanceAmount;
                    }
                    else
                    {
                        redeemedRewardPointsAmount = resultTemp;
                        redeemedRewardPoints = ConvertAmountToRewardPoints(redeemedRewardPointsAmount);
                    }
                }
            }

            #endregion

            if (resultTemp < decimal.Zero)
            {
                resultTemp = decimal.Zero;
            }

            resultTemp = resultTemp.RoundIfEnabledFor(currency);

            var roundingAmount = decimal.Zero;
            var roundingAmountConverted = decimal.Zero;
            // Return null if we have errors:
            var orderTotal = shoppingCartShipping.HasValue ? resultTemp : (decimal?)null;
            var orderTotalConverted = orderTotal;
            var appliedCreditBalance = decimal.Zero;

            if (orderTotal.HasValue)
            {
                orderTotal = orderTotal.Value - redeemedRewardPointsAmount;

                // Credit balance.
                if (!ignoreCreditBalance && customer != null && orderTotal > decimal.Zero)
                {
                    var creditBalance = customer.GetAttribute<decimal>(SystemCustomerAttributeNames.UseCreditBalanceDuringCheckout, _genericAttributeService, store.Id);
                    if (creditBalance > decimal.Zero)
                    {
                        if (creditBalance > orderTotal)
                        {
                            // Normalize used amount.
                            appliedCreditBalance = orderTotal.Value;
                            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.UseCreditBalanceDuringCheckout, orderTotal.Value, store.Id);
                        }
                        else
                        {
                            appliedCreditBalance = creditBalance;
                        }
                    }
                }

                orderTotal = orderTotal.Value - appliedCreditBalance;
                orderTotal = orderTotal.Value.RoundIfEnabledFor(currency);

                orderTotalConverted = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotal.Value, currency, store);

                // Order total rounding
                if (currency.RoundOrderTotalEnabled && paymentMethodSystemName.HasValue())
                {
                    var paymentMethod = _paymentService.GetPaymentMethodBySystemName(paymentMethodSystemName);
                    if (paymentMethod != null && paymentMethod.RoundOrderTotalEnabled)
                    {
                        orderTotal = orderTotal.Value.RoundToNearest(currency, out roundingAmount);
                        orderTotalConverted = orderTotalConverted.Value.RoundToNearest(currency, out roundingAmountConverted);
                    }
                }
            }

            var result = new ShoppingCartTotal(orderTotal);
            result.RoundingAmount = roundingAmount;
            result.DiscountAmount = discountAmount;
            result.AppliedDiscount = appliedDiscount;
            result.AppliedGiftCards = appliedGiftCards;
            result.RedeemedRewardPoints = redeemedRewardPoints;
            result.RedeemedRewardPointsAmount = redeemedRewardPointsAmount;
            result.CreditBalance = appliedCreditBalance;

            result.ConvertedFromPrimaryStoreCurrency.TotalAmount = orderTotalConverted;
            result.ConvertedFromPrimaryStoreCurrency.RoundingAmount = roundingAmountConverted;

            return result;
        }

        /// <summary>
        /// Gets an order discount (applied to order total)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderTotal">Order total</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Order discount</returns>
        public virtual decimal GetOrderTotalDiscount(Customer customer, decimal orderTotal, out Discount appliedDiscount)
        {
            appliedDiscount = null;
            decimal discountAmount = decimal.Zero;
            if (_catalogSettings.IgnoreDiscounts)
                return discountAmount;

            var allDiscounts = _discountService.GetAllDiscounts(DiscountType.AssignedToOrderTotal);
            var allowedDiscounts = new List<Discount>();

            if (allDiscounts != null)
            {
                foreach (var discount in allDiscounts)
                {
                    if (discount.DiscountType == DiscountType.AssignedToOrderTotal && !allowedDiscounts.Any(x => x.Id == discount.Id) && _discountService.IsDiscountValid(discount, customer))
                    {
                        allowedDiscounts.Add(discount);
                    }
                }
            }

            appliedDiscount = allowedDiscounts.GetPreferredDiscount(orderTotal);
            if (appliedDiscount != null)
                discountAmount = appliedDiscount.GetDiscountAmount(orderTotal);

            if (discountAmount < decimal.Zero)
                discountAmount = decimal.Zero;

            discountAmount = discountAmount.RoundIfEnabledFor(_workContext.WorkingCurrency);
            return discountAmount;
        }





        /// <summary>
        /// Converts reward points to amount primary store currency
        /// </summary>
        /// <param name="rewardPoints">Reward points</param>
        /// <returns>Converted value</returns>
        public virtual decimal ConvertRewardPointsToAmount(int rewardPoints)
        {
            decimal result = decimal.Zero;
            if (rewardPoints <= 0)
                return decimal.Zero;

            result = rewardPoints * _rewardPointsSettings.ExchangeRate;
            result = result.RoundIfEnabledFor(_workContext.WorkingCurrency);

            return result;
        }

        /// <summary>
        /// Converts an amount in primary store currency to reward points
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <returns>Converted value</returns>
        public virtual int ConvertAmountToRewardPoints(decimal amount)
        {
            int result = 0;
            if (amount <= 0)
                return 0;

            if (_rewardPointsSettings.ExchangeRate > 0)
            {
                if (_rewardPointsSettings.RoundDownRewardPoints)
                    result = (int)Math.Floor(amount / _rewardPointsSettings.ExchangeRate);
                else
                    result = (int)Math.Ceiling(amount / _rewardPointsSettings.ExchangeRate);
            }

            return result;
        }

        #endregion
    }


    internal class CartTaxingInfo
    {
        internal CartTaxingInfo()
        {
            ProRataWeighting = decimal.Zero;
        }

        public decimal SubTotalWithoutDiscount { get; internal set; }
        public bool HasHighestCartAmount { get; internal set; }
        public bool HasHighestTaxRate { get; internal set; }
        public decimal ProRataWeighting { get; internal set; }
    }
}
