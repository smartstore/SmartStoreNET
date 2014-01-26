using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Discounts;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial class PriceCalculationService : IPriceCalculationService
    {
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IDiscountService _discountService;
        private readonly ICategoryService _categoryService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;

        public PriceCalculationService(IWorkContext workContext,
			IStoreContext storeContext,
            IDiscountService discountService,
			ICategoryService categoryService,
            IProductAttributeParser productAttributeParser,
			ShoppingCartSettings shoppingCartSettings, 
            CatalogSettings catalogSettings)
        {
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._discountService = discountService;
            this._categoryService = categoryService;
            this._productAttributeParser = productAttributeParser;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
        }
        
        #region Utilities

        /// <summary>
        /// Gets allowed discounts
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<Discount> GetAllowedDiscounts(Product product, 
            Customer customer)
        {
            var allowedDiscounts = new List<Discount>();
            if (_catalogSettings.IgnoreDiscounts)
                return allowedDiscounts;

			if (product.HasDiscountsApplied)
            {
                //we use this property ("HasDiscountsApplied") for performance optimziation to avoid unnecessary database calls
				foreach (var discount in product.AppliedDiscounts)
                {
                    if (_discountService.IsDiscountValid(discount, customer) &&
                        discount.DiscountType == DiscountType.AssignedToSkus &&
                        !allowedDiscounts.ContainsDiscount(discount))
                        allowedDiscounts.Add(discount);
                }
            }

            //performance optimization
            //load all category discounts just to ensure that we have at least one
            if (_discountService.GetAllDiscounts(DiscountType.AssignedToCategories).Any())
            {
				var productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
                if (productCategories != null)
                {
                    foreach (var productCategory in productCategories)
                    {
                        var category = productCategory.Category;

                        if (category.HasDiscountsApplied)
                        {
                            //we use this property ("HasDiscountsApplied") for performance optimziation to avoid unnecessary database calls
                            var categoryDiscounts = category.AppliedDiscounts;
                            foreach (var discount in categoryDiscounts)
                            {
                                if (_discountService.IsDiscountValid(discount, customer) &&
                                    discount.DiscountType == DiscountType.AssignedToCategories &&
                                    !allowedDiscounts.ContainsDiscount(discount))
                                    allowedDiscounts.Add(discount);
                            }
                        }
                    }
                }
            }
            return allowedDiscounts;
        }

        /// <summary>
        /// Gets a preferred discount
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="quantity">Product quantity</param>
        /// <returns>Preferred discount</returns>
        protected virtual Discount GetPreferredDiscount(Product product,
            Customer customer, decimal additionalCharge = decimal.Zero, int quantity = 1)
        {
            if (_catalogSettings.IgnoreDiscounts)
                return null;

			var allowedDiscounts = GetAllowedDiscounts(product, customer);
            decimal finalPriceWithoutDiscount = GetFinalPrice(product, customer, additionalCharge, false, quantity);
            var preferredDiscount = allowedDiscounts.GetPreferredDiscount(finalPriceWithoutDiscount);
            return preferredDiscount;
        }

        /// <summary>
        /// Gets a tier price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Price</returns>
        protected virtual decimal? GetMinimumTierPrice(Product product, Customer customer, int quantity)
        {
            if (!product.HasTierPrices)
                return decimal.Zero;

            var tierPrices = product.TierPrices
                .OrderBy(tp => tp.Quantity)
				.FilterByStore(_storeContext.CurrentStore.Id)
                .FilterForCustomer(customer)
                .ToList()
                .RemoveDuplicatedQuantities();

            int previousQty = 1;
            decimal? previousPrice = null;
            foreach (var tierPrice in tierPrices)
            {
                //check quantity
                if (quantity < tierPrice.Quantity)
                    continue;
                if (tierPrice.Quantity < previousQty)
                    continue;

                //save new price
                previousPrice = tierPrice.Price;
                previousQty = tierPrice.Quantity;
            }
            
            return previousPrice;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get product special price (is valid)
        /// </summary>
		/// <param name="product">Product</param>
        /// <returns>Product special price</returns>
		public virtual decimal? GetSpecialPrice(Product product)
        {
			if (product == null)
				throw new ArgumentNullException("product");

            if (!product.SpecialPrice.HasValue)
                return null;

            //check date range
            DateTime now = DateTime.UtcNow;
			if (product.SpecialPriceStartDateTimeUtc.HasValue)
            {
				DateTime startDate = DateTime.SpecifyKind(product.SpecialPriceStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (startDate.CompareTo(now) > 0)
                    return null;
            }
			if (product.SpecialPriceEndDateTimeUtc.HasValue)
            {
				DateTime endDate = DateTime.SpecifyKind(product.SpecialPriceEndDateTimeUtc.Value, DateTimeKind.Utc);
                if (endDate.CompareTo(now) < 0)
                    return null;
            }

			return product.SpecialPrice.Value;
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
		public virtual decimal GetFinalPrice(Product product, 
            bool includeDiscounts)
        {
            var customer = _workContext.CurrentCustomer;
			return GetFinalPrice(product, customer, includeDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
		public virtual decimal GetFinalPrice(Product product, 
            Customer customer, 
            bool includeDiscounts)
        {
			return GetFinalPrice(product, customer, decimal.Zero, includeDiscounts);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <returns>Final price</returns>
		public virtual decimal GetFinalPrice(Product product, 
            Customer customer, 
            decimal additionalCharge, 
            bool includeDiscounts)
        {
            return GetFinalPrice(product, customer, additionalCharge, 
                includeDiscounts, 1);
        }

        /// <summary>
        /// Gets the final price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
        /// <param name="quantity">Shopping cart item quantity</param>
		/// <param name="bundleItem">A product bundle item</param>
        /// <returns>Final price</returns>
		public virtual decimal GetFinalPrice(Product product, 
            Customer customer,
            decimal additionalCharge, 
            bool includeDiscounts, 
            int quantity,
			ProductBundleItem bundleItem = null)
        {
            //initial price
			decimal result = product.Price;

            //special price
			var specialPrice = GetSpecialPrice(product);
            if (specialPrice.HasValue)
                result = specialPrice.Value;

            //tier prices
			if (product.HasTierPrices && bundleItem == null)
            {
				decimal? tierPrice = GetMinimumTierPrice(product, customer, quantity);
                if (tierPrice.HasValue)
                    result = Math.Min(result, tierPrice.Value);
            }

            //discount + additional charge
            if (includeDiscounts)
            {
                Discount appliedDiscount = null;
				decimal discountAmount = GetDiscountAmount(product, customer, additionalCharge, quantity, out appliedDiscount, bundleItem);
                result = result + additionalCharge - discountAmount;
            }
            else
            {
                result = result + additionalCharge;
            }
            if (result < decimal.Zero)
                result = decimal.Zero;
            return result;
        }

		/// <summary>
		/// Gets the final price including bundle per-item pricing
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="bundleItems">Bundle items</param>
		/// <param name="customer">The customer</param>
		/// <param name="additionalCharge">Additional charge</param>
		/// <param name="includeDiscounts">A value indicating whether include discounts or not for final price computation</param>
		/// <param name="quantity">Shopping cart item quantity</param>
		/// <param name="bundleItem">A product bundle item</param>
		/// <returns>Final price</returns>
		public virtual decimal GetFinalPrice(Product product, IList<ProductBundleItem> bundleItems,
			Customer customer, decimal additionalCharge, bool includeDiscounts, int quantity, ProductBundleItem bundleItem = null)
		{
			if (!(product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing))
				return GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, bundleItem);

			decimal result = additionalCharge;

			if (bundleItems != null)
			{
				foreach (var item in bundleItems)
				{
					var bundleItemPrice = GetFinalPrice(item.Product, customer, decimal.Zero, includeDiscounts, 1, item);
					result = result + decimal.Multiply(bundleItemPrice, item.Quantity);
				}
			}

			return (result < decimal.Zero ? decimal.Zero : result);
		}


        /// <summary>
        /// Gets discount amount
        /// </summary>
		/// <param name="product">Product</param>
        /// <returns>Discount amount</returns>
		public virtual decimal GetDiscountAmount(Product product)
        {
            var customer = _workContext.CurrentCustomer;
            return GetDiscountAmount(product, customer, decimal.Zero);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(Product product, 
            Customer customer)
        {
            return GetDiscountAmount(product, customer, decimal.Zero);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <returns>Discount amount</returns>
		public virtual decimal GetDiscountAmount(Product product, 
            Customer customer, 
            decimal additionalCharge)
        {
            Discount appliedDiscount = null;
            return GetDiscountAmount(product, customer, additionalCharge, out appliedDiscount);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(Product product, 
            Customer customer,
            decimal additionalCharge, 
            out Discount appliedDiscount)
        {
            return GetDiscountAmount(product, customer, additionalCharge, 1, out appliedDiscount);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">The customer</param>
        /// <param name="additionalCharge">Additional charge</param>
        /// <param name="quantity">Product quantity</param>
        /// <param name="appliedDiscount">Applied discount</param>
		/// <param name="bundleItem">A product bundle item</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(Product product,
            Customer customer,
            decimal additionalCharge,
            int quantity,
            out Discount appliedDiscount,
			ProductBundleItem bundleItem = null)
        {
            appliedDiscount = null;
            decimal appliedDiscountAmount = decimal.Zero;

			if (bundleItem == null)
			{
				//we don't apply discounts to products with price entered by a customer
				if (product.CustomerEntersPrice)
					return appliedDiscountAmount;

				appliedDiscount = GetPreferredDiscount(product, customer, additionalCharge, quantity);
			}
			else if (bundleItem.Discount.HasValue && bundleItem.BundleProduct.BundlePerItemPricing)
			{
				appliedDiscount = new Discount()
				{
					UsePercentage = bundleItem.DiscountPercentage,
					DiscountPercentage = bundleItem.Discount.Value,
					DiscountAmount = bundleItem.Discount.Value
				};
			}

            if (appliedDiscount != null)
            {
                decimal finalPriceWithoutDiscount = GetFinalPrice(product, customer, additionalCharge, false, quantity);
                appliedDiscountAmount = appliedDiscount.GetDiscountAmount(finalPriceWithoutDiscount);
            }

            return appliedDiscountAmount;
        }


        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart item sub total</returns>
        public virtual decimal GetSubTotal(ShoppingCartItem shoppingCartItem, bool includeDiscounts)
        {
            return GetUnitPrice(shoppingCartItem, includeDiscounts) * shoppingCartItem.Quantity;
        }

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart unit price (one item)</returns>
        public virtual decimal GetUnitPrice(ShoppingCartItem shoppingCartItem, bool includeDiscounts)
        {
            var customer = shoppingCartItem.Customer;
            decimal finalPrice = decimal.Zero;
			var product = shoppingCartItem.Product;
            if (product != null)
            {
                decimal attributesTotalPrice = decimal.Zero;

                var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(shoppingCartItem.AttributesXml);
                if (pvaValues != null)
                {
                    foreach (var pvaValue in pvaValues)
                    {
                        attributesTotalPrice += pvaValue.PriceAdjustment;
                    }
                }

				if (product.CustomerEntersPrice)
                {
                    finalPrice = shoppingCartItem.CustomerEnteredPrice;
                }
                else
                {
					finalPrice = GetFinalPrice(product,
                        customer,
                        attributesTotalPrice,
                        includeDiscounts,
                        shoppingCartItem.Quantity);
                }
            }

            if (_shoppingCartSettings.RoundPricesDuringCalculation)
                finalPrice = Math.Round(finalPrice, 2);

            return finalPrice;
        }
        


        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(ShoppingCartItem shoppingCartItem)
        {
            Discount appliedDiscount;
            return GetDiscountAmount(shoppingCartItem, out appliedDiscount);
        }

        /// <summary>
        /// Gets discount amount
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="appliedDiscount">Applied discount</param>
        /// <returns>Discount amount</returns>
        public virtual decimal GetDiscountAmount(ShoppingCartItem shoppingCartItem, out Discount appliedDiscount)
        {
            var customer = shoppingCartItem.Customer;
            appliedDiscount = null;
			decimal totalDiscountAmount = decimal.Zero;
			var product = shoppingCartItem.Product;
			if (product != null)
            {
                decimal attributesTotalPrice = decimal.Zero;

                var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(shoppingCartItem.AttributesXml);
                foreach (var pvaValue in pvaValues)
                {
                    attributesTotalPrice += pvaValue.PriceAdjustment;
                }

				decimal productDiscountAmount = GetDiscountAmount(product, customer, attributesTotalPrice, shoppingCartItem.Quantity, out appliedDiscount);
				totalDiscountAmount = productDiscountAmount * shoppingCartItem.Quantity;
            }
            
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
				totalDiscountAmount = Math.Round(totalDiscountAmount, 2);
			return totalDiscountAmount;
        }
        
        #endregion
    }
}
