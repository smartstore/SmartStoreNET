using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Discounts;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Discounts;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.ShoppingCart;
using System.IO;
using System.Web.Mvc.Html;

namespace SmartStore.Web.Controllers
{
	public partial class ShoppingCartController : PublicControllerBase
    {
        #region Fields

        private readonly ICommonServices _services;
        private readonly IProductService _productService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ITaxService _taxService;
        private readonly ICurrencyService _currencyService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IDiscountService _discountService;
        private readonly ICustomerService _customerService;
        private readonly IGiftCardService _giftCardService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IShippingService _shippingService;
        private readonly IOrderTotalCalculationService _orderTotalCalculationService;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly IPaymentService _paymentService;
        private readonly IPermissionService _permissionService;
        private readonly IDownloadService _downloadService;
        private readonly ICacheManager _cacheManager;
        private readonly IWebHelper _webHelper;
        private readonly ICustomerActivityService _customerActivityService;
		private readonly IGenericAttributeService _genericAttributeService;
        private readonly IDeliveryTimeService _deliveryTimeService;
		private readonly HttpContextBase _httpContext;
        private readonly MediaSettings _mediaSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly OrderSettings _orderSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly TaxSettings _taxSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly AddressSettings _addressSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly PluginMediator _pluginMediator;
        private readonly IQuantityUnitService _quantityUnitService;
		private readonly Lazy<ITopicService> _topicService;
        private readonly IMeasureService _measureService;
        private readonly MeasureSettings _measureSettings;
        private readonly ICompareProductsService _compareProductsService;
        private readonly CatalogHelper _helper;
		private readonly ProductUrlHelper _productUrlHelper;
        private readonly RewardPointsSettings _rewardPointsSettings;

        #endregion

        #region Constructors

        public ShoppingCartController(ICommonServices services, IProductService productService,
			IWorkContext workContext, IStoreContext storeContext,
            IShoppingCartService shoppingCartService, IPictureService pictureService,
            ILocalizationService localizationService, 
            IProductAttributeService productAttributeService, IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            ITaxService taxService, ICurrencyService currencyService, 
            IPriceCalculationService priceCalculationService, IPriceFormatter priceFormatter,
            ICheckoutAttributeParser checkoutAttributeParser, ICheckoutAttributeFormatter checkoutAttributeFormatter, 
            IOrderProcessingService orderProcessingService,
            IDiscountService discountService,ICustomerService customerService, 
            IGiftCardService giftCardService, ICountryService countryService,
            IStateProvinceService stateProvinceService, IShippingService shippingService, 
            IOrderTotalCalculationService orderTotalCalculationService,
            ICheckoutAttributeService checkoutAttributeService, IPaymentService paymentService,
            IPermissionService permissionService, IDeliveryTimeService deliveryTimeService,
            IDownloadService downloadService, ICacheManager cacheManager,
            IWebHelper webHelper, ICustomerActivityService customerActivityService,
			IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings, ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings, OrderSettings orderSettings,
            ShippingSettings shippingSettings, TaxSettings taxSettings,
            CaptchaSettings captchaSettings, AddressSettings addressSettings,
            CustomerSettings customerSettings,
            HttpContextBase httpContext, PluginMediator pluginMediator,
            IQuantityUnitService quantityUnitService,
			Lazy<ITopicService> topicService,
            IMeasureService measureService, MeasureSettings measureSettings,
            CatalogHelper helper, ICompareProductsService compareProductsService,
			ProductUrlHelper productUrlHelper, RewardPointsSettings rewardPointsSettings)
        {
            this._services = services;
            this._productService = productService;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._shoppingCartService = shoppingCartService;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._productAttributeService = productAttributeService;
            this._productAttributeFormatter = productAttributeFormatter;
            this._productAttributeParser = productAttributeParser;
            this._taxService = taxService;
            this._currencyService = currencyService;
            this._priceCalculationService = priceCalculationService;
            this._priceFormatter = priceFormatter;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            this._orderProcessingService = orderProcessingService;
            this._discountService = discountService;
            this._customerService = customerService;
            this._giftCardService = giftCardService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._shippingService = shippingService;
            this._orderTotalCalculationService = orderTotalCalculationService;
            this._checkoutAttributeService = checkoutAttributeService;
            this._paymentService = paymentService;
            this._permissionService = permissionService;
            this._downloadService = downloadService;
            this._cacheManager = cacheManager;
            this._webHelper = webHelper;
            this._customerActivityService = customerActivityService;
			this._genericAttributeService = genericAttributeService;
            this._deliveryTimeService = deliveryTimeService;
			this._httpContext = httpContext;
            this._mediaSettings = mediaSettings;
            this._shoppingCartSettings = shoppingCartSettings;
            this._catalogSettings = catalogSettings;
            this._orderSettings = orderSettings;
            this._shippingSettings = shippingSettings;
            this._taxSettings = taxSettings;
            this._captchaSettings = captchaSettings;
            this._addressSettings = addressSettings;
            this._customerSettings = customerSettings;
            this._pluginMediator = pluginMediator;
            this._quantityUnitService = quantityUnitService;
			this._topicService = topicService;
            this._measureService = measureService;
            this._measureSettings = measureSettings;
            this._helper = helper;
            this._compareProductsService = compareProductsService;
			this._productUrlHelper = productUrlHelper;
            this._rewardPointsSettings = rewardPointsSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected PictureModel PrepareCartItemPictureModel(Product product, int pictureSize, string productName, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException("product");

			var combination = _productAttributeParser.FindProductVariantAttributeCombination(product.Id, attributesXml);

            var pictureCacheKey = string.Format(ModelCacheEventConsumer.CART_PICTURE_MODEL_KEY, product.Id, combination == null ? 0 : combination.Id,
				pictureSize, true, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);

			// TODO: (mc) refactor > GetPictureInfos()
            var model = _cacheManager.Get(pictureCacheKey, () =>
            {
				Picture picture = null;

				if (combination != null)
				{
					var picturesIds = combination.GetAssignedPictureIds();
					if (picturesIds != null && picturesIds.Length > 0)
						picture = _pictureService.GetPictureById(picturesIds[0]);
				}

                // no attribute combination image, then load product picture
				if (picture == null)
					picture = _pictureService.GetPicturesByProductId(product.Id, 1).FirstOrDefault();

				if (picture == null && !product.VisibleIndividually && product.ParentGroupedProductId > 0)
				{
					//let's check whether this product has some parent "grouped" product
					picture = _pictureService.GetPicturesByProductId(product.ParentGroupedProductId, 1).FirstOrDefault();
				}

                return new PictureModel
                {
                    PictureId = picture != null ? picture.Id : 0,
					Size = pictureSize,
					ImageUrl = _pictureService.GetUrl(picture, pictureSize, !_catalogSettings.HideProductDefaultPictures),
                    Title = string.Format(_localizationService.GetResource("Media.Product.ImageLinkTitleFormat"), productName),
                    AlternateText = string.Format(_localizationService.GetResource("Media.Product.ImageAlternateTextFormat"), productName),
                };
            });
            return model;
        }

		private ShoppingCartModel.ShoppingCartItemModel PrepareShoppingCartItemModel(OrganizedShoppingCartItem sci)
		{
			var item = sci.Item;
			var product = sci.Item.Product;
			var currency = _workContext.WorkingCurrency;
			var customer = _workContext.CurrentCustomer;

			product.MergeWithCombination(item.AttributesXml);

			var model = new ShoppingCartModel.ShoppingCartItemModel
			{
				Id = item.Id,
				Sku = product.Sku,
				ProductId = product.Id,
				ProductName = product.GetLocalized(x => x.Name),
				ProductSeName = product.GetSeName(),
				VisibleIndividually = product.VisibleIndividually,
				EnteredQuantity = item.Quantity,
                MinOrderAmount = product.OrderMinimumQuantity,
                MaxOrderAmount = product.OrderMaximumQuantity,
                QuantityStep = product.QuantityStep > 0 ? product.QuantityStep : 1,
                IsShipEnabled = product.IsShipEnabled,
				ShortDesc = product.GetLocalized(x => x.ShortDescription),
				ProductType = product.ProductType,
				Weight = product.Weight,
				IsDownload = product.IsDownload,
				HasUserAgreement = product.HasUserAgreement,
				IsEsd = product.IsEsd,
				CreatedOnUtc = item.UpdatedOnUtc,
				DisableWishlistButton = product.DisableWishlistButton
			};

			model.BasePrice = product.GetBasePriceInfo(_localizationService, _priceFormatter, _currencyService, _taxService, _priceCalculationService, customer, currency);
			model.ProductUrl = _productUrlHelper.GetProductUrl(model.ProductSeName, sci);

			if (item.BundleItem != null)
			{
				model.BundlePerItemPricing = item.BundleItem.BundleProduct.BundlePerItemPricing;
				model.BundlePerItemShoppingCart = item.BundleItem.BundleProduct.BundlePerItemShoppingCart;

				model.AttributeInfo = _productAttributeFormatter.FormatAttributes(product, item.AttributesXml, customer,
					renderPrices: false, renderGiftCardAttributes: true, allowHyperlinks: true);

				var bundleItemName = item.BundleItem.GetLocalized(x => x.Name);
				if (bundleItemName.Value.HasValue())
					model.ProductName = bundleItemName;

				var bundleItemShortDescription = item.BundleItem.GetLocalized(x => x.ShortDescription);
				if (bundleItemShortDescription.Value.HasValue())
					model.ShortDesc = bundleItemShortDescription;

				model.BundleItem.Id = item.BundleItem.Id;
				model.BundleItem.DisplayOrder = item.BundleItem.DisplayOrder;
				model.BundleItem.HideThumbnail = item.BundleItem.HideThumbnail;
				
				if (model.BundlePerItemPricing && model.BundlePerItemShoppingCart)
				{
					decimal taxRate = decimal.Zero;
					decimal bundleItemSubTotalWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetSubTotal(sci, true), out taxRate);
					decimal bundleItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(bundleItemSubTotalWithDiscountBase, currency);

					model.BundleItem.PriceWithDiscount = _priceFormatter.FormatPrice(bundleItemSubTotalWithDiscount);
				}
			}
			else
			{
				model.AttributeInfo = _productAttributeFormatter.FormatAttributes(product, item.AttributesXml);

                var selectedAttributeValues = _productAttributeParser.ParseProductVariantAttributeValues(item.AttributesXml).ToList();
                if (selectedAttributeValues != null)
                {
                    foreach (var attributeValue in selectedAttributeValues)
                        model.Weight = decimal.Add(model.Weight, attributeValue.WeightAdjustment);
                }
			}

			if (product.DisplayDeliveryTimeAccordingToStock(_catalogSettings))
			{
				var deliveryTime = _deliveryTimeService.GetDeliveryTime(product);
				if (deliveryTime != null)
				{
					model.DeliveryTimeName = deliveryTime.GetLocalized(x => x.Name);
					model.DeliveryTimeHexValue = deliveryTime.ColorHexValue;
				}
			}

            // quantity unit
            var quantityUnit = _quantityUnitService.GetQuantityUnitById(product.QuantityUnitId);
            if(quantityUnit != null)
            {
                model.QuantityUnitName = quantityUnit.GetLocalized(x => x.Name);
            }
            
			//allowed quantities
			var allowedQuantities = product.ParseAllowedQuatities();
			foreach (var qty in allowedQuantities)
			{
				model.AllowedQuantities.Add(new SelectListItem()
				{
					Text = qty.ToString(),
					Value = qty.ToString(),
					Selected = item.Quantity == qty
				});
			}

			//recurring info
			if (product.IsRecurring)
			{
				model.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), 
					product.RecurringCycleLength, product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));
			}

			//unit prices
			if (product.CallForPrice)
			{
				model.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
			}
			else
			{
				decimal taxRate = decimal.Zero;
				decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
				decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, currency);

				model.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
			}

			// Subtotal, discount
			if (product.CallForPrice)
			{
				model.SubTotal = _localizationService.GetResource("Products.CallForPrice");
			}
			else
			{
				// Sub total
				decimal taxRate, itemSubTotalWithDiscountBase, itemSubTotalWithDiscount, itemSubTotalWithoutDiscountBase = decimal.Zero;

                if (currency.RoundOrderItemsEnabled)
				{
					// Gross > Net RoundFix
					var priceWithDiscount = _taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
					itemSubTotalWithDiscountBase = priceWithDiscount.RoundIfEnabledFor(currency) * sci.Item.Quantity;

					itemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(itemSubTotalWithDiscountBase, currency);
					model.SubTotal = _priceFormatter.FormatPrice(itemSubTotalWithDiscount);

					var priceWithoutDiscount = _taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(sci, false), out taxRate);
					itemSubTotalWithoutDiscountBase = priceWithoutDiscount.RoundIfEnabledFor(currency) * sci.Item.Quantity;
				}
				else
				{
					itemSubTotalWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetSubTotal(sci, true), out taxRate);

					itemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(itemSubTotalWithDiscountBase, currency);
					model.SubTotal = _priceFormatter.FormatPrice(itemSubTotalWithDiscount);

					itemSubTotalWithoutDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetSubTotal(sci, false), out taxRate);
				}

				decimal itemDiscountBase = itemSubTotalWithoutDiscountBase - itemSubTotalWithDiscountBase;

				if (itemDiscountBase > decimal.Zero)
				{
					decimal itemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(itemDiscountBase, currency);
					model.Discount = _priceFormatter.FormatPrice(itemDiscount);
				}

                model.BasePrice = product.GetBasePriceInfo(
                    _localizationService, 
                    _priceFormatter,
                    _currencyService,
                    _taxService,
                    _priceCalculationService,
					customer,
                    currency,
                    (product.Price - _priceCalculationService.GetUnitPrice(sci, true)) * (-1)
                );
			}

			// picture
			if (item.BundleItem != null)
			{
				if (_shoppingCartSettings.ShowProductBundleImagesOnShoppingCart)
					model.Picture = PrepareCartItemPictureModel(product, _mediaSettings.CartThumbBundleItemPictureSize, model.ProductName, item.AttributesXml);
			}
			else
			{
				if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
					model.Picture = PrepareCartItemPictureModel(product, _mediaSettings.CartThumbPictureSize, model.ProductName, item.AttributesXml);
			}

			// item warnings
			var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(customer, item.ShoppingCartType, product, item.StoreId,
				item.AttributesXml, item.CustomerEnteredPrice, item.Quantity, false, bundleItem: item.BundleItem, childItems: sci.ChildItems);

			foreach (var warning in itemWarnings)
			{
				model.Warnings.Add(warning);
			}

			if (sci.ChildItems != null)
			{
				foreach (var childItem in sci.ChildItems.Where(x => x.Item.Id != item.Id))
				{
					var childModel = PrepareShoppingCartItemModel(childItem);

					model.ChildItems.Add(childModel);
				}
			}

			return model;
		}
		
		private WishlistModel.ShoppingCartItemModel PrepareWishlistCartItemModel(OrganizedShoppingCartItem sci)
		{
			var item = sci.Item;
			var product = sci.Item.Product;

			product.MergeWithCombination(item.AttributesXml);

			var model = new WishlistModel.ShoppingCartItemModel
			{
				Id = item.Id,
				Sku = product.Sku,
				ProductId = product.Id,
				ProductName = product.GetLocalized(x => x.Name),
				ProductSeName = product.GetSeName(),
                EnteredQuantity = item.Quantity,
                MinOrderAmount = product.OrderMinimumQuantity,
                MaxOrderAmount = product.OrderMaximumQuantity,
                QuantityStep = product.QuantityStep > 0 ? product.QuantityStep : 1,
                ShortDesc = product.GetLocalized(x => x.ShortDescription),
				ProductType = product.ProductType,
				VisibleIndividually = product.VisibleIndividually,
				CreatedOnUtc = item.UpdatedOnUtc,
				DisableBuyButton = product.DisableBuyButton
			};

			model.ProductUrl = _productUrlHelper.GetProductUrl(model.ProductSeName, sci);

			if (item.BundleItem != null)
			{
				model.BundlePerItemPricing = item.BundleItem.BundleProduct.BundlePerItemPricing;
				model.BundlePerItemShoppingCart = item.BundleItem.BundleProduct.BundlePerItemShoppingCart;
				model.AttributeInfo = _productAttributeFormatter.FormatAttributes(product, item.AttributesXml, _workContext.CurrentCustomer,
					renderPrices: false, renderGiftCardAttributes: false, allowHyperlinks: false);

				var bundleItemName = item.BundleItem.GetLocalized(x => x.Name);
				if (bundleItemName.Value.HasValue())
					model.ProductName = bundleItemName;

				var bundleItemShortDescription = item.BundleItem.GetLocalized(x => x.ShortDescription);
				if (bundleItemShortDescription.Value.HasValue())
					model.ShortDesc = bundleItemShortDescription;

				model.BundleItem.Id = item.BundleItem.Id;
				model.BundleItem.DisplayOrder = item.BundleItem.DisplayOrder;
				model.BundleItem.HideThumbnail = item.BundleItem.HideThumbnail;

				if (model.BundlePerItemPricing && model.BundlePerItemShoppingCart)
				{
					decimal taxRate = decimal.Zero;
					decimal bundleItemSubTotalWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetSubTotal(sci, true), out taxRate);
					decimal bundleItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(bundleItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);

					model.BundleItem.PriceWithDiscount = _priceFormatter.FormatPrice(bundleItemSubTotalWithDiscount);
				}
			}
			else
			{
				model.AttributeInfo = _productAttributeFormatter.FormatAttributes(product, item.AttributesXml);
			}

			//allowed quantities
			var allowedQuantities = product.ParseAllowedQuatities();
			foreach (var qty in allowedQuantities)
			{
				model.AllowedQuantities.Add(new SelectListItem
				{
					Text = qty.ToString(),
					Value = qty.ToString(),
					Selected = item.Quantity == qty
				});
			}

            // quantity unit
            var quantityUnit = _quantityUnitService.GetQuantityUnitById(product.QuantityUnitId);
            if (quantityUnit != null)
            {
                model.QuantityUnitName = quantityUnit.GetLocalized(x => x.Name);
            }

            //recurring info
            if (product.IsRecurring)
			{
				model.RecurringInfo = string.Format(_localizationService.GetResource("ShoppingCart.RecurringPeriod"), 
					product.RecurringCycleLength, product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));
			}

			//unit prices
			if (product.CallForPrice)
			{
				model.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
			}
			else
			{
				decimal taxRate = decimal.Zero;
				decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
				decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);
				
				model.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
			}

			//subtotal, discount
			if (product.CallForPrice)
			{
				model.SubTotal = _localizationService.GetResource("Products.CallForPrice");
			}
			else
			{
				//sub total
				decimal taxRate = decimal.Zero;
				decimal shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetSubTotal(sci, true), out taxRate);
				decimal shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
				
				model.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);

				//display an applied discount amount
				decimal shoppingCartItemSubTotalWithoutDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetSubTotal(sci, false), out taxRate);
				decimal shoppingCartItemDiscountBase = shoppingCartItemSubTotalWithoutDiscountBase - shoppingCartItemSubTotalWithDiscountBase;
				
				if (shoppingCartItemDiscountBase > decimal.Zero)
				{
					decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
					
					model.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
				}
			}

			//picture
			if (item.BundleItem != null)
			{
				if (_shoppingCartSettings.ShowProductBundleImagesOnShoppingCart)
					model.Picture = PrepareCartItemPictureModel(product, _mediaSettings.CartThumbBundleItemPictureSize, model.ProductName, item.AttributesXml);
			}
			else
			{
				if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
					model.Picture = PrepareCartItemPictureModel(product, _mediaSettings.CartThumbPictureSize, model.ProductName, item.AttributesXml);
			}

			//item warnings
			var itemWarnings = _shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer, item.ShoppingCartType, product, item.StoreId,
				item.AttributesXml, item.CustomerEnteredPrice, item.Quantity, false, childItems: sci.ChildItems);

			foreach (var warning in itemWarnings)
			{
				model.Warnings.Add(warning);
			}

			if (sci.ChildItems != null)
			{
				foreach (var childItem in sci.ChildItems.Where(x => x.Item.Id != item.Id))
				{
					var childModel = PrepareWishlistCartItemModel(childItem);

					model.ChildItems.Add(childModel);
				}
			}

			return model;
		}

		private void PrepareButtonPaymentMethodModel(ButtonPaymentMethodModel model, IList<OrganizedShoppingCartItem> cart)
		{
			model.Items.Clear();

			var paymentTypes = new PaymentMethodType[] { PaymentMethodType.Button, PaymentMethodType.StandardAndButton };

			var boundPaymentMethods = _paymentService
				.LoadActivePaymentMethods(_workContext.CurrentCustomer, cart, _storeContext.CurrentStore.Id, paymentTypes, false)
				.ToList();

			foreach (var pm in boundPaymentMethods)
			{
				if (cart.IsRecurring() && pm.Value.RecurringPaymentType == RecurringPaymentType.NotSupported)
					continue;

				pm.Value.GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);

				if (actionName.HasValue() && controllerName.HasValue())
				{
					model.Items.Add(new ButtonPaymentMethodModel.ButtonPaymentMethodItem
					{
						ActionName = actionName,
						ControllerName = controllerName,
						RouteValues = routeValues
					});
				}
			}
		}

		/// <summary>
		/// Prepare shopping cart model
		/// </summary>
		/// <param name="model">Model instance</param>
		/// <param name="cart">Shopping cart</param>
		/// <param name="isEditable">A value indicating whether cart is editable</param>
		/// <param name="validateCheckoutAttributes">A value indicating whether we should validate checkout attributes when preparing the model</param>
		/// <param name="prepareEstimateShippingIfEnabled">A value indicating whether we should prepare "Estimate shipping" model</param>
		/// <param name="setEstimateShippingDefaultAddress">A value indicating whether we should prefill "Estimate shipping" model with the default customer address</param>
		/// <param name="prepareAndDisplayOrderReviewData">A value indicating whether we should prepare review data (such as billing/shipping address, payment or shipping data entered during checkout)</param>
		/// <returns>Model</returns>
		[NonAction]
		protected void PrepareShoppingCartModel(ShoppingCartModel model,
			IList<OrganizedShoppingCartItem> cart, bool isEditable = true,
			bool validateCheckoutAttributes = false,
			bool prepareEstimateShippingIfEnabled = true, bool setEstimateShippingDefaultAddress = true,
			bool prepareAndDisplayOrderReviewData = false)
		{
			if (cart == null)
				throw new ArgumentNullException("cart");

			if (model == null)
				throw new ArgumentNullException("model");

			if (cart.Count == 0)
				return;

			#region Simple properties

			model.MediaDimensions = _mediaSettings.CartThumbPictureSize;
			model.BundleThumbSize = _mediaSettings.CartThumbBundleItemPictureSize;
			model.DisplayDeliveryTime = _shoppingCartSettings.ShowDeliveryTimes;
			model.DisplayShortDesc = _shoppingCartSettings.ShowShortDesc;
			model.DisplayBasePrice = _shoppingCartSettings.ShowBasePrice;
			model.DisplayWeight = _shoppingCartSettings.ShowWeight;
            model.DisplayMoveToWishlistButton = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist);
            model.IsEditable = isEditable;
			model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
			model.ShowProductBundleImages = _shoppingCartSettings.ShowProductBundleImagesOnShoppingCart;
			model.ShowSku = _catalogSettings.ShowProductSku;

            var measure = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            if(measure != null) 
            {
                model.MeasureUnitName = measure.Name;
            }
            
			var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
			model.CheckoutAttributeInfo = HtmlUtils.ConvertPlainTextToTable(HtmlUtils.ConvertHtmlToPlainText(
				_checkoutAttributeFormatter.FormatAttributes(checkoutAttributesXml, _workContext.CurrentCustomer)
			));

			model.IsValidMinOrderSubtotal = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
			model.TermsOfServiceEnabled = _orderSettings.TermsOfServiceEnabled;

			//gift card and gift card boxes
			model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
			var discountCouponCode = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.DiscountCouponCode);
			var discount = _discountService.GetDiscountByCouponCode(discountCouponCode);
			if (discount != null && discount.RequiresCouponCode && _discountService.IsDiscountValid(discount, _workContext.CurrentCustomer))
			{
				model.DiscountBox.CurrentCode = discount.CouponCode;
			}

			model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;
			model.DisplayCommentBox = _shoppingCartSettings.ShowCommentBox;
			model.DisplayEsdRevocationWaiverBox = _shoppingCartSettings.ShowEsdRevocationWaiverBox;

            //reward points
            if (_rewardPointsSettings.Enabled && !cart.IsRecurring() && !_workContext.CurrentCustomer.IsGuest())
            {
                int rewardPointsBalance = _workContext.CurrentCustomer.GetRewardPointsBalance();
                decimal rewardPointsAmountBase = _orderTotalCalculationService.ConvertRewardPointsToAmount(rewardPointsBalance);
                decimal rewardPointsAmount = _currencyService.ConvertFromPrimaryStoreCurrency(rewardPointsAmountBase, _workContext.WorkingCurrency);

                if (rewardPointsAmount > decimal.Zero)
                {
                    model.RewardPoints.DisplayRewardPoints = true;
                    model.RewardPoints.RewardPointsAmount = _priceFormatter.FormatPrice(rewardPointsAmount, true, false);
                    model.RewardPoints.RewardPointsBalance = rewardPointsBalance;
                    model.RewardPoints.UseRewardPoints = _workContext.CurrentCustomer.GetAttribute<bool>(SystemCustomerAttributeNames.UseRewardPointsDuringCheckout,
                        _genericAttributeService, _storeContext.CurrentStore.Id);
                }
            }

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, validateCheckoutAttributes);
			foreach (var warning in cartWarnings)
			{
				model.Warnings.Add(warning);
			}

			#endregion

			#region Checkout attributes

			var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id);
			if (!cart.RequiresShipping())
			{
				//remove attributes which require shippable products
				checkoutAttributes = checkoutAttributes.RemoveShippableAttributes();
			}

			foreach (var attribute in checkoutAttributes)
			{
				var caModel = new ShoppingCartModel.CheckoutAttributeModel
				{
					Id = attribute.Id,
					Name = attribute.GetLocalized(x => x.Name),
					TextPrompt = attribute.GetLocalized(x => x.TextPrompt),
					IsRequired = attribute.IsRequired,
					AttributeControlType = attribute.AttributeControlType
				};

				if (attribute.ShouldHaveValues())
				{
					var caValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
					foreach (var caValue in caValues)
					{
						var pvaValueModel = new ShoppingCartModel.CheckoutAttributeValueModel
						{
							Id = caValue.Id,
							Name = caValue.GetLocalized(x => x.Name),
							IsPreSelected = caValue.IsPreSelected
						};
						caModel.Values.Add(pvaValueModel);

						// Display price if allowed.
						if (_permissionService.Authorize(StandardPermissionProvider.DisplayPrices))
						{
							decimal priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(caValue);
							decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
							if (priceAdjustmentBase > decimal.Zero)
								pvaValueModel.PriceAdjustment = "+" + _priceFormatter.FormatPrice(priceAdjustment);
							else if (priceAdjustmentBase < decimal.Zero)
								pvaValueModel.PriceAdjustment = "-" + _priceFormatter.FormatPrice(-priceAdjustment);
						}
					}
				}

				// Set already selected attributes.
				var selectedCheckoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
				switch (attribute.AttributeControlType)
				{
					case AttributeControlType.DropdownList:
					case AttributeControlType.RadioList:
					case AttributeControlType.Boxes:
					case AttributeControlType.Checkboxes:
						if (!string.IsNullOrEmpty(selectedCheckoutAttributes))
						{
							// Clear default selection.
							foreach (var item in caModel.Values)
							{
								item.IsPreSelected = false;
							}

							// Select new values.
							var selectedCaValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(selectedCheckoutAttributes);
							foreach (var caValue in selectedCaValues)
							{
								foreach (var item in caModel.Values)
								{
									if (caValue.Id == item.Id)
										item.IsPreSelected = true;
								}
							}
						}
						break;

					case AttributeControlType.TextBox:
					case AttributeControlType.MultilineTextbox:
						if (!string.IsNullOrEmpty(selectedCheckoutAttributes))
						{
							var enteredText = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
							if (enteredText.Count > 0)
								caModel.TextValue = enteredText[0];
						}
						break;

					case AttributeControlType.Datepicker:
						{
							// Keep in mind my that the code below works only in the current culture.
							var selectedDateStr = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
							if (selectedDateStr.Count > 0)
							{
								DateTime selectedDate;
								if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,  DateTimeStyles.None, out selectedDate))
								{
									caModel.SelectedDay = selectedDate.Day;
									caModel.SelectedMonth = selectedDate.Month;
									caModel.SelectedYear = selectedDate.Year;
								}
							}
						}
						break;

					case AttributeControlType.FileUpload:
						if (!string.IsNullOrEmpty(selectedCheckoutAttributes))
						{
							var values = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
							if (values.Any())
							{
								caModel.UploadedFileGuid = values.First();

								Guid guid;
								if (caModel.UploadedFileGuid.HasValue() && Guid.TryParse(caModel.UploadedFileGuid, out guid))
								{
									var download = _downloadService.GetDownloadByGuid(guid);
									if (download != null)
									{
										caModel.UploadedFileName = string.Concat(download.Filename ?? download.DownloadGuid.ToString(), download.Extension);
									}
								}
							}
						}
						break;

					default:
						break;
				}

				model.CheckoutAttributes.Add(caModel);
			}

			#endregion

			#region Estimate shipping

			if (prepareEstimateShippingIfEnabled)
			{
				model.EstimateShipping.Enabled = cart.Count > 0 && cart.RequiresShipping() && _shippingSettings.EstimateShippingEnabled;
				if (model.EstimateShipping.Enabled)
				{
					//countries
					int? defaultEstimateCountryId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null) ? _workContext.CurrentCustomer.ShippingAddress.CountryId : model.EstimateShipping.CountryId;
					//model.EstimateShipping.AvailableCountries.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.SelectCountry"), Value = "0" });
					foreach (var c in _countryService.GetAllCountriesForShipping())
						model.EstimateShipping.AvailableCountries.Add(new SelectListItem()
						{
							Text = c.GetLocalized(x => x.Name),
							Value = c.Id.ToString(),
							Selected = c.Id == defaultEstimateCountryId
						});
					//states
					int? defaultEstimateStateId = (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null) ? _workContext.CurrentCustomer.ShippingAddress.StateProvinceId : model.EstimateShipping.StateProvinceId;
					var states = defaultEstimateCountryId.HasValue ? _stateProvinceService.GetStateProvincesByCountryId(defaultEstimateCountryId.Value).ToList() : new List<StateProvince>();
					if (states.Count > 0)
						foreach (var s in states)
							model.EstimateShipping.AvailableStates.Add(new SelectListItem()
							{
								Text = s.GetLocalized(x => x.Name),
								Value = s.Id.ToString(),
								Selected = s.Id == defaultEstimateStateId
							});
					else
						model.EstimateShipping.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Address.OtherNonUS"), Value = "0" });

					if (setEstimateShippingDefaultAddress && _workContext.CurrentCustomer.ShippingAddress != null)
						model.EstimateShipping.ZipPostalCode = _workContext.CurrentCustomer.ShippingAddress.ZipPostalCode;
				}
			}

			#endregion

			#region Cart items

			foreach (var sci in cart)
			{
				var shoppingCartItemModel = PrepareShoppingCartItemModel(sci);

				model.Items.Add(shoppingCartItemModel);
			}

			#endregion

			#region Order review data

			if (prepareAndDisplayOrderReviewData)
			{
                var checkoutState = _httpContext.GetCheckoutState();

                model.OrderReviewData.Display = true;

				//billing info
				var billingAddress = _workContext.CurrentCustomer.BillingAddress;
				if (billingAddress != null)
					model.OrderReviewData.BillingAddress.PrepareModel(billingAddress, false, _addressSettings);

				//shipping info
				if (cart.RequiresShipping())
				{
					model.OrderReviewData.IsShippable = true;

					var shippingAddress = _workContext.CurrentCustomer.ShippingAddress;
					if (shippingAddress != null)
						model.OrderReviewData.ShippingAddress.PrepareModel(shippingAddress, false, _addressSettings);

					//selected shipping method
					var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
					if (shippingOption != null)
						model.OrderReviewData.ShippingMethod = shippingOption.Name;
                    
                    if (checkoutState.CustomProperties.ContainsKey("HasOnlyOneActiveShippingMethod"))
                        model.OrderReviewData.DisplayShippingMethodChangeOption = !(bool)checkoutState.CustomProperties.Get("HasOnlyOneActiveShippingMethod");
                    
				}

				//payment info
				var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
					 SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);

                if (checkoutState.CustomProperties.ContainsKey("HasOnlyOneActivePaymentMethod"))
                    model.OrderReviewData.DisplayPaymentMethodChangeOption = !(bool)checkoutState.CustomProperties.Get("HasOnlyOneActivePaymentMethod");

                var paymentMethod = _paymentService.LoadPaymentMethodBySystemName(selectedPaymentMethodSystemName);

				model.OrderReviewData.PaymentMethod = paymentMethod != null ? _pluginMediator.GetLocalizedFriendlyName(paymentMethod.Metadata) : "";
				model.OrderReviewData.PaymentSummary = checkoutState.PaymentSummary;
				model.OrderReviewData.IsPaymentSelectionSkipped = checkoutState.IsPaymentSelectionSkipped;
			}
			#endregion

			PrepareButtonPaymentMethodModel(model.ButtonPaymentMethods, cart);
		}

        [NonAction]
		protected void PrepareWishlistModel(WishlistModel model, IList<OrganizedShoppingCartItem> cart, bool isEditable = true)
        {
            if (cart == null)
                throw new ArgumentNullException("cart");

            if (model == null)
                throw new ArgumentNullException("model");

            model.EmailWishlistEnabled = _shoppingCartSettings.EmailWishlistEnabled;
            model.IsEditable = isEditable;
            model.DisplayAddToCart = _permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart);

            if (cart.Count == 0)
                return;

            #region Simple properties

            var customer = cart.FirstOrDefault().Item.Customer;
            model.CustomerGuid = customer.CustomerGuid;
            model.CustomerFullname = customer.GetFullName();
            model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
			model.ShowProductBundleImages = _shoppingCartSettings.ShowProductBundleImagesOnShoppingCart;
			model.ShowItemsFromWishlistToCartButton = _shoppingCartSettings.ShowItemsFromWishlistToCartButton;
            model.ShowSku = _catalogSettings.ShowProductSku;
			model.DisplayShortDesc = _shoppingCartSettings.ShowShortDesc;
			model.BundleThumbSize = _mediaSettings.CartThumbBundleItemPictureSize;

            //cart warnings
            var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, "", false);
            foreach (var warning in cartWarnings)
                model.Warnings.Add(warning);

            #endregion

            #region Cart items

            foreach (var sci in cart)
            {
				var wishlistCartItemModel = PrepareWishlistCartItemModel(sci);

                model.Items.Add(wishlistCartItemModel);
            }

            #endregion
        }

        [NonAction]
        protected MiniShoppingCartModel PrepareMiniShoppingCartModel()
        {
            var model = new MiniShoppingCartModel
            {
                ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
                ThumbSize = _mediaSettings.MiniCartThumbPictureSize,
                CurrentCustomerIsGuest = _workContext.CurrentCustomer.IsGuest(),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
                DisplayMoveToWishlistButton = _permissionService.Authorize(StandardPermissionProvider.EnableWishlist)
			};

			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            model.TotalProducts = cart.GetTotalProducts();

            if (cart.Count > 0)
            {
                model.SubTotal = _shoppingCartService.GetFormattedCurrentCartSubTotal(cart);

                //a customer should visit the shopping cart page before going to checkout if:
                //1. we have at least one checkout attribute that is reqired
                //2. min order sub-total is OK
                var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id);
                if (!cart.RequiresShipping())
                {
                    //remove attributes which require shippable products
                    checkoutAttributes = checkoutAttributes.RemoveShippableAttributes();
                }
                bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
                model.DisplayCheckoutButton = checkoutAttributes.Where(x => x.IsRequired).Count() == 0 && minOrderSubtotalAmountOk;

				// Products. sort descending (recently added products)
				cart = cart.ToList(); // TBD: (mc) Why?
                foreach (var sci in cart)
                {
					var item = sci.Item;
					var product = sci.Item.Product;

                    var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel
                    {
                        Id = item.Id,
                        ProductId = product.Id,
						ProductName = product.GetLocalized(x => x.Name),
                        ShortDesc = product.GetLocalized(x => x.ShortDescription),
                        ProductSeName = product.GetSeName(),
                        EnteredQuantity = item.Quantity,
                        MaxOrderAmount = product.OrderMaximumQuantity,
                        MinOrderAmount = product.OrderMinimumQuantity,
                        QuantityStep = product.QuantityStep > 0 ? product.QuantityStep : 1,
						CreatedOnUtc = item.UpdatedOnUtc,
                        AttributeInfo = _productAttributeFormatter.FormatAttributes(
                            product, 
                            item.AttributesXml, 
                            null,
                            serapator: ", ", 
                            renderPrices: false, 
                            renderGiftCardAttributes: false, 
                            allowHyperlinks: false)
                    };

                    cartItemModel.QuantityUnitName = null;
					cartItemModel.ProductUrl = _productUrlHelper.GetProductUrl(cartItemModel.ProductSeName, sci);

					if (sci.ChildItems != null && _shoppingCartSettings.ShowProductBundleImagesOnShoppingCart)
					{
						foreach (var childItem in sci.ChildItems.Where(x => x.Item.Id != item.Id && x.Item.BundleItem != null && !x.Item.BundleItem.HideThumbnail))
						{
							var bundleItemModel = new MiniShoppingCartModel.ShoppingCartItemBundleItem
							{
								ProductName = childItem.Item.Product.GetLocalized(x => x.Name),
								ProductSeName = childItem.Item.Product.GetSeName(),
							};

							bundleItemModel.ProductUrl = _productUrlHelper.GetProductUrl(
								childItem.Item.ProductId, bundleItemModel.ProductSeName, childItem.Item.AttributesXml);

							var itemPicture = _pictureService.GetPicturesByProductId(childItem.Item.ProductId, 1).FirstOrDefault();
							if (itemPicture != null)
								bundleItemModel.PictureUrl = _pictureService.GetUrl(itemPicture, 32);

							cartItemModel.BundleItems.Add(bundleItemModel);
						}
					}

                    //unit prices
                    if (product.CallForPrice)
                    {
                        cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                    }
                    else
                    {
						product.MergeWithCombination(item.AttributesXml);

                        decimal taxRate = decimal.Zero;
                        decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
                        decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);

                        cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                    }

                    //picture
                    if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                    {
                        cartItemModel.Picture = PrepareCartItemPictureModel(product, _mediaSettings.MiniCartThumbPictureSize, cartItemModel.ProductName, item.AttributesXml);
                    }

                    model.Items.Add(cartItemModel);
                }
            }

            return model;
        }

        [NonAction]
        protected void ParseAndSaveCheckoutAttributes(List<OrganizedShoppingCartItem> cart, ProductVariantQuery query)
        {
            var selectedAttributes = "";
            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes(_storeContext.CurrentStore.Id);

            if (!cart.RequiresShipping())
            {
                // Remove attributes which require shippable products.
                checkoutAttributes = checkoutAttributes.RemoveShippableAttributes();
            }

            foreach (var attribute in checkoutAttributes)
            {
				var selectedItems = query.CheckoutAttributes.Where(x => x.AttributeId == attribute.Id);

                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.Boxes:
						{
							var firstItemValue = selectedItems.FirstOrDefault()?.Value;
							if (firstItemValue.HasValue())
							{
								var selectedAttributeId = firstItemValue.SplitSafe(",").SafeGet(0).ToInt();
								if (selectedAttributeId > 0)
								{
									selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes, attribute, selectedAttributeId.ToString());
								}
							}
						}
						break;

                    case AttributeControlType.Checkboxes:
						foreach (var item in selectedItems)
						{
							var selectedAttributeId = item.Value.SplitSafe(",").SafeGet(0).ToInt();
							if (selectedAttributeId > 0)
							{
								selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes, attribute, selectedAttributeId.ToString());
							}
						}
						break;

                    case AttributeControlType.TextBox:    
                    case AttributeControlType.MultilineTextbox:
						{
							var selectedValue = string.Join(",", selectedItems.Select(x => x.Value));
							if (selectedValue.HasValue())
							{
								selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes, attribute, selectedValue);
							}
						}
                        break;

                    case AttributeControlType.Datepicker:
						{
							var firstItemDate = selectedItems.FirstOrDefault()?.Date;
							if (firstItemDate.HasValue)
							{
								selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes, attribute, firstItemDate.Value.ToString("D"));
							}
						}
                        break;

                    case AttributeControlType.FileUpload:
						{
							var selectedValue = string.Join(",", selectedItems.Select(x => x.Value));
							if (selectedValue.HasValue())
							{
								selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes, attribute, selectedValue);
							}
						}
                        break;
                }
            }

			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.CheckoutAttributes, selectedAttributes);
        }

        [HttpPost]
        public ActionResult UploadFileCheckoutAttribute(string controlId)
        {
            var postedFile = this.Request.Files["file"].ToPostedFileResult();
            if (postedFile != null && postedFile.FileName.HasValue())
            {
                int fileMaxSize = _catalogSettings.FileUploadMaximumSizeBytes;
                if (postedFile.Size > fileMaxSize)
                {
                    return Json(new
                    {
                        success = false,
                        message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024))
                    });
                }
                else
                {
                    //save an uploaded file
                    var download = new Download
                    {
                        DownloadGuid = Guid.NewGuid(),
                        UseDownloadUrl = false,
                        DownloadUrl = "",
                        ContentType = postedFile.ContentType,
                        Filename = postedFile.FileTitle,
                        Extension = postedFile.FileExtension,
                        IsNew = true,
                        UpdatedOnUtc = DateTime.UtcNow
                    };

                    _downloadService.InsertDownload(download, postedFile.Buffer);

                    return Json(new
                    {
                        success = true,
                        message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                        downloadGuid = download.DownloadGuid,
                    });
                }
            }

            return Json(new
            {
                success = false,
                downloadGuid = Guid.Empty
            });
        }

        #endregion

        #region Shopping cart

        [HttpPost]
        public ActionResult AddProductSimple(int productId, int shoppingCartTypeId = 1, bool forceredirection = false)
        {
			// Add product to cart using AJAX
			// Currently we use this method on catalog pages (category/manufacturer/etc)

			var shoppingCartType = (ShoppingCartType)shoppingCartTypeId;

            var product = _productService.GetProductById(productId);
			if (product == null)
			{
				return Json(new
				{
					success = false,
					message = T("Products.NotFound", productId)
				});
			}

			// filter out cases where a product cannot be added to cart
			if (product.ProductType == ProductType.GroupedProduct || product.CustomerEntersPrice || product.IsGiftCard)
			{
				return Json(new
				{
					redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
				});
			}

            // quantity to add
			var qtyToAdd = product.OrderMinimumQuantity > 0 ? product.OrderMinimumQuantity : 1;

			var allowedQuantities = product.ParseAllowedQuatities();
            if (allowedQuantities.Length > 0)
            {
                // cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            // get standard warnings without attribute validations
            // first, try to find existing shopping cart item
			var cart = _workContext.CurrentCustomer.GetCartItems(shoppingCartType, _storeContext.CurrentStore.Id);

            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, shoppingCartType, product);
            
			// if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Item.Quantity + qtyToAdd : qtyToAdd;

			var addToCartWarnings = (List<string>)_shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer, shoppingCartType,
				product, _storeContext.CurrentStore.Id, string.Empty, decimal.Zero, quantityToValidate, false, true, false, false, false, false);

            if (addToCartWarnings.Count > 0)
            {
                // cannot be added to the cart
                // let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            // now let's try adding product to the cart (now including product attribute validation, etc)
			var addToCartContext = new AddToCartContext
			{
				Product = product,
				CartType = shoppingCartType,
				Quantity = qtyToAdd,
				AddRequiredProducts = true
			};

			_shoppingCartService.AddToCart(addToCartContext);

            if (addToCartContext.Warnings.Count > 0)
            {
                // cannot be added to the cart
                // but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            // now product is in the cart
            // activity log
			_customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

            if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
            {
                // redirect to the shopping cart page
                return Json(new
                {
                    redirect = Url.RouteUrl("ShoppingCart"),
                });
            }

            return Json(new
            {
                success = true,
                message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart"), Url.RouteUrl("ShoppingCart"))
            });

        }

        //add product to cart using AJAX
		//currently we use this method on the product details pages
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddProduct(int productId, int shoppingCartTypeId, ProductVariantQuery query, FormCollection form)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                return Json(new
                {
                    redirect = Url.RouteUrl("HomePage"),
                });
            }

            #region Customer entered price
            decimal customerEnteredPriceConverted = decimal.Zero;
            if (product.CustomerEntersPrice)
            {
                foreach (string formKey in form.AllKeys)
                {
                    if (formKey.Equals(string.Format("addtocart_{0}.CustomerEnteredPrice", productId), StringComparison.InvariantCultureIgnoreCase))
                    {
                        decimal customerEnteredPrice = decimal.Zero;
                        if (decimal.TryParse(form[formKey], out customerEnteredPrice))
                            customerEnteredPriceConverted = _currencyService.ConvertToPrimaryStoreCurrency(customerEnteredPrice, _workContext.WorkingCurrency);
                        break;
                    }
                }
            }
            #endregion

            #region Quantity

			int quantity = product.OrderMinimumQuantity;
			string key1 = "addtocart_{0}.EnteredQuantity".FormatWith(productId);
			string key2 = "addtocart_{0}.AddToCart.EnteredQuantity".FormatWith(productId);

			if (form.AllKeys.Contains(key1))
				int.TryParse(form[key1], out quantity);
			else if (form.AllKeys.Contains(key2))
				int.TryParse(form[key2], out quantity);

            #endregion

            //save item
            var cartType = (ShoppingCartType)shoppingCartTypeId;

			var addToCartContext = new AddToCartContext
			{
				Product = product,
				VariantQuery = query,
				CartType = cartType,
				CustomerEnteredPrice = customerEnteredPriceConverted,
				Quantity = quantity,
				AddRequiredProducts = true
			};

			_shoppingCartService.AddToCart(addToCartContext);

            #region Return result

            if (addToCartContext.Warnings.Count > 0)
            {
                //cannot be added to the cart/wishlist
                //let's display warnings
                return Json(new
                {
                    success = false,
                    message = addToCartContext.Warnings.ToArray()
                });
            }

            //added to the cart/wishlist
            switch (cartType)
            {
                case ShoppingCartType.Wishlist:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToWishlist", _localizationService.GetResource("ActivityLog.PublicStore.AddToWishlist"), product.Name);

                        if (_shoppingCartSettings.DisplayWishlistAfterAddingProduct)
                        {
                            //redirect to the wishlist page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("Wishlist"),
                            });
                        }
                        else
                        {
                            return Json(new
                            {
                                success = true
                            });
                        }
                    }
                case ShoppingCartType.ShoppingCart:
                default:
                    {
                        //activity log
                        _customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

                        if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                        {
                            //redirect to the shopping cart page
                            return Json(new
                            {
                                redirect = Url.RouteUrl("ShoppingCart"),
                            });
                        }
                        else
                        {
                            return Json(new
                            {
                                success = true
                            });
                        }
                    }
            }

            #endregion
        }

        [HttpPost]
        public ActionResult UploadFileProductAttribute(int productId, int productAttributeId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null || !product.Published || product.Deleted || product.IsSystemProduct)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                });
            }

            // ensure that this attribute belong to this product and has "file upload" type
            var pva = _productAttributeService
                .GetProductVariantAttributesByProductId(productId)
                .Where(pa => pa.ProductAttributeId == productAttributeId)
                .FirstOrDefault();

            if (pva == null || pva.AttributeControlType != AttributeControlType.FileUpload)
            {
                return Json(new
                {
                    success = false,
                    downloadGuid = Guid.Empty,
                });
            }

			var postedFile = Request.ToPostedFileResult();
			if (postedFile == null)
			{
				throw new ArgumentException(T("Common.NoFileUploaded"));
			}

            int fileMaxSize = _catalogSettings.FileUploadMaximumSizeBytes;
			if (postedFile.Size > fileMaxSize)
            {
                return Json(new
                {
                    success = false,
                    message = string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024)),
                    downloadGuid = Guid.Empty,
                });
            }

            var download = new Download
            {
                DownloadGuid = Guid.NewGuid(),
                UseDownloadUrl = false,
                DownloadUrl = "",
                ContentType = postedFile.ContentType,
                // we store filename without extension for downloads
                Filename = postedFile.FileTitle,
                Extension = postedFile.FileExtension,
                IsNew = true,
				IsTransient = true,
				UpdatedOnUtc = DateTime.UtcNow
			};

            _downloadService.InsertDownload(download, postedFile.Buffer);

            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadGuid = download.DownloadGuid,
            });
        }


        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult Cart(ProductVariantQuery query)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			// Allow to fill checkout attributes with values from query string.
			if (query.CheckoutAttributes.Any())
			{
				ParseAndSaveCheckoutAttributes(cart, query);
			}

			var model = new ShoppingCartModel();
			PrepareShoppingCartModel(model, cart);

			_httpContext.Session.SafeSet(CheckoutState.CheckoutStateSessionKey, new CheckoutState());

			return View(model);
        }

        [ChildActionOnly]
        public ActionResult OrderSummary(bool? prepareAndDisplayOrderReviewData)
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            var model = new ShoppingCartModel();

			PrepareShoppingCartModel(model, cart, 
				isEditable: false, 
				prepareEstimateShippingIfEnabled: false, 
				prepareAndDisplayOrderReviewData: prepareAndDisplayOrderReviewData.HasValue ? prepareAndDisplayOrderReviewData.Value : false);

			return PartialView(model);
        }
        
        // Ajax deletion
        [HttpPost]
        public ActionResult DeleteCartItem(int cartItemId, bool? wishlistItem)
        {
            var isWishlistItem = wishlistItem.GetValueOrDefault(false);

			if (!_permissionService.Authorize(isWishlistItem ? StandardPermissionProvider.EnableWishlist : StandardPermissionProvider.EnableShoppingCart))
			{
				return Json(new { success = false, showCheckoutButtons = true });
			}

            // Get shopping cart item.
			var cartType = (isWishlistItem ? ShoppingCartType.Wishlist : ShoppingCartType.ShoppingCart);
            var item = _workContext.CurrentCustomer.ShoppingCartItems.FirstOrDefault(x => x.Id == cartItemId && x.ShoppingCartType == cartType);

            if (item == null)
            {
				return Json(new { success = false, showCheckoutButtons = true, message = _localizationService.GetResource("ShoppingCart.DeleteCartItem.Failed") });
            }
            
            // Remove the cart item.
            _shoppingCartService.DeleteShoppingCartItem(item, ensureOnlyActiveCheckoutAttributes: true);

            // Create updated cart model.
            var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            var wishlist = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
            var cartHtml = String.Empty;
            var totalsHtml = String.Empty;
            var cartItemCount = 0;
			var showCheckoutButtons = true;

			if (cartType == ShoppingCartType.Wishlist)
            {
                var model = new WishlistModel();
                PrepareWishlistModel(model, wishlist);
                cartHtml = this.RenderPartialViewToString("WishlistItems", model);
                cartItemCount = wishlist.Count;
            }
            else
            {
                var model = new ShoppingCartModel();
                PrepareShoppingCartModel(model, cart);
                cartHtml = this.RenderPartialViewToString("CartItems", model);
                totalsHtml = InvokeAction("OrderTotals", "ShoppingCart", new RouteValueDictionary(new { isEditable = true }));
                cartItemCount = cart.Count;
				showCheckoutButtons = model.IsValidMinOrderSubtotal;
			}
            
            // Updated cart.
            return Json(new
            {
                cartItemCount = cartItemCount,
                success = true,
                message = _localizationService.GetResource("ShoppingCart.DeleteCartItem.Success"),
                cartHtml = cartHtml,
                totalsHtml = totalsHtml,
				showCheckoutButtons = showCheckoutButtons
			});
        }
       
        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("continueshopping")]
        public ActionResult ContinueShopping()
        {
			string returnUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
			return RedirectToReferrer(returnUrl);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("startcheckout")]
        public ActionResult StartCheckout(ProductVariantQuery query, bool? useRewardPoints)
        {
            var customer = _workContext.CurrentCustomer;
            var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            
            ParseAndSaveCheckoutAttributes(cart, query);

            //validate attributes
			string checkoutAttributes = customer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
			var checkoutAttributeWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributes, true);
            if (checkoutAttributeWarnings.Count > 0)
            {
                //something wrong, redisplay the page with warnings
                var model = new ShoppingCartModel();
                PrepareShoppingCartModel(model, cart, validateCheckoutAttributes: true);
                return View(model);
            }

            // reward points
            if (_rewardPointsSettings.Enabled)
            {   
                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, useRewardPoints, _storeContext.CurrentStore.Id);
            }

            //everything is OK
            if (customer.IsGuest())
            {
                if (_customerSettings.UserRegistrationType == UserRegistrationType.Disabled)
                {
                    return RedirectToAction("BillingAddress", "Checkout");
                }
                else if (_orderSettings.AnonymousCheckoutAllowed)
                {
					return RedirectToAction("Login", "Customer", new { checkoutAsGuest = true, returnUrl = Url.RouteUrl("ShoppingCart") });
                }
                else
                {
                    return new HttpUnauthorizedResult();
                }
            }
            else
            {
				return RedirectToRoute("Checkout");
            }
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applydiscountcouponcode")]
        public ActionResult ApplyDiscountCoupon(string discountcouponcode, ProductVariantQuery query)
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            ParseAndSaveCheckoutAttributes(cart, query);

            var model = new ShoppingCartModel();
			model.DiscountBox.IsWarning = true;

			if (!String.IsNullOrWhiteSpace(discountcouponcode))
			{
				var discount = _discountService.GetDiscountByCouponCode(discountcouponcode);
				var isDiscountValid = 
					discount != null &&
					discount.RequiresCouponCode &&
					_discountService.IsDiscountValid(discount, _workContext.CurrentCustomer, discountcouponcode);

				if (isDiscountValid)
				{
					_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.DiscountCouponCode, discountcouponcode);

					model.DiscountBox.Message = T("ShoppingCart.DiscountCouponCode.Applied");
					model.DiscountBox.IsWarning = false;
				}
				else
				{
					model.DiscountBox.Message = T("ShoppingCart.DiscountCouponCode.WrongDiscount");
				}
			}
			else
			{
				model.DiscountBox.Message = T("ShoppingCart.DiscountCouponCode.WrongDiscount");
			}

            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applygiftcardcouponcode")]
        public ActionResult ApplyGiftCard(string giftcardcouponcode, ProductVariantQuery query)
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            ParseAndSaveCheckoutAttributes(cart, query);

            var model = new ShoppingCartModel();
			model.GiftCardBox.IsWarning = true;

			if (!cart.IsRecurring())
			{
				if (!String.IsNullOrWhiteSpace(giftcardcouponcode))
				{
					var giftCard = _giftCardService.GetAllGiftCards(null, null, null, null, giftcardcouponcode).FirstOrDefault();
					var isGiftCardValid = giftCard != null && giftCard.IsGiftCardValid(_storeContext.CurrentStore.Id);

					if (isGiftCardValid)
					{
						_workContext.CurrentCustomer.ApplyGiftCardCouponCode(giftcardcouponcode);
						_customerService.UpdateCustomer(_workContext.CurrentCustomer);

						model.GiftCardBox.Message = T("ShoppingCart.GiftCardCouponCode.Applied");
						model.GiftCardBox.IsWarning = false;
					}
					else
					{
						model.GiftCardBox.Message = T("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
					}
				}
				else
				{
					model.GiftCardBox.Message = T("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
				}
			}
			else
			{
				model.GiftCardBox.Message = T("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");
			}

            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applyrewardpoints")]
        public ActionResult ApplyRewardPoints(bool useRewardPoints, ProductVariantQuery query)
        {
            var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            ParseAndSaveCheckoutAttributes(cart, query);

            var model = new ShoppingCartModel();
            model.RewardPoints.UseRewardPoints = useRewardPoints;

            _genericAttributeService.SaveAttribute(_services.WorkContext.CurrentCustomer, 
                SystemCustomerAttributeNames.UseRewardPointsDuringCheckout, useRewardPoints, _services.StoreContext.CurrentStore.Id);

            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("estimateshipping")]
        public ActionResult GetEstimateShipping(EstimateShippingModel shippingModel, ProductVariantQuery query)
        {
			var store = _storeContext.CurrentStore;
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

            ParseAndSaveCheckoutAttributes(cart, query);

            var model = new ShoppingCartModel();
            model.EstimateShipping.CountryId = shippingModel.CountryId;
            model.EstimateShipping.StateProvinceId = shippingModel.StateProvinceId;
            model.EstimateShipping.ZipPostalCode = shippingModel.ZipPostalCode;

            PrepareShoppingCartModel(model, cart, setEstimateShippingDefaultAddress: false);

            if (cart.RequiresShipping())
            {
				var shippingInfoUrl = Url.TopicUrl("ShippingInfo");
				if (shippingInfoUrl.HasValue())
				{
					model.EstimateShipping.ShippingInfoUrl = shippingInfoUrl;
				}

                var address = new Address
                {
                    CountryId = shippingModel.CountryId,
                    Country = shippingModel.CountryId.HasValue ? _countryService.GetCountryById(shippingModel.CountryId.Value) : null,
                    StateProvinceId  = shippingModel.StateProvinceId,
                    StateProvince = shippingModel.StateProvinceId.HasValue ? _stateProvinceService.GetStateProvinceById(shippingModel.StateProvinceId.Value) : null,
                    ZipPostalCode = shippingModel.ZipPostalCode,
                };

				var getShippingOptionResponse = _shippingService.GetShippingOptions(cart, address, "", store.Id);

                if (!getShippingOptionResponse.Success)
                {
					foreach (var error in getShippingOptionResponse.Errors)
					{
						model.EstimateShipping.Warnings.Add(error);
					}
                }
                else
                {
                    if (getShippingOptionResponse.ShippingOptions.Count > 0)
                    {
						var shippingMethods = _shippingService.GetAllShippingMethods(null, store.Id);

                        foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                        {
                            var soModel = new EstimateShippingModel.ShippingOptionModel
                            {
								ShippingMethodId = shippingOption.ShippingMethodId,
                                Name = shippingOption.Name,
                                Description = shippingOption.Description
                            };

                            //calculate discounted and taxed rate
                            Discount appliedDiscount = null;
                            decimal shippingTotal = _orderTotalCalculationService.AdjustShippingRate(
								shippingOption.Rate, cart, shippingOption, shippingMethods, out appliedDiscount);

                            decimal rateBase = _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer);
                            decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                            soModel.Price = _priceFormatter.FormatShippingPrice(rate, true);

                            model.EstimateShipping.ShippingOptions.Add(soModel);
                        }
                    }
                    else
                    {
                       model.EstimateShipping.Warnings.Add(T("Checkout.ShippingIsNotAllowed"));
                    }
                }
            }

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult OrderTotals(bool isEditable)
        {
            var customer = _workContext.CurrentCustomer;
            var currency = _workContext.WorkingCurrency;
            var store = _storeContext.CurrentStore;
            var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);
            
            var model = new OrderTotalsModel();
            model.IsEditable = isEditable;

            if (cart.Count > 0)
            {             
                model.Weight = decimal.Zero;

                foreach (var sci in cart) 
                {
                    model.Weight += sci.Item.Product.Weight * sci.Item.Quantity;
                }

                var measure = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
                if (measure != null)
                {
                    model.WeightMeasureUnitName = measure.Name;
                }

                // Subtotal
                decimal subtotalBase = decimal.Zero;
                decimal orderSubTotalDiscountAmountBase = decimal.Zero;
                Discount orderSubTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;

                _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

				subtotalBase = subTotalWithoutDiscountBase;

				decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, currency);
				model.SubTotal = _priceFormatter.FormatPrice(subtotal);

				if (orderSubTotalDiscountAmountBase > decimal.Zero)
				{
					decimal orderSubTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, currency);

					model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount);
					model.AllowRemovingSubTotalDiscount = orderSubTotalAppliedDiscount != null 
                        && orderSubTotalAppliedDiscount.RequiresCouponCode 
                        && !string.IsNullOrEmpty(orderSubTotalAppliedDiscount.CouponCode) 
                        && model.IsEditable;
				}
                

                //shipping info
                model.RequiresShipping = cart.RequiresShipping();
                if (model.RequiresShipping)
                {
                    decimal? shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, currency);
                        model.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, true);

                        //selected shipping method
						var shippingOption = customer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, store.Id);
                        if (shippingOption != null)
                            model.SelectedShippingMethod = shippingOption.Name;
                    }
                }

                //payment method fee
				string paymentMethodSystemName = customer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, store.Id);
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, paymentMethodSystemName);
                decimal paymentMethodAdditionalFeeWithTaxBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, customer);

                if (paymentMethodAdditionalFeeWithTaxBase != decimal.Zero)
                {
                    decimal paymentMethodAdditionalFeeWithTax = _currencyService.ConvertFromPrimaryStoreCurrency(paymentMethodAdditionalFeeWithTaxBase, currency);
                    model.PaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeWithTax, true);
                }

                //tax
                bool displayTax = true;
                bool displayTaxRates = true;
                if (_taxSettings.HideTaxInOrderSummary && _workContext.TaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    SortedDictionary<decimal, decimal> taxRates = null;
                    decimal shoppingCartTaxBase = _orderTotalCalculationService.GetTaxTotal(cart, out taxRates);
                    decimal shoppingCartTax = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, currency);

                    if (shoppingCartTaxBase == 0 && _taxSettings.HideZeroTax)
                    {
                        displayTax = false;
                        displayTaxRates = false;
                    }
                    else
                    {
                        displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                        displayTax = !displayTaxRates;
							
                        model.Tax = _priceFormatter.FormatPrice(shoppingCartTax, true, false);
                        foreach (var tr in taxRates)
                        {
							var rate = _priceFormatter.FormatTaxRate(tr.Key);
							var labelKey = "ShoppingCart.Totals.TaxRateLine" + (_workContext.TaxDisplayType == TaxDisplayType.IncludingTax ? "Incl" : "Excl");
							model.TaxRates.Add(new OrderTotalsModel.TaxRate
                            {
								Rate = rate,
                                Value = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value, currency), true, false),
								Label = _localizationService.GetResource(labelKey).FormatCurrent(rate)
                            });
                        }
                    }
                }

                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                model.DisplayWeight = _shoppingCartSettings.ShowWeight;
                model.ShowConfirmOrderLegalHint = _shoppingCartSettings.ShowConfirmOrderLegalHint;

				var minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
				if (!minOrderSubtotalAmountOk)
				{
					var minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, currency);
					model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), 
                        _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
				}

				// Cart total
                var cartTotal = _orderTotalCalculationService.GetShoppingCartTotal(cart);
                if (cartTotal.ConvertedFromPrimaryStoreCurrency.TotalAmount.HasValue)
                {
                    model.OrderTotal = _priceFormatter.FormatPrice(cartTotal.ConvertedFromPrimaryStoreCurrency.TotalAmount.Value, true, false);
                    if (cartTotal.ConvertedFromPrimaryStoreCurrency.RoundingAmount != decimal.Zero)
                    {
                        model.OrderTotalRounding = _priceFormatter.FormatPrice(cartTotal.ConvertedFromPrimaryStoreCurrency.RoundingAmount, true, false);
                    }
                }

                // Discount
                if (cartTotal.DiscountAmount > decimal.Zero)
                {
                    decimal orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(cartTotal.DiscountAmount, currency);
                    model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, true, false);
                    model.AllowRemovingOrderTotalDiscount = cartTotal.AppliedDiscount != null 
                        && cartTotal.AppliedDiscount.RequiresCouponCode
                        && !string.IsNullOrEmpty(cartTotal.AppliedDiscount.CouponCode) 
                        && model.IsEditable;
                }

                // Gift cards
                if (cartTotal.AppliedGiftCards != null && cartTotal.AppliedGiftCards.Count > 0)
                {
                    foreach (var appliedGiftCard in cartTotal.AppliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard
                        {
                            Id = appliedGiftCard.GiftCard.Id,
                            CouponCode =  appliedGiftCard.GiftCard.GiftCardCouponCode,
                        };

                        decimal amountCanBeUsed = _currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, currency);
                        gcModel.Amount = _priceFormatter.FormatPrice(-amountCanBeUsed, true, false);

                        decimal remainingAmountBase = appliedGiftCard.GiftCard.GetGiftCardRemainingAmount() - appliedGiftCard.AmountCanBeUsed;
                        decimal remainingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, currency);
                        gcModel.Remaining = _priceFormatter.FormatPrice(remainingAmount, true, false);
                        
                        model.GiftCards.Add(gcModel);
                    }
                }

                // Reward points
                if (cartTotal.RedeemedRewardPointsAmount > decimal.Zero)
                {
                    decimal redeemedRewardPointsAmountInCustomerCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(cartTotal.RedeemedRewardPointsAmount, currency);
                    model.RedeemedRewardPoints = cartTotal.RedeemedRewardPoints;
                    model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
                }

				// Credit balance.
				if (cartTotal.CreditBalance > decimal.Zero)
				{
					var convertedCreditBalance = _currencyService.ConvertFromPrimaryStoreCurrency(cartTotal.CreditBalance, currency);
					model.CreditBalance = _priceFormatter.FormatPrice(-convertedCreditBalance, true, false);
				}
            }
            
            return PartialView(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("removesubtotaldiscount", "removeordertotaldiscount", "removediscountcouponcode")]
        public ActionResult RemoveDiscountCoupon()
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
			var model = new ShoppingCartModel();

			_genericAttributeService.SaveAttribute<string>(_workContext.CurrentCustomer,
				 SystemCustomerAttributeNames.DiscountCouponCode, null);

            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("removegiftcard")]
        public ActionResult RemoveGiftardCode(int giftCardId)
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            var model = new ShoppingCartModel();

            var gc = _giftCardService.GetGiftCardById(giftCardId);
            if (gc != null)
            {
                _workContext.CurrentCustomer.RemoveGiftCardCouponCode(gc.GiftCardCouponCode);
                _customerService.UpdateCustomer(_workContext.CurrentCustomer);
            }

            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        public ActionResult OffCanvasCart()
        {
            var model = new OffCanvasCartModel
			{
				ShoppingCartEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart) && _shoppingCartSettings.MiniShoppingCartEnabled,
				WishlistEnabled = _services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist),
				CompareProductsEnabled = _catalogSettings.CompareProductsEnabled
			};

            return PartialView(model);
        }

        public ActionResult OffCanvasShoppingCart()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return Content("");

            var model = PrepareMiniShoppingCartModel();

            _httpContext.Session.SafeSet(CheckoutState.CheckoutStateSessionKey, new CheckoutState());

            return PartialView(model);
        }

        public ActionResult OffCanvasWishlist()
        {
            Customer customer = _workContext.CurrentCustomer;

            var cart = customer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
            var model = new WishlistModel();

            PrepareWishlistModel(model, cart, true);
            
            // reformat AttributeInfo: this is bad! Put this in PrepareMiniWishlistModel later.
            model.Items.Each(x =>
            {
                // don't display QuantityUnitName in OffCanvasWishlist
                x.QuantityUnitName = null;
                
                var sci = cart.Where(c => c.Item.Id == x.Id).FirstOrDefault();
                
                if (sci != null)
                {
                    x.AttributeInfo = _productAttributeFormatter.FormatAttributes(
                        sci.Item.Product,
                        sci.Item.AttributesXml,
                        null,
                        htmlEncode: false,
                        serapator: ", ",
                        renderPrices: false,
                        renderGiftCardAttributes: false,
                        allowHyperlinks: false);
                }
            });
            
            model.ThumbSize = _mediaSettings.MiniCartThumbPictureSize;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult UpdateCartItem(int sciItemId, int newQuantity, bool isCartPage = false, bool isWishlist = false)
        {
            if (!_permissionService.Authorize(isWishlist ? StandardPermissionProvider.EnableWishlist : StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            var warnings = new List<string>();
            warnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer, sciItemId, newQuantity, false));

            var cartHtml = String.Empty;
            var totalsHtml = String.Empty;
			var showCheckoutButtons = true;

            if (isCartPage)
            {
                var cart = _workContext.CurrentCustomer.GetCartItems(isWishlist ? ShoppingCartType.Wishlist : ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
                
                if (isWishlist)
                {
                    var model = new WishlistModel();
                    PrepareWishlistModel(model, cart);
                    cartHtml = this.RenderPartialViewToString("WishlistItems", model);
                }
                else
                {
                    var model = new ShoppingCartModel();
                    PrepareShoppingCartModel(model, cart);
                    cartHtml = this.RenderPartialViewToString("CartItems", model);
                    totalsHtml = InvokeAction("OrderTotals", "ShoppingCart", new RouteValueDictionary(new { isEditable = true }));
					showCheckoutButtons = model.IsValidMinOrderSubtotal;
				}
            }
			
            return Json(new
            {
                success = warnings.Count > 0 ? false : true,
                SubTotal = _shoppingCartService.GetFormattedCurrentCartSubTotal(),
                message = warnings,
                cartHtml,
                totalsHtml,
				showCheckoutButtons
            });
        }

		[HttpPost]
		public ActionResult CartSummary(bool cart = false, bool wishlist = false, bool compare = false)
		{
			var cartEnabled = cart && _services.Permissions.Authorize(StandardPermissionProvider.EnableShoppingCart) && _shoppingCartSettings.MiniShoppingCartEnabled;
			var wishlistEnabled = wishlist && _services.Permissions.Authorize(StandardPermissionProvider.EnableWishlist);
			var compareEnabled = compare && _catalogSettings.CompareProductsEnabled;

			int cartItemsCount = 0;
			int wishlistItemsCount = 0;
			int compareItemsCount = 0;

			decimal subtotal = 0;
			string subtotalFormatted = string.Empty;

			if (cartEnabled || wishlistEnabled)
			{
				var customer = _services.WorkContext.CurrentCustomer;

				if (cartEnabled)
				{
					var cartItems = _services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id);
					cartItemsCount = cartItems.GetTotalProducts();
					//cartItemsCount = _shoppingCartService.CountItems(customer, ShoppingCartType.ShoppingCart, _services.StoreContext.CurrentStore.Id);

					subtotal = _shoppingCartService.GetCurrentCartSubTotal(cartItems);
					if (subtotal != 0)
					{
						subtotalFormatted = _priceFormatter.FormatPrice(subtotal, true, false);
					}
				}

				if (wishlistEnabled)
				{
					//wishlistItemsCount = customer.CountProductsInCart(ShoppingCartType.Wishlist, _services.StoreContext.CurrentStore.Id);
					wishlistItemsCount = _shoppingCartService.CountItems(customer, ShoppingCartType.Wishlist, _services.StoreContext.CurrentStore.Id);
				}
			}

			if (compareEnabled)
			{
				compareItemsCount = _compareProductsService.GetComparedProductsCount();
			}

			return Json(new
			{
				CartItemsCount = cartItemsCount,
				CartSubTotal = subtotalFormatted,
				WishlistItemsCount = wishlistItemsCount,
				CompareItemsCount = compareItemsCount
			});
		}

		#endregion

		#region Wishlist

		[RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult Wishlist(Guid? customerGuid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            var customer = customerGuid.HasValue ? _customerService.GetCustomerByGuid(customerGuid.Value) : _workContext.CurrentCustomer;
            if (customer == null)
                return RedirectToRoute("HomePage");

			var cart = customer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
            var model = new WishlistModel();

            PrepareWishlistModel(model, cart, !customerGuid.HasValue);
            return View(model);
        }

		[ValidateInput(false)]
		[HttpPost, ActionName("Wishlist")]
		[FormValueRequired("addtocartbutton")]
		public ActionResult AddItemstoCartFromWishlist(Guid? customerGuid, FormCollection form)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
				return RedirectToRoute("HomePage");

			if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
				return RedirectToRoute("HomePage");

			var pageCustomer = customerGuid.HasValue
				? _customerService.GetCustomerByGuid(customerGuid.Value)
				: _workContext.CurrentCustomer;

			if (pageCustomer == null)
				return RedirectToRoute("HomePage");

			var store = _storeContext.CurrentStore;
			var pageCart = pageCustomer.GetCartItems(ShoppingCartType.Wishlist, store.Id);

			var allWarnings = new List<string>();
			var numberOfAddedItems = 0;
			var allIdsToAdd = form["addtocart"] != null 
				? form["addtocart"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList()
				: new List<int>();

			foreach (var sci in pageCart)
			{
				if (allIdsToAdd.Contains(sci.Item.Id))
				{
					var warnings = _shoppingCartService.Copy(sci, _workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, store.Id, true);

					if (warnings.Count == 0)
						numberOfAddedItems++;

					if (_shoppingCartSettings.MoveItemsFromWishlistToCart && !customerGuid.HasValue && 	warnings.Count == 0)
					{
						_shoppingCartService.DeleteShoppingCartItem(sci.Item);
					}
					allWarnings.AddRange(warnings);
				}
			}

			if (numberOfAddedItems > 0)
			{
				return RedirectToRoute("ShoppingCart");
			}

			var cart = pageCustomer.GetCartItems(ShoppingCartType.Wishlist, store.Id);
			var model = new WishlistModel();

			PrepareWishlistModel(model, cart, !customerGuid.HasValue);

			NotifyInfo(_localizationService.GetResource("Products.SelectProducts"), true);

			return View(model);
		}

		// ajax
		[HttpPost]
        [ActionName("MoveItemBetweenCartAndWishlist")]
        public ActionResult MoveItemBetweenCartAndWishlistAjax(int cartItemId, ShoppingCartType cartType, bool isCartPage = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart) || !_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
            {
                return Json(new
                {
                    success = false,
					message = _localizationService.GetResource("Common.NoProcessingSecurityIssue")
                });
            }

            var customer = _workContext.CurrentCustomer;
			var cart = customer.GetCartItems(cartType, _storeContext.CurrentStore.Id);
            var sci = cart.Where(x => x.Item.Id == cartItemId).FirstOrDefault();

			if (sci != null)
			{
				var warnings = _shoppingCartService.Copy(sci, customer, 
                    cartType == ShoppingCartType.Wishlist ? ShoppingCartType.ShoppingCart : ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id, true);

				if (_shoppingCartSettings.MoveItemsFromWishlistToCart && warnings.Count == 0) //no warnings ( already in the cart)
				{
					//let's remove the item from origin
					_shoppingCartService.DeleteShoppingCartItem(sci.Item);
				}

				if (warnings.Count == 0)
				{
                    var cartHtml = String.Empty;
                    var totalsHtml = String.Empty;
                    var message = String.Empty;
                    var cartItemCount = 0;

                    if (isCartPage)
                    {
                        if (cartType == ShoppingCartType.Wishlist)
                        {
                            var model = new WishlistModel();
                            var wishlist = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
                            PrepareWishlistModel(model, wishlist);
                            cartHtml = this.RenderPartialViewToString("WishlistItems", model);
                            message = _localizationService.GetResource("Products.ProductHasBeenAddedToTheCart");
                            cartItemCount = wishlist.Count;
                        }
                        else
                        {
                            var model = new ShoppingCartModel();
                            cart = customer.GetCartItems(cartType, _storeContext.CurrentStore.Id);
                            PrepareShoppingCartModel(model, cart);
                            cartHtml = this.RenderPartialViewToString("CartItems", model);
                            totalsHtml = InvokeAction("OrderTotals", "ShoppingCart", new RouteValueDictionary(new { isEditable = true }));
                            message = _localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist");
                            cartItemCount = cart.Count;
                        }
                    }

                    if (_shoppingCartSettings.DisplayCartAfterAddingProduct)
                    {
                        //redirect to the shopping cart page
                        return Json(new
                        {
                            redirect = Url.RouteUrl("ShoppingCart"),
                        });
                    }
                    
                    return Json(new
                    {
                        success = true,
                        wasMoved = _shoppingCartSettings.MoveItemsFromWishlistToCart,
                        message = message,
                        cartHtml = cartHtml,
                        totalsHtml = totalsHtml,
                        cartItemCount = cartItemCount
                    });
				}
			}

            return Json(new
            {
                success = false,
				message = _localizationService.GetResource("Products.ProductNotAddedToTheCart")
            });
        }

        [RequireHttpsByConfig(SslRequirement.Yes)]
		[GdprConsent]
		public ActionResult EmailWishlist()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToRoute("HomePage");

			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

            if (cart.Count == 0)
                return RedirectToRoute("HomePage");

            var model = new WishlistEmailAFriendModel()
            {
                YourEmailAddress = _workContext.CurrentCustomer.Email,
                DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage
            };
            return View(model);
        }

        [HttpPost, ActionName("EmailWishlist")]
        [FormValueRequired("send-email")]
        [ValidateCaptcha]
		[GdprConsent]
		public ActionResult EmailWishlistSend(WishlistEmailAFriendModel model, bool captchaValid)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist) || !_shoppingCartSettings.EmailWishlistEnabled)
                return RedirectToRoute("HomePage");

			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

            if (cart.Count == 0)
                return RedirectToRoute("HomePage");

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            //check whether the current customer is guest and ia allowed to email wishlist
            if (_workContext.CurrentCustomer.IsGuest() && !_shoppingCartSettings.AllowAnonymousUsersToEmailWishlist)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Wishlist.EmailAFriend.OnlyRegisteredUsers"));
            }

            if (ModelState.IsValid)
            {
                //email
                Services.MessageFactory.SendShareWishlistMessage(
					_workContext.CurrentCustomer,
					model.YourEmailAddress,
                    model.FriendEmail, 
					Core.Html.HtmlUtils.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Wishlist.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            ModelState.AddModelError("", _localizationService.GetResource("Common.Error.Sendmail"));
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage;
            return View(model);
        }

        #endregion

        // TODO: (mc) duplicate of output cache plugin method, find a place for it and remove duplicates
        private string InvokeAction(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            var viewContext = new ViewContext(
                   ControllerContext,
                   new WebFormView(ControllerContext, "tmp"),
                   ViewData,
                   TempData,
                   TextWriter.Null
            );

            var htmlHelper = new HtmlHelper(viewContext, new ViewPage());

            return htmlHelper.Action(actionName, controllerName, routeValues).ToString();
        }
    }
}
