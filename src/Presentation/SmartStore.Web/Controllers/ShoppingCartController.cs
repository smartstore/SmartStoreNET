﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
using SmartStore.Services.Catalog;
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
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI.Captcha;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.ShoppingCart;

namespace SmartStore.Web.Controllers
{
    public partial class ShoppingCartController : PublicControllerBase
    {
        #region Fields

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
        private readonly IWorkflowMessageService _workflowMessageService;
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
		private readonly PluginMediator _pluginMediator;
        private readonly IQuantityUnitService _quantityUnitService;

        #endregion

        #region Constructors

        public ShoppingCartController(IProductService productService,
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
            IWorkflowMessageService workflowMessageService,
            IPermissionService permissionService, IDeliveryTimeService deliveryTimeService,
            IDownloadService downloadService, ICacheManager cacheManager,
            IWebHelper webHelper, ICustomerActivityService customerActivityService,
			IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings, ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings, OrderSettings orderSettings,
            ShippingSettings shippingSettings, TaxSettings taxSettings,
            CaptchaSettings captchaSettings, AddressSettings addressSettings,
			HttpContextBase httpContext, PluginMediator pluginMediator,
            IQuantityUnitService quantityUnitService)
        {
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
            this._workflowMessageService = workflowMessageService;
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
			this._pluginMediator = pluginMediator;
            this._quantityUnitService = quantityUnitService;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected PictureModel PrepareCartItemPictureModel(Product product, int pictureSize, bool showDefaultPicture, string productName, string attributesXml)
        {
            if (product == null)
                throw new ArgumentNullException("product");

			var combination = _productAttributeParser.FindProductVariantAttributeCombination(product.Id, attributesXml);

            var pictureCacheKey = string.Format(ModelCacheEventConsumer.CART_PICTURE_MODEL_KEY, product.Id, combination == null ? 0 : combination.Id,
				pictureSize, true, _workContext.WorkingLanguage.Id, _webHelper.IsCurrentConnectionSecured(), _storeContext.CurrentStore.Id);

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

                return new PictureModel()
                {
                    PictureId = picture != null ? picture.Id : 0,
                    ImageUrl = _pictureService.GetPictureUrl(picture, pictureSize, showDefaultPicture),
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
			
			product.MergeWithCombination(item.AttributesXml);

			var model = new ShoppingCartModel.ShoppingCartItemModel()
			{
				Id = item.Id,
				Sku = product.Sku,
				ProductId = product.Id,
				ProductName = product.GetLocalized(x => x.Name),
				ProductSeName = product.GetSeName(),
				VisibleIndividually = product.VisibleIndividually,
				Quantity = item.Quantity,
				IsShipEnabled = product.IsShipEnabled,
				ShortDesc = product.GetLocalized(x => x.ShortDescription),
				ProductType = product.ProductType,
                BasePrice = product.GetBasePriceInfo(_localizationService, _priceFormatter, _currencyService, _taxService, _priceCalculationService, _workContext.WorkingCurrency),
                Weight = product.Weight
			};

			if (item.BundleItem != null)
			{
				model.BundlePerItemPricing = item.BundleItem.BundleProduct.BundlePerItemPricing;
				model.BundlePerItemShoppingCart = item.BundleItem.BundleProduct.BundlePerItemShoppingCart;

				model.AttributeInfo = _productAttributeFormatter.FormatAttributes(product, item.AttributesXml, _workContext.CurrentCustomer,
					renderPrices: false, renderGiftCardAttributes: true, allowHyperlinks: false);

				string bundleItemName = item.BundleItem.GetLocalized(x => x.Name);
				if (bundleItemName.HasValue())
					model.ProductName = bundleItemName;

				string bundleItemShortDescription = item.BundleItem.GetLocalized(x => x.ShortDescription);
				if (bundleItemShortDescription.HasValue())
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

            //if show measure Unit
            if (product.QuantityUnitId != null)
            { 
                var quantityUnit = _quantityUnitService.GetQuantityUnitById(product.QuantityUnitId);
                if(quantityUnit != null)
                    model.QuantityUnit = quantityUnit.GetLocalized(x => x.Name);
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
				decimal taxRate, shoppingCartItemSubTotalWithDiscountBase, shoppingCartItemSubTotalWithDiscount, shoppingCartItemSubTotalWithoutDiscountBase = decimal.Zero;

				if (_shoppingCartSettings.RoundPricesDuringCalculation)
				{
					// Gross > Net RoundFix
					shoppingCartItemSubTotalWithDiscountBase = Math.Round(_taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate), 2) * sci.Item.Quantity;
					shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
					model.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
					// display an applied discount amount
					shoppingCartItemSubTotalWithoutDiscountBase = Math.Round(_taxService.GetProductPrice(product, _priceCalculationService.GetUnitPrice(sci, false), out taxRate), 2) * sci.Item.Quantity;

				}
				else
				{
					shoppingCartItemSubTotalWithDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetSubTotal(sci, true), out taxRate);
					shoppingCartItemSubTotalWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemSubTotalWithDiscountBase, _workContext.WorkingCurrency);
					model.SubTotal = _priceFormatter.FormatPrice(shoppingCartItemSubTotalWithDiscount);
					// display an applied discount amount
					shoppingCartItemSubTotalWithoutDiscountBase = _taxService.GetProductPrice(product, _priceCalculationService.GetSubTotal(sci, false), out taxRate);
				}

				decimal shoppingCartItemDiscountBase = shoppingCartItemSubTotalWithoutDiscountBase - shoppingCartItemSubTotalWithDiscountBase;

				if (shoppingCartItemDiscountBase > decimal.Zero)
				{
					decimal shoppingCartItemDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartItemDiscountBase, _workContext.WorkingCurrency);
					model.Discount = _priceFormatter.FormatPrice(shoppingCartItemDiscount);
				}

                model.BasePrice = product.GetBasePriceInfo(
                    _localizationService, 
                    _priceFormatter,
                    _currencyService,
                    _taxService,
                    _priceCalculationService,
                    _workContext.WorkingCurrency,
                    (product.Price - _priceCalculationService.GetUnitPrice(sci, true)) * (-1)
                );
			}

			//picture
			if (item.BundleItem != null)
			{
				if (_shoppingCartSettings.ShowProductBundleImagesOnShoppingCart)
					model.Picture = PrepareCartItemPictureModel(product, _mediaSettings.CartThumbBundleItemPictureSize, true, model.ProductName, item.AttributesXml);
			}
			else
			{
				if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
					model.Picture = PrepareCartItemPictureModel(product, _mediaSettings.CartThumbPictureSize, true, model.ProductName, item.AttributesXml);
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

			var model = new WishlistModel.ShoppingCartItemModel()
			{
				Id = item.Id,
				Sku = product.Sku,
				ProductId = product.Id,
				ProductName = product.GetLocalized(x => x.Name),
				ProductSeName = product.GetSeName(),
				Quantity = item.Quantity,
				ShortDesc = product.GetLocalized(x => x.ShortDescription),
				ProductType = product.ProductType,
				VisibleIndividually = product.VisibleIndividually
			};

			if (item.BundleItem != null)
			{
				model.BundlePerItemPricing = item.BundleItem.BundleProduct.BundlePerItemPricing;
				model.BundlePerItemShoppingCart = item.BundleItem.BundleProduct.BundlePerItemShoppingCart;
				model.AttributeInfo = _productAttributeFormatter.FormatAttributes(product, item.AttributesXml, _workContext.CurrentCustomer,
					renderPrices: false, renderGiftCardAttributes: false, allowHyperlinks: false);

				string bundleItemName = item.BundleItem.GetLocalized(x => x.Name);
				if (bundleItemName.HasValue())
					model.ProductName = bundleItemName;

				string bundleItemShortDescription = item.BundleItem.GetLocalized(x => x.ShortDescription);
				if (bundleItemShortDescription.HasValue())
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
					model.Picture = PrepareCartItemPictureModel(product, _mediaSettings.CartThumbBundleItemPictureSize, true, model.ProductName, item.AttributesXml);
			}
			else
			{
				if (_shoppingCartSettings.ShowProductImagesOnShoppingCart)
					model.Picture = PrepareCartItemPictureModel(product, _mediaSettings.CartThumbPictureSize, true, model.ProductName, item.AttributesXml);
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

				string actionName;
				string controllerName;
				RouteValueDictionary routeValues;
				pm.Value.GetPaymentInfoRoute(out actionName, out controllerName, out routeValues);

				model.Items.Add(new ButtonPaymentMethodModel.ButtonPaymentMethodItem
				{
					ActionName = actionName,
					ControllerName = controllerName,
					RouteValues = routeValues
				});
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
			model.IsEditable = isEditable;
			model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
			model.ShowProductBundleImages = _shoppingCartSettings.ShowProductBundleImagesOnShoppingCart;
			model.ShowSku = _catalogSettings.ShowProductSku;
			var checkoutAttributesXml = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
			model.CheckoutAttributeInfo = HtmlUtils.ConvertPlainTextToTable(HtmlUtils.ConvertHtmlToPlainText(
				_checkoutAttributeFormatter.FormatAttributes(checkoutAttributesXml, _workContext.CurrentCustomer)
			));
			//model.CheckoutAttributeInfo = _checkoutAttributeFormatter.FormatAttributes(_workContext.CurrentCustomer.CheckoutAttributes, _workContext.CurrentCustomer);
			//model.CheckoutAttributeInfo = _checkoutAttributeFormatter.FormatAttributes(_workContext.CurrentCustomer.CheckoutAttributes, _workContext.CurrentCustomer, "", false);
			bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
			if (!minOrderSubtotalAmountOk)
			{
				decimal minOrderSubtotalAmount = _currencyService.ConvertFromPrimaryStoreCurrency(_orderSettings.MinOrderSubtotalAmount, _workContext.WorkingCurrency);
				model.MinOrderSubtotalWarning = string.Format(_localizationService.GetResource("Checkout.MinOrderSubtotalAmount"), _priceFormatter.FormatPrice(minOrderSubtotalAmount, true, false));
			}
			model.TermsOfServiceEnabled = _orderSettings.TermsOfServiceEnabled;

			//gift card and gift card boxes
			model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
			var discountCouponCode = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.DiscountCouponCode);
			var discount = _discountService.GetDiscountByCouponCode(discountCouponCode);
			if (discount != null &&
				discount.RequiresCouponCode &&
				_discountService.IsDiscountValid(discount, _workContext.CurrentCustomer))
				model.DiscountBox.CurrentCode = discount.CouponCode;
			model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;
			model.DisplayCommentBox = _shoppingCartSettings.ShowCommentBox;

			//cart warnings
			var cartWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributesXml, validateCheckoutAttributes);
			foreach (var warning in cartWarnings)
				model.Warnings.Add(warning);

			#endregion

			#region Checkout attributes

			var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes();
			if (!cart.RequiresShipping())
			{
				//remove attributes which require shippable products
				checkoutAttributes = checkoutAttributes.RemoveShippableAttributes();
			}
			foreach (var attribute in checkoutAttributes)
			{
				var caModel = new ShoppingCartModel.CheckoutAttributeModel()
				{
					Id = attribute.Id,
					Name = attribute.GetLocalized(x => x.Name),
					TextPrompt = attribute.GetLocalized(x => x.TextPrompt),
					IsRequired = attribute.IsRequired,
					AttributeControlType = attribute.AttributeControlType
				};

				if (attribute.ShouldHaveValues())
				{
					//values
					var caValues = _checkoutAttributeService.GetCheckoutAttributeValues(attribute.Id);
					foreach (var caValue in caValues)
					{
						var pvaValueModel = new ShoppingCartModel.CheckoutAttributeValueModel()
						{
							Id = caValue.Id,
							Name = caValue.GetLocalized(x => x.Name),
							IsPreSelected = caValue.IsPreSelected
						};
						caModel.Values.Add(pvaValueModel);

						//display price if allowed
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



				//set already selected attributes
				string selectedCheckoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
				switch (attribute.AttributeControlType)
				{
					case AttributeControlType.DropdownList:
					case AttributeControlType.RadioList:
					case AttributeControlType.ColorSquares:
					case AttributeControlType.Checkboxes:
						{
							if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
							{
								//clear default selection
								foreach (var item in caModel.Values)
									item.IsPreSelected = false;

								//select new values
								var selectedCaValues = _checkoutAttributeParser.ParseCheckoutAttributeValues(selectedCheckoutAttributes);
								foreach (var caValue in selectedCaValues)
									foreach (var item in caModel.Values)
										if (caValue.Id == item.Id)
											item.IsPreSelected = true;
							}
						}
						break;
					case AttributeControlType.TextBox:
					case AttributeControlType.MultilineTextbox:
						{
							if (!String.IsNullOrEmpty(selectedCheckoutAttributes))
							{
								var enteredText = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
								if (enteredText.Count > 0)
									caModel.DefaultValue = enteredText[0];
							}
						}
						break;
					case AttributeControlType.Datepicker:
						{
							//keep in mind my that the code below works only in the current culture
							var selectedDateStr = _checkoutAttributeParser.ParseValues(selectedCheckoutAttributes, attribute.Id);
							if (selectedDateStr.Count > 0)
							{
								DateTime selectedDate;
								if (DateTime.TryParseExact(selectedDateStr[0], "D", CultureInfo.CurrentCulture,
													   DateTimeStyles.None, out selectedDate))
								{
									//successfully parsed
									caModel.SelectedDay = selectedDate.Day;
									caModel.SelectedMonth = selectedDate.Month;
									caModel.SelectedYear = selectedDate.Year;
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
				}

				//payment info
				var selectedPaymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(
					 SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);

				var checkoutState = _httpContext.GetCheckoutState();
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
            var model = new MiniShoppingCartModel()
            {
                ShowProductImages = _shoppingCartSettings.ShowProductImagesInMiniShoppingCart,
                ThumbSize = _mediaSettings.MiniCartThumbPictureSize,
                //let's always display it
                DisplayShoppingCartButton = true,
                CurrentCustomerIsGuest = _workContext.CurrentCustomer.IsGuest(),
                AnonymousCheckoutAllowed = _orderSettings.AnonymousCheckoutAllowed,
            };

			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            model.TotalProducts = cart.GetTotalProducts();
            model.IgnoredProductsCount = Math.Max(0, cart.Count - _shoppingCartSettings.MiniShoppingCartProductNumber);

            if (cart.Count > 0)
            {
                //subtotal
                decimal subtotalBase = decimal.Zero;
                decimal orderSubTotalDiscountAmountBase = decimal.Zero;
                Discount orderSubTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;

                _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

                subtotalBase = subTotalWithoutDiscountBase;
                decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
                model.SubTotal = _priceFormatter.FormatPrice(subtotal);

                //a customer should visit the shopping cart page before going to checkout if:
                //1. "terms of services" are enabled (OBSOLETE now)
                //2. we have at least one checkout attribute
                //3. min order sub-total is OK
                var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes();
                if (!cart.RequiresShipping())
                {
                    //remove attributes which require shippable products
                    checkoutAttributes = checkoutAttributes.RemoveShippableAttributes();
                }
                bool minOrderSubtotalAmountOk = _orderProcessingService.ValidateMinOrderSubtotalAmount(cart);
                model.DisplayCheckoutButton = checkoutAttributes.Count == 0 && minOrderSubtotalAmountOk;

                //products. sort descending (recently added products)
                foreach (var sci in cart
                    .OrderByDescending(x => x.Item.Id)
                    .Take(_shoppingCartSettings.MiniShoppingCartProductNumber)
                    .ToList())
                {
                    var cartItemModel = new MiniShoppingCartModel.ShoppingCartItemModel()
                    {
                        Id = sci.Item.Id,
                        ProductId = sci.Item.Product.Id,
						ProductName = sci.Item.Product.GetLocalized(x => x.Name),
                        ProductSeName = sci.Item.Product.GetSeName(),
                        Quantity = sci.Item.Quantity,
                        AttributeInfo = _productAttributeFormatter.FormatAttributes(
                            sci.Item.Product, 
                            sci.Item.AttributesXml, 
                            null,
                            serapator: ", ", 
                            renderPrices: false, 
                            renderGiftCardAttributes: false, 
                            allowHyperlinks: false)
                    };

                    if (sci.Item.Product.ProductType == ProductType.BundledProduct)
                    {                        
                        var bundleItems = _productService.GetBundleItems(sci.Item.Product.Id);
                        foreach (var bundleItem in bundleItems)
                        {
                            var bundleItemModel = new MiniShoppingCartModel.ShoppingCartItemBundleItem();
                            bundleItemModel.ProductName = bundleItem.Item.Product.GetLocalized(x => x.Name);
                            var bundlePic = _pictureService.GetPicturesByProductId(bundleItem.Item.ProductId).FirstOrDefault();
                            if(bundlePic != null)
                                bundleItemModel.PictureUrl = _pictureService.GetPictureUrl(bundlePic.Id, 32);

                            bundleItemModel.ProductSeName = bundleItem.Item.Product.GetSeName();

                            if (!bundleItem.Item.HideThumbnail) 
                                cartItemModel.BundleItems.Add(bundleItemModel);
                        }
                    }

                    //unit prices
                    if (sci.Item.Product.CallForPrice)
                    {
                        cartItemModel.UnitPrice = _localizationService.GetResource("Products.CallForPrice");
                    }
                    else
                    {
						sci.Item.Product.MergeWithCombination(sci.Item.AttributesXml);

                        decimal taxRate = decimal.Zero;
                        decimal shoppingCartUnitPriceWithDiscountBase = _taxService.GetProductPrice(sci.Item.Product, _priceCalculationService.GetUnitPrice(sci, true), out taxRate);
                        decimal shoppingCartUnitPriceWithDiscount = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartUnitPriceWithDiscountBase, _workContext.WorkingCurrency);

                        cartItemModel.UnitPrice = _priceFormatter.FormatPrice(shoppingCartUnitPriceWithDiscount);
                    }

                    //picture
                    if (_shoppingCartSettings.ShowProductImagesInMiniShoppingCart)
                    {
                        cartItemModel.Picture = PrepareCartItemPictureModel(sci.Item.Product,
                            _mediaSettings.MiniCartThumbPictureSize, true, cartItemModel.ProductName, sci.Item.AttributesXml);
                    }

                    model.Items.Add(cartItemModel);
                }
            }

            return model;
        }

        [NonAction]
        protected void ParseAndSaveCheckoutAttributes(List<OrganizedShoppingCartItem> cart, FormCollection form)
        {
            string selectedAttributes = "";
            var checkoutAttributes = _checkoutAttributeService.GetAllCheckoutAttributes();
            if (!cart.RequiresShipping())
            {
                //remove attributes which require shippable products
                checkoutAttributes = checkoutAttributes.RemoveShippableAttributes();
            }
            foreach (var attribute in checkoutAttributes)
            {
                string controlId = string.Format("checkout_attribute_{0}", attribute.Id);
                switch (attribute.AttributeControlType)
                {
                    case AttributeControlType.DropdownList:
                    case AttributeControlType.RadioList:
                    case AttributeControlType.ColorSquares:
                        {
                            var rblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(rblAttributes))
                            {
                                int selectedAttributeId = int.Parse(rblAttributes);
                                if (selectedAttributeId > 0)
                                    selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                        attribute, selectedAttributeId.ToString());
                            }
                        }
                        break;
                    case AttributeControlType.Checkboxes:
                        {
                            var cblAttributes = form[controlId];
                            if (!String.IsNullOrEmpty(cblAttributes))
                            {
                                foreach (var item in cblAttributes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    int selectedAttributeId = int.Parse(item);
                                    if (selectedAttributeId > 0)
                                        selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                            attribute, selectedAttributeId.ToString());
                                }
                            }
                        }
                        break;
                    case AttributeControlType.TextBox:    
                    case AttributeControlType.MultilineTextbox:
                        {
                            var txtAttribute = form[controlId];
                            if (!String.IsNullOrEmpty(txtAttribute))
                            {
                                string enteredText = txtAttribute.Trim();
                                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                    attribute, enteredText);
                            }
                        }
                        break;
                    case AttributeControlType.Datepicker:
                        {
                            var date = form[controlId + "_day"];
                            var month = form[controlId + "_month"];
                            var year = form[controlId + "_year"];
                            DateTime? selectedDate = null;
                            try
                            {
                                selectedDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                            }
                            catch { }
                            if (selectedDate.HasValue)
                            {
                                selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes,
                                    attribute, selectedDate.Value.ToString("D"));
                            }
                        }
                        break;
                    case AttributeControlType.FileUpload:
                        {
                            var postedFile = this.Request.Files[controlId].ToPostedFileResult();
                            if (postedFile != null && postedFile.FileName.HasValue())
                            {
                                int fileMaxSize = _catalogSettings.FileUploadMaximumSizeBytes;
                                if (postedFile.Size > fileMaxSize)
                                {
                                    //TODO display warning
                                    //warnings.Add(string.Format(_localizationService.GetResource("ShoppingCart.MaximumUploadedFileSize"), (int)(fileMaxSize / 1024)));
                                }
                                else
                                {
                                    //save an uploaded file
                                    var download = new Download
                                    {
                                        DownloadGuid = Guid.NewGuid(),
                                        UseDownloadUrl = false,
                                        DownloadUrl = "",
                                        DownloadBinary = postedFile.Buffer,
                                        ContentType = postedFile.ContentType,
                                        Filename = postedFile.FileTitle,
                                        Extension = postedFile.FileExtension,
                                        IsNew = true
                                    };
                                    _downloadService.InsertDownload(download);
                                    //save attribute
                                    selectedAttributes = _checkoutAttributeParser.AddCheckoutAttribute(selectedAttributes, attribute, download.DownloadGuid.ToString());
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            //save checkout attributes
			_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer, SystemCustomerAttributeNames.CheckoutAttributes, selectedAttributes);
        }

        #endregion

        #region Shopping cart

        //add product to cart using AJAX
        //currently we use this method on catalog pages (category/manufacturer/etc)
        [HttpPost]
        public ActionResult AddProductSimple(int productId, bool forceredirection = false)
        {
            //current we support only ShoppingCartType.ShoppingCart
            const ShoppingCartType shoppingCartType = ShoppingCartType.ShoppingCart;

            var product = _productService.GetProductById(productId);
			if (product == null)
			{
				return Json(new
				{
					success = false,
					message = "No product found with the specified ID"
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

            //quantity to add
			var qtyToAdd = product.OrderMinimumQuantity > 0 ? product.OrderMinimumQuantity : 1;

			var allowedQuantities = product.ParseAllowedQuatities();
            if (allowedQuantities.Length > 0)
            {
                //cannot be added to the cart (requires a customer to select a quantity from dropdownlist)
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            //get standard warnings without attribute validations
            //first, try to find existing shopping cart item
			var cart = _workContext.CurrentCustomer.GetCartItems(shoppingCartType, _storeContext.CurrentStore.Id);

            var shoppingCartItem = _shoppingCartService.FindShoppingCartItemInTheCart(cart, shoppingCartType, product);
            
			//if we already have the same product in the cart, then use the total quantity to validate
            var quantityToValidate = shoppingCartItem != null ? shoppingCartItem.Item.Quantity + qtyToAdd : qtyToAdd;

			var addToCartWarnings = (List<string>)_shoppingCartService.GetShoppingCartItemWarnings(_workContext.CurrentCustomer, shoppingCartType,
				product, _storeContext.CurrentStore.Id, string.Empty, decimal.Zero, quantityToValidate, false, true, false, false, false, false);

            if (addToCartWarnings.Count > 0)
            {
                //cannot be added to the cart
                //let's display standard warnings
                return Json(new
                {
                    success = false,
                    message = addToCartWarnings.ToArray()
                });
            }

            //now let's try adding product to the cart (now including product attribute validation, etc)
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
                //cannot be added to the cart
                //but we do not display attribute and gift card warnings here. let's do it on the product details page
                return Json(new
                {
                    redirect = Url.RouteUrl("Product", new { SeName = product.GetSeName() }),
                });
            }

            //now product is in the cart
            //activity log
			_customerActivityService.InsertActivity("PublicStore.AddToShoppingCart", _localizationService.GetResource("ActivityLog.PublicStore.AddToShoppingCart"), product.Name);

            if (_shoppingCartSettings.DisplayCartAfterAddingProduct || forceredirection)
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
                message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart"))
            });

        }

        //add product to cart using AJAX
		//currently we use this method on the product details pages
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult AddProduct(int productId, int shoppingCartTypeId, FormCollection form)
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

			int quantity = 1;
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
				AttributeForm = form,
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
                                success = true,
                                message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheWishlist.Link"), Url.RouteUrl("Wishlist")),
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
                                success = true,
                                message = string.Format(_localizationService.GetResource("Products.ProductHasBeenAddedToTheCart.Link"), Url.RouteUrl("ShoppingCart"))
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
            if (product == null || !product.Published || product.Deleted)
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
				throw new ArgumentException("No file uploaded");
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
                DownloadBinary = postedFile.Buffer,
                ContentType = postedFile.ContentType,
                // we store filename without extension for downloads
                Filename = postedFile.FileTitle,
                Extension = postedFile.FileExtension,
                IsNew = true,
				IsTransient = true
            };
            _downloadService.InsertDownload(download);

            return Json(new
            {
                success = true,
                message = _localizationService.GetResource("ShoppingCart.FileUploaded"),
                downloadGuid = download.DownloadGuid,
            });
        }


        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
        public ActionResult Cart()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

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

        //update all shopping cart items on the page
        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("updatecart")]
        public ActionResult UpdateCartAll(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            var allIdsToRemove = form["removefromcart"] != null ? 
				form["removefromcart"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                bool remove = allIdsToRemove.Contains(sci.Item.Id);
				if (remove)
				{
					_shoppingCartService.DeleteShoppingCartItem(sci.Item, ensureOnlyActiveCheckoutAttributes: true);
				}
				else
				{
					foreach (string formKey in form.AllKeys)
					{
						if (formKey.Equals(string.Format("itemquantity{0}", sci.Item.Id), StringComparison.InvariantCultureIgnoreCase))
						{
							int newQuantity = sci.Item.Quantity;
							if (int.TryParse(form[formKey], out newQuantity))
							{
								var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
									sci.Item.Id, newQuantity, true);
								innerWarnings.Add(sci.Item.Id, currSciWarnings);
							}
							break;
						}
					}
				}
            }

            //updated cart
			cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			var model = new ShoppingCartModel();
            PrepareShoppingCartModel(model, cart);

            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
                var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
				if (sciModel != null)
				{
					foreach (var w in warnings)
					{
						if (!sciModel.Warnings.Contains(w))
							sciModel.Warnings.Add(w);
					}
				}
            }
            return View(model);
        }

        //update a certain shopping cart item on the page
        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired(FormValueRequirement.StartsWith, "updatecartitem-")]
        public ActionResult UpdateCartItem(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            //get shopping cart item identifier
            int sciId = 0;
			foreach (var formValue in form.AllKeys)
			{
				if (formValue.StartsWith("updatecartitem-", StringComparison.InvariantCultureIgnoreCase))
					sciId = Convert.ToInt32(formValue.Substring("updatecartitem-".Length));
			}

            //get shopping cart item
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			var sci = cart.FirstOrDefault(x => x.Item.Id == sciId);
            if (sci == null)
            {
                return RedirectToRoute("ShoppingCart");
            }

            //update the cart item
            var warnings = new List<string>();
			foreach (string formKey in form.AllKeys)
			{
				if (formKey.Equals(string.Format("itemquantity{0}", sci.Item.Id), StringComparison.InvariantCultureIgnoreCase))
				{
					int newQuantity = sci.Item.Quantity;
					if (int.TryParse(form[formKey], out newQuantity))
					{
						warnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,	sci.Item.Id, newQuantity, true));
					}
					break;
				}
			}

            //updated cart
			cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			var model = new ShoppingCartModel();
            PrepareShoppingCartModel(model, cart);

            //update current warnings... find model
			var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
			if (sciModel != null)
			{
				foreach (var w in warnings)
				{
					if (!sciModel.Warnings.Contains(w))
						sciModel.Warnings.Add(w);
				}
			}
            return View(model);
        }

        //remove a certain shopping cart item on the page
        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired(FormValueRequirement.StartsWith, "removefromcart-")]
        public ActionResult RemoveCartItem(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            //get shopping cart item identifier
            int sciId = 0;
			foreach (var formValue in form.AllKeys)
			{
				if (formValue.StartsWith("removefromcart-", StringComparison.InvariantCultureIgnoreCase))
					sciId = Convert.ToInt32(formValue.Substring("removefromcart-".Length));
			}

            //get shopping cart item
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			var sci = cart.FirstOrDefault(x => x.Item.Id == sciId);
            if (sci == null)
            {
                return RedirectToRoute("ShoppingCart");
            }

            //remove the cart item
            _shoppingCartService.DeleteShoppingCartItem(sci.Item, ensureOnlyActiveCheckoutAttributes: true);

            //updated cart
			cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			var model = new ShoppingCartModel();
            PrepareShoppingCartModel(model, cart);

            return View(model);
        }

        // Ajax deletion
        [HttpPost]
        public ActionResult DeleteCartItem(int cartItemId, bool? wishlistItem)
        {
            bool isWishlistItem = wishlistItem.GetValueOrDefault(false);

            if (!_permissionService.Authorize(isWishlistItem ? StandardPermissionProvider.EnableWishlist : StandardPermissionProvider.EnableShoppingCart))
                return Json(new { success = false });

            //get shopping cart item
			var cartType = (isWishlistItem ? ShoppingCartType.Wishlist : ShoppingCartType.ShoppingCart);
            var item = _workContext.CurrentCustomer.ShoppingCartItems.FirstOrDefault(x => x.Id == cartItemId && x.ShoppingCartType == cartType);

            if (item == null)
            {
				return Json(new { success = false, message = _localizationService.GetResource("ShoppingCart.DeleteCartItem.Failed") });
            }
            
            //remove the cart item
            _shoppingCartService.DeleteShoppingCartItem(item, ensureOnlyActiveCheckoutAttributes: true);

            //updated cart
            return Json(new 
            { 
                success = true,
				message = _localizationService.GetResource("ShoppingCart.DeleteCartItem.Success")
            });
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("continueshopping")]
        public ActionResult ContinueShopping()
        {
			string returnUrl = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.LastContinueShoppingPage, _storeContext.CurrentStore.Id);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToRoute("HomePage");
            }
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("startcheckout")]
        public ActionResult StartCheckout(FormCollection form)
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);
            
            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            //validate attributes
			string checkoutAttributes = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.CheckoutAttributes, _genericAttributeService);
			var checkoutAttributeWarnings = _shoppingCartService.GetShoppingCartWarnings(cart, checkoutAttributes, true);
            if (checkoutAttributeWarnings.Count > 0)
            {
                //something wrong, redisplay the page with warnings
                var model = new ShoppingCartModel();
                PrepareShoppingCartModel(model, cart, validateCheckoutAttributes: true);
                return View(model);
            }

            //everything is OK
            if (_workContext.CurrentCustomer.IsGuest())
            {
                if (_orderSettings.AnonymousCheckoutAllowed)
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
        public ActionResult ApplyDiscountCoupon(string discountcouponcode, FormCollection form)
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            var model = new ShoppingCartModel();
            if (!String.IsNullOrWhiteSpace(discountcouponcode))
            {
                var discount = _discountService.GetDiscountByCouponCode(discountcouponcode);
                bool isDiscountValid = discount != null && 
                    discount.RequiresCouponCode &&
                    _discountService.IsDiscountValid(discount, _workContext.CurrentCustomer, discountcouponcode);
                if (isDiscountValid)
                {
					_genericAttributeService.SaveAttribute(_workContext.CurrentCustomer,
						 SystemCustomerAttributeNames.DiscountCouponCode, discountcouponcode);
                    model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.Applied");
                }
                else
                {
                    model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");
                }
            }
            else
                model.DiscountBox.Message = _localizationService.GetResource("ShoppingCart.DiscountCouponCode.WrongDiscount");

            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("applygiftcardcouponcode")]
        public ActionResult ApplyGiftCard(string giftcardcouponcode, FormCollection form)
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            var model = new ShoppingCartModel();
            if (!cart.IsRecurring())
            {
                if (!String.IsNullOrWhiteSpace(giftcardcouponcode))
                {
                    var giftCard = _giftCardService.GetAllGiftCards(null, null, null, null, giftcardcouponcode).FirstOrDefault();
                    bool isGiftCardValid = giftCard != null && giftCard.IsGiftCardValid();
                    if (isGiftCardValid)
                    {
                        _workContext.CurrentCustomer.ApplyGiftCardCouponCode(giftcardcouponcode);
                        _customerService.UpdateCustomer(_workContext.CurrentCustomer);
                        model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.Applied");
                    }
                    else
                        model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
                }
                else
                    model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.WrongGiftCard");
            }
            else
                model.GiftCardBox.Message = _localizationService.GetResource("ShoppingCart.GiftCardCouponCode.DontWorkWithAutoshipProducts");

            PrepareShoppingCartModel(model, cart);
            return View(model);
        }

        [ValidateInput(false)]
        [HttpPost, ActionName("Cart")]
        [FormValueRequired("estimateshipping")]
        public ActionResult GetEstimateShipping(EstimateShippingModel shippingModel, FormCollection form)
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

            //parse and save checkout attributes
            ParseAndSaveCheckoutAttributes(cart, form);

            var model = new ShoppingCartModel();
            model.EstimateShipping.CountryId = shippingModel.CountryId;
            model.EstimateShipping.StateProvinceId = shippingModel.StateProvinceId;
            model.EstimateShipping.ZipPostalCode = shippingModel.ZipPostalCode;
            PrepareShoppingCartModel(model, cart, setEstimateShippingDefaultAddress: false);

            if (cart.RequiresShipping())
            {
                var address = new Address()
                {
                    CountryId = shippingModel.CountryId,
                    Country = shippingModel.CountryId.HasValue ? _countryService.GetCountryById(shippingModel.CountryId.Value) : null,
                    StateProvinceId  = shippingModel.StateProvinceId,
                    StateProvince = shippingModel.StateProvinceId.HasValue ? _stateProvinceService.GetStateProvinceById(shippingModel.StateProvinceId.Value) : null,
                    ZipPostalCode = shippingModel.ZipPostalCode,
                };
				GetShippingOptionResponse getShippingOptionResponse = _shippingService
					 .GetShippingOptions(cart, address, "", _storeContext.CurrentStore.Id);
                if (!getShippingOptionResponse.Success)
                {
                    foreach (var error in getShippingOptionResponse.Errors)
                        model.EstimateShipping.Warnings.Add(error);
                }
                else
                {
                    if (getShippingOptionResponse.ShippingOptions.Count > 0)
                    {
						var shippingMethods = _shippingService.GetAllShippingMethods();

                        foreach (var shippingOption in getShippingOptionResponse.ShippingOptions)
                        {
                            var soModel = new EstimateShippingModel.ShippingOptionModel()
                            {
								ShippingMethodId = shippingOption.ShippingMethodId,
                                Name = shippingOption.Name,
                                Description = shippingOption.Description,

                            };
                            //calculate discounted and taxed rate
                            Discount appliedDiscount = null;
                            decimal shippingTotal = _orderTotalCalculationService.AdjustShippingRate(
								shippingOption.Rate, cart, shippingOption.Name, shippingMethods, out appliedDiscount);

                            decimal rateBase = _taxService.GetShippingPrice(shippingTotal, _workContext.CurrentCustomer);
                            decimal rate = _currencyService.ConvertFromPrimaryStoreCurrency(rateBase, _workContext.WorkingCurrency);
                            soModel.Price = _priceFormatter.FormatShippingPrice(rate, false /*true*/);
                            model.EstimateShipping.ShippingOptions.Add(soModel);
                        }
                    }
                    else
                    {
                       model.EstimateShipping.Warnings.Add(_localizationService.GetResource("Checkout.ShippingIsNotAllowed"));
                    }
                }
            }

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult OrderTotals(bool isEditable)
        {
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			var model = new OrderTotalsModel();
            model.IsEditable = isEditable;

            if (cart.Count > 0)
            {             
 
                //weight
                model.Weight = decimal.Zero;

                foreach (var sci in cart) 
                {
                    model.Weight += sci.Item.Product.Weight * sci.Item.Quantity;
                }

                //subtotal
                decimal subtotalBase = decimal.Zero;
                decimal orderSubTotalDiscountAmountBase = decimal.Zero;
                Discount orderSubTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;

                _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount,
                    out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

				subtotalBase = subTotalWithoutDiscountBase;

				decimal subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
				model.SubTotal = _priceFormatter.FormatPrice(subtotal);

				if (orderSubTotalDiscountAmountBase > decimal.Zero)
				{
					decimal orderSubTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderSubTotalDiscountAmountBase, _workContext.WorkingCurrency);

					model.SubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountAmount);
					model.AllowRemovingSubTotalDiscount = orderSubTotalAppliedDiscount != null && orderSubTotalAppliedDiscount.RequiresCouponCode &&
						!String.IsNullOrEmpty(orderSubTotalAppliedDiscount.CouponCode) && model.IsEditable;
				}
                

                //shipping info
                model.RequiresShipping = cart.RequiresShipping();
                if (model.RequiresShipping)
                {
                    decimal? shoppingCartShippingBase = _orderTotalCalculationService.GetShoppingCartShippingTotal(cart);
                    if (shoppingCartShippingBase.HasValue)
                    {
                        decimal shoppingCartShipping = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartShippingBase.Value, _workContext.WorkingCurrency);
                        model.Shipping = _priceFormatter.FormatShippingPrice(shoppingCartShipping, true);

                        //selected shipping method
						var shippingOption = _workContext.CurrentCustomer.GetAttribute<ShippingOption>(SystemCustomerAttributeNames.SelectedShippingOption, _storeContext.CurrentStore.Id);
                        if (shippingOption != null)
                            model.SelectedShippingMethod = shippingOption.Name;
                    }
                }

                //payment method fee
				string paymentMethodSystemName = _workContext.CurrentCustomer.GetAttribute<string>(SystemCustomerAttributeNames.SelectedPaymentMethod, _storeContext.CurrentStore.Id);
                decimal paymentMethodAdditionalFee = _paymentService.GetAdditionalHandlingFee(cart, paymentMethodSystemName);
                decimal paymentMethodAdditionalFeeWithTaxBase = _taxService.GetPaymentMethodAdditionalFee(paymentMethodAdditionalFee, _workContext.CurrentCustomer);

                if (paymentMethodAdditionalFeeWithTaxBase != decimal.Zero)
                {
                    decimal paymentMethodAdditionalFeeWithTax = _currencyService.ConvertFromPrimaryStoreCurrency(paymentMethodAdditionalFeeWithTaxBase, _workContext.WorkingCurrency);
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
                    decimal shoppingCartTax = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTaxBase, _workContext.WorkingCurrency);

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
								model.TaxRates.Add(new OrderTotalsModel.TaxRate()
                                    {
										Rate = rate,
                                        Value = _priceFormatter.FormatPrice(_currencyService.ConvertFromPrimaryStoreCurrency(tr.Value, _workContext.WorkingCurrency), true, false),
										Label = _localizationService.GetResource(labelKey).FormatCurrent(rate)
                                    });
                            }
                        }
                }
                model.DisplayTaxRates = displayTaxRates;
                model.DisplayTax = displayTax;

                model.DisplayWeight = _shoppingCartSettings.ShowWeight;

                //total
                decimal orderTotalDiscountAmountBase = decimal.Zero;
                Discount orderTotalAppliedDiscount = null;
                List<AppliedGiftCard> appliedGiftCards = null;
                int redeemedRewardPoints = 0;
                decimal redeemedRewardPointsAmount = decimal.Zero;

                decimal? shoppingCartTotalBase = _orderTotalCalculationService.GetShoppingCartTotal(cart,
                    out orderTotalDiscountAmountBase, out orderTotalAppliedDiscount,
                    out appliedGiftCards, out redeemedRewardPoints, out redeemedRewardPointsAmount);

                if (shoppingCartTotalBase.HasValue)
                {
                    decimal shoppingCartTotal = _currencyService.ConvertFromPrimaryStoreCurrency(shoppingCartTotalBase.Value, _workContext.WorkingCurrency);
                    model.OrderTotal = _priceFormatter.FormatPrice(shoppingCartTotal, true, false);
                }

                //discount
                if (orderTotalDiscountAmountBase > decimal.Zero)
                {
                    decimal orderTotalDiscountAmount = _currencyService.ConvertFromPrimaryStoreCurrency(orderTotalDiscountAmountBase, _workContext.WorkingCurrency);
                    model.OrderTotalDiscount = _priceFormatter.FormatPrice(-orderTotalDiscountAmount, true, false);
                    model.AllowRemovingOrderTotalDiscount = orderTotalAppliedDiscount != null && orderTotalAppliedDiscount.RequiresCouponCode &&
                        !String.IsNullOrEmpty(orderTotalAppliedDiscount.CouponCode) && model.IsEditable;
                }

                //gift cards
                if (appliedGiftCards != null && appliedGiftCards.Count > 0)
                {
                    foreach (var appliedGiftCard in appliedGiftCards)
                    {
                        var gcModel = new OrderTotalsModel.GiftCard()
                            {
                                Id = appliedGiftCard.GiftCard.Id,
                                CouponCode =  appliedGiftCard.GiftCard.GiftCardCouponCode,
                            };
                        decimal amountCanBeUsed = _currencyService.ConvertFromPrimaryStoreCurrency(appliedGiftCard.AmountCanBeUsed, _workContext.WorkingCurrency);
                        gcModel.Amount = _priceFormatter.FormatPrice(-amountCanBeUsed, true, false);

                        decimal remainingAmountBase = appliedGiftCard.GiftCard.GetGiftCardRemainingAmount() - appliedGiftCard.AmountCanBeUsed;
                        decimal remainingAmount = _currencyService.ConvertFromPrimaryStoreCurrency(remainingAmountBase, _workContext.WorkingCurrency);
                        gcModel.Remaining = _priceFormatter.FormatPrice(remainingAmount, true, false);
                        
                        model.GiftCards.Add(gcModel);
                    }
                }

                //reward points
                if (redeemedRewardPointsAmount > decimal.Zero)
                {
                    decimal redeemedRewardPointsAmountInCustomerCurrency = _currencyService.ConvertFromPrimaryStoreCurrency(redeemedRewardPointsAmount, _workContext.WorkingCurrency);
                    model.RedeemedRewardPoints = redeemedRewardPoints;
                    model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-redeemedRewardPointsAmountInCustomerCurrency, true, false);
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

        //[ChildActionOnly]
        public ActionResult FlyoutShoppingCart()
        {
            if (!_shoppingCartSettings.MiniShoppingCartEnabled)
                return Content("");

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return Content("");

            var model = PrepareMiniShoppingCartModel();

			_httpContext.Session.SafeSet(CheckoutState.CheckoutStateSessionKey, new CheckoutState());

            return PartialView(model);
        }

        // Ajax
        public ActionResult ShoppingCartSummary(bool isWishlist = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
			{
				return Json(new
				{
					success = false,
					message = _localizationService.GetResource("Common.NoProcessingSecurityIssue")
				});
			}

            decimal subtotal = 0;
            var cart = _workContext.CurrentCustomer.GetCartItems(isWishlist ? ShoppingCartType.Wishlist : ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			if (cart.Count > 0)
            {
                //subtotal
                decimal subtotalBase = decimal.Zero;
                decimal orderSubTotalDiscountAmountBase = decimal.Zero;
                Discount orderSubTotalAppliedDiscount = null;
                decimal subTotalWithoutDiscountBase = decimal.Zero;
                decimal subTotalWithDiscountBase = decimal.Zero;

                _orderTotalCalculationService.GetShoppingCartSubTotal(cart,
                    out orderSubTotalDiscountAmountBase, out orderSubTotalAppliedDiscount, out subTotalWithoutDiscountBase, out subTotalWithDiscountBase);

                subtotalBase = subTotalWithoutDiscountBase;
                subtotal = _currencyService.ConvertFromPrimaryStoreCurrency(subtotalBase, _workContext.WorkingCurrency);
            }

            return Json(new
			{ 
                TotalProducts = cart.GetTotalProducts(),
                SubTotal = _priceFormatter.FormatPrice(subtotal)
            }, JsonRequestBehavior.AllowGet);
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

        //update all wishlist cart items on the page
        [ValidateInput(false)]
        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired("updatecart")]
        public ActionResult UpdateWishlistAll(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

            var allIdsToRemove = form["removefromcart"] != null ? 
				form["removefromcart"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();

            //current warnings <cart item identifier, warnings>
            var innerWarnings = new Dictionary<int, IList<string>>();
            foreach (var sci in cart)
            {
                bool remove = allIdsToRemove.Contains(sci.Item.Id);
				if (remove)
				{
					_shoppingCartService.DeleteShoppingCartItem(sci.Item);
				}
				else
				{
					foreach (string formKey in form.AllKeys)
					{
						if (formKey.Equals(string.Format("itemquantity{0}", sci.Item.Id), StringComparison.InvariantCultureIgnoreCase))
						{
							int newQuantity = sci.Item.Quantity;
							if (int.TryParse(form[formKey], out newQuantity))
							{
								var currSciWarnings = _shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,
									sci.Item.Id, newQuantity, true);
								innerWarnings.Add(sci.Item.Id, currSciWarnings);
							}
							break;
						}
					}
				}
            }

            //updated wishlist
			cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
			var model = new WishlistModel();

            PrepareWishlistModel(model, cart);

            //update current warnings
            foreach (var kvp in innerWarnings)
            {
                //kvp = <cart item identifier, warnings>
                var sciId = kvp.Key;
                var warnings = kvp.Value;
                //find model
				var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
				if (sciModel != null)
				{
					foreach (var w in warnings)
					{
						if (!sciModel.Warnings.Contains(w))
							sciModel.Warnings.Add(w);
					}
				}
            }
            return View(model);
        }

        //update a certain wishlist cart item on the page
        [ValidateInput(false)]
        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired(FormValueRequirement.StartsWith, "updatecartitem-")]
        public ActionResult UpdateWishlistItem(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            //get wishlist cart item identifier
            int sciId = 0;
			foreach (var formValue in form.AllKeys)
			{
				if (formValue.StartsWith("updatecartitem-", StringComparison.InvariantCultureIgnoreCase))
					sciId = Convert.ToInt32(formValue.Substring("updatecartitem-".Length));
			}

            //get shopping cart item
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

			var sci = cart.FirstOrDefault(x => x.Item.Id == sciId);
            if (sci == null)
            {
                return RedirectToRoute("Wishlist");
            }

            //update the wishlist cart item
            var warnings = new List<string>();
			foreach (string formKey in form.AllKeys)
			{
				if (formKey.Equals(string.Format("itemquantity{0}", sci.Item.Id), StringComparison.InvariantCultureIgnoreCase))
				{
					int newQuantity = sci.Item.Quantity;
					if (int.TryParse(form[formKey], out newQuantity))
					{
						warnings.AddRange(_shoppingCartService.UpdateShoppingCartItem(_workContext.CurrentCustomer,	sci.Item.Id, newQuantity, true));
					}
					break;
				}
			}

            //updated wishlist
			cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
            var model = new WishlistModel();

            PrepareWishlistModel(model, cart);

            //update current warnings
            //find model
			var sciModel = model.Items.FirstOrDefault(x => x.Id == sciId);
			if (sciModel != null)
			{
				foreach (var w in warnings)
				{
					if (!sciModel.Warnings.Contains(w))
						sciModel.Warnings.Add(w);
				}
			}
            return View(model);
        }

        //remove a certain wishlist cart item on the page
        [ValidateInput(false)]
        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired(FormValueRequirement.StartsWith, "removefromcart-")]
        public ActionResult RemoveWishlistItem(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            //get wishlist cart item identifier
            int sciId = 0;
			foreach (var formValue in form.AllKeys)
			{
				if (formValue.StartsWith("removefromcart-", StringComparison.InvariantCultureIgnoreCase))
					sciId = Convert.ToInt32(formValue.Substring("removefromcart-".Length));
			}

            //get wishlist cart item
			var cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

			var sci = cart.FirstOrDefault(x => x.Item.Id == sciId);
            if (sci == null)
            {
                return RedirectToRoute("Wishlist");
            }

            //remove the wishlist cart item
            _shoppingCartService.DeleteShoppingCartItem(sci.Item);

            //updated wishlist
			cart = _workContext.CurrentCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
			var model = new WishlistModel();

            PrepareWishlistModel(model, cart);

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

			var pageCart = pageCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

            var allWarnings = new List<string>();
            var numberOfAddedItems = 0;
            var allIdsToAdd = form["addtocart"] != null ? 
				form["addtocart"].Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToList() : new List<int>();

			foreach (var sci in pageCart)
            {
                if (allIdsToAdd.Contains(sci.Item.Id))
                {
					var warnings = _shoppingCartService.Copy(sci, _workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id, true);

					if (warnings.Count == 0)
						numberOfAddedItems++;

                    if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                        !customerGuid.HasValue && //own wishlist
                        warnings.Count == 0) //no warnings (already in the cart)
                    {
                        //let's remove the item from wishlist
                        _shoppingCartService.DeleteShoppingCartItem(sci.Item);
                    }
                    allWarnings.AddRange(warnings);
                }
            }

            if (numberOfAddedItems > 0)
            {
                //redirect to the shopping cart page
                return RedirectToRoute("ShoppingCart");
            }
            else
            {
                //no items added. redisplay the wishlist page
				var cart = pageCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id); 
                var model = new WishlistModel();

                PrepareWishlistModel(model, cart, !customerGuid.HasValue);

				this.NotifyInfo(_localizationService.GetResource("Products.SelectProducts"), true);

                return View(model);
            }
        }

        //add a certain wishlist cart item on the page to the shopping cart
        [ValidateInput(false)]
        [HttpPost, ActionName("Wishlist")]
        [FormValueRequired(FormValueRequirement.StartsWith, "addtocart-")]
        public ActionResult AddOneItemtoCartFromWishlist(Guid? customerGuid, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.EnableShoppingCart))
                return RedirectToRoute("HomePage");

            if (!_permissionService.Authorize(StandardPermissionProvider.EnableWishlist))
                return RedirectToRoute("HomePage");

            //get wishlist cart item identifier
            int sciId = 0;
			foreach (var formValue in form.AllKeys)
			{
				if (formValue.StartsWith("addtocart-", StringComparison.InvariantCultureIgnoreCase))
					sciId = Convert.ToInt32(formValue.Substring("addtocart-".Length));
			}

            //get wishlist cart item
            var pageCustomer = customerGuid.HasValue
                ? _customerService.GetCustomerByGuid(customerGuid.Value)
                : _workContext.CurrentCustomer;
            if (pageCustomer == null)
                return RedirectToRoute("HomePage");

			var pageCart = pageCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

			var sci = pageCart.FirstOrDefault(x => x.Item.Id == sciId);
            if (sci == null)
            {
                return RedirectToRoute("Wishlist");
            }

			var warnings = _shoppingCartService.Copy(sci, _workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id, true);

            if (_shoppingCartSettings.MoveItemsFromWishlistToCart && //settings enabled
                        !customerGuid.HasValue && //own wishlist
                        warnings.Count == 0) //no warnings ( already in the cart)
            {
                //let's remove the item from wishlist
                _shoppingCartService.DeleteShoppingCartItem(sci.Item);
            }

            if (warnings.Count == 0)
            {
                //redirect to the shopping cart page
                return RedirectToRoute("ShoppingCart");
            }
            else
            {
                //no items added. redisplay the wishlist page
				var cart = pageCustomer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);
                var model = new WishlistModel();

                PrepareWishlistModel(model, cart, !customerGuid.HasValue);

                return View(model);
            }
        }

        // ajax
        [HttpPost]
        [ActionName("AddOneItemtoCartFromWishlist")]
        public ActionResult AddOneItemtoCartFromWishlistAjax(int cartItemId)
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
			var wishlist = customer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id);

            var sci = wishlist.Where(x => x.Item.Id == cartItemId).FirstOrDefault();

			if (sci != null)
			{
				var warnings = _shoppingCartService.Copy(sci, customer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id, true);

				if (_shoppingCartSettings.MoveItemsFromWishlistToCart && warnings.Count == 0) //no warnings ( already in the cart)
				{
					//let's remove the item from wishlist
					_shoppingCartService.DeleteShoppingCartItem(sci.Item);
				}

				if (warnings.Count == 0)
				{
					return Json(new
					{
						success = true,
						wasMoved = _shoppingCartSettings.MoveItemsFromWishlistToCart,
						message = _localizationService.GetResource("Products.ProductHasBeenAddedToTheCart")
					});
				}
			}

            return Json(new
            {
                success = false,
				message = _localizationService.GetResource("Products.ProductNotAddedToTheCart")
            });
        }

        [RequireHttpsByConfigAttribute(SslRequirement.Yes)]
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
        [CaptchaValidator]
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
                _workflowMessageService.SendWishlistEmailAFriendMessage(_workContext.CurrentCustomer,
                        _workContext.WorkingLanguage.Id, model.YourEmailAddress,
                        model.FriendEmail, Core.Html.HtmlUtils.FormatText(model.PersonalMessage, false, true, false, false, false, false));

                model.SuccessfullySent = true;
                model.Result = _localizationService.GetResource("Wishlist.EmailAFriend.SuccessfullySent");

                return View(model);
            }

            //If we got this far, something failed, redisplay form
            ModelState.AddModelError("", _localizationService.GetResource("Common.Error.Sendmail"));
            model.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnEmailWishlistToFriendPage;
            return View(model);
        }

        public ActionResult FlyoutWishlist()
        {
            Customer customer = _workContext.CurrentCustomer;

            var cart = customer.GetCartItems(ShoppingCartType.Wishlist, _storeContext.CurrentStore.Id, true);
			var model = new WishlistModel();

            PrepareWishlistModel(model, cart, true);
            
            // TODO: MiniWishlistModel analog zu MiniCart implementieren
            model.Items = model.Items.Take(_shoppingCartSettings.MiniShoppingCartProductNumber).ToList();
            
            // reformat AttributeInfo: this is bad! Put this in PrepareMiniWishlistModel later.
            model.Items.Each(x =>
            {
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

            model.IgnoredProductsCount = Math.Max(0, cart.Count - _shoppingCartSettings.MiniShoppingCartProductNumber);
            model.ThumbSize = _mediaSettings.MiniCartThumbPictureSize;

            return PartialView(model);
        }

        #endregion
    }
}
