using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Admin.Models.Orders;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Search;
using SmartStore.Core.Security;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Extensions;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Pdf;
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Tax;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Pdf;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
	public partial class OrderController : AdminControllerBase
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderReportService _orderReportService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly ICurrencyService _currencyService;
        private readonly IEncryptionService _encryptionService;
        private readonly IPaymentService _paymentService;
        private readonly IMeasureService _measureService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IProductService _productService;
	    private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
	    private readonly IProductAttributeService _productAttributeService;
	    private readonly IProductAttributeParser _productAttributeParser;
	    private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGiftCardService _giftCardService;
        private readonly IDownloadService _downloadService;
	    private readonly IShipmentService _shipmentService;
        private readonly ITaxService _taxService;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly IEventPublisher _eventPublisher;
		private readonly ICustomerService _customerService;
		private readonly PluginMediator _pluginMediator;
		private readonly IAffiliateService _affiliateService;
		private readonly ICustomerActivityService _customerActivityService;
		private readonly ICatalogSearchService _catalogSearchService;
        private readonly IPdfConverter _pdfConverter;

        private readonly CatalogSettings _catalogSettings;
        private readonly TaxSettings _taxSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly AddressSettings _addressSettings;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly SearchSettings _searchSettings;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        #endregion

        #region Ctor

        public OrderController(
            IOrderService orderService, 
            IOrderReportService orderReportService, 
			IOrderProcessingService orderProcessingService,
            IDateTimeHelper dateTimeHelper, 
			IPriceFormatter priceFormatter,
			ILocalizationService localizationService,
            IWorkContext workContext, 
			ICurrencyService currencyService,
            IEncryptionService encryptionService, 
			IPaymentService paymentService,
            IMeasureService measureService,
            IAddressService addressService, 
			ICountryService countryService,
            IStateProvinceService stateProvinceService, 
			IProductService productService,
            ICategoryService categoryService, 
			IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService, 
			IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter, 
			IShoppingCartService shoppingCartService,
            IGiftCardService giftCardService, 
			IDownloadService downloadService,
			IShipmentService shipmentService, 
			ITaxService taxService,
			IPriceCalculationService priceCalculationService,
			IEventPublisher eventPublisher,
			ICustomerService customerService,
			PluginMediator pluginMediator,
			IAffiliateService affiliateService,
			ICustomerActivityService customerActivityService,
			ICatalogSearchService catalogSearchService,
            IPdfConverter pdfConverter,
            CatalogSettings catalogSettings, 
			TaxSettings taxSettings,
            MeasureSettings measureSettings, 
			PdfSettings pdfSettings, 
			AddressSettings addressSettings,
			AdminAreaSettings adminAreaSettings,
			SearchSettings searchSettings,
            ShoppingCartSettings shoppingCartSettings)
		{
            _orderService = orderService;
            _orderReportService = orderReportService;
            _orderProcessingService = orderProcessingService;
            _dateTimeHelper = dateTimeHelper;
            _priceFormatter = priceFormatter;
            _localizationService = localizationService;
            _workContext = workContext;
            _currencyService = currencyService;
            _encryptionService = encryptionService;
            _paymentService = paymentService;
            _measureService = measureService;
            _addressService = addressService;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _productService = productService;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _productAttributeFormatter = productAttributeFormatter;
            _shoppingCartService = shoppingCartService;
            _giftCardService = giftCardService;
            _downloadService = downloadService;
            _shipmentService = shipmentService;
			_taxService = taxService;
			_priceCalculationService = priceCalculationService;
			_eventPublisher = eventPublisher;
			_customerService = customerService;
			_pluginMediator = pluginMediator;
			_affiliateService = affiliateService;
			_customerActivityService = customerActivityService;
			_catalogSearchService = catalogSearchService;
            _pdfConverter = pdfConverter;

            _catalogSettings = catalogSettings;
            _taxSettings = taxSettings;
            _measureSettings = measureSettings;
            _pdfSettings = pdfSettings;
            _addressSettings = addressSettings;
			_adminAreaSettings = adminAreaSettings;
			_searchSettings = searchSettings;
            _shoppingCartSettings = shoppingCartSettings;
		}
        
        #endregion

        #region Utilities

        [NonAction]
        protected void PrepareOrderDetailsModel(OrderModel model, Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            if (model == null)
                throw new ArgumentNullException("model");

            var language = _workContext.WorkingLanguage;
			var store = Services.StoreService.GetStoreById(order.StoreId);
			var currency = store?.PrimaryStoreCurrency ?? _workContext.WorkingCurrency;

            model.Id = order.Id;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
			model.StatusOrder = order.OrderStatus;
            model.OrderNumber = order.GetOrderNumber();
            model.OrderGuid = order.OrderGuid;
			model.StoreName = store != null ? store.Name : "".NaIfEmpty();
            model.CustomerId = order.CustomerId;
			model.CustomerName = order.Customer.GetFullName();
            model.CustomerIp = order.CustomerIp;
            model.VatNumber = order.VatNumber;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, DateTimeKind.Utc);
			model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(order.UpdatedOnUtc, DateTimeKind.Utc);
            model.DisplayPdfInvoice = _pdfSettings.Enabled;
            model.AllowCustomersToSelectTaxDisplayType = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            model.TaxDisplayType = _taxSettings.TaxDisplayType;
            model.AffiliateId = order.AffiliateId;
            model.CustomerComment = order.CustomerOrderComment;
			model.HasNewPaymentNotification = order.HasNewPaymentNotification;
			model.AcceptThirdPartyEmailHandOver = order.AcceptThirdPartyEmailHandOver;

			if (order.AffiliateId != 0)
			{
				var affiliate = _affiliateService.GetAffiliateById(order.AffiliateId);
                if (affiliate != null && affiliate.Address != null)
                {
                    model.AffiliateFullName = affiliate.Address.GetFullName();
                }
			}

            #region Order totals

            // Subtotal.
            model.OrderSubtotalInclTax = _priceFormatter.FormatPrice(order.OrderSubtotalInclTax, true, currency, language, true);
            model.OrderSubtotalExclTax = _priceFormatter.FormatPrice(order.OrderSubtotalExclTax, true, currency, language, false);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;
            // Discount (applied to order subtotal).
            var orderSubtotalDiscountInclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountInclTax, true, currency, language, true);
            var orderSubtotalDiscountExclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountExclTax, true, currency, language, false);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
            {
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            }
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
            {
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            }
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            // Shipping.
            model.OrderShippingInclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingInclTax, true, currency, language, true);
            model.OrderShippingExclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingExclTax, true, currency, language, false);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;

            // Payment method additional fee.
            if (order.PaymentMethodAdditionalFeeInclTax != decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeInclTax, true, currency, language, true);
                model.PaymentMethodAdditionalFeeExclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeExclTax, true, currency, language, false);
            }
            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;

            // Tax.
            var taxRates = order.TaxRatesDictionary;
            var displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
            var displayTax = !displayTaxRates;
            foreach (var tr in order.TaxRatesDictionary)
            {
                model.TaxRates.Add(new OrderModel.TaxRate
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = _priceFormatter.FormatPrice(tr.Value, true, false),
                });
            }
            model.Tax = _priceFormatter.FormatPrice(order.OrderTax, true, false);
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

			// Discount.
			if (order.OrderDiscount > 0)
			{
				model.OrderTotalDiscount = _priceFormatter.FormatPrice(-order.OrderDiscount, true, false);
			}
            model.OrderTotalDiscountValue = order.OrderDiscount;

            if (order.OrderTotalRounding != decimal.Zero)
            {
                model.OrderTotalRounding = _priceFormatter.FormatPrice(order.OrderTotalRounding, true, false);
            }
            model.OrderTotalRoundingValue = order.OrderTotalRounding;

            // Gift cards.
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                model.GiftCards.Add(new OrderModel.GiftCard
                {
                    CouponCode = gcuh.GiftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-gcuh.UsedValue, true, false),
                });
            }

            // Reward points.
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-order.RedeemedRewardPointsEntry.UsedAmount, true, false);
            }

			// Credit balance.
			if (order.CreditBalance > decimal.Zero)
			{
				model.CreditBalance = _priceFormatter.FormatPrice(-order.CreditBalance, true, false);
			}
			model.CreditBalanceValue = order.CreditBalance;

			// Total.
			model.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
            model.OrderTotalValue = order.OrderTotal;

			// Refunded amount.
			if (order.RefundedAmount > decimal.Zero)
			{
				model.RefundedAmount = _priceFormatter.FormatPrice(order.RefundedAmount, true, false);
			}

            #endregion

            #region Payment info

            if (order.AllowStoringCreditCardNumber)
            {
                model.CardType = _encryptionService.DecryptText(order.CardType);
                model.CardName = _encryptionService.DecryptText(order.CardName);
                model.CardNumber = _encryptionService.DecryptText(order.CardNumber);
                model.CardCvv2 = _encryptionService.DecryptText(order.CardCvv2);
                model.AllowStoringCreditCardNumber = true;

                // Expiration date.
                var cardExpirationMonthDecrypted = _encryptionService.DecryptText(order.CardExpirationMonth);
                if (cardExpirationMonthDecrypted.HasValue() && cardExpirationMonthDecrypted != "0")
                {
                    model.CardExpirationMonth = cardExpirationMonthDecrypted;
                }
                var cardExpirationYearDecrypted = _encryptionService.DecryptText(order.CardExpirationYear);
                if (cardExpirationYearDecrypted.HasValue() && cardExpirationYearDecrypted != "0")
                {
                    model.CardExpirationYear = cardExpirationYearDecrypted;
                }
            }
            else
            {
                var maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
                if (maskedCreditCardNumberDecrypted.HasValue())
                {
                    model.CardNumber = maskedCreditCardNumberDecrypted;
                }
            }

            if (order.AllowStoringDirectDebit)
            {
                model.DirectDebitAccountHolder = _encryptionService.DecryptText(order.DirectDebitAccountHolder);
                model.DirectDebitAccountNumber = _encryptionService.DecryptText(order.DirectDebitAccountNumber);
                model.DirectDebitBankCode = _encryptionService.DecryptText(order.DirectDebitBankCode);
                model.DirectDebitBankName = _encryptionService.DecryptText(order.DirectDebitBankName);
                model.DirectDebitBIC = _encryptionService.DecryptText(order.DirectDebitBIC);
                model.DirectDebitCountry = _encryptionService.DecryptText(order.DirectDebitCountry);
                model.DirectDebitIban = _encryptionService.DecryptText(order.DirectDebitIban);

                model.AllowStoringDirectDebit = true;
            }

            var pm = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
			if (pm != null)
			{
				model.DisplayCompletePaymentNote = order.PaymentStatus == PaymentStatus.Pending && pm.Value.CanRePostProcessPayment(order);
				model.PaymentMethod = _pluginMediator.GetLocalizedFriendlyName(pm.Metadata);
			}
			else
			{
				model.PaymentMethod = order.PaymentMethodSystemName;
			}

            // Purchase order number (we have to find a better to inject this information because it's related to a certain plugin).
            if (order.PaymentMethodSystemName.IsCaseInsensitiveEqual("SmartStore.PurchaseOrderNumber"))
            {
                model.DisplayPurchaseOrderNumber = true;
                model.PurchaseOrderNumber = order.PurchaseOrderNumber;
            }

            // Payment transaction info.
            model.PaymentMethodSystemName = order.PaymentMethodSystemName;
            model.AuthorizationTransactionId = order.AuthorizationTransactionId;
            model.CaptureTransactionId = order.CaptureTransactionId;
            model.SubscriptionTransactionId = order.SubscriptionTransactionId;
			model.AuthorizationTransactionResult = order.AuthorizationTransactionResult;
			model.CaptureTransactionResult = order.CaptureTransactionResult;
			model.StatusPayment = order.PaymentStatus;
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);

            // Payment method buttons.
            model.CanCancelOrder = _orderProcessingService.CanCancelOrder(order);
			model.CanCompleteOrder = _orderProcessingService.CanCompleteOrder(order);
            model.CanCapture = _orderProcessingService.CanCapture(order);
            model.CanMarkOrderAsPaid = _orderProcessingService.CanMarkOrderAsPaid(order);
            model.CanRefund = _orderProcessingService.CanRefund(order);
            model.CanRefundOffline = _orderProcessingService.CanRefundOffline(order);
            model.CanPartiallyRefund = _orderProcessingService.CanPartiallyRefund(order, decimal.Zero);
            model.CanPartiallyRefundOffline = _orderProcessingService.CanPartiallyRefundOffline(order, decimal.Zero);
            model.CanVoid = _orderProcessingService.CanVoid(order);
            model.CanVoidOffline = _orderProcessingService.CanVoidOffline(order);

            model.MaxAmountToRefund = order.OrderTotal - order.RefundedAmount;
			model.MaxAmountToRefundFormatted =	_priceFormatter.FormatPrice(model.MaxAmountToRefund, true, currency, language, false, false);

            // Recurring payment record.
            var recurringPayment = _orderService.SearchRecurringPayments(0, 0, order.Id, null, true).FirstOrDefault();
            if (recurringPayment != null)
            {
                model.RecurringPaymentId = recurringPayment.Id;
            }

            #endregion

            #region Billing & shipping info

            model.BillingAddress = order.BillingAddress.ToModel();
            model.BillingAddress.FirstNameEnabled = true;
            model.BillingAddress.FirstNameRequired = true;
            model.BillingAddress.LastNameEnabled = true;
            model.BillingAddress.LastNameRequired = true;
            model.BillingAddress.EmailEnabled = true;
            model.BillingAddress.EmailRequired = true;
            model.BillingAddress.ValidateEmailAddress = _addressSettings.ValidateEmailAddress;
            model.BillingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
            model.BillingAddress.CompanyRequired = _addressSettings.CompanyRequired;
            model.BillingAddress.CountryEnabled = _addressSettings.CountryEnabled;
            model.BillingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
            model.BillingAddress.CityEnabled = _addressSettings.CityEnabled;
            model.BillingAddress.CityRequired = _addressSettings.CityRequired;
            model.BillingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
            model.BillingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
            model.BillingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
            model.BillingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
            model.BillingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
            model.BillingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
            model.BillingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
            model.BillingAddress.PhoneRequired = _addressSettings.PhoneRequired;
            model.BillingAddress.FaxEnabled = _addressSettings.FaxEnabled;
            model.BillingAddress.FaxRequired = _addressSettings.FaxRequired;

            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
			model.StatusShipping = order.ShippingStatus;

            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.ShippingAddress = order.ShippingAddress.ToModel();
                model.ShippingAddress.FirstNameEnabled = true;
                model.ShippingAddress.FirstNameRequired = true;
                model.ShippingAddress.LastNameEnabled = true;
                model.ShippingAddress.LastNameRequired = true;
                model.ShippingAddress.EmailEnabled = true;
                model.ShippingAddress.EmailRequired = true;
                model.ShippingAddress.ValidateEmailAddress = _addressSettings.ValidateEmailAddress;
                model.ShippingAddress.CompanyEnabled = _addressSettings.CompanyEnabled;
                model.ShippingAddress.CompanyRequired = _addressSettings.CompanyRequired;
                model.ShippingAddress.CountryEnabled = _addressSettings.CountryEnabled;
                model.ShippingAddress.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
                model.ShippingAddress.CityEnabled = _addressSettings.CityEnabled;
                model.ShippingAddress.CityRequired = _addressSettings.CityRequired;
                model.ShippingAddress.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
                model.ShippingAddress.StreetAddressRequired = _addressSettings.StreetAddressRequired;
                model.ShippingAddress.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
                model.ShippingAddress.StreetAddress2Required = _addressSettings.StreetAddress2Required;
                model.ShippingAddress.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
                model.ShippingAddress.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
                model.ShippingAddress.PhoneEnabled = _addressSettings.PhoneEnabled;
                model.ShippingAddress.PhoneRequired = _addressSettings.PhoneRequired;
                model.ShippingAddress.FaxEnabled = _addressSettings.FaxEnabled;
                model.ShippingAddress.FaxRequired = _addressSettings.FaxRequired;

                model.IsShippable = true;
                model.ShippingMethod = order.ShippingMethod;
				model.CanAddNewShipments = order.CanAddItemsToShipment();

				var googleAddressQuery = string.Concat(
					order.ShippingAddress.Address1,
					" ",
					order.ShippingAddress.ZipPostalCode,
					" ",
					order.ShippingAddress.City,
					" ",
					order.ShippingAddress.Country != null ? order.ShippingAddress.Country.Name : "");

				var googleMapsUrl = CommonHelper.GetAppSetting<string>("g:MapsUrl");

				model.ShippingAddressGoogleMapsUrl = googleMapsUrl.FormatInvariant(language.UniqueSeoCode.EmptyNull().ToLower(), Server.UrlEncode(googleAddressQuery));
			}

            #endregion

            #region Products

            model.CheckoutAttributeInfo = HtmlUtils.ConvertPlainTextToTable(HtmlUtils.ConvertHtmlToPlainText(order.CheckoutAttributeDescription));
            //model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;
            //model.CheckoutAttributeInfo = _checkoutAttributeFormatter.FormatAttributes(_workContext.CurrentCustomer.CheckoutAttributes, _workContext.CurrentCustomer, "", false);
            var hasDownloadableItems = false;
            foreach (var orderItem in order.OrderItems)
            {
                if (orderItem.Product.IsDownload)
                    hasDownloadableItems = true;

                orderItem.Product.MergeWithCombination(orderItem.AttributesXml);
                var orderItemModel = new OrderModel.OrderItemModel
                {
                    Id = orderItem.Id,
					ProductId = orderItem.ProductId,
					ProductName = orderItem.Product.GetLocalized(x => x.Name),
                    Sku = orderItem.Product.Sku,
					ProductType = orderItem.Product.ProductType,
					ProductTypeName = orderItem.Product.GetProductTypeLabel(_localizationService),
					ProductTypeLabelHint = orderItem.Product.ProductTypeLabelHint,
                    Quantity = orderItem.Quantity,
                    IsDownload = orderItem.Product.IsDownload,
                    DownloadCount = orderItem.DownloadCount,
                    DownloadActivationType = orderItem.Product.DownloadActivationType,
                    IsDownloadActivated = orderItem.IsDownloadActivated,
                    LicenseDownloadId = orderItem.LicenseDownloadId
                };

				if (orderItem.Product.ProductType == ProductType.BundledProduct && orderItem.BundleData.HasValue())
				{
					var bundleData = orderItem.GetBundleData();

					orderItemModel.BundlePerItemPricing = orderItem.Product.BundlePerItemPricing;
					orderItemModel.BundlePerItemShoppingCart = bundleData.Any(x => x.PerItemShoppingCart);

					foreach (var bundleItem in bundleData)
					{
						var bundleItemModel = new OrderModel.BundleItemModel
						{
							ProductId = bundleItem.ProductId,
							Sku = bundleItem.Sku,
							ProductName = bundleItem.ProductName,
							ProductSeName = bundleItem.ProductSeName,
							VisibleIndividually = bundleItem.VisibleIndividually,
							Quantity = bundleItem.Quantity,
							DisplayOrder = bundleItem.DisplayOrder,
							AttributeInfo = bundleItem.AttributesInfo
						};

						if (orderItemModel.BundlePerItemShoppingCart)
						{
							bundleItemModel.PriceWithDiscount = _priceFormatter.FormatPrice(bundleItem.PriceWithDiscount, true, currency, language, false);
						}

						orderItemModel.BundleItems.Add(bundleItemModel);
					}
				}

                // Unit price.
                orderItemModel.UnitPriceInclTaxValue = orderItem.UnitPriceInclTax;
                orderItemModel.UnitPriceExclTaxValue = orderItem.UnitPriceExclTax;
				orderItemModel.TaxRate = orderItem.TaxRate;
                orderItemModel.UnitPriceInclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceInclTax, true, currency, language, true, true);
                orderItemModel.UnitPriceExclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceExclTax, true, currency, language, false, true);
                // Discounts.
                orderItemModel.DiscountInclTaxValue = orderItem.DiscountAmountInclTax;
                orderItemModel.DiscountExclTaxValue = orderItem.DiscountAmountExclTax;
                orderItemModel.DiscountInclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountInclTax, true, currency, language, true, true);
                orderItemModel.DiscountExclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountExclTax, true, currency, language, false, true);
                // Subtotal.
                orderItemModel.SubTotalInclTaxValue = orderItem.PriceInclTax;
                orderItemModel.SubTotalExclTaxValue = orderItem.PriceExclTax;
                orderItemModel.SubTotalInclTax = _priceFormatter.FormatPrice(orderItem.PriceInclTax, true, currency, language, true, true);
                orderItemModel.SubTotalExclTax = _priceFormatter.FormatPrice(orderItem.PriceExclTax, true, currency, language, false, true);

                orderItemModel.AttributeInfo = orderItem.AttributeDescription;
				if (orderItem.Product.IsRecurring)
				{
					orderItemModel.RecurringInfo = string.Format(_localizationService.GetResource("Admin.Orders.Products.RecurringPeriod"), 
						orderItem.Product.RecurringCycleLength, orderItem.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));
				}

                // Return requests.
				orderItemModel.ReturnRequests = _orderService.SearchReturnRequests(0, 0, orderItem.Id, null, 0, int.MaxValue).Select(x =>
					{
						return new OrderModel.ReturnRequestModel
						{
							Id = x.Id,
							Quantity = x.Quantity,
							Status = x.ReturnRequestStatus,
							StatusString = x.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext)
						};
					})
					.ToList();

                // Gift cards.
                orderItemModel.PurchasedGiftCardIds = _giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id)
                    .Select(gc => gc.Id).ToList();

                model.Items.Add(orderItemModel);
            }

            model.HasDownloadableProducts = hasDownloadableItems;

			model.AutoUpdateOrderItem.Caption = T("Admin.Orders.EditOrderDetails");
			model.AutoUpdateOrderItem.ShowUpdateTotals = (order.OrderStatusId <= (int)OrderStatus.Pending);
			// UpdateRewardPoints only visible for unpending orders (see RewardPointsSettingsValidator).
			model.AutoUpdateOrderItem.ShowUpdateRewardPoints = (order.OrderStatusId > (int)OrderStatus.Pending && order.RewardPointsWereAdded);
			model.AutoUpdateOrderItem.UpdateTotals = model.AutoUpdateOrderItem.ShowUpdateTotals;
			model.AutoUpdateOrderItem.UpdateRewardPoints = order.RewardPointsWereAdded;

			model.AutoUpdateOrderItemInfo = TempData[AutoUpdateOrderItemContext.InfoKey] as string;

            #endregion
        }

        [NonAction]
        protected OrderModel.AddOrderProductModel.ProductDetailsModel PrepareAddProductToOrderModel(int orderId, int productId)
        {
            var product = _productService.GetProductById(productId);
			if (product == null)
				throw new ArgumentException(T("Products.NotFound", productId));

			var customer = _workContext.CurrentCustomer;	// TODO: we need a customer representing entity instance for backend work
			var order = _orderService.GetOrderById(orderId);
			var currency = _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);

			var taxRate = decimal.Zero;
			var unitPriceTaxRate = decimal.Zero;
			var unitPrice = _priceCalculationService.GetFinalPrice(product, null, customer, decimal.Zero, false, 1);
			var unitPriceInclTax = _taxService.GetProductPrice(product, product.TaxCategoryId, unitPrice, true, customer, currency, _taxSettings.PricesIncludeTax, out unitPriceTaxRate);
			var unitPriceExclTax = _taxService.GetProductPrice(product, product.TaxCategoryId, unitPrice, false, customer, currency, _taxSettings.PricesIncludeTax, out taxRate);

            var model = new OrderModel.AddOrderProductModel.ProductDetailsModel()
            {
				ProductId = productId,
				OrderId = orderId,
				Name = product.Name,
				ProductType = product.ProductType,
				Quantity = 1,
				UnitPriceInclTax = unitPriceInclTax,
				UnitPriceExclTax = unitPriceExclTax,
				TaxRate = unitPriceTaxRate,
				SubTotalInclTax = unitPriceInclTax,
				SubTotalExclTax = unitPriceExclTax,
				ShowUpdateTotals = (order.OrderStatusId <= (int)OrderStatus.Pending),
				AdjustInventory = true,
				UpdateTotals = true
            };

            //attributes
            var productVariantAttributes = _productAttributeService.GetProductVariantAttributesByProductId(product.Id);
            foreach (var attribute in productVariantAttributes)
            {
                var pvaModel = new OrderModel.AddOrderProductModel.ProductVariantAttributeModel()
                {
                    Id = attribute.Id,
                    ProductAttributeId = attribute.ProductAttributeId,
                    Name = attribute.ProductAttribute.Name,
                    TextPrompt = attribute.TextPrompt,
                    IsRequired = attribute.IsRequired,
                    AttributeControlType = attribute.AttributeControlType
                };

                if (attribute.ShouldHaveValues())
                {
                    //values
                    var pvaValues = _productAttributeService.GetProductVariantAttributeValues(attribute.Id);
                    foreach (var pvaValue in pvaValues)
                    {
                        var pvaValueModel = new OrderModel.AddOrderProductModel.ProductVariantAttributeValueModel()
                        {
                            Id = pvaValue.Id,
                            Name = pvaValue.Name,
                            IsPreSelected = pvaValue.IsPreSelected
                        };
                        pvaModel.Values.Add(pvaValueModel);
                    }
                }

                model.ProductVariantAttributes.Add(pvaModel);
            }
            //gift card
            model.GiftCard.IsGiftCard = product.IsGiftCard;
            if (model.GiftCard.IsGiftCard)
            {
                model.GiftCard.GiftCardType = product.GiftCardType;
            }
            return model;
        }

		private ShipmentModel.ShipmentItemModel PrepareShipmentItemModel(
			Order order,
			OrderItem orderItem,
			ShipmentItem shipmentItem,
			MeasureDimension baseDimension,
			MeasureWeight baseWeight)
		{
			orderItem.Product.MergeWithCombination(orderItem.AttributesXml);

			var language = Services.WorkContext.WorkingLanguage;
			var maxQtyToAdd = orderItem.GetItemsCanBeAddedToShipmentCount();
			var qtyInAllShipments = orderItem.GetShipmentItemsCount();

			var model = new ShipmentModel.ShipmentItemModel
			{
				Id = shipmentItem?.Id ?? 0,
				OrderItemId = orderItem.Id,
				ProductId = orderItem.ProductId,
				ProductName = orderItem.Product.Name,
				ProductType = orderItem.Product.ProductType,
				ProductTypeName = orderItem.Product.GetProductTypeLabel(_localizationService),
				ProductTypeLabelHint = orderItem.Product.ProductTypeLabelHint,
				Sku = orderItem.Product.Sku,
				Gtin = orderItem.Product.Gtin,
				AttributeInfo = orderItem.AttributeDescription,
				ItemWeight = orderItem.ItemWeight.HasValue ? string.Format("{0:F2} [{1}]", orderItem.ItemWeight, baseWeight?.GetLocalized(x => x.Name) ?? "") : "",
				ItemDimensions = string.Format("{0:F2} x {1:F2} x {2:F2} [{3}]", orderItem.Product.Length, orderItem.Product.Width, orderItem.Product.Height, baseDimension?.GetLocalized(x => x.Name) ?? ""),
				QuantityOrdered = orderItem.Quantity,
				QuantityInThisShipment = shipmentItem?.Quantity ?? 0,
				QuantityInAllShipments = qtyInAllShipments,
				QuantityToAdd = maxQtyToAdd
			};

			if (orderItem.Product.ProductType == ProductType.BundledProduct && orderItem.BundleData.HasValue())
			{
				var bundleData = orderItem.GetBundleData();

				model.BundlePerItemPricing = orderItem.Product.BundlePerItemPricing;
				model.BundlePerItemShoppingCart = bundleData.Any(x => x.PerItemShoppingCart);

				foreach (var bundleItem in bundleData)
				{
					var bundleItemModel = new ShipmentModel.BundleItemModel
					{
						Sku = bundleItem.Sku,
						ProductName = bundleItem.ProductName,
						ProductSeName = bundleItem.ProductSeName,
						VisibleIndividually = bundleItem.VisibleIndividually,
						Quantity = bundleItem.Quantity,
						DisplayOrder = bundleItem.DisplayOrder,
						AttributeInfo = bundleItem.AttributesInfo
					};

					model.BundleItems.Add(bundleItemModel);
				}
			}

			return model;
		}

		private void PrepareShipmentModel(ShipmentModel model, Shipment shipment, bool withAllDetails)
        {
            var order = shipment.Order;
            var baseWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            var baseDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);

            model.Id = shipment.Id;
            model.OrderId = shipment.OrderId;
            model.StoreId = order.StoreId;
            model.LanguageId = order.CustomerLanguageId;
            model.OrderNumber = order.GetOrderNumber();
            model.PurchaseOrderNumber = order.PurchaseOrderNumber;
            model.ShippingMethod = order.ShippingMethod;
            model.TrackingNumber = shipment.TrackingNumber;
            model.TrackingUrl = shipment.TrackingUrl;
            model.TotalWeight = shipment.TotalWeight.HasValue ? string.Format("{0:F2} [{1}]", shipment.TotalWeight, baseWeight?.GetLocalized(x => x.Name) ?? "") : "";
            model.CanShip = !shipment.ShippedDateUtc.HasValue;
            model.CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue;
            model.ShippedDate = shipment.ShippedDateUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
            model.DeliveryDate = shipment.DeliveryDateUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc) : (DateTime?)null;
            model.DisplayPdfPackagingSlip = _pdfSettings.Enabled;
            model.ShowSku = _catalogSettings.ShowProductSku;

            if (withAllDetails)
			{
                // Shipping address.
				model.ShippingAddress = order.ShippingAddress;

				var store = Services.StoreService.GetStoreById(order.StoreId) ?? Services.StoreContext.CurrentStore;
				var companyInfoSettings = Services.Settings.LoadSetting<CompanyInformationSettings>(store.Id);
				model.MerchantCompanyInfo = companyInfoSettings;

				if (model.ShippingAddress != null)
				{
					model.FormattedShippingAddress = _addressService.FormatAddress(model.ShippingAddress, true);
				}

				model.FormattedMerchantAddress = _addressService.FormatAddress(model.MerchantCompanyInfo, true);

                // Shipment items.
                foreach (var shipmentItem in shipment.ShipmentItems)
                {
                    var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                    if (orderItem != null)
                    {
                        var itemModel = PrepareShipmentItemModel(order, orderItem, shipmentItem, baseDimension, baseWeight);
                        model.Items.Add(itemModel);
                    }
                }

                // TODO: Tracking URL.
            }
        }

		private void PrepareOrderAddressModel(OrderAddressModel model, Address address)
		{
			model.Address = address.ToModel();

			model.Address.FirstNameEnabled = true;
			model.Address.FirstNameRequired = true;
			model.Address.LastNameEnabled = true;
			model.Address.LastNameRequired = true;
			model.Address.EmailEnabled = true;
			model.Address.EmailRequired = true;
            model.Address.ValidateEmailAddress = _addressSettings.ValidateEmailAddress;
			model.Address.CompanyEnabled = _addressSettings.CompanyEnabled;
			model.Address.CompanyRequired = _addressSettings.CompanyRequired;
			model.Address.CountryEnabled = _addressSettings.CountryEnabled;
			model.Address.StateProvinceEnabled = _addressSettings.StateProvinceEnabled;
			model.Address.CityEnabled = _addressSettings.CityEnabled;
			model.Address.CityRequired = _addressSettings.CityRequired;
			model.Address.StreetAddressEnabled = _addressSettings.StreetAddressEnabled;
			model.Address.StreetAddressRequired = _addressSettings.StreetAddressRequired;
			model.Address.StreetAddress2Enabled = _addressSettings.StreetAddress2Enabled;
			model.Address.StreetAddress2Required = _addressSettings.StreetAddress2Required;
			model.Address.ZipPostalCodeEnabled = _addressSettings.ZipPostalCodeEnabled;
			model.Address.ZipPostalCodeRequired = _addressSettings.ZipPostalCodeRequired;
			model.Address.PhoneEnabled = _addressSettings.PhoneEnabled;
			model.Address.PhoneRequired = _addressSettings.PhoneRequired;
			model.Address.FaxEnabled = _addressSettings.FaxEnabled;
			model.Address.FaxRequired = _addressSettings.FaxRequired;

			//countries
			foreach (var c in _countryService.GetAllCountries(true))
			{
				model.Address.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString(), Selected = (c.Id == address.CountryId) });
			}

			//states
			var states = address.Country != null ? _stateProvinceService.GetStateProvincesByCountryId(address.Country.Id, true).ToList() : new List<StateProvince>();
			if (states.Count > 0)
			{
				foreach (var s in states)
				{
					model.Address.AvailableStates.Add(new SelectListItem() { Text = s.Name, Value = s.Id.ToString(), Selected = (s.Id == address.StateProvinceId) });
				}
			}
			else
			{
				model.Address.AvailableStates.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.OtherNonUS"), Value = "0" });
			}
		}

        #endregion

        #region Order list

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Order.Read)]
        public ActionResult List(OrderListModel model)
        {
            var allStores = Services.StoreService.GetAllStores();
            var paymentMethods = _paymentService.LoadAllPaymentMethods();

            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();

            model.AvailableStores = allStores
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();

            model.AvailablePaymentMethods = paymentMethods
                .Select(x => new SelectListItem
                {
                    Text = (_pluginMediator.GetLocalizedFriendlyName(x.Metadata).NullEmpty() ?? x.Metadata.FriendlyName.NullEmpty() ?? x.Metadata.SystemName).EmptyNull(),
                    Value = x.Metadata.SystemName
                })
                .ToList();

            var paymentMethodsCounts = model.AvailablePaymentMethods
                .GroupBy(x => x.Text)
                .Select(x => new { Name = x.Key.EmptyNull(), Count = x.Count() })
                .ToDictionarySafe(x => x.Name, x => x.Count);

            // Append system name if there are payment methods with the same friendly name.
            model.AvailablePaymentMethods = model.AvailablePaymentMethods
                .OrderBy(x => x.Text)
                .Select(x =>
                {
                    if (paymentMethodsCounts.TryGetValue(x.Text, out var count) && count > 1)
                    {
                        x.Text = "{0} ({1})".FormatInvariant(x.Text, x.Value);
                    }

                    return x;
                })
                .ToList();

            model.GridPageSize = _adminAreaSettings.GridPageSize;

			return View(model);
		}

		[GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult OrderList(GridCommand command, OrderListModel model)
        {
			var gridModel = new GridModel<OrderModel>();

			DateTime? startDateValue = (model.StartDate == null) ? null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);
			DateTime? endDateValue = (model.EndDate == null) ? null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

			var viaShippingMethodString = T("Admin.Order.ViaShippingMethod").Text;
			var withPaymentMethodString = T("Admin.Order.WithPaymentMethod").Text;
			var fromStoreString = T("Admin.Order.FromStore").Text;
			var orderStatusIds = model.OrderStatusIds.ToIntArray();
			var paymentStatusIds = model.PaymentStatusIds.ToIntArray();
			var shippingStatusIds = model.ShippingStatusIds.ToIntArray();
			var paymentMethods = new Dictionary<string, Provider<IPaymentMethod>>(StringComparer.OrdinalIgnoreCase);
			Provider<IPaymentMethod> paymentMethod = null;

			var orders = _orderService.SearchOrders(model.StoreId, 0, startDateValue, endDateValue, orderStatusIds, paymentStatusIds, shippingStatusIds,
				model.CustomerEmail, model.OrderGuid, model.OrderNumber, command.Page - 1, command.PageSize, model.CustomerName, model.PaymentMethods.SplitSafe(","));

			gridModel.Data = orders.Select(x =>
			{
				var store = Services.StoreService.GetStoreById(x.StoreId);

				var orderModel = new OrderModel
				{
					Id = x.Id,
					OrderNumber = x.GetOrderNumber(),
					StoreName = store != null ? store.Name : "".NaIfEmpty(),
					OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
					OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
					StatusOrder = x.OrderStatus,
					PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
					StatusPayment = x.PaymentStatus,
					IsShippable = x.ShippingStatus != ShippingStatus.ShippingNotRequired,
					ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
					StatusShipping = x.ShippingStatus,
					ShippingMethod = x.ShippingMethod.NullEmpty() ?? "".NaIfEmpty(),
					CustomerName = x.BillingAddress.GetFullName(),
					CustomerEmail = x.BillingAddress.Email,
					CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
					HasNewPaymentNotification = x.HasNewPaymentNotification
				};

				orderModel.CreatedOnString = orderModel.CreatedOn.ToString("g");

				if (x.PaymentMethodSystemName.HasValue())
				{
					if (!paymentMethods.TryGetValue(x.PaymentMethodSystemName, out paymentMethod))
					{
						paymentMethod = _paymentService.LoadPaymentMethodBySystemName(x.PaymentMethodSystemName);
						paymentMethods[x.PaymentMethodSystemName] = paymentMethod;
					}
					if (paymentMethod != null)
					{
						orderModel.PaymentMethod = _pluginMediator.GetLocalizedFriendlyName(paymentMethod.Metadata);
					}
				}

				if (orderModel.PaymentMethod.IsEmpty())
				{
					orderModel.PaymentMethod = x.PaymentMethodSystemName;
				}

				orderModel.HasPaymentMethod = orderModel.PaymentMethod.HasValue();

				if (x.ShippingAddress != null && orderModel.IsShippable)
				{
					orderModel.ShippingAddressString = string.Concat(x.ShippingAddress.Address1, 
						", ", x.ShippingAddress.ZipPostalCode,
							" ", x.ShippingAddress.City);

					if (x.ShippingAddress.CountryId > 0)
					{
						orderModel.ShippingAddressString += ", " + x.ShippingAddress.Country.TwoLetterIsoCode;
					}
				}

				orderModel.ViaShippingMethod = viaShippingMethodString.FormatInvariant(orderModel.ShippingMethod);
				orderModel.WithPaymentMethod = withPaymentMethodString.FormatInvariant(orderModel.PaymentMethod);
				orderModel.FromStore = fromStoreString.FormatInvariant(orderModel.StoreName);

				return orderModel;
			});

			gridModel.Total = orders.TotalCount;

			// Summary report.
			// Implemented as a workaround described here: http://www.telerik.com/community/forums/aspnet-mvc/grid/gridmodel-aggregates-how-to-use.aspx.
			var reportSummary = _orderReportService.GetOrderAverageReportLine(model.StoreId, orderStatusIds,
				paymentStatusIds, shippingStatusIds, startDateValue, endDateValue, model.CustomerEmail);

			var profit = _orderReportService.ProfitReport(model.StoreId, orderStatusIds,
				paymentStatusIds, shippingStatusIds, startDateValue, endDateValue, model.CustomerEmail);

			var aggregator = new OrderModel
			{
				aggregatorprofit = _priceFormatter.FormatPrice(profit, true, false),
				aggregatortax = _priceFormatter.FormatPrice(reportSummary.SumTax, true, false),
				aggregatortotal = _priceFormatter.FormatPrice(reportSummary.SumOrders, true, false)
			};

			gridModel.Aggregates = aggregator;

			return new JsonResult
			{
				Data = gridModel
			};
		}
        
        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-order-by-number")]
        [Permission(Permissions.Order.Read)]
        public ActionResult GoToOrderId(OrderListModel model)
        {
            var order = _orderService.GetOrderByNumber(model.GoDirectlyToNumber);
            if (order != null)
            {
                return RedirectToAction("Edit", "Order", new { id = order.Id });
            }

			NotifyWarning(T("Admin.Order.NotFound"));

			return RedirectToAction("List", "Order");
        }

		#endregion

		#region Export / Import

		[HttpPost]
        [Permission(Permissions.Order.Read)]
        public ActionResult ExportPdf(bool all, string selectedIds = null)
		{
			if (!all && selectedIds.IsEmpty())
			{
				NotifyInfo(T("Admin.Common.ExportNoData"));

				return RedirectToAction("List");
			}

			return RedirectToAction("PrintMany", "Order", new { ids = selectedIds, pdf = true, area = "" });
		}

        #endregion

        #region Order details

        #region Payments and other order workflow

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelorder")]
        [Permission(Permissions.Order.Update)]
        public ActionResult CancelOrder(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }
            
            try
            {
                _orderProcessingService.CancelOrder(order, true);
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

			return RedirectToAction("Edit", new { id });
		}

		[HttpPost, ActionName("Edit")]
		[FormValueRequired("completeorder")]
        [Permission(Permissions.Order.Update)]
        public ActionResult CompleteOrder(int id)
		{
			var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

			try
			{
				_orderProcessingService.CompleteOrder(order);
			}
			catch (Exception ex)
			{
				NotifyError(ex);
			}

			return RedirectToAction("Edit", new { id });
		}

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("captureorder")]
        [Permission(Permissions.Order.Update)]
        public ActionResult CaptureOrder(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }
            
            try
            {
                var errors = _orderProcessingService.Capture(order);
				foreach (var error in errors)
				{
					NotifyError(error);
				}
			}
            catch (Exception ex)
            {
                NotifyError(ex);
			}

			return RedirectToAction("Edit", new { id });
		}

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markorderaspaid")]
        [Permission(Permissions.Order.Update)]
        public ActionResult MarkOrderAsPaid(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }
            
            try
            {
                _orderProcessingService.MarkOrderAsPaid(order);
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

			return RedirectToAction("Edit", new { id });
		}

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorder")]
        [Permission(Permissions.Order.Update)]
        public ActionResult RefundOrder(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                var errors = _orderProcessingService.Refund(order);
				foreach (var error in errors)
				{
					NotifyError(error);
				}
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

			return RedirectToAction("Edit", new { id });
		}

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorderoffline")]
        [Permission(Permissions.Order.Update)]
        public ActionResult RefundOrderOffline(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                _orderProcessingService.RefundOffline(order);
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

			return RedirectToAction("Edit", new { id });
		}

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorder")]
        [Permission(Permissions.Order.Update)]
        public ActionResult VoidOrder(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                var errors = _orderProcessingService.Void(order);
                foreach (var error in errors)
                {
                    NotifyError(error);
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

			return RedirectToAction("Edit", new { id });
		}

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorderoffline")]
        [Permission(Permissions.Order.Update)]
        public ActionResult VoidOrderOffline(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                _orderProcessingService.VoidOffline(order);
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

			return RedirectToAction("Edit", new { id });
		}

        [Permission(Permissions.Order.Update)]
        public ActionResult PartiallyRefundOrderPopup(int id, bool online)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("partialrefundorder")]
        [Permission(Permissions.Order.Update)]
        public ActionResult PartiallyRefundOrderPopup(string btnId, string formId, int id, bool online, OrderModel model)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                decimal amountToRefund = model.AmountToRefund;
                if (amountToRefund <= decimal.Zero)
                {
                    throw new SmartException("Enter amount to refund");
                }

                decimal maxAmountToRefund = order.OrderTotal - order.RefundedAmount;
                if (amountToRefund > maxAmountToRefund)
                {
                    amountToRefund = maxAmountToRefund;
                }

                var errors = new List<string>();
                if (online)
                {
                    errors = _orderProcessingService.PartiallyRefund(order, amountToRefund).ToList();
                }
                else
                {
                    _orderProcessingService.PartiallyRefundOffline(order, amountToRefund);
                }

                if (errors.Count == 0)
                {
                    ViewBag.RefreshPage = true;
                    ViewBag.btnId = btnId;
                    ViewBag.formId = formId;

                    PrepareOrderDetailsModel(model, order);
                    return View(model);
                }
                else
                {
                    PrepareOrderDetailsModel(model, order);
                    foreach (var error in errors)
                    {
                        NotifyError(error, false);
                    }
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                PrepareOrderDetailsModel(model, order);
                NotifyError(ex, false);
                return View(model);
            }
        }

        #endregion

        #region Edit, delete

        [Permission(Permissions.Order.Read)]
        public ActionResult Edit(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
            {
                return RedirectToAction("List");
            }

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);

            return View(model);
        }

        [HttpPost]
        [Permission(Permissions.Order.Delete)]
        public ActionResult Delete(int id)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            _orderProcessingService.DeleteOrder(order);

			var msg = T("ActivityLog.DeleteOrder", order.GetOrderNumber());

			_customerActivityService.InsertActivity("DeleteOrder", msg);
			NotifySuccess(msg);

			return RedirectToAction("List");
        }

        [Permission(Permissions.Order.Read)]
        public ActionResult Print(int orderId, bool pdf = false)
        {
            return RedirectToAction("Print", "Order", new { id = orderId, pdf, area = "" });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveCC")]
        [Permission(Permissions.Order.Update)]
        public ActionResult EditCreditCardInfo(int id, OrderModel model)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            if (order.AllowStoringCreditCardNumber)
            {
                string cardType = model.CardType;
                string cardName = model.CardName;
                string cardNumber = model.CardNumber;
                string cardCvv2 = model.CardCvv2;
                string cardExpirationMonth = model.CardExpirationMonth;
                string cardExpirationYear = model.CardExpirationYear;

                order.CardType = _encryptionService.EncryptText(cardType);
                order.CardName = _encryptionService.EncryptText(cardName);
                order.CardNumber = _encryptionService.EncryptText(cardNumber);
                order.MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(cardNumber));
                order.CardCvv2 = _encryptionService.EncryptText(cardCvv2);
                order.CardExpirationMonth = _encryptionService.EncryptText(cardExpirationMonth);
                order.CardExpirationYear = _encryptionService.EncryptText(cardExpirationYear);
                _orderService.UpdateOrder(order);
            }

            PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveDD")]
        [Permission(Permissions.Order.Update)]
        public ActionResult EditDirectDebitInfo(int id, OrderModel model)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            if (order.AllowStoringDirectDebit)
            {

                string accountHolder = model.DirectDebitAccountHolder;
                string accountNumber = model.DirectDebitAccountNumber;
                string bankCode = model.DirectDebitBankCode;
                string bankName = model.DirectDebitBankName;
                string bic = model.DirectDebitBIC;
                string country = model.DirectDebitCountry;
                string iban = model.DirectDebitIban;

                order.DirectDebitAccountHolder = _encryptionService.EncryptText(accountHolder);
                order.DirectDebitAccountNumber = _encryptionService.EncryptText(accountNumber);
                order.DirectDebitBankCode = _encryptionService.EncryptText(bankCode);
                order.DirectDebitBankName = _encryptionService.EncryptText(bankName);
                order.DirectDebitBIC = _encryptionService.EncryptText(bic);
                order.DirectDebitCountry = _encryptionService.EncryptText(country);
                order.DirectDebitIban = _encryptionService.EncryptText(iban);

                _orderService.UpdateOrder(order);
            }
            
            PrepareOrderDetailsModel(model, order);
            return View(model);
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderTotals")]
        [Permission(Permissions.Order.Update)]
        public ActionResult EditOrderTotals(int id, OrderModel model)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            order.OrderSubtotalInclTax = model.OrderSubtotalInclTaxValue;
            order.OrderSubtotalExclTax = model.OrderSubtotalExclTaxValue;
            order.OrderSubTotalDiscountInclTax = model.OrderSubTotalDiscountInclTaxValue;
            order.OrderSubTotalDiscountExclTax = model.OrderSubTotalDiscountExclTaxValue;
            order.OrderShippingInclTax = model.OrderShippingInclTaxValue;
            order.OrderShippingExclTax = model.OrderShippingExclTaxValue;
            order.PaymentMethodAdditionalFeeInclTax = model.PaymentMethodAdditionalFeeInclTaxValue;
            order.PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFeeExclTaxValue;
            order.TaxRates = model.TaxRatesValue;
            order.OrderTax = model.TaxValue;
            order.OrderDiscount = model.OrderTotalDiscountValue;
			order.CreditBalance = model.CreditBalanceValue;
            order.OrderTotalRounding = model.OrderTotalRoundingValue;
            order.OrderTotal = model.OrderTotalValue;
            _orderService.UpdateOrder(order);

            PrepareOrderDetailsModel(model, order);
            return View(model);
        }
        
        [HttpPost]
        [Permission(Permissions.Order.EditItem)]
        public ActionResult EditOrderItem(AutoUpdateOrderItemModel model, FormCollection form)
        {
			var oi = _orderService.GetOrderItemById(model.Id);
            if (oi == null)
            {
                return HttpNotFound();
            }

			int orderId = oi.Order.Id;

			if (model.NewQuantity.HasValue)
			{
				var context = new AutoUpdateOrderItemContext
				{
					OrderItem = oi,
					QuantityOld = oi.Quantity,
					QuantityNew = model.NewQuantity.Value,
					AdjustInventory = model.AdjustInventory,
					UpdateRewardPoints = model.UpdateRewardPoints,
					UpdateTotals = model.UpdateTotals
				};

				oi.Quantity = model.NewQuantity.Value;
				oi.UnitPriceInclTax = model.NewUnitPriceInclTax ?? oi.UnitPriceInclTax;
				oi.UnitPriceExclTax = model.NewUnitPriceExclTax ?? oi.UnitPriceExclTax;
				oi.TaxRate = model.NewTaxRate ?? oi.TaxRate;
				oi.DiscountAmountInclTax = model.NewDiscountInclTax ?? oi.DiscountAmountInclTax;
				oi.DiscountAmountExclTax = model.NewDiscountExclTax ?? oi.DiscountAmountExclTax;
				oi.PriceInclTax = model.NewPriceInclTax ?? oi.PriceInclTax;
				oi.PriceExclTax = model.NewPriceExclTax ?? oi.PriceExclTax;

				_orderService.UpdateOrder(oi.Order);

				_orderProcessingService.AutoUpdateOrderDetails(context);

				// we do not delete order item automatically anymore.
				//if (oi.Quantity <= 0)
				//{
				//	_orderService.DeleteOrderItem(oi);
				//}

				TempData[AutoUpdateOrderItemContext.InfoKey] = context.ToString(_localizationService);
			}

			return RedirectToAction("Edit", new { id = orderId });
        }

		[HttpPost]
        [Permission(Permissions.Order.EditItem)]
        public ActionResult DeleteOrderItem(AutoUpdateOrderItemModel model)
        {
			var oi = _orderService.GetOrderItemById(model.Id);
            if (oi == null)
            {
                return HttpNotFound();
            }

			int orderId = oi.Order.Id;
			var context = new AutoUpdateOrderItemContext
			{
				OrderItem = oi,
				QuantityOld = oi.Quantity,
				QuantityNew = 0,
				AdjustInventory = model.AdjustInventory,
				UpdateRewardPoints = model.UpdateRewardPoints,
				UpdateTotals = model.UpdateTotals
			};

			_orderProcessingService.AutoUpdateOrderDetails(context);

			_orderService.DeleteOrderItem(oi);

			TempData[AutoUpdateOrderItemContext.InfoKey] = context.ToString(_localizationService);

			return RedirectToAction("Edit", new { id = orderId });
        }

		[HttpPost, ActionName("Edit")]
		[FormValueRequired(FormValueRequirement.StartsWith, "btnAddReturnRequest")]
		[ValidateInput(false)]
        [Permission(Permissions.Order.ReturnRequest.Create)]
        public ActionResult AddReturnRequest(int id, FormCollection form)
		{
			var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

			// Get order item identifier.
			int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
            {
                if (formValue.StartsWith("btnAddReturnRequest", StringComparison.InvariantCultureIgnoreCase))
                {
                    orderItemId = Convert.ToInt32(formValue.Substring("btnAddReturnRequest".Length));
                }
            }

			var orderItem = order.OrderItems.Where(x => x.Id == orderItemId).FirstOrDefault();
            if (orderItem == null)
            {
                return HttpNotFound();
            }

			if (orderItem.Quantity > 0)
			{
				var returnRequest = new ReturnRequest
				{
					StoreId = order.StoreId,
					OrderItemId = orderItem.Id,
					Quantity = orderItem.Quantity,
					CustomerId = order.CustomerId,
					ReasonForReturn = "",
					RequestedAction = "",
					StaffNotes = "",
					ReturnRequestStatus = ReturnRequestStatus.Pending
				};

				order.Customer.ReturnRequests.Add(returnRequest);
				_customerService.UpdateCustomer(order.Customer);

				return RedirectToAction("Edit", "ReturnRequest", new { id = returnRequest.Id });
			}

			var model = new OrderModel();
			PrepareOrderDetailsModel(model, order);
			return View(model);
		}

        [HttpPost, ValidateInput(false), ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnResetDownloadCount")]
        [Permission(Permissions.Order.Update)]
        public ActionResult ResetDownloadCount(int id, FormCollection form)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            // Get order item identifier.
            var orderItemId = 0;
            foreach (var formValue in form.AllKeys)
            {
                if (formValue.StartsWith("btnResetDownloadCount", StringComparison.InvariantCultureIgnoreCase))
                {
                    orderItemId = Convert.ToInt32(formValue.Substring("btnResetDownloadCount".Length));
                }
            }

            var orderItem = order.OrderItems.Where(x => x.Id == orderItemId).FirstOrDefault();
            if (orderItem == null)
            {
                return HttpNotFound();
            }

            orderItem.DownloadCount = 0;
            _orderService.UpdateOrder(order);

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [HttpPost, ValidateInput(false), ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnPvActivateDownload")]
        [Permission(Permissions.Order.Update)]
        public ActionResult ActivateDownloadOrderItem(int id, FormCollection form)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            // Get order item identifier.
            var orderItemId = 0;
            foreach (var formValue in form.AllKeys)
            {
                if (formValue.StartsWith("btnPvActivateDownload", StringComparison.InvariantCultureIgnoreCase))
                {
                    orderItemId = Convert.ToInt32(formValue.Substring("btnPvActivateDownload".Length));
                }
            }
            var orderItem = order.OrderItems.Where(x => x.Id == orderItemId).FirstOrDefault();
            if (orderItem == null)
            {
                return HttpNotFound();
            }

            orderItem.IsDownloadActivated = !orderItem.IsDownloadActivated;
            _orderService.UpdateOrder(order);

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [Permission(Permissions.Order.Read)]
        public ActionResult UploadLicenseFilePopup(int id, int orderItemId)
        {
            var order = _orderService.GetOrderById(id);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            var orderItem = order.OrderItems.Where(x => x.Id == orderItemId).FirstOrDefault();
            if (orderItem == null)
            {
                return HttpNotFound();
            }

            if (!orderItem.Product.IsDownload)
            {
                throw new ArgumentException("Product is not downloadable");
            }

            var model = new OrderModel.UploadLicenseModel
            {
                LicenseDownloadId = orderItem.LicenseDownloadId ?? 0,
                OrderId = order.Id,
                OrderItemId = orderItem.Id
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("uploadlicense")]
        [Permission(Permissions.Order.Update)]
        public ActionResult UploadLicenseFilePopup(string btnId, string formId, OrderModel.UploadLicenseModel model)
        {
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            var orderItem = order.OrderItems.Where(x => x.Id == model.OrderItemId).FirstOrDefault();
            if (orderItem == null)
            {
                return HttpNotFound();
            }

            // Attach license.
            if (model.LicenseDownloadId > 0)
                orderItem.LicenseDownloadId = model.LicenseDownloadId;
            else
                orderItem.LicenseDownloadId = null;

			MediaHelper.UpdateDownloadTransientStateFor(orderItem, x => x.LicenseDownloadId);
            _orderService.UpdateOrder(order);

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            return View(model);
        }

        [HttpPost, ActionName("UploadLicenseFilePopup")]
        [FormValueRequired("deletelicense")]
        [Permission(Permissions.Order.Update)]
        public ActionResult DeleteLicenseFilePopup(string btnId, string formId, OrderModel.UploadLicenseModel model)
        {
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            var orderItem = order.OrderItems.Where(x => x.Id == model.OrderItemId).FirstOrDefault();
            if (orderItem == null)
            {
                return HttpNotFound();
            }

            // Attach license.
            orderItem.LicenseDownloadId = null;
			MediaHelper.UpdateDownloadTransientStateFor(orderItem, x => x.LicenseDownloadId);
            _orderService.UpdateOrder(order);

            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            return View(model);
        }

        [Permission(Permissions.Order.EditItem)]
        public ActionResult AddProductToOrder(int orderId)
        {
            var model = new OrderModel.AddOrderProductModel { OrderId = orderId };

            foreach (var c in _categoryService.GetCategoryTree(includeHidden: true).FlattenNodes(false))
            {
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameIndented(), Value = c.Id.ToString() });
            }

            foreach (var m in _manufacturerService.GetAllManufacturers(true))
            {
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });
            }

			model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.EditItem)]
        public ActionResult AddProductToOrder(GridCommand command, OrderModel.AddOrderProductModel model)
        {
			var gridModel = new GridModel<OrderModel.AddOrderProductModel.ProductModel>();
			var fields = new List<string> { "name" };
			if (_searchSettings.SearchFields.Contains("sku"))
				fields.Add("sku");
			if (_searchSettings.SearchFields.Contains("shortdescription"))
				fields.Add("shortdescription");

			var searchQuery = new CatalogSearchQuery(fields.ToArray(), model.SearchProductName);

			if (model.SearchCategoryId != 0)
				searchQuery = searchQuery.WithCategoryIds(null, model.SearchCategoryId);

			if (model.SearchManufacturerId != 0)
				searchQuery = searchQuery.WithManufacturerIds(null, model.SearchManufacturerId);

			if (model.SearchProductTypeId > 0)
				searchQuery = searchQuery.IsProductType((ProductType)model.SearchProductTypeId);

			var query = _catalogSearchService.PrepareQuery(searchQuery);
			var products = new PagedList<Product>(query.OrderBy(x => x.Name), command.Page - 1, command.PageSize);

			gridModel.Data = products.Select(x =>
			{
				var productModel = new OrderModel.AddOrderProductModel.ProductModel
				{
					Id = x.Id,
					Name = x.Name,
					Sku = x.Sku,
					ProductTypeName = x.GetProductTypeLabel(_localizationService),
					ProductTypeLabelHint = x.ProductTypeLabelHint
				};

				return productModel;
			});

			gridModel.Total = products.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Order.EditItem)]
        public ActionResult AddProductToOrderDetails(int orderId, int productId)
        {
            var model = PrepareAddProductToOrderModel(orderId, productId);
            return View(model);
        }

        [HttpPost, ValidateInput(false)]
        [Permission(Permissions.Order.EditItem)]
        public ActionResult AddProductToOrderDetails(int orderId, int productId, bool adjustInventory, bool? updateTotals, ProductVariantQuery query, FormCollection form)
        {
            var order = _orderService.GetOrderById(orderId);
            var product = _productService.GetProductById(productId);
			var currency = _currencyService.GetCurrencyByCode(order.CustomerCurrencyCode);
			var includingTax = _workContext.TaxDisplayType == TaxDisplayType.IncludingTax;

			var unitPriceInclTax = decimal.Zero;
            decimal.TryParse(form["UnitPriceInclTax"], out unitPriceInclTax);
            var unitPriceExclTax = decimal.Zero;
            decimal.TryParse(form["UnitPriceExclTax"], out unitPriceExclTax);
            int.TryParse(form["Quantity"], out var quantity);
            var priceInclTax = decimal.Zero;
            decimal.TryParse(form["SubTotalInclTax"], out priceInclTax);
            var priceExclTax = decimal.Zero;
            decimal.TryParse(form["SubTotalExclTax"], out priceExclTax);
			var unitPriceTaxRate = decimal.Zero;
			decimal.TryParse(form["TaxRate"], out unitPriceTaxRate);

            var warnings = new List<string>();
            var attributes = "";

			if (product.ProductType != ProductType.BundledProduct)
			{
				var variantAttributes = _productAttributeService.GetProductVariantAttributesByProductId(product.Id);

				attributes = query.CreateSelectedAttributesXml(product.Id, 0, variantAttributes, _productAttributeParser, _localizationService, _downloadService,
					_catalogSettings, this.Request, warnings);
			}

            #region Gift cards

            string recipientName = "";
            string recipientEmail = "";
            string senderName = "";
            string senderEmail = "";
            string giftCardMessage = "";

            if (product.IsGiftCard)
            {
				recipientName = query.GetGiftCardValue(product.Id, 0, "RecipientName");
				recipientEmail = query.GetGiftCardValue(product.Id, 0, "RecipientEmail");
				senderName = query.GetGiftCardValue(product.Id, 0, "SenderName");
				senderEmail = query.GetGiftCardValue(product.Id, 0, "SenderEmail");
				giftCardMessage = query.GetGiftCardValue(product.Id, 0, "Message");

                attributes = _productAttributeParser.AddGiftCardAttribute(attributes, recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
            }

            #endregion

            warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(order.Customer, ShoppingCartType.ShoppingCart, product, attributes, quantity));
            warnings.AddRange(_shoppingCartService.GetShoppingCartItemGiftCardWarnings(ShoppingCartType.ShoppingCart, product, attributes));

            if (warnings.Count == 0)
            {
                var attributeDescription = _productAttributeFormatter.FormatAttributes(product, attributes, order.Customer);
                var displayDeliveryTime = 
                    _shoppingCartSettings.ShowDeliveryTimes &&
                    product.DeliveryTimeId.HasValue &&
                    product.IsShipEnabled &&
                    product.DisplayDeliveryTimeAccordingToStock(_catalogSettings);

                var orderItem = new OrderItem
                {
                    OrderItemGuid = Guid.NewGuid(),
                    Order = order,
					ProductId = product.Id,
                    UnitPriceInclTax = unitPriceInclTax,
                    UnitPriceExclTax = unitPriceExclTax,
                    PriceInclTax = priceInclTax,
                    PriceExclTax = priceExclTax,
					TaxRate = unitPriceTaxRate,
                    AttributeDescription = attributeDescription,
                    AttributesXml = attributes,
                    Quantity = quantity,
                    DiscountAmountInclTax = decimal.Zero,
                    DiscountAmountExclTax = decimal.Zero,
                    DownloadCount = 0,
                    IsDownloadActivated = false,
                    LicenseDownloadId = 0,
					ProductCost = _priceCalculationService.GetProductCost(product, attributes),
                    DeliveryTimeId = product.GetDeliveryTimeIdAccordingToStock(_catalogSettings),
                    DisplayDeliveryTime = displayDeliveryTime
                };

				if (product.ProductType == ProductType.BundledProduct)
				{
					var listBundleData = new List<ProductBundleItemOrderData>();
					var bundleItems = _productService.GetBundleItems(product.Id);

					foreach (var bundleItem in bundleItems)
					{
						var finalPrice = _priceCalculationService.GetFinalPrice(bundleItem.Item.Product, bundleItems, order.Customer, decimal.Zero, true, bundleItem.Item.Quantity);
						var bundleItemSubTotalWithDiscountBase = _taxService.GetProductPrice(bundleItem.Item.Product, bundleItem.Item.Product.TaxCategoryId, finalPrice,
							includingTax, order.Customer, currency, _taxSettings.PricesIncludeTax, out var taxRate);

						bundleItem.ToOrderData(listBundleData, bundleItemSubTotalWithDiscountBase);
					}

					orderItem.SetBundleData(listBundleData);
				}

                order.OrderItems.Add(orderItem);
                _orderService.UpdateOrder(order);

                // Gift cards.
                if (product.IsGiftCard)
                {
                    for (int i = 0; i < orderItem.Quantity; i++)
                    {
                        var gc = new GiftCard
                        {
                            GiftCardType = product.GiftCardType,
                            PurchasedWithOrderItem = orderItem,
                            Amount = unitPriceExclTax,
                            IsGiftCardActivated = false,
                            GiftCardCouponCode = _giftCardService.GenerateGiftCardCode(),
                            RecipientName = recipientName,
                            RecipientEmail = recipientEmail,
                            SenderName = senderName,
                            SenderEmail = senderEmail,
                            Message = giftCardMessage,
                            IsRecipientNotified = false,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        _giftCardService.InsertGiftCard(gc);
                    }
                }

				if (adjustInventory || (updateTotals ?? false))
				{
					var context = new AutoUpdateOrderItemContext
					{
						IsNewOrderItem = true,
						OrderItem = orderItem,
						QuantityOld = 0,
						QuantityNew = orderItem.Quantity,
						AdjustInventory = adjustInventory,
						UpdateTotals = (updateTotals ?? false)
					};

					_orderProcessingService.AutoUpdateOrderDetails(context);

					TempData[AutoUpdateOrderItemContext.InfoKey] = context.ToString(_localizationService);
				}

                // Redirect to order details page.
                return RedirectToAction("Edit", "Order", new { id = order.Id });
            }
            else
            {
                var model = PrepareAddProductToOrderModel(order.Id, product.Id);
                model.Warnings.AddRange(warnings);
                return View(model);
            }
        }

        #endregion

        #endregion

        #region Addresses

        [Permission(Permissions.Order.Read)]
        public ActionResult AddressEdit(int addressId, int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            var address = _addressService.GetAddressById(addressId);
            if (address == null)
            {
                return HttpNotFound();
            }

            var model = new OrderAddressModel { OrderId = orderId };
			PrepareOrderAddressModel(model, address);

            return View(model);
        }

        [HttpPost]
        [Permission(Permissions.Order.Update)]
        public ActionResult AddressEdit(OrderAddressModel model)
        {
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
            {
                return RedirectToAction("List");
            }

            var address = _addressService.GetAddressById(model.Address.Id);
            if (address == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                _addressService.UpdateAddress(address);

				_eventPublisher.PublishOrderUpdated(order);

				NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, orderId = model.OrderId });
            }

            // If we got this far, something failed, redisplay form.
            model.OrderId = order.Id;
			PrepareOrderAddressModel(model, address);

            return View(model);
        }

        #endregion

        #region Shipments

        [Permission(Permissions.Order.Read)]
        public ActionResult ShipmentList()
        {
			var model = new ShipmentListModel
			{
				DisplayPdfPackagingSlip = _pdfSettings.Enabled
			};

			return View(model);
		}

		[GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult ShipmentListSelect(GridCommand command, ShipmentListModel model)
        {
			DateTime? startDateValue = (model.StartDate == null) 
                ? null
				: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

			DateTime? endDateValue = (model.EndDate == null) 
                ? null
				: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

			var shipments = _shipmentService.GetAllShipments(model.TrackingNumber, startDateValue, endDateValue, command.Page - 1, command.PageSize);

            var shipmentModels = shipments
                .Select(x =>
                {
                    var sm = new ShipmentModel();
                    PrepareShipmentModel(sm, x, false);
                    return sm;
                })
                .ToList();

            var gridModel = new GridModel<ShipmentModel>
            {
                Data = shipmentModels,
                Total = shipmentModels.Count
            };

			return new JsonResult
			{
				Data = gridModel
			};
		}
        
        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult ShipmentsSelect(int orderId, GridCommand command)
        {
            var order = _orderService.GetOrderById(orderId);
			var shipments = order.Shipments.OrderBy(s => s.CreatedOnUtc).ToList();

            var shipmentModels = shipments
                .Select(x =>
                {
                    var sm = new ShipmentModel();
                    PrepareShipmentModel(sm, x, false);
                    return sm;
                })
                .ToList();

            var model = new GridModel<ShipmentModel>
            {
                Data = shipmentModels,
                Total = shipmentModels.Count
            };

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Order.EditShipment)]
        public ActionResult AddShipment(int orderId)
        {
            var order = _orderService.GetOrderById(orderId);
			if (order == null)
			{
				return RedirectToAction("List");
			}

            var model = new ShipmentModel
            {
                OrderId = order.Id,
            };

            var baseWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            var baseDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);

            foreach (var orderItem in order.OrderItems)
            {
                // We can ship only shippable products.
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                // Ensure that this product can be added to a shipment.
                if (orderItem.GetItemsCanBeAddedToShipmentCount() <= 0)
                    continue;

				var itemModel = PrepareShipmentItemModel(order, orderItem, null, baseDimension, baseWeight);
                model.Items.Add(itemModel);
            }

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [Permission(Permissions.Order.EditShipment)]
        public ActionResult AddShipment(ShipmentModel model, FormCollection form, bool continueEditing)
        {
            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                var quantities = new Dictionary<int, int>();

                foreach (var orderItem in order.OrderItems)
                {
                    foreach (string formKey in form.AllKeys)
                    {
                        if (formKey.Equals(string.Format("qtyToAdd{0}", orderItem.Id), StringComparison.InvariantCultureIgnoreCase))
                        {
                            quantities.Add(orderItem.Id, form[formKey].ToInt());
                            break;
                        }
                    }
                }

                var shipment = _orderProcessingService.AddShipment(order, model.TrackingNumber, model.TrackingUrl, quantities);
                if (shipment != null)
                {
                    NotifySuccess(T("Admin.Orders.Shipments.Added"));

                    return continueEditing
                       ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                       : RedirectToAction("Edit", new { id = order.Id });
                }
                else
                {
                    NotifyError(T("Admin.Orders.Shipments.NoProductsSelected"));

                    return RedirectToAction("AddShipment", new { order.Id });
                }
            }

            return AddShipment(order.Id);
        }

        [Permission(Permissions.Order.Read)]
        public ActionResult ShipmentDetails(int id)
        {
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
            {
                return HttpNotFound();
            }

            var model = new ShipmentModel();
            PrepareShipmentModel(model, shipment, true);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Order.EditShipment)]
        public ActionResult ShipmentDetails(ShipmentModel model, bool continueEditing)
        {
            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                shipment.TrackingNumber = model.TrackingNumber;
                shipment.TrackingUrl = model.TrackingUrl;

                _shipmentService.UpdateShipment(shipment);

                return continueEditing 
                    ? RedirectToAction("ShipmentDetails", new { id = shipment.Id })
                    : RedirectToAction("Edit", new { id = shipment.OrderId });
            }

            PrepareShipmentModel(model, shipment, true);

            return View(model);
        }

        [HttpPost]
        [Permission(Permissions.Order.EditShipment)]
        public ActionResult DeleteShipment(int id)
        {
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
            {
                return HttpNotFound();
            }

            var orderId = shipment.OrderId;

            NotifySuccess(T("Admin.Orders.Shipments.Deleted"));
            _shipmentService.DeleteShipment(shipment);

            return RedirectToAction("Edit", new { id = orderId });
        }

        [HttpPost]
        [Permission(Permissions.Order.EditShipment)]
        public ActionResult SetAsShipped(int id)
        {
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
            {
                return HttpNotFound();
            }

            try
            {
                _orderProcessingService.Ship(shipment, true);
            }
            catch (Exception ex)
            {
                NotifyError(ex, true);
            }

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost]
        [Permission(Permissions.Order.EditShipment)]
        public ActionResult SetAsDelivered(int id)
        {
            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
            {
                return HttpNotFound();
            }

            try
            {
                _orderProcessingService.Deliver(shipment, true);
            }
            catch (Exception ex)
            {
                NotifyError(ex, true);
            }

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [Permission(Permissions.Order.Read)]
        public ActionResult PdfPackagingSlips(bool all, string selectedIds = null)
		{
			if (!all && selectedIds.IsEmpty())
			{
				NotifyInfo(T("Admin.Common.ExportNoData"));
				return RedirectToReferrer();
			}

			IList<Shipment> shipments;

			using (var scope = new DbContextScope(Services.DbContext, autoDetectChanges: false, forceNoTracking: true))
			{
				if (all)
				{
					shipments = _shipmentService.GetAllShipments(null, null, null, 0, int.MaxValue);
				}
				else
				{
					var ids = selectedIds.ToIntArray();
					shipments = _shipmentService.GetShipmentsByIds(ids);
				}
			}

			if (shipments.Count == 0)
			{
				NotifyInfo(T("Admin.Common.ExportNoData"));
				return RedirectToReferrer();
			}

			if (shipments.Count > 500)
			{
				NotifyWarning(T("Admin.Common.ExportToPdf.TooManyItems"));
				return RedirectToReferrer();
			}

			var pdfFileName = "PackagingSlips.pdf";
			if (shipments.Count == 1)
			{
				pdfFileName = "PackagingSlip-{0}.pdf".FormatInvariant(shipments[0].Id);
			}

			var model = shipments
                .Select(x =>
                {
                    var sm = new ShipmentModel();
                    PrepareShipmentModel(sm, x, true);
                    return sm;
                })
                .ToList();

			// TODO: (mc) this is bad for multi-document processing, where orders can originate from different stores.
			var storeId = model[0].StoreId;
			var routeValues = new RouteValueDictionary(new { storeId, lid = Services.WorkContext.WorkingLanguage.Id, area = "" });
			var pdfSettings = Services.Settings.LoadSetting<PdfSettings>(storeId);

			var settings = new PdfConvertSettings
			{
				Size = pdfSettings.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4,
				Margins = new PdfPageMargins { Top = 35, Bottom = 35 },
				Page = new PdfViewContent("~/Administration/Views/Order/ShipmentDetails.Print.cshtml", model, this.ControllerContext),
				Header = new PdfRouteContent("PdfReceiptHeader", "Common", routeValues, this.ControllerContext),
				Footer = new PdfRouteContent("PdfReceiptFooter", "Common", routeValues, this.ControllerContext)
			};

			return new PdfResult(_pdfConverter, settings) { FileName = pdfFileName };
		}

        #endregion

        #region Order notes

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult OrderNotesSelect(int orderId, GridCommand command)
        {
			var model = new GridModel<OrderModel.OrderNote>();
			var order = _orderService.GetOrderById(orderId);
			var orderNoteModels = new List<OrderModel.OrderNote>();

			foreach (var orderNote in order.OrderNotes.OrderByDescending(on => on.CreatedOnUtc))
			{
				orderNoteModels.Add(new OrderModel.OrderNote
				{
					Id = orderNote.Id,
					OrderId = orderNote.OrderId,
					DisplayToCustomer = orderNote.DisplayToCustomer,
					Note = orderNote.FormatOrderNoteText(),
					CreatedOn = _dateTimeHelper.ConvertToUserTime(orderNote.CreatedOnUtc, DateTimeKind.Utc)
				});
			}

			model.Data = orderNoteModels;
			model.Total = orderNoteModels.Count;

			if (order.HasNewPaymentNotification)
			{
				order.HasNewPaymentNotification = false;
				_orderService.UpdateOrder(order);
			}

            return new JsonResult
            {
				MaxJsonLength = int.MaxValue,
                Data = model
            };
        }

        [ValidateInput(false)]
        [Permission(Permissions.Order.Update)]
        public ActionResult OrderNoteAdd(int orderId, bool displayToCustomer, string message)
        {
            var order = _orderService.GetOrderById(orderId);
            if (order == null)
            {
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);
            }

            var orderNote = new OrderNote
            {
                DisplayToCustomer = displayToCustomer,
                Note = message,
                CreatedOnUtc = DateTime.UtcNow,
            };

            order.OrderNotes.Add(orderNote);
            _orderService.UpdateOrder(order);

            if (displayToCustomer)
            {
                Services.MessageFactory.SendNewOrderNoteAddedCustomerNotification(orderNote, _workContext.WorkingLanguage.Id);
            }

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Update)]
        public ActionResult OrderNoteDelete(int orderId, int orderNoteId, GridCommand command)
        {
			var order = _orderService.GetOrderById(orderId);
			var orderNote = order.OrderNotes.Where(on => on.Id == orderNoteId).FirstOrDefault();

			_orderService.DeleteOrderNote(orderNote);

            return OrderNotesSelect(orderId, command);
        }

        #endregion

        #region Reports

        [NonAction]
        protected IList<BestsellersReportLineModel> GetBestsellersBriefReportModel(int recordsToReturn, int orderBy)
        {
			var report = _orderReportService.BestSellersReport(0, null, null, null, null, null, 0, recordsToReturn, orderBy, true);

            var model = report.Select(x =>
            {
				var product = _productService.GetProductById(x.ProductId);

                var m = new BestsellersReportLineModel
                {
                    ProductId = x.ProductId,
                    TotalAmount = _priceFormatter.FormatPrice(x.TotalAmount, true, false),
                    TotalQuantity = x.TotalQuantity,
                };

				if (product != null)
				{
					m.ProductName = product.Name;
					m.ProductTypeName = product.GetProductTypeLabel(_localizationService);
					m.ProductTypeLabelHint = product.ProductTypeLabelHint;
				}
                return m;
            }).ToList();

            return model;
        }

        [Permission(Permissions.Order.Read, false)]
        public ActionResult BestsellersBriefReportByQuantity()
        {
            var model = GetBestsellersBriefReportModel(5, 1);
            return PartialView(model);
        }

		[GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult BestsellersBriefReportByQuantityList(GridCommand command)
        {
            var model = GetBestsellersBriefReportModel(5, 1);
            var gridModel = new GridModel<BestsellersReportLineModel>
            {
                Data = model,
                Total = model.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Order.Read, false)]
        public ActionResult BestsellersBriefReportByAmount()
        {
            var model = GetBestsellersBriefReportModel(5, 2);
            return PartialView(model);
        }

		[GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult BestsellersBriefReportByAmountList(GridCommand command)
        {
            var model = GetBestsellersBriefReportModel(5, 2);
            var gridModel = new GridModel<BestsellersReportLineModel>
            {
                Data = model,
                Total = model.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Order.Read)]
        public ActionResult BestsellersReport()
        {
			var model = new BestsellersReportModel
			{
				AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList(),
				AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList()
			};

			foreach (var c in _countryService.GetAllCountriesForBilling())
            {
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            }

            model.AvailableCountries.Insert(0, new SelectListItem { Text = T("Admin.Address.SelectCountry"), Value = "0" });

            return View(model);
        }

		[GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult BestsellersReportList(GridCommand command, BestsellersReportModel model)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
				: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
				: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            OrderStatus? orderStatus = model.OrderStatusId > 0 ? (OrderStatus?)(model.OrderStatusId) : null;
            PaymentStatus? paymentStatus = model.PaymentStatusId > 0 ? (PaymentStatus?)(model.PaymentStatusId) : null;

			var items = _orderReportService.BestSellersReport(0, startDateValue, endDateValue,
                orderStatus, paymentStatus, null, model.BillingCountryId, 100, 2, true);

            var gridModel = new GridModel<BestsellersReportLineModel>
            {
                Data = items.Select(x =>
                {
					var product = _productService.GetProductById(x.ProductId);

                    var m = new BestsellersReportLineModel
                    {
                        ProductId = x.ProductId,
                        TotalAmount = _priceFormatter.FormatPrice(x.TotalAmount, true, false),
                        TotalQuantity = x.TotalQuantity
                    };

					if (product != null)
					{
						m.ProductName = product.Name;
						m.ProductTypeName = product.GetProductTypeLabel(_localizationService);
						m.ProductTypeLabelHint = product.ProductTypeLabelHint;
					}
                    return m;
                }),
                Total = items.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }


        [Permission(Permissions.Order.Read)]
        public ActionResult NeverSoldReport()
        {
            var model = new NeverSoldReportModel();
            return View(model);
        }

		[GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult NeverSoldReportList(GridCommand command, NeverSoldReportModel model)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
				: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
				: (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);
            
            var items = _orderReportService.ProductsNeverSold(startDateValue, endDateValue, command.Page - 1, command.PageSize, true);

			var gridModel = new GridModel<NeverSoldReportLineModel>
			{
				Data = items.Select(x =>
				{
					var m = new NeverSoldReportLineModel
					{
						ProductId = x.Id,
						ProductName = x.Name,
						ProductTypeName = x.GetProductTypeLabel(_localizationService),
						ProductTypeLabelHint = x.ProductTypeLabelHint
					};
					return m;
				}),
				Total = items.TotalCount
			};

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost]
        [Permission(Permissions.Order.Read)]
        public ActionResult PendingTodayReport()
        {
            var report = _orderReportService.OrderAverageReport(0, OrderStatus.Pending);
            var data = new
			{ 
                Count = report.CountTodayOrders, 
                Amount = _priceFormatter.FormatPrice(report.SumTodayOrders, true, false)
            };

            return Json(data/*, JsonRequestBehavior.AllowGet*/);
        }

        [NonAction]
        protected virtual IList<OrderAverageReportLineSummaryModel> GetOrderAverageReportModel()
        {
			var report = new List<OrderAverageReportLineSummary>
			{
				_orderReportService.OrderAverageReport(0, OrderStatus.Pending),
				_orderReportService.OrderAverageReport(0, OrderStatus.Processing),
				_orderReportService.OrderAverageReport(0, OrderStatus.Complete),
				_orderReportService.OrderAverageReport(0, OrderStatus.Cancelled)
			};

			var model = report.Select(x =>
            {
                return new OrderAverageReportLineSummaryModel
                {
                    OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    CountTodayOrders = x.CountTodayOrders,
                    SumTodayOrders = _priceFormatter.FormatPrice(x.SumTodayOrders, true, false),
                    SumThisWeekOrders = _priceFormatter.FormatPrice(x.SumThisWeekOrders, true, false),
                    SumThisMonthOrders = _priceFormatter.FormatPrice(x.SumThisMonthOrders, true, false),
                    SumThisYearOrders = _priceFormatter.FormatPrice(x.SumThisYearOrders, true, false),
                    SumAllTimeOrders = _priceFormatter.FormatPrice(x.SumAllTimeOrders, true, false),
					SumTodayOrdersRaw = x.SumTodayOrders,
					SumThisWeekOrdersRaw = x.SumThisWeekOrders,
					SumThisMonthOrdersRaw = x.SumThisMonthOrders,
					SumThisYearOrdersRaw = x.SumThisYearOrders,
					SumAllTimeOrdersRaw = x.SumAllTimeOrders,
					Url = Url.Action("List", "Order", new { OrderStatusIds = (int)x.OrderStatus })
                };
            }).ToList();

            return model;
        }

        [ChildActionOnly]
        [Permission(Permissions.Order.Read, false)]
        public ActionResult OrderAverageReport()
        {
			var model = GetOrderAverageReportModel();
            return PartialView(model);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult OrderAverageReportList(GridCommand command)
        {
            var model = GetOrderAverageReportModel();
            var gridModel = new GridModel<OrderAverageReportLineSummaryModel>
            {
                Data = model,
                Total = model.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [NonAction]
        protected virtual IList<OrderIncompleteReportLineModel> GetOrderIncompleteReportModel()
        {
            var model = new List<OrderIncompleteReportLineModel>();

            // Not paid.
			var psPending = _orderReportService.GetOrderAverageReportLine(0, null, new int[] { (int)PaymentStatus.Pending }, null, null, null, null, true);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalUnpaidOrders"),
                Count = psPending.CountOrders,
                Total = _priceFormatter.FormatPrice(psPending.SumOrders, true, false),
				Url = Url.Action("List", "Order", new { PaymentStatusIds = (int)PaymentStatus.Pending })
            });
            
            // Not shipped.
			var ssPending = _orderReportService.GetOrderAverageReportLine(0, null, null, new int[] { (int)ShippingStatus.NotYetShipped }, null, null, null, true);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalNotShippedOrders"),
                Count = ssPending.CountOrders,
                Total = _priceFormatter.FormatPrice(ssPending.SumOrders, true, false),
				Url = Url.Action("List", "Order", new { ShippingStatusIds = (int)ShippingStatus.NotYetShipped })
            });

            // Pending.
			var osPending = _orderReportService.GetOrderAverageReportLine(0, new int[] { (int)OrderStatus.Pending }, null, null, null, null, null, true);
            model.Add(new OrderIncompleteReportLineModel
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalIncompleteOrders"),
                Count = osPending.CountOrders,
                Total = _priceFormatter.FormatPrice(osPending.SumOrders, true, false),
				Url = Url.Action("List", "Order", new { OrderStatusIds = (int)OrderStatus.Pending })
            });

            return model;
        }

		[ChildActionOnly]
        [Permission(Permissions.Order.Read, false)]
        public ActionResult OrderIncompleteReport()
        {
            var model = GetOrderIncompleteReportModel();
            return PartialView(model);
        }

		[GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult OrderIncompleteReportList(GridCommand command)
        {
            var model = GetOrderIncompleteReportModel();
            var gridModel = new GridModel<OrderIncompleteReportLineModel>
            {
                Data = model,
                Total = model.Count
            };

            return new JsonResult
            {
                Data = gridModel
            };
        }

        #endregion
    }
}
