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
		private readonly IProductService _productService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;

        public PriceCalculationService(IWorkContext workContext,
			IStoreContext storeContext,
            IDiscountService discountService,
			ICategoryService categoryService,
            IProductAttributeParser productAttributeParser,
			IProductService productService,
			ShoppingCartSettings shoppingCartSettings, 
            CatalogSettings catalogSettings)
        {
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._discountService = discountService;
            this._categoryService = categoryService;
            this._productAttributeParser = productAttributeParser;
			this._productService = productService;
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
            return GetFinalPrice(product, customer, additionalCharge, includeDiscounts, 1);
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
			ProductBundleItemData bundleItem = null)
        {
            //initial price
			decimal result = product.Price;

            //special price
			var specialPrice = GetSpecialPrice(product);
            if (specialPrice.HasValue)
                result = specialPrice.Value;

            //tier prices
			if (product.HasTierPrices && !bundleItem.IsValid())
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
		public virtual decimal GetFinalPrice(Product product, IList<ProductBundleItemData> bundleItems,
			Customer customer, decimal additionalCharge, bool includeDiscounts, int quantity, ProductBundleItemData bundleItem = null)
		{
			if (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing)
			{
				decimal result = decimal.Zero;
				var items = bundleItems ?? _productService.GetBundleItems(product.Id);

				foreach (var itemData in items.Where(x => x.IsValid()))
				{
					decimal itemPrice = GetFinalPrice(itemData.Item.Product, customer, itemData.AdditionalCharge, includeDiscounts, 1, itemData);

					result = result + decimal.Multiply(itemPrice, itemData.Item.Quantity);
				}

				return (result < decimal.Zero ? decimal.Zero : result);
			}
			return GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, bundleItem);
		}

		/// <summary>
		/// Get the lowest possible price for a product.
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="displayFromMessage">Whether to display the from message.</param>
		/// <returns>The lowest price.</returns>
		public virtual decimal GetLowestPrice(Product product, out bool displayFromMessage)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (product.ProductType == ProductType.GroupedProduct)
				throw Error.InvalidOperation("Choose the other override for products of type grouped product.");

			displayFromMessage = false;

			IList<ProductBundleItemData> bundleItems = null;
			bool isBundlePerItemPricing = (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing);

			if (isBundlePerItemPricing)
				bundleItems = _productService.GetBundleItems(product.Id);

			decimal lowestPrice = GetFinalPrice(product, bundleItems, _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);

			if (product.LowestAttributeCombinationPrice.HasValue && product.LowestAttributeCombinationPrice.Value < lowestPrice)
			{
				lowestPrice = product.LowestAttributeCombinationPrice.Value;
				displayFromMessage = true;
			}

			if (!displayFromMessage)
			{
				foreach (var attribute in product.ProductVariantAttributes)
				{
					if (attribute.ProductVariantAttributeValues.Any(x => x.PriceAdjustment != decimal.Zero))
					{
						displayFromMessage = true;
						break;
					}
				}
			}

			return lowestPrice;
		}

		/// <summary>
		/// Get the lowest price of a grouped product.
		/// </summary>
		/// <param name="product">Grouped product.</param>
		/// <param name="associatedProducts">Products associated to product.</param>
		/// <param name="lowestPriceProduct">The associated product with the lowest price.</param>
		/// <returns>The lowest price.</returns>
		public virtual decimal? GetLowestPrice(Product product, IEnumerable<Product> associatedProducts, out Product lowestPriceProduct)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (associatedProducts == null)
				throw new ArgumentNullException("associatedProducts");

			if (product.ProductType != ProductType.GroupedProduct)
				throw Error.InvalidOperation("Choose the other override for products not of type grouped product.");

			lowestPriceProduct = product;
			decimal? lowestPrice = null;

			foreach (var associatedProduct in associatedProducts)
			{
				var tmpPrice = GetFinalPrice(associatedProduct, _workContext.CurrentCustomer, decimal.Zero, true, int.MaxValue);

				if (associatedProduct.LowestAttributeCombinationPrice.HasValue && associatedProduct.LowestAttributeCombinationPrice.Value < tmpPrice)
				{
					tmpPrice = associatedProduct.LowestAttributeCombinationPrice.Value;
				}

				if (!lowestPrice.HasValue || tmpPrice < lowestPrice.Value)
				{
					lowestPrice = tmpPrice;
					lowestPriceProduct = associatedProduct;
				}
			}
			return lowestPrice;
		}

		/// <summary>
		/// Gets the product cost
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="attributesXml">Shopping cart item attributes in XML</param>
		/// <returns>Product cost</returns>
		public virtual decimal GetProductCost(Product product, string attributesXml)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			decimal result = product.ProductCost;

			_productAttributeParser
				.ParseProductVariantAttributeValues(attributesXml)
				.Where(x => x.ValueType == ProductVariantAttributeValueType.ProductLinkage)
				.Each(x =>
				{
					var linkedProduct = _productService.GetProductById(x.LinkedProductId);

					if (linkedProduct != null)
						result += (linkedProduct.ProductCost * x.Quantity);
				});

			return result;
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
        public virtual decimal GetDiscountAmount(
			Product product,
            Customer customer,
            decimal additionalCharge,
            int quantity,
            out Discount appliedDiscount,
			ProductBundleItemData bundleItem = null)
        {
            appliedDiscount = null;
            decimal appliedDiscountAmount = decimal.Zero;
			decimal finalPriceWithoutDiscount = decimal.Zero;

			if (bundleItem.IsValid())
			{
				if (bundleItem.Item.Discount.HasValue && bundleItem.Item.BundleProduct.BundlePerItemPricing)
				{
					appliedDiscount = new Discount()
					{
						UsePercentage = bundleItem.Item.DiscountPercentage,
						DiscountPercentage = bundleItem.Item.Discount.Value,
						DiscountAmount = bundleItem.Item.Discount.Value
					};

					finalPriceWithoutDiscount = GetFinalPrice(product, customer, additionalCharge, false, quantity, bundleItem);
					appliedDiscountAmount = appliedDiscount.GetDiscountAmount(finalPriceWithoutDiscount);
				}
			}
			else
			{
				// dont't apply when customer entered price or discounts should be ignored completely
				if (product.CustomerEntersPrice || _catalogSettings.IgnoreDiscounts)
				{
					return appliedDiscountAmount;
				}

				var allowedDiscounts = GetAllowedDiscounts(product, customer);
				if (allowedDiscounts.Count == 0)
				{
					return appliedDiscountAmount;
				}

				finalPriceWithoutDiscount = GetFinalPrice(product, customer, additionalCharge, false, quantity, bundleItem);
				appliedDiscount = allowedDiscounts.GetPreferredDiscount(finalPriceWithoutDiscount);

				if (appliedDiscount != null)
				{
					appliedDiscountAmount = appliedDiscount.GetDiscountAmount(finalPriceWithoutDiscount);
				}
			}

            return appliedDiscountAmount;
        }


        /// <summary>
        /// Gets the shopping cart item sub total
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart item sub total</returns>
        public virtual decimal GetSubTotal(OrganizedShoppingCartItem shoppingCartItem, bool includeDiscounts)
        {
            return GetUnitPrice(shoppingCartItem, includeDiscounts) * shoppingCartItem.Item.Quantity;
        }

        /// <summary>
        /// Gets the shopping cart unit price (one item)
        /// </summary>
        /// <param name="shoppingCartItem">The shopping cart item</param>
        /// <param name="includeDiscounts">A value indicating whether include discounts or not for price computation</param>
        /// <returns>Shopping cart unit price (one item)</returns>
		public virtual decimal GetUnitPrice(OrganizedShoppingCartItem shoppingCartItem, bool includeDiscounts)
        {
			decimal finalPrice = decimal.Zero;
            var customer = shoppingCartItem.Item.Customer;
			var product = shoppingCartItem.Item.Product;

            if (product != null)
            {
				if (product.CustomerEntersPrice)
                {
                    finalPrice = shoppingCartItem.Item.CustomerEnteredPrice;
                }
				else if (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing)
				{
					if (shoppingCartItem.ChildItems != null)
					{
						foreach (var bundleItem in shoppingCartItem.ChildItems)
						{
							bundleItem.Item.Product.MergeWithCombination(bundleItem.Item.AttributesXml);
						}

						var bundleItems = shoppingCartItem.ChildItems.Where(x => x.BundleItemData.IsValid()).Select(x => x.BundleItemData).ToList();

						finalPrice = GetFinalPrice(product, bundleItems, customer, decimal.Zero, includeDiscounts, shoppingCartItem.Item.Quantity);
					}
				}
                else
                {
					decimal attributesTotalPrice = decimal.Zero;
					var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(shoppingCartItem.Item.AttributesXml);

					if (pvaValues != null)
					{
						foreach (var pvaValue in pvaValues)
							attributesTotalPrice += GetProductVariantAttributeValuePriceAdjustment(pvaValue);
					}

					finalPrice = GetFinalPrice(product, customer, attributesTotalPrice, includeDiscounts, shoppingCartItem.Item.Quantity, shoppingCartItem.BundleItemData);
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
		public virtual decimal GetDiscountAmount(OrganizedShoppingCartItem shoppingCartItem)
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
		public virtual decimal GetDiscountAmount(OrganizedShoppingCartItem shoppingCartItem, out Discount appliedDiscount)
        {
            var customer = shoppingCartItem.Item.Customer;
            appliedDiscount = null;
			decimal totalDiscountAmount = decimal.Zero;
			var product = shoppingCartItem.Item.Product;
			if (product != null)
            {
                decimal attributesTotalPrice = decimal.Zero;

                var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(shoppingCartItem.Item.AttributesXml);
                foreach (var pvaValue in pvaValues)
                {
                    attributesTotalPrice += GetProductVariantAttributeValuePriceAdjustment(pvaValue);
                }

				decimal productDiscountAmount = GetDiscountAmount(product, customer, attributesTotalPrice, shoppingCartItem.Item.Quantity, out appliedDiscount);
				totalDiscountAmount = productDiscountAmount * shoppingCartItem.Item.Quantity;
            }
            
            if (_shoppingCartSettings.RoundPricesDuringCalculation)
				totalDiscountAmount = Math.Round(totalDiscountAmount, 2);
			return totalDiscountAmount;
        }


		/// <summary>
		/// Gets the price adjustment of a variant attribute value
		/// </summary>
		/// <param name="attributeValue">Product variant attribute value</param>
		/// <returns>Price adjustment of a variant attribute value</returns>
		public virtual decimal GetProductVariantAttributeValuePriceAdjustment(ProductVariantAttributeValue attributeValue)
		{
			if (attributeValue == null)
				throw new ArgumentNullException("attributeValue");

			if (attributeValue.ValueType == ProductVariantAttributeValueType.Simple)
				return attributeValue.PriceAdjustment;

			if (attributeValue.ValueType == ProductVariantAttributeValueType.ProductLinkage)
			{
				var linkedProduct = _productService.GetProductById(attributeValue.LinkedProductId);

				if (linkedProduct != null)
				{
					var productPrice = GetFinalPrice(linkedProduct, true) * attributeValue.Quantity;
					return productPrice;
				}
			}
			return decimal.Zero;
		}

        #endregion
    }
}
