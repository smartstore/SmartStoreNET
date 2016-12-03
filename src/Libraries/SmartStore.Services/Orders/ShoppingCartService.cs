using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Events;
using SmartStore.Core.Localization;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Orders
{
	public partial class ShoppingCartService : IShoppingCartService
    {
        private readonly IRepository<ShoppingCartItem> _sciRepository;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly ICurrencyService _currencyService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeParser _productAttributeParser;
		private readonly IProductAttributeService _productAttributeService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICustomerService _customerService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly IPermissionService _permissionService;
        private readonly IAclService _aclService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly IGenericAttributeService _genericAttributeService;
		private readonly IDownloadService _downloadService;
		private readonly CatalogSettings _catalogSettings;

		public ShoppingCartService(
			IRepository<ShoppingCartItem> sciRepository,
			IWorkContext workContext, 
			IStoreContext storeContext, 
			ICurrencyService currencyService,
            IProductService productService, ILocalizationService localizationService,
            IProductAttributeParser productAttributeParser,
			IProductAttributeService productAttributeService,
            ICheckoutAttributeService checkoutAttributeService,
            ICheckoutAttributeParser checkoutAttributeParser,
            IPriceFormatter priceFormatter,
            ICustomerService customerService,
            ShoppingCartSettings shoppingCartSettings,
            IEventPublisher eventPublisher,
            IPermissionService permissionService, 
            IAclService aclService,
			IStoreMappingService storeMappingService,
			IGenericAttributeService genericAttributeService,
			IDownloadService downloadService,
			CatalogSettings catalogSettings)
        {
            this._sciRepository = sciRepository;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._currencyService = currencyService;
            this._productService = productService;
            this._localizationService = localizationService;
            this._productAttributeParser = productAttributeParser;
			this._productAttributeService = productAttributeService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._priceFormatter = priceFormatter;
            this._customerService = customerService;
            this._shoppingCartSettings = shoppingCartSettings;
            this._eventPublisher = eventPublisher;
            this._permissionService = permissionService;
            this._aclService = aclService;
			this._storeMappingService = storeMappingService;
			this._genericAttributeService = genericAttributeService;
			this._downloadService = downloadService;
			this._catalogSettings = catalogSettings;

			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		/// <summary>
		/// Delete shopping cart item
		/// </summary>
		/// <param name="shoppingCartItem">Shopping cart item</param>
		/// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
		/// <param name="ensureOnlyActiveCheckoutAttributes">A value indicating whether to ensure that only active checkout attributes are attached to the current customer</param>
		/// <param name="deleteChildCartItems">A value indicating whether to delete child cart items</param>
		public virtual void DeleteShoppingCartItem(ShoppingCartItem shoppingCartItem, bool resetCheckoutData = true, 
            bool ensureOnlyActiveCheckoutAttributes = false, bool deleteChildCartItems = true)
        {
            if (shoppingCartItem == null)
                throw new ArgumentNullException("shoppingCartItem");

            var customer = shoppingCartItem.Customer;
			var storeId = shoppingCartItem.StoreId;
			int cartItemId = shoppingCartItem.Id;

            //reset checkout data
            if (resetCheckoutData)
            {
				_customerService.ResetCheckoutData(shoppingCartItem.Customer, shoppingCartItem.StoreId);
            }

            //delete item
            _sciRepository.Delete(shoppingCartItem);

            //validate checkout attributes
            if (ensureOnlyActiveCheckoutAttributes && shoppingCartItem.ShoppingCartType == ShoppingCartType.ShoppingCart)
            {
				var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, storeId);

				var checkoutAttributesXml = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
				checkoutAttributesXml = _checkoutAttributeParser.EnsureOnlyActiveAttributes(checkoutAttributesXml, cart);
				_genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.CheckoutAttributes, checkoutAttributesXml);
            }

            //event notification
            _eventPublisher.EntityDeleted(shoppingCartItem);

			// delete child items
			if (deleteChildCartItems)
			{
				var childCartItems = _sciRepository.Table
					.Where(x => x.CustomerId == customer.Id && x.ParentItemId != null && x.ParentItemId.Value == cartItemId && x.Id != cartItemId)
					.ToList();

				foreach (var cartItem in childCartItems)
				{
					DeleteShoppingCartItem(cartItem, resetCheckoutData, ensureOnlyActiveCheckoutAttributes, false);
				}
			}
        }

		public virtual void DeleteShoppingCartItem(int shoppingCartItemId, bool resetCheckoutData = true,
			bool ensureOnlyActiveCheckoutAttributes = false, bool deleteChildCartItems = true)
		{
			if (shoppingCartItemId != 0)
			{
				var shoppingCartItem = _sciRepository.GetById(shoppingCartItemId);

				DeleteShoppingCartItem(shoppingCartItem, resetCheckoutData, ensureOnlyActiveCheckoutAttributes, deleteChildCartItems);
			}
		}

        /// <summary>
        /// Deletes expired shopping cart items
        /// </summary>
        /// <param name="olderThanUtc">Older than date and time</param>
        /// <returns>Number of deleted items</returns>
        public virtual int DeleteExpiredShoppingCartItems(DateTime olderThanUtc)
        {
            var query =
				from sci in _sciRepository.Table
				where sci.UpdatedOnUtc < olderThanUtc && sci.ParentItemId == null
				select sci;

            var cartItems = query.ToList();

			foreach (var parentItem in cartItems)
			{
				var childItems = _sciRepository.Table
					.Where(x => x.ParentItemId != null && x.ParentItemId.Value == parentItem.Id && x.Id != parentItem.Id).ToList();

				foreach (var childItem in childItems)
					_sciRepository.Delete(childItem);

				_sciRepository.Delete(parentItem);
			}

            return cartItems.Count;
        }
        
        /// <summary>
		/// Validates required products (products which require other variant to be added to the cart)
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
		/// <param name="product">Product</param>
		/// <param name="storeId">Store identifier</param>
		/// <param name="automaticallyAddRequiredProductsIfEnabled">Automatically add required products if enabled</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetRequiredProductWarnings(Customer customer,
			ShoppingCartType shoppingCartType, Product product,
			int storeId, bool automaticallyAddRequiredProductsIfEnabled)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var cart = customer.GetCartItems(shoppingCartType, storeId);
            var warnings = new List<string>();

            if (product.RequireOtherProducts)
            {
                var requiredProducts = new List<Product>();
                foreach (var id in product.ParseRequiredProductIds())
                {
					var rp = _productService.GetProductById(id);
                    if (rp != null)
                        requiredProducts.Add(rp);
                }
                
                foreach (var rp in requiredProducts)
                {
                    //ensure that product is in the cart
                    bool alreadyInTheCart = false;
                    foreach (var sci in cart)
                    {
                        if (sci.Item.ProductId == rp.Id)
                        {
                            alreadyInTheCart = true;
                            break;
                        }
                    }
                    //not in the cart
                    if (!alreadyInTheCart)
                    {
                        if (product.AutomaticallyAddRequiredProducts)
                        {
                            //add to cart (if possible)
                            if (automaticallyAddRequiredProductsIfEnabled)
                            {
                                //pass 'false' for 'automaticallyAddRequiredProducsIfEnabled' to prevent circular references
								var addToCartWarnings = AddToCart(customer, rp, shoppingCartType, storeId, "", decimal.Zero, 1, false, null);

                                if (addToCartWarnings.Count > 0)
                                {
                                    //a product wasn't atomatically added for some reasons

                                    //don't display specific errors from 'addToCartWarnings' variable
                                    //display only generic error
									warnings.Add(T("ShoppingCart.RequiredProductWarning", rp.GetLocalized(x => x.Name)));
                                }
                            }
                            else
                            {
								warnings.Add(T("ShoppingCart.RequiredProductWarning", rp.GetLocalized(x => x.Name)));
                            }
                        }
                        else
                        {
							warnings.Add(T("ShoppingCart.RequiredProductWarning", rp.GetLocalized(x => x.Name)));
                        }
                    }
                }
            }

            return warnings;
        }
        
        /// <summary>
		/// Validates a product for standard properties
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
		/// <param name="product">Product</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="customerEnteredPrice">Customer entered price</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetStandardWarnings(Customer customer, ShoppingCartType shoppingCartType,
            Product product, string selectedAttributes, decimal customerEnteredPrice, int quantity)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            if (product == null)
                throw new ArgumentNullException("product");

            var warnings = new List<string>();

            //deleted?
            if (product.Deleted)
            {
                warnings.Add(T("ShoppingCart.ProductDeleted"));
                return warnings;
            }

			// check if the product type is available for order
			if (product.ProductType == ProductType.GroupedProduct)
			{
				warnings.Add(T("ShoppingCart.ProductNotAvailableForOrder"));
			}

			// validate bundle
			if (product.ProductType == ProductType.BundledProduct)
			{
				if (product.BundlePerItemPricing && customerEnteredPrice != decimal.Zero)
					warnings.Add(T("ShoppingCart.Bundle.NoCustomerEnteredPrice"));
			}

            //published?
            if (!product.Published)
            {
                warnings.Add(T("ShoppingCart.ProductUnpublished"));
            }
            
            //ACL
            if (!_aclService.Authorize(product, customer))
            {
                warnings.Add(T("ShoppingCart.ProductUnpublished"));
            }

			//Store mapping
			if (!_storeMappingService.Authorize(product, _storeContext.CurrentStore.Id))
			{
				warnings.Add(T("ShoppingCart.ProductUnpublished"));
			}

            //disabled "add to cart" button
            if (shoppingCartType == ShoppingCartType.ShoppingCart && product.DisableBuyButton)
            {
                warnings.Add(T("ShoppingCart.BuyingDisabled"));
            }

            //disabled "add to wishlist" button
            if (shoppingCartType == ShoppingCartType.Wishlist && product.DisableWishlistButton)
            {
                warnings.Add(T("ShoppingCart.WishlistDisabled"));
            }

            //call for price
            if (shoppingCartType == ShoppingCartType.ShoppingCart && product.CallForPrice)
            {
                warnings.Add(T("Products.CallForPrice"));
            }

            //customer entered price
            if (product.CustomerEntersPrice)
            {
                if (customerEnteredPrice < product.MinimumCustomerEnteredPrice ||
                    customerEnteredPrice > product.MaximumCustomerEnteredPrice)
                {
                    var minimumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MinimumCustomerEnteredPrice, _workContext.WorkingCurrency);
                    var maximumCustomerEnteredPrice = _currencyService.ConvertFromPrimaryStoreCurrency(product.MaximumCustomerEnteredPrice, _workContext.WorkingCurrency);

                    warnings.Add(T("ShoppingCart.CustomerEnteredPrice.RangeError",
						_priceFormatter.FormatPrice(minimumCustomerEnteredPrice, true, false),
                        _priceFormatter.FormatPrice(maximumCustomerEnteredPrice, true, false))
					);
                }
            }

            //quantity validation
            var hasQtyWarnings = false;
            if (quantity < product.OrderMinimumQuantity)
            {
                warnings.Add(T("ShoppingCart.MinimumQuantity", product.OrderMinimumQuantity));
                hasQtyWarnings = true;
            }

            if (quantity > product.OrderMaximumQuantity)
            {
                warnings.Add(T("ShoppingCart.MaximumQuantity", product.OrderMaximumQuantity));
                hasQtyWarnings = true;
            }

            var allowedQuantities = product.ParseAllowedQuatities();
            if (allowedQuantities.Length > 0 && !allowedQuantities.Contains(quantity))
            {
                warnings.Add(T("ShoppingCart.AllowedQuantities", string.Join(", ", allowedQuantities)));
            }

            var validateOutOfStock = shoppingCartType == ShoppingCartType.ShoppingCart || !_shoppingCartSettings.AllowOutOfStockItemsToBeAddedToWishlist;
            if (validateOutOfStock && !hasQtyWarnings)
            {
                switch (product.ManageInventoryMethod)
                {
                    case ManageInventoryMethod.DontManageStock:
                        {
                        }
                        break;
                    case ManageInventoryMethod.ManageStock:
                        {
                            if ((BackorderMode)product.BackorderMode == BackorderMode.NoBackorders)
                            {
                                if (product.StockQuantity < quantity)
                                {
                                    var maximumQuantityCanBeAdded = product.StockQuantity;

                                    if (maximumQuantityCanBeAdded <= 0)
                                        warnings.Add(T("ShoppingCart.OutOfStock"));
                                    else
                                        warnings.Add(T("ShoppingCart.QuantityExceedsStock", maximumQuantityCanBeAdded));
                                }
                            }
                        }
                        break;
                    case ManageInventoryMethod.ManageStockByAttributes:
                        {
                            var combination = _productAttributeParser.FindProductVariantAttributeCombination(product.Id, selectedAttributes);

							if (combination != null)
                            {
								if (!combination.AllowOutOfStockOrders && combination.StockQuantity < quantity)
								{
									int maximumQuantityCanBeAdded = combination.StockQuantity;

									if (maximumQuantityCanBeAdded <= 0)
										warnings.Add(T("ShoppingCart.OutOfStock"));
									else
										warnings.Add(T("ShoppingCart.QuantityExceedsStock", maximumQuantityCanBeAdded));
								}
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //availability dates
            var availableStartDateError = false;
            if (product.AvailableStartDateTimeUtc.HasValue)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableStartDateTime = DateTime.SpecifyKind(product.AvailableStartDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableStartDateTime.CompareTo(now) > 0)
                {
                    warnings.Add(T("ShoppingCart.NotAvailable"));
                    availableStartDateError = true;
                }
            }
            if (product.AvailableEndDateTimeUtc.HasValue && !availableStartDateError)
            {
                DateTime now = DateTime.UtcNow;
                DateTime availableEndDateTime = DateTime.SpecifyKind(product.AvailableEndDateTimeUtc.Value, DateTimeKind.Utc);
                if (availableEndDateTime.CompareTo(now) < 0)
                {
                    warnings.Add(T("ShoppingCart.NotAvailable"));
                }
            }
            return warnings;
        }

		public virtual IList<string> GetShoppingCartItemAttributeWarnings(
			Customer customer, 
			ShoppingCartType shoppingCartType,
			Product product, 
			string selectedAttributes, 
			int quantity = 1, 
			ProductBundleItem bundleItem = null,
			ProductVariantAttributeCombination combination = null)
        {
			Guard.NotNull(product, nameof(product));

            var warnings = new List<string>();

			if (product.ProductType == ProductType.BundledProduct)
				return warnings;	// customer cannot select anything cause bundles have no attributes

			if (bundleItem != null && !bundleItem.BundleProduct.BundlePerItemPricing)
				return warnings;	// customer cannot select anything... selectedAttribute is always empty

            //selected attributes
            var pva1Collection = _productAttributeParser.ParseProductVariantAttributes(selectedAttributes);
            foreach (var pva1 in pva1Collection)
            {
                var pv1 = pva1.Product;

				if (pv1 == null || pv1.Id != product.Id)
				{
					warnings.Add(T("ShoppingCart.AttributeError"));
					return warnings;
				}
            }

            //existing product attributes
            var pva2Collection = product.ProductVariantAttributes;
            foreach (var pva2 in pva2Collection)
            {
                if (pva2.IsRequired)
                {
                    bool found = false;
                    //selected product attributes
                    foreach (var pva1 in pva1Collection)
                    {
                        if (pva1.Id == pva2.Id)
                        {
                            var pvaValuesStr = _productAttributeParser.ParseValues(selectedAttributes, pva1.Id);
                            foreach (string str1 in pvaValuesStr)
                            {
                                if (!String.IsNullOrEmpty(str1.Trim()))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

					if (!found && bundleItem != null && bundleItem.FilterAttributes && !bundleItem.AttributeFilters.Any(x => x.AttributeId == pva2.ProductAttributeId))
					{
						found = true;	// attribute is filtered out on bundle item level... it cannot be selected by customer
					}

                    if (!found)
                    {
						warnings.Add(T("ShoppingCart.SelectAttribute", pva2.TextPrompt.HasValue() ? pva2.TextPrompt : pva2.ProductAttribute.GetLocalized(a => a.Name)));
                    }
                }
            }

			// check if there is a selected attribute combination and if it is active
			if (warnings.Count == 0 && selectedAttributes.HasValue())
			{
				if (combination == null)
				{
					combination = _productAttributeParser.FindProductVariantAttributeCombination(product.Id, selectedAttributes);
				}

				if (combination != null && !combination.IsActive)
				{
					warnings.Add(T("ShoppingCart.NotAvailable"));
				}
			}

			if (warnings.Count == 0)
			{
				var pvaValues = _productAttributeParser.ParseProductVariantAttributeValues(selectedAttributes).ToList();
				foreach (var pvaValue in pvaValues)
				{
					if (pvaValue.ValueType ==  ProductVariantAttributeValueType.ProductLinkage)
					{
						var linkedProduct = _productService.GetProductById(pvaValue.LinkedProductId);
						if (linkedProduct != null)
						{
							var linkageWarnings = GetShoppingCartItemWarnings(customer, shoppingCartType, linkedProduct, _storeContext.CurrentStore.Id,
								"", decimal.Zero, quantity * pvaValue.Quantity, false, true, true, true, true);

							foreach (var linkageWarning in linkageWarnings)
							{
								warnings.Add(T("ShoppingCart.ProductLinkageAttributeWarning",
									pvaValue.ProductVariantAttribute.ProductAttribute.GetLocalized(a => a.Name),
									pvaValue.GetLocalized(a => a.Name),
									linkageWarning)
								);
							}
						}
						else
						{
							warnings.Add(T("ShoppingCart.ProductLinkageProductNotLoading", pvaValue.LinkedProductId));
						}
					}
				}
			}

            return warnings;
        }

        /// <summary>
        /// Validates if all required attributes are selected
        /// </summary>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="product">Product</param>
        /// <returns>bool</returns>
        public virtual bool AreAllAttributesForCombinationSelected(string selectedAttributes, Product product) 
        {
			Guard.NotNull(product, nameof(product));

			var hasAttributeCombinations = _sciRepository.Context.QueryForCollection(product, (Product p) => p.ProductVariantAttributeCombinations).Any();
			if (!hasAttributeCombinations)
				return true;

            //selected attributes
            var pva1Collection = _productAttributeParser.ParseProductVariantAttributes(selectedAttributes);

            //existing product attributes
            var pva2Collection = product.ProductVariantAttributes;
            foreach (var pva2 in pva2Collection)
            {
                if (pva2.IsRequired)
                {
                    bool found = false;
                    //selected product attributes
                    foreach (var pva1 in pva1Collection)
                    {
                        if (pva1.Id == pva2.Id)
                        {
                            var pvaValuesStr = _productAttributeParser.ParseValues(selectedAttributes, pva1.Id);
                            foreach (string str1 in pvaValuesStr)
                            {
                                if (!String.IsNullOrEmpty(str1.Trim()))
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!found)
                    {
                        return found;
                    }
                }
                else
                {
                    return true;
                }
            }

            return true;
        }

        /// <summary>
        /// Validates shopping cart item (gift card)
        /// </summary>
        /// <param name="shoppingCartType">Shopping cart type</param>
		/// <param name="product">Product</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetShoppingCartItemGiftCardWarnings(ShoppingCartType shoppingCartType,
            Product product, string selectedAttributes)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var warnings = new List<string>();

            //gift cards
            if (product.IsGiftCard)
            {
                string giftCardRecipientName = string.Empty;
                string giftCardRecipientEmail = string.Empty;
                string giftCardSenderName = string.Empty;
                string giftCardSenderEmail = string.Empty;
                string giftCardMessage = string.Empty;

                _productAttributeParser.GetGiftCardAttribute(selectedAttributes,
                    out giftCardRecipientName, out giftCardRecipientEmail, out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

				if (String.IsNullOrEmpty(giftCardRecipientName))
				{
					warnings.Add(T("ShoppingCart.RecipientNameError"));
				}

                if (product.GiftCardType == GiftCardType.Virtual)
                {
					//validate for virtual gift cards only
					if (String.IsNullOrEmpty(giftCardRecipientEmail) || !giftCardRecipientEmail.IsEmail())
					{
						warnings.Add(T("ShoppingCart.RecipientEmailError"));
					}
                }

				if (String.IsNullOrEmpty(giftCardSenderName))
				{
					warnings.Add(T("ShoppingCart.SenderNameError"));
				}

                if (product.GiftCardType == GiftCardType.Virtual)
                {
					//validate for virtual gift cards only
					if (String.IsNullOrEmpty(giftCardSenderEmail) || !giftCardSenderEmail.IsEmail())
					{
						warnings.Add(T("ShoppingCart.SenderEmailError"));
					}
                }
            }

            return warnings;
        }

		/// <summary>
		/// Validates bundle items
		/// </summary>
		/// <param name="shoppingCartType">Shopping cart type</param>
		/// <param name="bundleItem">Product bundle item</param>
		/// <returns>Warnings</returns>
		public virtual IList<string> GetBundleItemWarnings(ProductBundleItem bundleItem)
		{
			var warnings = new List<string>();

			if (bundleItem != null)
			{
				var name = bundleItem.GetLocalizedName();

				if (!bundleItem.Published)
				{
					warnings.Add(T("ShoppingCart.Bundle.BundleItemUnpublished", name));
				}

				if (bundleItem.ProductId == 0 || bundleItem.BundleProductId == 0 || bundleItem.Product == null || bundleItem.BundleProduct == null)
				{
					warnings.Add(T("ShoppingCart.Bundle.MissingProduct", name));
				}

				if (bundleItem.Quantity <= 0)
				{
					warnings.Add(T("ShoppingCart.Bundle.Quantity", name));
				}

				if (bundleItem.Product.IsDownload || bundleItem.Product.IsRecurring)
				{
					warnings.Add(T("ShoppingCart.Bundle.ProductResrictions", name));
				}
			}

			return warnings;
		}
		public virtual IList<string> GetBundleItemWarnings(IList<OrganizedShoppingCartItem> cartItems)
		{
			var warnings = new List<string>();

			if (cartItems != null)
			{
				foreach (var sci in cartItems.Where(x => x.Item.BundleItem != null))
				{
					warnings.AddRange(GetBundleItemWarnings(sci.Item.BundleItem));
				}
			}
			return warnings;
		}
        
        /// <summary>
        /// Validates shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
		/// <param name="product">Product</param>
		/// <param name="storeId">Store identifier</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="customerEnteredPrice">Customer entered price</param>
        /// <param name="quantity">Quantity</param>
		/// <param name="automaticallyAddRequiredProductsIfEnabled">Automatically add required products if enabled</param>
        /// <param name="getStandardWarnings">A value indicating whether we should validate a product for standard properties</param>
        /// <param name="getAttributesWarnings">A value indicating whether we should validate product attributes</param>
        /// <param name="getGiftCardWarnings">A value indicating whether we should validate gift card properties</param>
        /// <param name="getRequiredProductWarnings">A value indicating whether we should validate required products (products which require other products to be added to the cart)</param>
		/// <param name="getBundleWarnings">A value indicating whether we should validate bundle and bundle items</param>
		/// <param name="bundleItem">Product bundle item if bundles should be validated</param>
		/// <param name="childItems">Child cart items to validate bundle items</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> GetShoppingCartItemWarnings(Customer customer, ShoppingCartType shoppingCartType,
			Product product, int storeId, 
			string selectedAttributes, decimal customerEnteredPrice,
			int quantity, bool automaticallyAddRequiredProductsIfEnabled,
            bool getStandardWarnings = true, bool getAttributesWarnings = true, 
            bool getGiftCardWarnings = true, bool getRequiredProductWarnings = true,
			bool getBundleWarnings = true, ProductBundleItem bundleItem = null, IList<OrganizedShoppingCartItem> childItems = null)
        {
            if (product == null)
                throw new ArgumentNullException("product");

            var warnings = new List<string>();
            
            //standard properties
            if (getStandardWarnings)
                warnings.AddRange(GetStandardWarnings(customer, shoppingCartType, product, selectedAttributes, customerEnteredPrice, quantity));

            //selected attributes
            if (getAttributesWarnings)
                warnings.AddRange(GetShoppingCartItemAttributeWarnings(customer, shoppingCartType, product, selectedAttributes, quantity, bundleItem));

            //gift cards
            if (getGiftCardWarnings)
                warnings.AddRange(GetShoppingCartItemGiftCardWarnings(shoppingCartType, product, selectedAttributes));

            //required products
            if (getRequiredProductWarnings)
				warnings.AddRange(GetRequiredProductWarnings(customer, shoppingCartType, product, storeId, automaticallyAddRequiredProductsIfEnabled));

			// bundle and bundle item warnings
			if (getBundleWarnings)
			{
				if (bundleItem != null)
					warnings.AddRange(GetBundleItemWarnings(bundleItem));

				if (childItems != null)
					warnings.AddRange(GetBundleItemWarnings(childItems));
			}
            
            return warnings;
        }

        /// <summary>
        /// Validates whether this shopping cart is valid
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <param name="checkoutAttributes">Checkout attributes</param>
        /// <param name="validateCheckoutAttributes">A value indicating whether to validate checkout attributes</param>
        /// <returns>Warnings</returns>
		public virtual IList<string> GetShoppingCartWarnings(IList<OrganizedShoppingCartItem> shoppingCart, 
            string checkoutAttributes, bool validateCheckoutAttributes)
        {
            var warnings = new List<string>();

            bool hasStandartProducts = false;
            bool hasRecurringProducts = false;

            foreach (var sci in shoppingCart)
            {
                var product = sci.Item.Product;
                if (product == null)
                {
                    warnings.Add(T("ShoppingCart.CannotLoadProduct", sci.Item.ProductId));
                    return warnings;
                }

                if (product.IsRecurring)
                    hasRecurringProducts = true;
                else
                    hasStandartProducts = true;
            }

			//don't mix standard and recurring products
			if (hasStandartProducts && hasRecurringProducts)
			{
				warnings.Add(T("ShoppingCart.CannotMixStandardAndAutoshipProducts"));
			}

            //recurring cart validation
            if (hasRecurringProducts)
            {
                var cycleLength = 0;
                var cyclePeriod =  RecurringProductCyclePeriod.Days;
                var totalCycles = 0;
                var cyclesError = shoppingCart.GetRecurringCycleInfo(_localizationService, out cycleLength, out cyclePeriod, out totalCycles);

                if (!string.IsNullOrEmpty(cyclesError))
                {
                    warnings.Add(cyclesError);
                    return warnings;
                }
            }

            //validate checkout attributes
            if (validateCheckoutAttributes)
            {
                //selected attributes
                var ca1Collection = _checkoutAttributeParser.ParseCheckoutAttributes(checkoutAttributes);

                //existing checkout attributes
                var ca2Collection = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id);
                if (!shoppingCart.RequiresShipping())
                {
                    //remove attributes which require shippable products
                    ca2Collection = ca2Collection.RemoveShippableAttributes();
                }

                foreach (var ca2 in ca2Collection)
                {
                    if (ca2.IsRequired)
                    {
                        bool found = false;
                        //selected checkout attributes
                        foreach (var ca1 in ca1Collection)
                        {
                            if (ca1.Id == ca2.Id)
                            {
                                var caValuesStr = _checkoutAttributeParser.ParseValues(checkoutAttributes, ca1.Id);
								foreach (string str1 in caValuesStr)
								{
									if (!String.IsNullOrEmpty(str1.Trim()))
									{
										found = true;
										break;
									}
								}
                            }
                        }

                        //if not found
                        if (!found)
                        {
                            if (!string.IsNullOrEmpty(ca2.GetLocalized(a => a.TextPrompt)))
                                warnings.Add(ca2.GetLocalized(a => a.TextPrompt));
                            else
                                warnings.Add(T("ShoppingCart.SelectAttribute", ca2.GetLocalized(a => a.Name)));
                        }
                    }
                }
            }

            return warnings;
        }

        /// <summary>
        /// Finds a shopping cart item in the cart
        /// </summary>
        /// <param name="shoppingCart">Shopping cart</param>
        /// <param name="shoppingCartType">Shopping cart type</param>
		/// <param name="product">Product</param>
        /// <param name="selectedAttributes">Selected attributes</param>
        /// <param name="customerEnteredPrice">Price entered by a customer</param>
        /// <returns>Found shopping cart item</returns>
		public virtual OrganizedShoppingCartItem FindShoppingCartItemInTheCart(IList<OrganizedShoppingCartItem> shoppingCart,
            ShoppingCartType shoppingCartType,
            Product product,
            string selectedAttributes = "",
            decimal customerEnteredPrice = decimal.Zero)
        {
            if (shoppingCart == null)
                throw new ArgumentNullException("shoppingCart");

            if (product == null)
                throw new ArgumentNullException("product");

			if (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing)
				return null;		// too complex

            foreach (var sci in shoppingCart.Where(a => a.Item.ShoppingCartType == shoppingCartType && a.Item.ParentItemId == null))
            {
                if (sci.Item.ProductId == product.Id && sci.Item.Product.ProductTypeId == product.ProductTypeId)
                {
                    //attributes
                    bool attributesEqual = _productAttributeParser.AreProductAttributesEqual(sci.Item.AttributesXml, selectedAttributes);

                    //gift cards
                    var giftCardInfoSame = true;
                    if (sci.Item.Product.IsGiftCard)
                    {
                        string giftCardRecipientName1 = string.Empty;
                        string giftCardRecipientEmail1 = string.Empty;
                        string giftCardSenderName1 = string.Empty;
                        string giftCardSenderEmail1 = string.Empty;
                        string giftCardMessage1 = string.Empty;

                        _productAttributeParser.GetGiftCardAttribute(selectedAttributes,
                            out giftCardRecipientName1, out giftCardRecipientEmail1, out giftCardSenderName1, out giftCardSenderEmail1, out giftCardMessage1);

                        string giftCardRecipientName2 = string.Empty;
                        string giftCardRecipientEmail2 = string.Empty;
                        string giftCardSenderName2 = string.Empty;
                        string giftCardSenderEmail2 = string.Empty;
                        string giftCardMessage2 = string.Empty;

                        _productAttributeParser.GetGiftCardAttribute(sci.Item.AttributesXml,
                            out giftCardRecipientName2, out giftCardRecipientEmail2, out giftCardSenderName2, out giftCardSenderEmail2, out giftCardMessage2);


						if (giftCardRecipientName1.ToLowerInvariant() != giftCardRecipientName2.ToLowerInvariant() ||
							giftCardSenderName1.ToLowerInvariant() != giftCardSenderName2.ToLowerInvariant())
						{
							giftCardInfoSame = false;
						}
                    }

                    //price is the same (for products which require customers to enter a price)
                    var customerEnteredPricesEqual = true;
					if (sci.Item.Product.CustomerEntersPrice)
					{
						customerEnteredPricesEqual = Math.Round(sci.Item.CustomerEnteredPrice, 2) == Math.Round(customerEnteredPrice, 2);
					}

					//found?
					if (attributesEqual && giftCardInfoSame && customerEnteredPricesEqual)
					{
						return sci;
					}
                }
            }

            return null;
        }

		/// <summary>
		/// Add product to cart
		/// </summary>
		/// <param name="customer">The customer</param>
		/// <param name="product">The product</param>
		/// <param name="cartType">Cart type</param>
		/// <param name="storeId">Store identifier</param>
		/// <param name="selectedAttributes">Selected attributes</param>
		/// <param name="customerEnteredPrice">Price entered by customer</param>
		/// <param name="quantity">Quantity</param>
		/// <param name="automaticallyAddRequiredProductsIfEnabled">Whether to add required products</param>
		/// <param name="ctx">Add to cart context</param>
		/// <returns>List with warnings</returns>
		public virtual List<string> AddToCart(Customer customer, Product product, ShoppingCartType cartType, int storeId, string selectedAttributes,
			decimal customerEnteredPrice, int quantity, bool automaticallyAddRequiredProductsIfEnabled,	AddToCartContext ctx = null)
		{
			if (customer == null)
				throw new ArgumentNullException("customer");

			if (product == null)
				throw new ArgumentNullException("product");

			var warnings = new List<string>();
			var bundleItem = (ctx == null ? null : ctx.BundleItem);

			if (ctx != null && bundleItem != null && ctx.Warnings.Count > 0)
				return ctx.Warnings;	// warnings while adding bundle items to cart -> no need for further processing

			if (cartType == ShoppingCartType.ShoppingCart && !_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart, customer))
			{
				warnings.Add(T("ShoppingCart.IsDisabled"));
				return warnings;
			}
			if (cartType == ShoppingCartType.Wishlist && !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist, customer))
			{
				warnings.Add(T("Wishlist.IsDisabled"));
				return warnings;
			}

			if (quantity <= 0)
			{
				warnings.Add(T("ShoppingCart.QuantityShouldPositive"));
				return warnings;
			}

			//if (parentItemId.HasValue && (parentItemId.Value == 0 || bundleItem == null || bundleItem.Id == 0))
			//{
			//	warnings.Add(T("ShoppingCart.Bundle.BundleItemNotFound", bundleItem.GetLocalizedName()));
			//	return warnings;
			//}

			//reset checkout info
			_customerService.ResetCheckoutData(customer, storeId);

			var cart = customer.GetCartItems(cartType, storeId);
			OrganizedShoppingCartItem existingCartItem = null;

			if (bundleItem == null)
			{
				existingCartItem = FindShoppingCartItemInTheCart(cart, cartType, product, selectedAttributes, customerEnteredPrice);
			}

			if (existingCartItem != null)
			{
				//update existing shopping cart item
				int newQuantity = existingCartItem.Item.Quantity + quantity;

				warnings.AddRange(
					GetShoppingCartItemWarnings(customer, cartType, product, storeId, selectedAttributes, customerEnteredPrice, newQuantity,
						automaticallyAddRequiredProductsIfEnabled, bundleItem: bundleItem)
				);

				if (warnings.Count == 0)
				{
					existingCartItem.Item.AttributesXml = selectedAttributes;
					existingCartItem.Item.Quantity = newQuantity;
					existingCartItem.Item.UpdatedOnUtc = DateTime.UtcNow;
					_customerService.UpdateCustomer(customer);

					//event notification
					_eventPublisher.EntityUpdated(existingCartItem.Item);
				}
			}
			else
			{
				//new shopping cart item
				warnings.AddRange(
					GetShoppingCartItemWarnings(customer, cartType, product, storeId, selectedAttributes, customerEnteredPrice, quantity,
						automaticallyAddRequiredProductsIfEnabled, bundleItem: bundleItem)
				);

				if (warnings.Count == 0)
				{
					//maximum items validation
					if (cartType == ShoppingCartType.ShoppingCart && cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
					{
						warnings.Add(T("ShoppingCart.MaximumShoppingCartItems"));
						return warnings;
					}
					else if (cartType == ShoppingCartType.Wishlist && cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
					{
						warnings.Add(T("ShoppingCart.MaximumWishlistItems"));
						return warnings;
					}

					var now = DateTime.UtcNow;
					var cartItem = new ShoppingCartItem
					{
						ShoppingCartType = cartType,
						StoreId = storeId,
						Product = product,
						AttributesXml = selectedAttributes,
						CustomerEnteredPrice = customerEnteredPrice,
						Quantity = quantity,
						ParentItemId = null	//parentItemId
					};

					if (bundleItem != null)
					{
						cartItem.BundleItemId = bundleItem.Id;
					}

					if (ctx == null)
					{
						customer.ShoppingCartItems.Add(cartItem);
						_customerService.UpdateCustomer(customer);
						_eventPublisher.EntityInserted(cartItem);
					}
					else
					{
						if (bundleItem == null)
						{
							Debug.Assert(ctx.Item == null, "Add to cart item already specified");
							ctx.Item = cartItem;
						}
						else
						{
							ctx.ChildItems.Add(cartItem);
						}
					}
				}
			}

			return warnings;
		}

		/// <summary>
		/// Add product to cart
		/// </summary>
		/// <param name="ctx">Add to cart context</param>
		public virtual void AddToCart(AddToCartContext ctx)
		{
			var customer = ctx.Customer ?? _workContext.CurrentCustomer;
			int storeId = ctx.StoreId ?? _storeContext.CurrentStore.Id;
			var cart = customer.GetCartItems(ctx.CartType, storeId);

			_customerService.ResetCheckoutData(customer, storeId);

			if (ctx.AttributeForm != null)
			{
				var attributes = _productAttributeService.GetProductVariantAttributesByProductId(ctx.Product.Id);

				ctx.Attributes = ctx.AttributeForm.CreateSelectedAttributesXml(ctx.Product.Id, attributes, _productAttributeParser, _localizationService,
					_downloadService, _catalogSettings, null, ctx.Warnings, true, ctx.BundleItemId);

				if (ctx.Product.ProductType == ProductType.BundledProduct && ctx.Attributes.HasValue())
					ctx.Warnings.Add(T("ShoppingCart.Bundle.NoAttributes"));

				if (ctx.Product.IsGiftCard)
					ctx.Attributes = ctx.AttributeForm.AddGiftCardAttribute(ctx.Attributes, ctx.Product.Id, _productAttributeParser, ctx.BundleItemId);
			}

			ctx.Warnings.AddRange(
				AddToCart(_workContext.CurrentCustomer, ctx.Product, ctx.CartType, storeId,	ctx.Attributes, ctx.CustomerEnteredPrice, ctx.Quantity, ctx.AddRequiredProducts, ctx)
			);

			if (ctx.Product.ProductType == ProductType.BundledProduct && ctx.Warnings.Count <= 0 && ctx.BundleItem == null)
			{
				foreach (var bundleItem in _productService.GetBundleItems(ctx.Product.Id).Select(x => x.Item))
				{
					AddToCart(new AddToCartContext
					{
						BundleItem = bundleItem,
						Warnings = ctx.Warnings,
						Item = ctx.Item,
						ChildItems = ctx.ChildItems,
						Product = bundleItem.Product,
						Customer = customer,
						AttributeForm = ctx.AttributeForm,
						CartType = ctx.CartType,
						Quantity = bundleItem.Quantity,
						AddRequiredProducts = ctx.AddRequiredProducts,
						StoreId = storeId
					});

					if (ctx.Warnings.Count > 0)
					{
						ctx.ChildItems.Clear();
						break;
					}
				}
			}

			if (ctx.BundleItem == null)
			{
				AddToCartStoring(ctx);
			}
		}

		/// <summary>
		/// Stores the shopping card items in the database
		/// </summary>
		/// <param name="ctx">Add to cart context</param>
		public virtual void AddToCartStoring(AddToCartContext ctx)
		{
			if (ctx.Warnings.Count == 0 && ctx.Item != null)
			{
				var customer = ctx.Customer ?? _workContext.CurrentCustomer;

				customer.ShoppingCartItems.Add(ctx.Item);
				_customerService.UpdateCustomer(customer);
				_eventPublisher.EntityInserted(ctx.Item);

				foreach (var childItem in ctx.ChildItems)
				{
					childItem.ParentItemId = ctx.Item.Id;

					customer.ShoppingCartItems.Add(childItem);
					_customerService.UpdateCustomer(customer);
					_eventPublisher.EntityInserted(childItem);
				}
			}
		}

        /// <summary>
        /// Updates the shopping cart item
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="shoppingCartItemId">Shopping cart item identifier</param>
        /// <param name="newQuantity">New shopping cart item quantity</param>
        /// <param name="resetCheckoutData">A value indicating whether to reset checkout data</param>
        /// <returns>Warnings</returns>
        public virtual IList<string> UpdateShoppingCartItem(Customer customer, int shoppingCartItemId, int newQuantity, bool resetCheckoutData)
        {
            if (customer == null)
                throw new ArgumentNullException("customer");

            var warnings = new List<string>();

			var shoppingCartItem = customer.ShoppingCartItems.FirstOrDefault(sci => sci.Id == shoppingCartItemId && sci.ParentItemId == null);
            if (shoppingCartItem != null)
            {
                if (resetCheckoutData)
                {
                    //reset checkout data
					_customerService.ResetCheckoutData(customer, shoppingCartItem.StoreId);
                }
                if (newQuantity > 0)
                {
                    //check warnings
                    warnings.AddRange(GetShoppingCartItemWarnings(customer, shoppingCartItem.ShoppingCartType, shoppingCartItem.Product, shoppingCartItem.StoreId,
						shoppingCartItem.AttributesXml, shoppingCartItem.CustomerEnteredPrice, newQuantity, false));

                    if (warnings.Count == 0)
                    {
                        //if everything is OK, then update a shopping cart item
                        shoppingCartItem.Quantity = newQuantity;
                        shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;
                        _customerService.UpdateCustomer(customer);

                        //event notification
                        _eventPublisher.EntityUpdated(shoppingCartItem);
                    }
                }
                else
                {
                    //delete a shopping cart item
                    DeleteShoppingCartItem(shoppingCartItem, resetCheckoutData, true);
                }
            }

            return warnings;
        }
        
        /// <summary>
        /// Migrate shopping cart
        /// </summary>
        /// <param name="fromCustomer">From customer</param>
        /// <param name="toCustomer">To customer</param>
        public virtual void MigrateShoppingCart(Customer fromCustomer, Customer toCustomer)
        {
            if (fromCustomer == null)
                throw new ArgumentNullException("fromCustomer");

            if (toCustomer == null)
                throw new ArgumentNullException("toCustomer");

            if (fromCustomer.Id == toCustomer.Id)
                return;

			int storeId = 0;
			var cartItems = fromCustomer.ShoppingCartItems.ToList().Organize().ToList();

			if (cartItems.Count <= 0)
				return;

			foreach (var cartItem in cartItems)
			{
				if (storeId == 0)
					storeId = cartItem.Item.StoreId;

				Copy(cartItem, toCustomer, cartItem.Item.ShoppingCartType, cartItem.Item.StoreId, false);
			}

			_eventPublisher.PublishMigrateShoppingCart(fromCustomer, toCustomer, storeId);

			foreach (var cartItem in cartItems)
			{
				DeleteShoppingCartItem(cartItem.Item);
			}
        }

		/// <summary>
		/// Copies a shopping cart item.
		/// </summary>
		/// <param name="sci">Shopping cart item</param>
		/// <param name="customer">The customer</param>
		/// <param name="cartType">Shopping cart type</param>
		/// <param name="storeId">Store Id</param>
		/// <param name="addRequiredProductsIfEnabled">Add required products if enabled</param>
		/// <returns>List with add-to-cart warnings.</returns>
		public virtual IList<string> Copy(OrganizedShoppingCartItem sci, Customer customer, ShoppingCartType cartType, int storeId, bool addRequiredProductsIfEnabled)
		{
			if (customer == null)
				throw new ArgumentNullException("customer");

			if (sci == null)
				throw new ArgumentNullException("item");

			var addToCartContext = new AddToCartContext
			{
				Customer = customer
			};

			addToCartContext.Warnings = AddToCart(customer, sci.Item.Product, cartType, storeId, sci.Item.AttributesXml, sci.Item.CustomerEnteredPrice,
				sci.Item.Quantity, addRequiredProductsIfEnabled, addToCartContext);

			if (addToCartContext.Warnings.Count == 0 && sci.ChildItems != null)
			{
				foreach (var childItem in sci.ChildItems)
				{
					addToCartContext.BundleItem = childItem.Item.BundleItem;

					addToCartContext.Warnings = AddToCart(customer, childItem.Item.Product, cartType, storeId, childItem.Item.AttributesXml, childItem.Item.CustomerEnteredPrice,
						childItem.Item.Quantity, false, addToCartContext);
				}
			}

			AddToCartStoring(addToCartContext);

			return addToCartContext.Warnings;
		}
    }
}
