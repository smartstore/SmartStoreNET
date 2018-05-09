using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
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
        public OrderTotalCalculationService(IWorkContext workContext,
			IStoreContext storeContext,
            IPriceCalculationService priceCalculationService,
            ITaxService taxService,
            IShippingService shippingService,
			IProviderManager providerManager,
            ICheckoutAttributeParser checkoutAttributeParser,
            IDiscountService discountService,
            IGiftCardService giftCardService,
            IGenericAttributeService genericAttributeService,
			IProductAttributeParser productAttributeParser,
            TaxSettings taxSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings)
        {
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._priceCalculationService = priceCalculationService;
            this._taxService = taxService;
            this._shippingService = shippingService;
			this._providerManager = providerManager;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._discountService = discountService;
            this._giftCardService = giftCardService;
            this._genericAttributeService = genericAttributeService;
			this._productAttributeParser = productAttributeParser;
            this._taxSettings = taxSettings;
            this._rewardPointsSettings = rewardPointsSettings;
            this._shippingSettings = shippingSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;

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
				var shippingMethods = _shippingService.GetAllShippingMethods();
				shippingTotal = AdjustShippingRate(shippingOption.Rate, cart, shippingOption.Name, shippingMethods, out appliedDiscount);
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
            Customer customer = cart.GetCustomer();

            // sub totals
            decimal subTotalExclTaxWithoutDiscount = decimal.Zero;
            decimal subTotalInclTaxWithoutDiscount = decimal.Zero;

            foreach (var shoppingCartItem in cart)
			{
				if (shoppingCartItem.Item.Product == null)
					continue;

				decimal taxRate, sciSubTotal, sciExclTax, sciInclTax = decimal.Zero;

				shoppingCartItem.Item.Product.MergeWithCombination(shoppingCartItem.Item.AttributesXml, _productAttributeParser);

				if (_shoppingCartSettings.RoundPricesDuringCalculation)
				{
					// Gross > Net RoundFix
					int qty = shoppingCartItem.Item.Quantity;

					sciSubTotal = _priceCalculationService.GetUnitPrice(shoppingCartItem, true);

					// Adaption to eliminate rounding issues
					sciExclTax = _taxService.GetProductPrice(shoppingCartItem.Item.Product, sciSubTotal, false, customer, out taxRate);
					sciExclTax = Math.Round(sciExclTax, 2) * qty;
					sciInclTax = _taxService.GetProductPrice(shoppingCartItem.Item.Product, sciSubTotal, true, customer, out taxRate);
					sciInclTax = Math.Round(sciInclTax, 2) * qty;
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
                subTotalWithoutDiscount = subTotalInclTaxWithoutDiscount;
            else
                subTotalWithoutDiscount = subTotalExclTaxWithoutDiscount;

            if (subTotalWithoutDiscount < decimal.Zero)
                subTotalWithoutDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithoutDiscount = Math.Round(subTotalWithoutDiscount, 2);

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
                        if (_shoppingCartSettings.RoundPricesDuringCalculation)
                            taxValue = Math.Round(taxValue, 2);
                        taxRates[taxRate] = taxValue;
                    }

                    //subtotal with discount (incl tax)
                    subTotalInclTaxWithDiscount += taxValue;
                }
            }

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                discountAmountInclTax = Math.Round(discountAmountInclTax, 2);

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
                subTotalWithDiscount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                subTotalWithDiscount = Math.Round(subTotalWithDiscount, 2);
        }

        /// <summary>
        /// Gets an order discount (applied to order subtotal)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="orderSubTotal">Order subtotal</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Order discount</returns>
        public virtual decimal GetOrderSubtotalDiscount(Customer customer,
            decimal orderSubTotal, out Discount appliedDiscount)
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
                //check whether customer is in a customer role with free shipping applied
                var customerRoles = customer.CustomerRoles.Where(cr => cr.Active);
                foreach (var customerRole in customerRoles)
                    if (customerRole.FreeShipping)
                        return true;
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
			string shippingMethodName, IList<ShippingMethod> shippingMethods, out Discount appliedDiscount)
        {
            appliedDiscount = null;

            //free shipping
            if (IsFreeShipping(cart))
                return decimal.Zero;

			decimal adjustedRate = decimal.Zero;
			decimal bundlePerItemShipping = decimal.Zero;
			bool ignoreAdditionalShippingCharge = false;
			ShippingMethod shippingMethod;

			foreach (var sci in cart)
			{
				if (sci.Item.Product != null && sci.Item.Product.ProductType == ProductType.BundledProduct && sci.Item.Product.BundlePerItemShipping)
				{
					if (sci.ChildItems != null)
					{
						foreach (var childItem in sci.ChildItems.Where(x => x.Item.IsShipEnabled && !x.Item.IsFreeShipping))
							bundlePerItemShipping += shippingRate;
					}
				}
				else if (adjustedRate == decimal.Zero)
				{
					adjustedRate = shippingRate;
				}
			}

			adjustedRate += bundlePerItemShipping;

			if (shippingMethodName.HasValue() && shippingMethods != null &&
				(shippingMethod = shippingMethods.FirstOrDefault(x => x.Name.IsCaseInsensitiveEqual(shippingMethodName))) != null)
			{
				ignoreAdditionalShippingCharge = shippingMethod.IgnoreCharges;
			}

            //additional shipping charges
			if (!ignoreAdditionalShippingCharge)
			{
				decimal additionalShippingCharge = GetShoppingCartAdditionalShippingCharge(cart);
				adjustedRate += additionalShippingCharge;
			}

            //discount
            var customer = cart.GetCustomer();
            decimal discountAmount = GetShippingDiscount(customer, adjustedRate, out appliedDiscount);
            adjustedRate = adjustedRate - discountAmount;

            if (adjustedRate < decimal.Zero)
                adjustedRate = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                adjustedRate = Math.Round(adjustedRate, 2);

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
			var storeId = _storeContext.CurrentStore.Id;

			var shippingTotal = GetAdjustedShippingTotal(cart, out appliedDiscount);

			if (!shippingTotal.HasValue)
				return null;

			if (shippingTotal.Value < decimal.Zero)
				shippingTotal = decimal.Zero;

			if (_shoppingCartSettings.RoundPricesDuringCalculation)
				shippingTotal = Math.Round(shippingTotal.Value, 2);

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
				taxCategoryId = _taxSettings.ShippingTaxClassId;

			shippingTotalTaxed = _taxService.GetShippingPrice(shippingTotal.Value, includingTax, customer, taxCategoryId, out taxRate);

			if (_shoppingCartSettings.RoundPricesDuringCalculation)
				shippingTotalTaxed = Math.Round(shippingTotalTaxed.Value, 2);

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
                shippingDiscountAmount = decimal.Zero;

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                shippingDiscountAmount = Math.Round(shippingDiscountAmount, 2);

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
						shippingTotal = decimal.Zero;

					if (_shoppingCartSettings.RoundPricesDuringCalculation)
						shippingTotal = Math.Round(shippingTotal.Value, 2);

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
						taxCategoryId = _taxSettings.ShippingTaxClassId;

					shippingTax = GetShippingTaxAmount(shippingTotal.Value, customer, taxCategoryId, taxRates);

					if (_shoppingCartSettings.RoundPricesDuringCalculation)
						shippingTax = Math.Round(shippingTax, 2);
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

					if (_shoppingCartSettings.RoundPricesDuringCalculation)
						paymentFee = Math.Round(paymentFee, 2);

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
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                taxTotal = Math.Round(taxTotal, 2);

            return taxTotal;
        }





        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="ignoreRewardPonts">A value indicating whether we should ignore reward points (if enabled and a customer is going to use them)</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>Shopping cart total;Null if shopping cart total couldn't be calculated now</returns>
		public virtual decimal? GetShoppingCartTotal(IList<OrganizedShoppingCartItem> cart,
            bool ignoreRewardPonts = false, bool usePaymentMethodAdditionalFee = true)
        {
            decimal discountAmount = decimal.Zero;
            Discount appliedDiscount = null;

            int redeemedRewardPoints = 0;
            decimal redeemedRewardPointsAmount = decimal.Zero;
            List<AppliedGiftCard> appliedGiftCards = null;

            return GetShoppingCartTotal(cart, out discountAmount, out appliedDiscount,
                out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount, ignoreRewardPonts, usePaymentMethodAdditionalFee);
        }

        /// <summary>
        /// Gets shopping cart total
        /// </summary>
        /// <param name="cart">Cart</param>
        /// <param name="appliedGiftCards">Applied gift cards</param>
        /// <param name="discountAmount">Applied discount amount</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <param name="redeemedRewardPoints">Reward points to redeem</param>
        /// <param name="redeemedRewardPointsAmount">Reward points amount in primary store currency to redeem</param>
        /// <param name="ignoreRewardPonts">A value indicating whether we should ignore reward points (if enabled and a customer is going to use them)</param>
        /// <param name="usePaymentMethodAdditionalFee">A value indicating whether we should use payment method additional fee when calculating order total</param>
        /// <returns>Shopping cart total;Null if shopping cart total couldn't be calculated now</returns>
		public virtual decimal? GetShoppingCartTotal(IList<OrganizedShoppingCartItem> cart,
            out decimal discountAmount, out Discount appliedDiscount,
            out List<AppliedGiftCard> appliedGiftCards,
            out int redeemedRewardPoints, out decimal redeemedRewardPointsAmount,
            bool ignoreRewardPonts = false, bool usePaymentMethodAdditionalFee = true)
        {
            redeemedRewardPoints = 0;
            redeemedRewardPointsAmount = decimal.Zero;

            var customer = cart.GetCustomer();
            string paymentMethodSystemName = "";
            if (customer != null)
			{
				paymentMethodSystemName = customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _genericAttributeService, _storeContext.CurrentStore.Id);
			}

            //subtotal without tax
            decimal subtotalBase = decimal.Zero;
            decimal orderSubTotalDiscountAmount = decimal.Zero;
            Discount orderSubTotalAppliedDiscount = null;
            decimal subTotalWithoutDiscountBase = decimal.Zero;
            decimal subTotalWithDiscountBase = decimal.Zero;

            GetShoppingCartSubTotal(cart, false, out orderSubTotalDiscountAmount, out orderSubTotalAppliedDiscount, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

            //subtotal with discount
            subtotalBase = subTotalWithDiscountBase;

            //shipping without tax
            decimal? shoppingCartShipping = GetShoppingCartShippingTotal(cart, false);

            //payment method additional fee without tax
            decimal paymentMethodAdditionalFeeWithoutTax = decimal.Zero;
            if (usePaymentMethodAdditionalFee && !String.IsNullOrEmpty(paymentMethodSystemName))
            {
				var provider = _providerManager.GetProvider<IPaymentMethod>(paymentMethodSystemName);
				var paymentMethodAdditionalFee = (provider != null ? provider.Value.GetAdditionalHandlingFee(cart) : decimal.Zero);

				if (_shoppingCartSettings.RoundPricesDuringCalculation)
				{
					paymentMethodAdditionalFee = Math.Round(paymentMethodAdditionalFee, 2);
				}

				paymentMethodAdditionalFeeWithoutTax = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, false, customer);
            }

            //tax
            decimal shoppingCartTax = GetTaxTotal(cart, usePaymentMethodAdditionalFee);

            //order total
            decimal resultTemp = decimal.Zero;
            resultTemp += subtotalBase;
            if (shoppingCartShipping.HasValue)
            {
                resultTemp += shoppingCartShipping.Value;
            }
            resultTemp += paymentMethodAdditionalFeeWithoutTax;

            ////// (VATFIX)
            ////resultTemp += shoppingCartTax;
            //if (_taxService.IsVatExempt(null, customer))
            //{
            //    // add nothing to total
            //}
            //else
            //{
                resultTemp += shoppingCartTax;
            //}

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = Math.Round(resultTemp, 2);

            #region Order total discount

            discountAmount = GetOrderTotalDiscount(customer, resultTemp, out appliedDiscount);

            //sub totals with discount        
            if (resultTemp < discountAmount)
                discountAmount = resultTemp;

            //reduce subtotal
            resultTemp -= discountAmount;

            if (resultTemp < decimal.Zero)
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = Math.Round(resultTemp, 2);

            #endregion

            #region Applied gift cards

            //let's apply gift cards now (gift cards that can be used)
            appliedGiftCards = new List<AppliedGiftCard>();
            if (!cart.IsRecurring())
            {
                //we don't apply gift cards for recurring products
                var giftCards = _giftCardService.GetActiveGiftCardsAppliedByCustomer(customer, _storeContext.CurrentStore.Id);
				if (giftCards != null)
				{
					foreach (var gc in giftCards)
					{
						if (resultTemp > decimal.Zero)
						{
							decimal remainingAmount = gc.GetGiftCardRemainingAmount();
							decimal amountCanBeUsed = decimal.Zero;
							if (resultTemp > remainingAmount)
								amountCanBeUsed = remainingAmount;
							else
								amountCanBeUsed = resultTemp;

							//reduce subtotal
							resultTemp -= amountCanBeUsed;

							var appliedGiftCard = new AppliedGiftCard();
							appliedGiftCard.GiftCard = gc;
							appliedGiftCard.AmountCanBeUsed = amountCanBeUsed;
							appliedGiftCards.Add(appliedGiftCard);
						}
					}
				}
            }

            #endregion

            #region Reward points

            if (_rewardPointsSettings.Enabled &&
                !ignoreRewardPonts && customer != null &&
                customer.GetAttribute<bool>(SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, _genericAttributeService, _storeContext.CurrentStore.Id))
            {
                int rewardPointsBalance = customer.GetRewardPointsBalance();
                decimal rewardPointsBalanceAmount = ConvertRewardPointsToAmount(rewardPointsBalance);

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
                resultTemp = decimal.Zero;
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                resultTemp = Math.Round(resultTemp, 2);



            decimal? orderTotal = null;
            if (!shoppingCartShipping.HasValue)
            {
                //return null if we have errors
                orderTotal = null;
                return orderTotal;
            }
            else
            {
                //return result if we have no errors
                orderTotal = resultTemp;
            }

            if (orderTotal.HasValue)
            {
                orderTotal = orderTotal.Value - redeemedRewardPointsAmount;
                if (_shoppingCartSettings.RoundPricesDuringCalculation)
                    orderTotal = Math.Round(orderTotal.Value, 2);
                return orderTotal;
            }
			return null;
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
					if (discount.DiscountType == DiscountType.AssignedToOrderTotal && !allowedDiscounts.Any(x => x.Id == discount.Id) &&_discountService.IsDiscountValid(discount, customer))
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

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                discountAmount = Math.Round(discountAmount, 2);

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
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                result = Math.Round(result, 2);
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
