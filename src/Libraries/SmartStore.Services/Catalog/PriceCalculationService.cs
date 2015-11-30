using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Discounts;
using SmartStore.Services.Media;
using SmartStore.Services.Tax;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Price calculation service
    /// </summary>
    public partial class PriceCalculationService : IPriceCalculationService
    {
        private readonly IDiscountService _discountService;
        private readonly ICategoryService _categoryService;
        private readonly IProductAttributeParser _productAttributeParser;
		private readonly IProductService _productService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
		private readonly IProductAttributeService _productAttributeService;
		private readonly IDownloadService _downloadService;
		private readonly ICommonServices _services;
		private readonly HttpRequestBase _httpRequestBase;
		private readonly ITaxService _taxService;

        public PriceCalculationService(
            IDiscountService discountService,
			ICategoryService categoryService,
            IProductAttributeParser productAttributeParser,
			IProductService productService,
			ShoppingCartSettings shoppingCartSettings, 
            CatalogSettings catalogSettings,
			IProductAttributeService productAttributeService,
			IDownloadService downloadService,
			ICommonServices services,
			HttpRequestBase httpRequestBase,
			ITaxService taxService)
        {
            this._discountService = discountService;
            this._categoryService = categoryService;
            this._productAttributeParser = productAttributeParser;
			this._productService = productService;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
			this._productAttributeService = productAttributeService;
			this._downloadService = downloadService;
			this._services = services;
			this._httpRequestBase = httpRequestBase;
			this._taxService = taxService;
        }
        
        #region Utilities

        /// <summary>
        /// Gets allowed discounts
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <returns>Discounts</returns>
        protected virtual IList<Discount> GetAllowedDiscounts(Product product, Customer customer, PriceCalculationContext context = null)
        {
            var result = new List<Discount>();
            if (_catalogSettings.IgnoreDiscounts)
                return result;

			if (product.HasDiscountsApplied)
            {
                //we use this property ("HasDiscountsApplied") for performance optimziation to avoid unnecessary database calls
				foreach (var discount in product.AppliedDiscounts)
                {
					if (discount.DiscountType == DiscountType.AssignedToSkus && !result.Any(x => x.Id == discount.Id) && _discountService.IsDiscountValid(discount, customer))
					{
						result.Add(discount);
					}
                }
            }

            //performance optimization. load all category discounts just to ensure that we have at least one
            if (_discountService.GetAllDiscounts(DiscountType.AssignedToCategories).Any())
            {
				IEnumerable<ProductCategory> productCategories = null;
				
				if (context == null)
					productCategories = _categoryService.GetProductCategoriesByProductId(product.Id);
				else
					productCategories = context.ProductCategories.Load(product.Id);

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
								if (discount.DiscountType == DiscountType.AssignedToCategories && !result.Any(x => x.Id == discount.Id) && _discountService.IsDiscountValid(discount, customer))
								{
									result.Add(discount);
								}
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a tier price
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="customer">Customer</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Price</returns>
		protected virtual decimal? GetMinimumTierPrice(Product product, Customer customer, int quantity, PriceCalculationContext context = null)
        {
			if (!product.HasTierPrices)
                return decimal.Zero;

			IEnumerable<TierPrice> tierPrices = null;

			if (context == null)
			{
				tierPrices = product.TierPrices
					.OrderBy(tp => tp.Quantity)
					.FilterByStore(_services.StoreContext.CurrentStore.Id)
					.FilterForCustomer(customer)
					.ToList()
					.RemoveDuplicatedQuantities();
			}
			else
			{
				tierPrices = context.TierPrices.Load(product.Id);
			}

			if (tierPrices == null)
				return decimal.Zero;

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

		protected virtual decimal GetPreselectedPrice(Product product, PriceCalculationContext context, ProductBundleItemData bundleItem, IEnumerable<ProductBundleItemData> bundleItems)
		{
			var taxRate = decimal.Zero;
			var attributesTotalPriceBase = decimal.Zero;
			var preSelectedPriceAdjustmentBase = decimal.Zero;
			var isBundle = (product.ProductType == ProductType.BundledProduct);
			var isBundleItemPricing = (bundleItem != null && bundleItem.Item.BundleProduct.BundlePerItemPricing);
			var isBundlePricing = (bundleItem != null && !bundleItem.Item.BundleProduct.BundlePerItemPricing);
			var bundleItemId = (bundleItem == null ? 0 : bundleItem.Item.Id);

			var selectedAttributes = new NameValueCollection();
			var selectedAttributeValues = new List<ProductVariantAttributeValue>();
			var attributes = context.Attributes.Load(product.Id);

			// 1. fill selectedAttributes with initially selected attributes
			foreach (var attribute in attributes.Where(x => x.ProductVariantAttributeValues.Count > 0 && x.ShouldHaveValues()))
			{
				int preSelectedValueId = 0;
				ProductVariantAttributeValue defaultValue = null;
				var selectedValueIds = new List<int>();
				var pvaValues = attribute.ProductVariantAttributeValues;
					
				foreach (var pvaValue in pvaValues)
				{
					ProductBundleItemAttributeFilter attributeFilter = null;

					if (bundleItem.FilterOut(pvaValue, out attributeFilter))
						continue;

					if (preSelectedValueId == 0 && attributeFilter != null && attributeFilter.IsPreSelected)
						preSelectedValueId = attributeFilter.AttributeValueId;

					if (!isBundlePricing && pvaValue.IsPreSelected)
					{
						decimal attributeValuePriceAdjustment = GetProductVariantAttributeValuePriceAdjustment(pvaValue);
						decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, out taxRate);

						preSelectedPriceAdjustmentBase = decimal.Add(preSelectedPriceAdjustmentBase, priceAdjustmentBase);
					}
				}

				// value pre-selected by a bundle item filter discards the default pre-selection
				if (preSelectedValueId != 0 && (defaultValue = pvaValues.FirstOrDefault(x => x.Id == preSelectedValueId)) != null)
				{
					//defaultValue.IsPreSelected = true;
					selectedAttributeValues.Add(defaultValue);
					selectedAttributes.AddProductAttribute(attribute.ProductAttributeId, attribute.Id, defaultValue.Id, product.Id, bundleItemId);
				}
				else
				{
					foreach (var value in pvaValues.Where(x => x.IsPreSelected))
					{
						selectedAttributeValues.Add(value);
						selectedAttributes.AddProductAttribute(attribute.ProductAttributeId, attribute.Id, value.Id, product.Id, bundleItemId);
					}
				}
			}

			// 2. find attribute combination for selected attributes and merge it
			if (!isBundle && selectedAttributes.Count > 0)
			{
				string attributeXml = selectedAttributes.CreateSelectedAttributesXml(product.Id, attributes, _productAttributeParser, _services.Localization,
					_downloadService, _catalogSettings, _httpRequestBase, new List<string>(), true, bundleItemId);

				var combinations = context.AttributeCombinations.Load(product.Id);

				var selectedCombination = combinations.FirstOrDefault(x => _productAttributeParser.AreProductAttributesEqual(x.AttributesXml, attributeXml, attributes));

				if (selectedCombination != null && selectedCombination.IsActive && selectedCombination.Price.HasValue)
				{
					product.MergedDataValues = new Dictionary<string, object> { { "Price", selectedCombination.Price.Value } };
				}
			}

			if (_catalogSettings.EnableDynamicPriceUpdate && !isBundlePricing)
			{
				if (selectedAttributeValues.Count > 0)
				{
					selectedAttributeValues.Each(x => attributesTotalPriceBase += GetProductVariantAttributeValuePriceAdjustment(x));
				}
				else
				{
					attributesTotalPriceBase = preSelectedPriceAdjustmentBase;
				}
			}

			if (bundleItem != null)
			{
				bundleItem.AdditionalCharge = attributesTotalPriceBase;
			}

			var result = GetFinalPrice(product, bundleItems, _services.WorkContext.CurrentCustomer, attributesTotalPriceBase, true, 1, bundleItem, context);
			return result;
		}

		public virtual PriceCalculationContext CreatePriceCalculationContext(IEnumerable<Product> products = null)
		{
			var context = new PriceCalculationContext(products,
				x => _productAttributeService.GetProductVariantAttributesByProductIds(x, null),
				x => _productAttributeService.GetProductVariantAttributeCombinations(x),
				x => _productService.GetTierPrices(x, _services.WorkContext.CurrentCustomer, _services.StoreContext.CurrentStore.Id),
				x => _categoryService.GetProductCategoriesByProductIds(x, true)
			);

			return context;
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
            var customer = _services.WorkContext.CurrentCustomer;
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

		public virtual decimal GetFinalPrice(
			Product product, 
            Customer customer,
            decimal additionalCharge, 
            bool includeDiscounts, 
            int quantity,
			ProductBundleItemData bundleItem = null,
			PriceCalculationContext context = null)
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
				decimal? tierPrice = GetMinimumTierPrice(product, customer, quantity, context);
				if (tierPrice.HasValue)
					result = Math.Min(result, tierPrice.Value);
            }

            //discount + additional charge
            if (includeDiscounts)
            {
                Discount appliedDiscount = null;
				decimal discountAmount = GetDiscountAmount(product, customer, additionalCharge, quantity, out appliedDiscount, bundleItem, context);
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

		public virtual decimal GetFinalPrice(
			Product product, 
			IEnumerable<ProductBundleItemData> bundleItems,
			Customer customer, 
			decimal additionalCharge, 
			bool includeDiscounts, 
			int quantity, 
			ProductBundleItemData bundleItem = null,
			PriceCalculationContext context = null)
		{
			if (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing)
			{
				decimal result = decimal.Zero;
				var items = bundleItems ?? _productService.GetBundleItems(product.Id);

				foreach (var itemData in items.Where(x => x.IsValid()))
				{
					decimal itemPrice = GetFinalPrice(itemData.Item.Product, customer, itemData.AdditionalCharge, includeDiscounts, 1, itemData, context);

					result = result + decimal.Multiply(itemPrice, itemData.Item.Quantity);
				}

				return (result < decimal.Zero ? decimal.Zero : result);
			}
			return GetFinalPrice(product, customer, additionalCharge, includeDiscounts, quantity, bundleItem, context);
		}

		/// <summary>
		/// Get the lowest possible price for a product.
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="context">Object with cargo data for better performance</param>
		/// <param name="displayFromMessage">Whether to display the from message.</param>
		/// <returns>The lowest price.</returns>
		public virtual decimal GetLowestPrice(Product product, PriceCalculationContext context, out bool displayFromMessage)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (product.ProductType == ProductType.GroupedProduct)
				throw Error.InvalidOperation("Choose the other override for products of type grouped product.");

			// note: attribute price adjustments were never regarded here cause of many reasons

			if (context == null)
				context = CreatePriceCalculationContext();

			bool isBundlePerItemPricing = (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing);

			displayFromMessage = isBundlePerItemPricing;

			var lowestPrice = GetFinalPrice(product, null, _services.WorkContext.CurrentCustomer, decimal.Zero, true, int.MaxValue, null, context);

			if (product.LowestAttributeCombinationPrice.HasValue && product.LowestAttributeCombinationPrice.Value < lowestPrice)
			{
				lowestPrice = product.LowestAttributeCombinationPrice.Value;
				displayFromMessage = true;
			}

			if (lowestPrice == decimal.Zero && product.Price == decimal.Zero)
			{
				lowestPrice = product.LowestAttributeCombinationPrice ?? decimal.Zero;
			}

			if (!displayFromMessage && product.ProductType != ProductType.BundledProduct)
			{
				var attributes = context.Attributes.Load(product.Id);
				displayFromMessage = attributes.Any(x => x.ProductVariantAttributeValues.Any(y => y.PriceAdjustment != decimal.Zero));
			}

			if (!displayFromMessage && product.HasTierPrices && !isBundlePerItemPricing)
			{
				var tierPrices = context.TierPrices.Load(product.Id);

				displayFromMessage = (tierPrices.Count > 0 && !(tierPrices.Count == 1 && tierPrices.First().Quantity <= 1));
			}

			return lowestPrice;
		}

		public virtual decimal? GetLowestPrice(Product product, PriceCalculationContext context, IEnumerable<Product> associatedProducts, out Product lowestPriceProduct)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			if (associatedProducts == null)
				throw new ArgumentNullException("associatedProducts");

			if (product.ProductType != ProductType.GroupedProduct)
				throw Error.InvalidOperation("Choose the other override for products not of type grouped product.");

			lowestPriceProduct = null;
			decimal? lowestPrice = null;
			var customer = _services.WorkContext.CurrentCustomer;

			if (context == null)
				context = CreatePriceCalculationContext();

			foreach (var associatedProduct in associatedProducts)
			{
				var tmpPrice = GetFinalPrice(associatedProduct, customer, decimal.Zero, true, int.MaxValue, null, context);

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

			if (lowestPriceProduct == null)
				lowestPriceProduct = associatedProducts.FirstOrDefault();

			return lowestPrice;
		}

		/// <summary>
		/// Get the initial price including preselected attributes
		/// </summary>
		/// <param name="product">Product</param>
		/// <param name="context">Object with cargo data for better performance</param>
		/// <returns>Preselected price</returns>
		public virtual decimal GetPreselectedPrice(Product product, PriceCalculationContext context)
		{
			if (product == null)
				throw new ArgumentNullException("product");

			var result = decimal.Zero;

			if (context == null)
				context = CreatePriceCalculationContext();

			if (product.ProductType == ProductType.BundledProduct)
			{
				var bundleItems = _productService.GetBundleItems(product.Id);

				var productIds = bundleItems.Select(x => x.Item.ProductId).ToList();
				productIds.Add(product.Id);

				context.Collect(productIds);

				foreach (var bundleItem in bundleItems.Where(x => x.Item.Product.CanBeBundleItem()))
				{
					// fetch bundleItems.AdditionalCharge for all bundle items
					var unused = GetPreselectedPrice(bundleItem.Item.Product, context, bundleItem, bundleItems);
				}

				result = GetPreselectedPrice(product, context, null, bundleItems);
			}
			else
			{
				result = GetPreselectedPrice(product, context, null, null);
			}

			return result;
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
            var customer = _services.WorkContext.CurrentCustomer;
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

        public virtual decimal GetDiscountAmount(
			Product product,
            Customer customer,
            decimal additionalCharge,
            int quantity,
            out Discount appliedDiscount,
			ProductBundleItemData bundleItem = null,
			PriceCalculationContext context = null)
        {
            appliedDiscount = null;
            decimal appliedDiscountAmount = decimal.Zero;
			decimal finalPriceWithoutDiscount = decimal.Zero;

			if (bundleItem.IsValid())
			{
				if (bundleItem.Item.Discount.HasValue && bundleItem.Item.BundleProduct.BundlePerItemPricing)
				{
					appliedDiscount = new Discount
					{
						UsePercentage = bundleItem.Item.DiscountPercentage,
						DiscountPercentage = bundleItem.Item.Discount.Value,
						DiscountAmount = bundleItem.Item.Discount.Value
					};

					finalPriceWithoutDiscount = GetFinalPrice(product, customer, additionalCharge, false, quantity, bundleItem, context);
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

				var allowedDiscounts = GetAllowedDiscounts(product, customer, context);
				if (allowedDiscounts.Count == 0)
				{
					return appliedDiscountAmount;
				}

				finalPriceWithoutDiscount = GetFinalPrice(product, customer, additionalCharge, false, quantity, bundleItem, context);
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
					product.MergeWithCombination(shoppingCartItem.Item.AttributesXml, _productAttributeParser);

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
