using System;
using System.Collections.Generic;
using System.IO;
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
using SmartStore.Services;
using SmartStore.Services.Affiliates;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.ExportImport;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Pdf;
using SmartStore.Services.Security;
using SmartStore.Services.Shipping;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Mvc;
using SmartStore.Web.Framework.Pdf;
using SmartStore.Web.Framework.Plugins;
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
        private readonly IExportManager _exportManager;
        private readonly IPermissionService _permissionService;
	    private readonly IWorkflowMessageService _workflowMessageService;
	    private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
	    private readonly IProductAttributeService _productAttributeService;
	    private readonly IProductAttributeParser _productAttributeParser;
	    private readonly IProductAttributeFormatter _productAttributeFormatter;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IGiftCardService _giftCardService;
        private readonly IDownloadService _downloadService;
	    private readonly IShipmentService _shipmentService;
		private readonly IStoreService _storeService;
		private readonly ITaxService _taxService;
		private readonly IPriceCalculationService _priceCalculationService;
		private readonly IEventPublisher _eventPublisher;
		private readonly ICustomerService _customerService;
		private readonly PluginMediator _pluginMediator;
		private readonly IAffiliateService _affiliateService;

        private readonly CatalogSettings _catalogSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly TaxSettings _taxSettings;
        private readonly MeasureSettings _measureSettings;
        private readonly PdfSettings _pdfSettings;
        private readonly AddressSettings _addressSettings;

        private readonly ICheckoutAttributeFormatter _checkoutAttributeFormatter;
        private readonly IPdfConverter _pdfConverter;
        private readonly ICommonServices _services;
        private readonly Lazy<IPictureService> _pictureService;

        #endregion

        #region Ctor

        public OrderController(IOrderService orderService, 
            IOrderReportService orderReportService, IOrderProcessingService orderProcessingService,
            IDateTimeHelper dateTimeHelper, IPriceFormatter priceFormatter, ILocalizationService localizationService,
            IWorkContext workContext, ICurrencyService currencyService,
            IEncryptionService encryptionService, IPaymentService paymentService,
            IMeasureService measureService,
            IAddressService addressService, ICountryService countryService,
            IStateProvinceService stateProvinceService, IProductService productService,
            IExportManager exportManager, IPermissionService permissionService,
            IWorkflowMessageService workflowMessageService,
            ICategoryService categoryService, IManufacturerService manufacturerService,
            IProductAttributeService productAttributeService, IProductAttributeParser productAttributeParser,
            IProductAttributeFormatter productAttributeFormatter, IShoppingCartService shoppingCartService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter, 
            IGiftCardService giftCardService, IDownloadService downloadService,
			IShipmentService shipmentService, IStoreService storeService,
			ITaxService taxService,
			IPriceCalculationService priceCalculationService,
			IEventPublisher eventPublisher,
			ICustomerService customerService,
			PluginMediator pluginMediator,
			IAffiliateService affiliateService,
            CatalogSettings catalogSettings, CurrencySettings currencySettings, TaxSettings taxSettings,
            MeasureSettings measureSettings, PdfSettings pdfSettings, AddressSettings addressSettings,
            IPdfConverter pdfConverter, ICommonServices services, Lazy<IPictureService> pictureService)
		{
            this._orderService = orderService;
            this._orderReportService = orderReportService;
            this._orderProcessingService = orderProcessingService;
            this._dateTimeHelper = dateTimeHelper;
            this._priceFormatter = priceFormatter;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._currencyService = currencyService;
            this._encryptionService = encryptionService;
            this._paymentService = paymentService;
            this._measureService = measureService;
            this._addressService = addressService;
            this._countryService = countryService;
            this._stateProvinceService = stateProvinceService;
            this._productService = productService;
            this._exportManager = exportManager;
            this._permissionService = permissionService;
            this._workflowMessageService = workflowMessageService;
            this._categoryService = categoryService;
            this._manufacturerService = manufacturerService;
            this._productAttributeService = productAttributeService;
            this._productAttributeParser = productAttributeParser;
            this._productAttributeFormatter = productAttributeFormatter;
            this._shoppingCartService = shoppingCartService;
            this._giftCardService = giftCardService;
            this._downloadService = downloadService;
            this._shipmentService = shipmentService;
			this._storeService = storeService;
			this._taxService = taxService;
			this._priceCalculationService = priceCalculationService;
			this._eventPublisher = eventPublisher;
			this._customerService = customerService;
			this._pluginMediator = pluginMediator;
			this._affiliateService = affiliateService;

            this._catalogSettings = catalogSettings;
            this._currencySettings = currencySettings;
            this._taxSettings = taxSettings;
            this._measureSettings = measureSettings;
            this._pdfSettings = pdfSettings;
            this._addressSettings = addressSettings;

            this._checkoutAttributeFormatter = checkoutAttributeFormatter;
            _pdfConverter = pdfConverter;
            _services = services;
            _pictureService = pictureService;
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

			var store = _storeService.GetStoreById(order.StoreId);
			var primaryStoreCurrency = store.PrimaryStoreCurrency;

            model.Id = order.Id;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.OrderNumber = order.GetOrderNumber();
            model.OrderGuid = order.OrderGuid;
			model.StoreName = (store != null ? store.Name : "".NaIfEmpty());
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

			if (order.AffiliateId != 0)
			{
				var affiliate = _affiliateService.GetAffiliateById(order.AffiliateId);
				if (affiliate != null && affiliate.Address != null)
					model.AffiliateFullName = affiliate.Address.GetFullName();
			}

            #region Order totals

            //subtotal
            model.OrderSubtotalInclTax = _priceFormatter.FormatPrice(order.OrderSubtotalInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderSubtotalExclTax = _priceFormatter.FormatPrice(order.OrderSubtotalExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.OrderSubtotalInclTaxValue = order.OrderSubtotalInclTax;
            model.OrderSubtotalExclTaxValue = order.OrderSubtotalExclTax;
            //discount (applied to order subtotal)
            string orderSubtotalDiscountInclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            string orderSubtotalDiscountExclTaxStr = _priceFormatter.FormatPrice(order.OrderSubTotalDiscountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            if (order.OrderSubTotalDiscountInclTax > decimal.Zero)
                model.OrderSubTotalDiscountInclTax = orderSubtotalDiscountInclTaxStr;
            if (order.OrderSubTotalDiscountExclTax > decimal.Zero)
                model.OrderSubTotalDiscountExclTax = orderSubtotalDiscountExclTaxStr;
            model.OrderSubTotalDiscountInclTaxValue = order.OrderSubTotalDiscountInclTax;
            model.OrderSubTotalDiscountExclTaxValue = order.OrderSubTotalDiscountExclTax;

            //shipping
            model.OrderShippingInclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true);
            model.OrderShippingExclTax = _priceFormatter.FormatShippingPrice(order.OrderShippingExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
            model.OrderShippingInclTaxValue = order.OrderShippingInclTax;
            model.OrderShippingExclTaxValue = order.OrderShippingExclTax;

            //payment method additional fee
            if (order.PaymentMethodAdditionalFeeInclTax != decimal.Zero)
            {
                model.PaymentMethodAdditionalFeeInclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeInclTax, true, 
					primaryStoreCurrency, _workContext.WorkingLanguage, true);
                model.PaymentMethodAdditionalFeeExclTax = _priceFormatter.FormatPaymentMethodAdditionalFee(order.PaymentMethodAdditionalFeeExclTax, true, 
					primaryStoreCurrency, _workContext.WorkingLanguage, false);
            }
            model.PaymentMethodAdditionalFeeInclTaxValue = order.PaymentMethodAdditionalFeeInclTax;
            model.PaymentMethodAdditionalFeeExclTaxValue = order.PaymentMethodAdditionalFeeExclTax;


            //tax
            model.Tax = _priceFormatter.FormatPrice(order.OrderTax, true, false);
            SortedDictionary<decimal, decimal> taxRates = order.TaxRatesDictionary;
            bool displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
            bool displayTax = !displayTaxRates;
            foreach (var tr in order.TaxRatesDictionary)
            {
                model.TaxRates.Add(new OrderModel.TaxRate()
                {
                    Rate = _priceFormatter.FormatTaxRate(tr.Key),
                    Value = _priceFormatter.FormatPrice(tr.Value, true, false),
                });
            }
            model.DisplayTaxRates = displayTaxRates;
            model.DisplayTax = displayTax;
            model.TaxValue = order.OrderTax;
            model.TaxRatesValue = order.TaxRates;

            //discount
            if (order.OrderDiscount > 0)
                model.OrderTotalDiscount = _priceFormatter.FormatPrice(-order.OrderDiscount, true, false);
            model.OrderTotalDiscountValue = order.OrderDiscount;

            //gift cards
            foreach (var gcuh in order.GiftCardUsageHistory)
            {
                model.GiftCards.Add(new OrderModel.GiftCard()
                {
                    CouponCode = gcuh.GiftCard.GiftCardCouponCode,
                    Amount = _priceFormatter.FormatPrice(-gcuh.UsedValue, true, false),
                });
            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                model.RedeemedRewardPoints = -order.RedeemedRewardPointsEntry.Points;
                model.RedeemedRewardPointsAmount = _priceFormatter.FormatPrice(-order.RedeemedRewardPointsEntry.UsedAmount, true, false);
            }

            //total
            model.OrderTotal = _priceFormatter.FormatPrice(order.OrderTotal, true, false);
            model.OrderTotalValue = order.OrderTotal;

            //refunded amount
            if (order.RefundedAmount > decimal.Zero)
                model.RefundedAmount = _priceFormatter.FormatPrice(order.RefundedAmount, true, false);


            #endregion

            #region Payment info

            if (order.AllowStoringCreditCardNumber)
            {
                //card type
                model.CardType = _encryptionService.DecryptText(order.CardType);
                //cardholder name
                model.CardName = _encryptionService.DecryptText(order.CardName);
                //card number
                model.CardNumber = _encryptionService.DecryptText(order.CardNumber);
                //cvv
                model.CardCvv2 = _encryptionService.DecryptText(order.CardCvv2);
                //expiry date
                string cardExpirationMonthDecrypted = _encryptionService.DecryptText(order.CardExpirationMonth);
                if (!String.IsNullOrEmpty(cardExpirationMonthDecrypted) && cardExpirationMonthDecrypted != "0")
                    model.CardExpirationMonth = cardExpirationMonthDecrypted;
                string cardExpirationYearDecrypted = _encryptionService.DecryptText(order.CardExpirationYear);
                if (!String.IsNullOrEmpty(cardExpirationYearDecrypted) && cardExpirationYearDecrypted != "0")
                    model.CardExpirationYear = cardExpirationYearDecrypted;

                model.AllowStoringCreditCardNumber = true;
            }
            else
            {
                string maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(order.MaskedCreditCardNumber);
                if (!String.IsNullOrEmpty(maskedCreditCardNumberDecrypted))
                    model.CardNumber = maskedCreditCardNumberDecrypted;
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

            //purchase order number (we have to find a better to inject this information because it's related to a certain plugin)
            var pm = _paymentService.LoadPaymentMethodBySystemName(order.PaymentMethodSystemName);
			if (pm != null)
			{
                if (pm.Metadata.SystemName.Equals("SmartStore.PurchaseOrderNumber", StringComparison.InvariantCultureIgnoreCase))
				{
					model.DisplayPurchaseOrderNumber = true;
					model.PurchaseOrderNumber = order.PurchaseOrderNumber;
				}

				model.PaymentMethod = _pluginMediator.GetLocalizedFriendlyName(pm.Metadata);
				model.PaymentMethodSystemName = order.PaymentMethodSystemName;
			}
			else
			{
				model.PaymentMethod = order.PaymentMethodSystemName;
			}

            //payment transaction info
            model.AuthorizationTransactionId = order.AuthorizationTransactionId;
            model.CaptureTransactionId = order.CaptureTransactionId;
            model.SubscriptionTransactionId = order.SubscriptionTransactionId;
			model.AuthorizationTransactionResult = order.AuthorizationTransactionResult;
			model.CaptureTransactionResult = order.CaptureTransactionResult;

            //payment method info
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);

            //payment method buttons
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
			model.MaxAmountToRefundFormatted =	_priceFormatter.FormatPrice(model.MaxAmountToRefund, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, false);

            //recurring payment record
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

            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext); ;
            if (order.ShippingStatus != ShippingStatus.ShippingNotRequired)
            {
                model.IsShippable = true;

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

                model.ShippingMethod = order.ShippingMethod;

                model.ShippingAddressGoogleMapsUrl = string.Format("http://maps.google.com/maps?f=q&hl=en&ie=UTF8&oe=UTF8&geocode=&q={0}", Server.UrlEncode(order.ShippingAddress.Address1 + " " + order.ShippingAddress.ZipPostalCode + " " + order.ShippingAddress.City + " " + (order.ShippingAddress.Country != null ? order.ShippingAddress.Country.Name : "")));
                model.CanAddNewShipments = order.HasItemsToAddToShipment();
            }

            #endregion

            #region Products
            model.CheckoutAttributeInfo = HtmlUtils.ConvertPlainTextToTable(HtmlUtils.ConvertHtmlToPlainText(order.CheckoutAttributeDescription));
            //model.CheckoutAttributeInfo = order.CheckoutAttributeDescription;
            //model.CheckoutAttributeInfo = _checkoutAttributeFormatter.FormatAttributes(_workContext.CurrentCustomer.CheckoutAttributes, _workContext.CurrentCustomer, "", false);
            bool hasDownloadableItems = false;
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
							bundleItemModel.PriceWithDiscount = _priceFormatter.FormatPrice(bundleItem.PriceWithDiscount, true, primaryStoreCurrency, _workContext.WorkingLanguage, false);
						}

						orderItemModel.BundleItems.Add(bundleItemModel);
					}
				}

                //unit price
                orderItemModel.UnitPriceInclTaxValue = orderItem.UnitPriceInclTax;
                orderItemModel.UnitPriceExclTaxValue = orderItem.UnitPriceExclTax;
				orderItemModel.TaxRate = orderItem.TaxRate;
                orderItemModel.UnitPriceInclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.UnitPriceExclTax = _priceFormatter.FormatPrice(orderItem.UnitPriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                //discounts
                orderItemModel.DiscountInclTaxValue = orderItem.DiscountAmountInclTax;
                orderItemModel.DiscountExclTaxValue = orderItem.DiscountAmountExclTax;
                orderItemModel.DiscountInclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.DiscountExclTax = _priceFormatter.FormatPrice(orderItem.DiscountAmountExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);
                //subtotal
                orderItemModel.SubTotalInclTaxValue = orderItem.PriceInclTax;
                orderItemModel.SubTotalExclTaxValue = orderItem.PriceExclTax;
                orderItemModel.SubTotalInclTax = _priceFormatter.FormatPrice(orderItem.PriceInclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, true, true);
                orderItemModel.SubTotalExclTax = _priceFormatter.FormatPrice(orderItem.PriceExclTax, true, primaryStoreCurrency, _workContext.WorkingLanguage, false, true);

                orderItemModel.AttributeInfo = orderItem.AttributeDescription;
				if (orderItem.Product.IsRecurring)
				{
					orderItemModel.RecurringInfo = string.Format(_localizationService.GetResource("Admin.Orders.Products.RecurringPeriod"), 
						orderItem.Product.RecurringCycleLength, orderItem.Product.RecurringCyclePeriod.GetLocalizedEnum(_localizationService, _workContext));
				}

                //return requests
				orderItemModel.ReturnRequests = _orderService.SearchReturnRequests(0, 0, orderItem.Id, null, 0, int.MaxValue).Select(x =>
					{
						return new OrderModel.ReturnRequestModel()
						{
							Id = x.Id,
							Quantity = x.Quantity,
							Status = x.ReturnRequestStatus,
							StatusString = x.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext)
						};
					})
					.ToList();

                //gift cards
                orderItemModel.PurchasedGiftCardIds = _giftCardService.GetGiftCardsByPurchasedWithOrderItemId(orderItem.Id)
                    .Select(gc => gc.Id).ToList();

                model.Items.Add(orderItemModel);
            }

            model.HasDownloadableProducts = hasDownloadableItems;

			model.AutoUpdateOrderItem.Caption = _localizationService.GetResource("Admin.Orders.EditOrderDetails");
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
				throw new ArgumentException("No product found with the specified id");

			var customer = _workContext.CurrentCustomer;	// TODO: we need a customer representing entity instance for backend work
			var order = _orderService.GetOrderById(orderId);

			decimal taxRate = decimal.Zero;
			decimal unitPriceTaxRate = decimal.Zero;
			decimal unitPrice = _priceCalculationService.GetFinalPrice(product, null, customer, decimal.Zero, false, 1);
			decimal unitPriceInclTax = _taxService.GetProductPrice(product, unitPrice, true, customer, out unitPriceTaxRate);
			decimal unitPriceExclTax = _taxService.GetProductPrice(product, unitPrice, false, customer, out taxRate);

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

        [NonAction]
		protected ShipmentModel PrepareShipmentModel(Shipment shipment, bool prepareProducts, bool prepareAddresses)
        {
            //measures
            var baseWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            var baseWeightIn = baseWeight != null ? baseWeight.Name : "";
            var baseDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            var baseDimensionIn = baseDimension != null ? baseDimension.Name : "";

			var orderStoreId = shipment.Order.StoreId;

			var model = new ShipmentModel
            {
                Id = shipment.Id,
                OrderId = shipment.OrderId,
				StoreId = orderStoreId,
				ShippingMethod = shipment.Order.ShippingMethod,
                TrackingNumber = shipment.TrackingNumber,
                TotalWeight = shipment.TotalWeight.HasValue ? string.Format("{0:F2} [{1}]", shipment.TotalWeight, baseWeightIn) : "",
                ShippedDate = shipment.ShippedDateUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(shipment.ShippedDateUtc.Value, DateTimeKind.Utc).ToString() : _localizationService.GetResource("Admin.Orders.Shipments.ShippedDate.NotYet"),
                CanShip = !shipment.ShippedDateUtc.HasValue,
                DeliveryDate = shipment.DeliveryDateUtc.HasValue ? _dateTimeHelper.ConvertToUserTime(shipment.DeliveryDateUtc.Value, DateTimeKind.Utc).ToString() : _localizationService.GetResource("Admin.Orders.Shipments.DeliveryDate.NotYet"),
                CanDeliver = shipment.ShippedDateUtc.HasValue && !shipment.DeliveryDateUtc.HasValue,
                DisplayPdfPackagingSlip = _pdfSettings.Enabled,
            };

			if (prepareAddresses)
			{
				model.ShippingAddress = shipment.Order.ShippingAddress;

				var store = _services.StoreService.GetStoreById(orderStoreId) ?? _services.StoreContext.CurrentStore;
				var companyInfoSettings = _services.Settings.LoadSetting<CompanyInformationSettings>(store.Id);
				model.MerchantCompanyInfo = companyInfoSettings;
			}

            if (prepareProducts)
            {
                foreach (var shipmentItem in shipment.ShipmentItems)
                {
                    var orderItem = _orderService.GetOrderItemById(shipmentItem.OrderItemId);
                    if (orderItem == null)
                        continue;

                    //quantities
                    var qtyInThisShipment = shipmentItem.Quantity;
                    var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                    var qtyOrdered = orderItem.Quantity;
                    var qtyInAllShipments = orderItem.GetTotalNumberOfItemsInAllShipment();

                    orderItem.Product.MergeWithCombination(orderItem.AttributesXml);
                    var shipmentItemModel = new ShipmentModel.ShipmentItemModel()
                    {
                        Id = shipmentItem.Id,
                        OrderItemId = orderItem.Id,
                        ProductId = orderItem.ProductId,
						ProductName = orderItem.Product.Name,
						ProductTypeName = orderItem.Product.GetProductTypeLabel(_localizationService),
						ProductTypeLabelHint = orderItem.Product.ProductTypeLabelHint,
                        Sku = orderItem.Product.Sku,
                        AttributeInfo = orderItem.AttributeDescription,
                        ItemWeight = orderItem.ItemWeight.HasValue ? string.Format("{0:F2} [{1}]", orderItem.ItemWeight, baseWeightIn) : "",
                        ItemDimensions = string.Format("{0:F2} x {1:F2} x {2:F2} [{3}]", orderItem.Product.Length, orderItem.Product.Width, orderItem.Product.Height, baseDimensionIn),
                        QuantityOrdered = qtyOrdered,
                        QuantityInThisShipment = qtyInThisShipment,
                        QuantityInAllShipments = qtyInAllShipments,
                        QuantityToAdd = maxQtyToAdd,
                    };

                    model.Items.Add(shipmentItemModel);
                }
            }
            return model;
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

        public ActionResult List(OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailableShippingStatuses = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();

			//stores
			model.AvailableStores.AddRange(_storeService.GetAllStores().ToList().ToSelectListItems());
			model.AvailableStores.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
		}

		[GridAction(EnableCustomBinding = true)]
		public ActionResult OrderList(GridCommand command, OrderListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);
            DateTime? endDateValue = (model.EndDate == null) ? null : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

			var orderStatusIds = model.OrderStatusIds.ToIntArray();
			var paymentStatusIds = model.PaymentStatusIds.ToIntArray();
			var shippingStatusIds = model.ShippingStatusIds.ToIntArray();

			var orders = _orderService.SearchOrders(model.StoreId, 0, startDateValue, endDateValue, orderStatusIds, paymentStatusIds, shippingStatusIds,
                model.CustomerEmail, model.OrderGuid, model.OrderNumber, command.Page - 1, command.PageSize, model.CustomerName);

            var gridModel = new GridModel<OrderModel>
            {
                Data = orders.Select(x =>
                {
					var store = _storeService.GetStoreById(x.StoreId);
                    return new OrderModel
                    {
                        Id = x.Id,
                        OrderNumber = x.GetOrderNumber(),
						StoreName = store != null ? store.Name : "Unknown",
                        OrderTotal = _priceFormatter.FormatPrice(x.OrderTotal, true, false),
                        OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                        PaymentStatus = x.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext),
                        ShippingStatus = x.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext),
                        CustomerEmail = x.BillingAddress.Email,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
						HasNewPaymentNotification = x.HasNewPaymentNotification
                    };
                }),
                Total = orders.TotalCount
            };

            //summary report
            //implemented as a workaround described here: http://www.telerik.com/community/forums/aspnet-mvc/grid/gridmodel-aggregates-how-to-use.aspx
			var reportSummary = _orderReportService.GetOrderAverageReportLine(model.StoreId, orderStatusIds,
				paymentStatusIds, shippingStatusIds, startDateValue, endDateValue, model.CustomerEmail);

			var profit = _orderReportService.ProfitReport(model.StoreId, orderStatusIds,
				paymentStatusIds, shippingStatusIds, startDateValue, endDateValue, model.CustomerEmail);

		    var aggregator = new OrderModel()
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
        public ActionResult GoToOrderId(OrderListModel model)
        {
            var order = _orderService.GetOrderByNumber(model.GoDirectlyToNumber);

            if (order != null)
                return RedirectToAction("Edit", "Order", new { id = order.Id });

			NotifyWarning(_localizationService.GetResource("Admin.Order.NotFound"));

			return RedirectToAction("List", "Order");
        }

        #endregion

        #region Export / Import

        public ActionResult ExportXmlAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            try
            {
				var orders = _orderService.SearchOrders(0, 0, null, null, null,
                    null, null, null, null, null, 0, int.MaxValue);

                var xml = _exportManager.ExportOrdersToXml(orders);
                return new XmlDownloadResult(xml, "orders.xml");
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("List");
            }
        }

		[HttpPost]
        public ActionResult ExportXmlSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }

            var xml = _exportManager.ExportOrdersToXml(orders);
            return new XmlDownloadResult(xml, "orders.xml");
        }

	    public ActionResult ExportExcelAll()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            try
            {
				var orders = _orderService.SearchOrders(0, 0, null, null, null,
                    null, null, null, null, null, 0, int.MaxValue);
                
                byte[] bytes = null;
                using (var stream = new MemoryStream())
                {
                    _exportManager.ExportOrdersToXlsx(stream, orders);
                    bytes = stream.ToArray();
                }
                return File(bytes, "text/xls", "orders.xlsx");
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("List");
            }
        }

		[HttpPost]
        public ActionResult ExportExcelSelected(string selectedIds)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(_orderService.GetOrdersByIds(ids));
            }

            byte[] bytes = null;
            using (var stream = new MemoryStream())
            {
                _exportManager.ExportOrdersToXlsx(stream, orders);
                bytes = stream.ToArray();
            }
            return File(bytes, "text/xls", "orders.xlsx");
        }

		public ActionResult ExportPdf(bool all, string selectedIds = null)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
				return AccessDeniedView();

			if (!all && selectedIds.IsEmpty())
			{
				NotifyInfo(_localizationService.GetResource("Admin.Common.ExportNoData"));
				return RedirectToAction("List");
			}

			return RedirectToAction("PrintMany", "Order", new { ids = selectedIds, pdf = true, area = "" });
		}

        #endregion

        #region Order details

        #region Payments and other order workflow

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelorder")]
        public ActionResult CancelOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");
            
            try
            {
                _orderProcessingService.CancelOrder(order, true);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                NotifyError(exc, false);
                return View(model);
            }
        }

		[HttpPost, ActionName("Edit")]
		[FormValueRequired("completeorder")]
		public ActionResult CompleteOrder(int id)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
				return AccessDeniedView();

			var order = _orderService.GetOrderById(id);
			if (order == null)
				return RedirectToAction("List");

			try
			{
				_orderProcessingService.CompleteOrder(order);
			}
			catch (Exception exc)
			{
				NotifyError(exc, false);
			}

			var model = new OrderModel();
			PrepareOrderDetailsModel(model, order);
			return View(model);
		}

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("captureorder")]
        public ActionResult CaptureOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");
            
            try
            {
                var errors = _orderProcessingService.Capture(order);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
					NotifyError(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                NotifyError(exc, false);
                return View(model);
            }

        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markorderaspaid")]
        public ActionResult MarkOrderAsPaid(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");
            
            try
            {
                _orderProcessingService.MarkOrderAsPaid(order);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                NotifyError(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorder")]
        public ActionResult RefundOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            try
            {
                var errors = _orderProcessingService.Refund(order);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
					NotifyError(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                NotifyError(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorderoffline")]
        public ActionResult RefundOrderOffline(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.RefundOffline(order);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                NotifyError(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorder")]
        public ActionResult VoidOrder(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            try
            {
                var errors = _orderProcessingService.Void(order);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                foreach (var error in errors)
					NotifyError(error, false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                NotifyError(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorderoffline")]
        public ActionResult VoidOrderOffline(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.VoidOffline(order);
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new OrderModel();
                PrepareOrderDetailsModel(model, order);
                NotifyError(exc, false);
                return View(model);
            }
        }
        
        public ActionResult PartiallyRefundOrderPopup(int id, bool online)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("partialrefundorder")]
        public ActionResult PartiallyRefundOrderPopup(string btnId, string formId, int id, bool online, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            try
            {
                decimal amountToRefund = model.AmountToRefund;
                if (amountToRefund <= decimal.Zero)
                    throw new SmartException("Enter amount to refund");

                decimal maxAmountToRefund = order.OrderTotal - order.RefundedAmount;
                if (amountToRefund > maxAmountToRefund)
                    amountToRefund = maxAmountToRefund;

                var errors = new List<string>();
                if (online)
                    errors = _orderProcessingService.PartiallyRefund(order, amountToRefund).ToList();
                else
                    _orderProcessingService.PartiallyRefundOffline(order, amountToRefund);

                if (errors.Count == 0)
                {
                    //success
                    ViewBag.RefreshPage = true;
                    ViewBag.btnId = btnId;
                    ViewBag.formId = formId;

                    PrepareOrderDetailsModel(model, order);
                    return View(model);
                }
                else
                {
                    //error
                    PrepareOrderDetailsModel(model, order);
                    foreach (var error in errors)
						NotifyError(error, false);
                    return View(model);
                }
            }
            catch (Exception exc)
            {
                //error
                PrepareOrderDetailsModel(model, order);
                NotifyError(exc, false);
                return View(model);
            }
        }

        #endregion

        #region Edit, delete

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null || order.Deleted)
                return RedirectToAction("List");

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);

            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

            _orderProcessingService.DeleteOrder(order);
            return RedirectToAction("List");
        }

        public ActionResult Print(int orderId, bool pdf = false)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            return RedirectToAction("Print", "Order", new { id = orderId, pdf = pdf, area = "" });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveCC")]
        public ActionResult EditCreditCardInfo(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

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
        public ActionResult EditDirectDebitInfo(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

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
        public ActionResult EditOrderTotals(int id, OrderModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                return RedirectToAction("List");

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
            order.OrderTotal = model.OrderTotalValue;
            _orderService.UpdateOrder(order);

            PrepareOrderDetailsModel(model, order);
            return View(model);
        }
        
        [HttpPost]
        public ActionResult EditOrderItem(AutoUpdateOrderItemModel model, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

			var oi = _orderService.GetOrderItemById(model.Id);

			if (oi == null)
				throw new ArgumentException("No order item found with the specified id");

			int orderId = oi.Order.Id;

			if (model.NewQuantity.HasValue)
			{
				var context = new AutoUpdateOrderItemContext()
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
		public ActionResult DeleteOrderItem(AutoUpdateOrderItemModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

			var oi = _orderService.GetOrderItemById(model.Id);

			if (oi == null)
				throw new ArgumentException("No order item found with the specified id");

			int orderId = oi.Order.Id;
			var context = new AutoUpdateOrderItemContext()
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
		public ActionResult AddReturnRequest(int id, FormCollection form)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageReturnRequests))
				return AccessDeniedView();

			var order = _orderService.GetOrderById(id);
			if (order == null)
				return RedirectToAction("List");

			//get order item identifier
			int orderItemId = 0;
			foreach (var formValue in form.AllKeys)
				if (formValue.StartsWith("btnAddReturnRequest", StringComparison.InvariantCultureIgnoreCase))
					orderItemId = Convert.ToInt32(formValue.Substring("btnAddReturnRequest".Length));

			var orderItem = order.OrderItems.Where(x => x.Id == orderItemId).FirstOrDefault();
			if (orderItem == null)
				throw new ArgumentException("No order item found with the specified id");

			if (orderItem.Quantity > 0)
			{
				var returnRequest = new ReturnRequest()
				{
					StoreId = order.StoreId,
					OrderItemId = orderItem.Id,
					Quantity = orderItem.Quantity,
					CustomerId = order.CustomerId,
					ReasonForReturn = "",
					RequestedAction = "",
					StaffNotes = "",
					ReturnRequestStatus = ReturnRequestStatus.Pending,
					CreatedOnUtc = DateTime.UtcNow,
					UpdatedOnUtc = DateTime.UtcNow
				};

				order.Customer.ReturnRequests.Add(returnRequest);
				_customerService.UpdateCustomer(order.Customer);

				return RedirectToAction("Edit", "ReturnRequest", new { id = returnRequest.Id });
			}

			var model = new OrderModel();
			PrepareOrderDetailsModel(model, order);
			return View(model);
		}

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnResetDownloadCount")]
        [ValidateInput(false)]
        public ActionResult ResetDownloadCount(int id, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //get order item identifier
            int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
                if (formValue.StartsWith("btnResetDownloadCount", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnResetDownloadCount".Length));

            var orderItem = order.OrderItems.Where(x => x.Id == orderItemId).FirstOrDefault();
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            orderItem.DownloadCount = 0;
            _orderService.UpdateOrder(order);

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnPvActivateDownload")]
        [ValidateInput(false)]
        public ActionResult ActivateDownloadOrderItem(int id, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            //get order item identifier
            int orderItemId = 0;
            foreach (var formValue in form.AllKeys)
                if (formValue.StartsWith("btnPvActivateDownload", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnPvActivateDownload".Length));

            var orderItem = order.OrderItems.Where(x => x.Id == orderItemId).FirstOrDefault();
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            orderItem.IsDownloadActivated = !orderItem.IsDownloadActivated;
            _orderService.UpdateOrder(order);

            var model = new OrderModel();
            PrepareOrderDetailsModel(model, order);
            return View(model);
        }

        public ActionResult UploadLicenseFilePopup(int id, int orderItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(id);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var orderItem = order.OrderItems.Where(x => x.Id == orderItemId).FirstOrDefault();
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            if (!orderItem.Product.IsDownload)
                throw new ArgumentException("Product is not downloadable");

            var model = new OrderModel.UploadLicenseModel
            {
                LicenseDownloadId = orderItem.LicenseDownloadId.HasValue ? orderItem.LicenseDownloadId.Value : 0,
                OrderId = order.Id,
                OrderItemId = orderItem.Id
            };

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("uploadlicense")]
        public ActionResult UploadLicenseFilePopup(string btnId, string formId, OrderModel.UploadLicenseModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var orderItem = order.OrderItems.Where(x => x.Id == model.OrderItemId).FirstOrDefault();
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //attach license
            if (model.LicenseDownloadId > 0)
                orderItem.LicenseDownloadId = model.LicenseDownloadId;
            else
                orderItem.LicenseDownloadId = null;

			MediaHelper.UpdateDownloadTransientStateFor(orderItem, x => x.LicenseDownloadId);

            _orderService.UpdateOrder(order);

            //success
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            return View(model);
        }

        [HttpPost, ActionName("UploadLicenseFilePopup")]
        [FormValueRequired("deletelicense")]
        public ActionResult DeleteLicenseFilePopup(string btnId, string formId, OrderModel.UploadLicenseModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var orderItem = order.OrderItems.Where(x => x.Id == model.OrderItemId).FirstOrDefault();
            if (orderItem == null)
                throw new ArgumentException("No order item found with the specified id");

            //attach license
            orderItem.LicenseDownloadId = null;
			MediaHelper.UpdateDownloadTransientStateFor(orderItem, x => x.LicenseDownloadId);
            _orderService.UpdateOrder(order);

            //success
            ViewBag.RefreshPage = true;
            ViewBag.btnId = btnId;
            ViewBag.formId = formId;

            return View(model);
        }

        public ActionResult AddProductToOrder(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new OrderModel.AddOrderProductModel();
            model.OrderId = orderId;

            //categories
            var allCategories = _categoryService.GetAllCategories(showHidden: true);
            var mappedCategories = allCategories.ToDictionary(x => x.Id);
            foreach (var c in allCategories)
            {
                model.AvailableCategories.Add(new SelectListItem() { Text = c.GetCategoryNameWithPrefix(_categoryService, mappedCategories), Value = c.Id.ToString() });
            }

            //manufacturers
            foreach (var m in _manufacturerService.GetAllManufacturers(true))
            {
                model.AvailableManufacturers.Add(new SelectListItem() { Text = m.Name, Value = m.Id.ToString() });
            }

			//product types
			model.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();
			model.AvailableProductTypes.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult AddProductToOrder(GridCommand command, OrderModel.AddOrderProductModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var gridModel = new GridModel();
			var searchContext = new ProductSearchContext()
			{
				CategoryIds = new List<int>() { model.SearchCategoryId },
				ManufacturerId = model.SearchManufacturerId,
				Keywords = model.SearchProductName,
				PageIndex = command.Page - 1,
				PageSize = command.PageSize,
				ShowHidden = true,
				ProductType = model.SearchProductTypeId > 0 ? (ProductType?)model.SearchProductTypeId : null
			};

            var products = _productService.SearchProducts(searchContext);
            gridModel.Data = products.Select(x =>
            {
                var productModel = new OrderModel.AddOrderProductModel.ProductModel
                {
                    Id = x.Id,
                    Name =  x.Name,
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

        public ActionResult AddProductToOrderDetails(int orderId, int productId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = PrepareAddProductToOrderModel(orderId, productId);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddProductToOrderDetails(int orderId, int productId, bool adjustInventory, bool? updateTotals, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            var product = _productService.GetProductById(productId);
            //save order item

            //basic properties
            var unitPriceInclTax = decimal.Zero;
            decimal.TryParse(form["UnitPriceInclTax"], out unitPriceInclTax);
            var unitPriceExclTax = decimal.Zero;
            decimal.TryParse(form["UnitPriceExclTax"], out unitPriceExclTax);
            var quantity = 1;
            int.TryParse(form["Quantity"], out quantity);
            var priceInclTax = decimal.Zero;
            decimal.TryParse(form["SubTotalInclTax"], out priceInclTax);
            var priceExclTax = decimal.Zero;
            decimal.TryParse(form["SubTotalExclTax"], out priceExclTax);
			var unitPriceTaxRate = decimal.Zero;
			decimal.TryParse(form["TaxRate"], out unitPriceTaxRate);

            var warnings = new List<string>();
            string attributes = "";

			if (product.ProductType != ProductType.BundledProduct)
			{
				var variantAttributes = _productAttributeService.GetProductVariantAttributesByProductId(product.Id);

				attributes = form.CreateSelectedAttributesXml(product.Id, variantAttributes, _productAttributeParser, _localizationService, _downloadService,
					_catalogSettings, this.Request, warnings, false);
			}

            #region Gift cards

            string recipientName = "";
            string recipientEmail = "";
            string senderName = "";
            string senderEmail = "";
            string giftCardMessage = "";
            if (product.IsGiftCard)
            {
                foreach (string formKey in form.AllKeys)
                {
                    if (formKey.Equals("giftcard.RecipientName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        recipientName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.RecipientEmail", StringComparison.InvariantCultureIgnoreCase))
                    {
                        recipientEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderName", StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderName = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.SenderEmail", StringComparison.InvariantCultureIgnoreCase))
                    {
                        senderEmail = form[formKey];
                        continue;
                    }
                    if (formKey.Equals("giftcard.Message", StringComparison.InvariantCultureIgnoreCase))
                    {
                        giftCardMessage = form[formKey];
                        continue;
                    }
                }

                attributes = _productAttributeParser.AddGiftCardAttribute(attributes,
                    recipientName, recipientEmail, senderName, senderEmail, giftCardMessage);
            }

            #endregion

            //warnings
            warnings.AddRange(_shoppingCartService.GetShoppingCartItemAttributeWarnings(order.Customer, ShoppingCartType.ShoppingCart, product, attributes, quantity));
            warnings.AddRange(_shoppingCartService.GetShoppingCartItemGiftCardWarnings(ShoppingCartType.ShoppingCart, product, attributes));

            if (warnings.Count == 0)
            {
                //attributes
                string attributeDescription = _productAttributeFormatter.FormatAttributes(product, attributes, order.Customer);

                //save item
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
					ProductCost = _priceCalculationService.GetProductCost(product, attributes)
                };

				if (product.ProductType == ProductType.BundledProduct)
				{
					var listBundleData = new List<ProductBundleItemOrderData>();
					var bundleItems = _productService.GetBundleItems(product.Id);

					foreach (var bundleItem in bundleItems)
					{
						decimal taxRate;
						decimal finalPrice = _priceCalculationService.GetFinalPrice(bundleItem.Item.Product, bundleItems, order.Customer, decimal.Zero, true, bundleItem.Item.Quantity);
						decimal bundleItemSubTotalWithDiscountBase = _taxService.GetProductPrice(bundleItem.Item.Product, finalPrice, out taxRate);

						bundleItem.ToOrderData(listBundleData, bundleItemSubTotalWithDiscountBase);
					}

					orderItem.SetBundleData(listBundleData);
				}

                order.OrderItems.Add(orderItem);
                _orderService.UpdateOrder(order);

                //gift cards
                if (product.IsGiftCard)
                {
                    for (int i = 0; i < orderItem.Quantity; i++)
                    {
                        var gc = new GiftCard()
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
					var context = new AutoUpdateOrderItemContext()
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

                //redirect to order details page
                return RedirectToAction("Edit", "Order", new { id = order.Id });
            }
            else
            {
                //errors
                var model = PrepareAddProductToOrderModel(order.Id, product.Id);
                model.Warnings.AddRange(warnings);
                return View(model);
            }
        }

        #endregion

        #endregion

        #region Addresses

        public ActionResult AddressEdit(int addressId, int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(addressId);
            if (address == null)
                throw new ArgumentException("No address found with the specified id", "addressId");

            var model = new OrderAddressModel();
            model.OrderId = orderId;

			PrepareOrderAddressModel(model, address);

            return View(model);
        }

        [HttpPost]
        public ActionResult AddressEdit(OrderAddressModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(model.OrderId);
            if (order == null)
                return RedirectToAction("List");

            var address = _addressService.GetAddressById(model.Address.Id);
            if (address == null)
                throw new ArgumentException("No address found with the specified id");

            if (ModelState.IsValid)
            {
                address = model.Address.ToEntity(address);
                _addressService.UpdateAddress(address);

				_eventPublisher.PublishOrderUpdated(order);

				NotifySuccess(_localizationService.GetResource("Admin.Common.DataSuccessfullySaved"));

                return RedirectToAction("AddressEdit", new { addressId = model.Address.Id, orderId = model.OrderId });
            }

            //If we got this far, something failed, redisplay form
            model.OrderId = order.Id;

			PrepareOrderAddressModel(model, address);

            return View(model);
        }

        #endregion

        #region Shipments


        public ActionResult ShipmentList()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var model = new ShipmentListModel();
            model.DisplayPdfPackagingSlip = _pdfSettings.Enabled;
            return View(model);
		}

		[GridAction(EnableCustomBinding = true)]
        public ActionResult ShipmentListSelect(GridCommand command, ShipmentListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null 
                            :(DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);

            //load shipments
			var shipments = _shipmentService.GetAllShipments(model.TrackingNumber, startDateValue, endDateValue, 
                command.Page - 1, command.PageSize);
            var gridModel = new GridModel<ShipmentModel>
            {
                Data = shipments.Select(shipment => PrepareShipmentModel(shipment, false, false)),
                Total = shipments.TotalCount
            };
			return new JsonResult
			{
				Data = gridModel
			};
		}
        
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult ShipmentsSelect(int orderId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            //shipments
            var shipmentModels = new List<ShipmentModel>();
            var shipments = order.Shipments.OrderBy(s => s.CreatedOnUtc).ToList();
            foreach (var shipment in shipments)
                shipmentModels.Add(PrepareShipmentModel(shipment, false, false));

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

        public ActionResult AddShipment(int orderId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            var model = new ShipmentModel()
            {
                OrderId = order.Id,
            };

            //measures
            var baseWeight = _measureService.GetMeasureWeightById(_measureSettings.BaseWeightId);
            var baseWeightIn = baseWeight != null ? baseWeight.Name : "";
            var baseDimension = _measureService.GetMeasureDimensionById(_measureSettings.BaseDimensionId);
            var baseDimensionIn = baseDimension != null ? baseDimension.Name : "";


            foreach (var orderItem in order.OrderItems)
            {
                //we can ship only shippable products
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                //quantities
                var qtyInThisShipment = 0;
                var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                var qtyOrdered = orderItem.Quantity;
                var qtyInAllShipments = orderItem.GetTotalNumberOfItemsInAllShipment();

                //ensure that this product can be added to a shipment
                if (maxQtyToAdd <= 0)
                    continue;

                orderItem.Product.MergeWithCombination(orderItem.AttributesXml);
                var shipmentItemModel = new ShipmentModel.ShipmentItemModel()
                {
                    OrderItemId = orderItem.Id,
					ProductId = orderItem.ProductId,
					ProductName = orderItem.Product.Name,
					ProductTypeName = orderItem.Product.GetProductTypeLabel(_localizationService),
					ProductTypeLabelHint = orderItem.Product.ProductTypeLabelHint,
                    Sku = orderItem.Product.Sku,
                    AttributeInfo = orderItem.AttributeDescription,
                    ItemWeight = orderItem.ItemWeight.HasValue ? string.Format("{0:F2} [{1}]", orderItem.ItemWeight, baseWeightIn) : "",
                    ItemDimensions = string.Format("{0:F2} x {1:F2} x {2:F2} [{3}]", orderItem.Product.Length, orderItem.Product.Width, orderItem.Product.Height, baseDimensionIn),
                    QuantityOrdered = qtyOrdered,
                    QuantityInThisShipment = qtyInThisShipment,
                    QuantityInAllShipments = qtyInAllShipments,
                    QuantityToAdd = maxQtyToAdd
                };

                model.Items.Add(shipmentItemModel);
            }

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult AddShipment(int orderId, FormCollection form, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                //No order found with the specified id
                return RedirectToAction("List");

            Shipment shipment = null;

            decimal? totalWeight = null;
            foreach (var orderItem in order.OrderItems)
            {
                //is shippable
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                //ensure that this product can be shipped (have at least one item to ship)
                var maxQtyToAdd = orderItem.GetTotalNumberOfItemsCanBeAddedToShipment();
                if (maxQtyToAdd <= 0)
                    continue;

                int qtyToAdd = 0; //parse quantity
                foreach (string formKey in form.AllKeys)
                    if (formKey.Equals(string.Format("qtyToAdd{0}", orderItem.Id), StringComparison.InvariantCultureIgnoreCase))
                    {
                        int.TryParse(form[formKey], out qtyToAdd);
                        break;
                    }

                //validate quantity
                if (qtyToAdd <= 0)
                    continue;
                if (qtyToAdd > maxQtyToAdd)
                    qtyToAdd = maxQtyToAdd;

                //ok. we have at least one item. let's create a shipment (if it does not exist)

                var orderItemTotalWeight = orderItem.ItemWeight.HasValue ? orderItem.ItemWeight * qtyToAdd : null;
                if (orderItemTotalWeight.HasValue)
                {
                    if (!totalWeight.HasValue)
                        totalWeight = 0;
                    totalWeight += orderItemTotalWeight.Value;
                }

                if (shipment == null)
                {
                    shipment = new Shipment()
                    {
                        OrderId = order.Id,
                        TrackingNumber = form["TrackingNumber"],
                        TotalWeight = null,
                        ShippedDateUtc = null,
                        DeliveryDateUtc = null,
                        CreatedOnUtc = DateTime.UtcNow,
                    };
                }
                //create a shipment item
                var shipmentItem = new ShipmentItem()
                {
                    OrderItemId = orderItem.Id,
                    Quantity = qtyToAdd,
                };
                shipment.ShipmentItems.Add(shipmentItem);
            }

            //if we have at least one item in the shipment, then save it
            if (shipment != null && shipment.ShipmentItems.Count > 0)
            {
                shipment.TotalWeight = totalWeight;
                _shipmentService.InsertShipment(shipment);

                NotifySuccess(_localizationService.GetResource("Admin.Orders.Shipments.Added"));
                return continueEditing
                           ? RedirectToAction("ShipmentDetails", new {id = shipment.Id})
                           : RedirectToAction("Edit", new { id = orderId });
            }
            else
            {
				NotifyError(_localizationService.GetResource("Admin.Orders.Shipments.NoProductsSelected"));
				return RedirectToAction("AddShipment", new { orderId = orderId });
            }
        }

        public ActionResult ShipmentDetails(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var model = PrepareShipmentModel(shipment, true, true);

            return View(model);
        }

        [HttpPost]
        public ActionResult DeleteShipment(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            var orderId = shipment.OrderId;

            NotifySuccess(_localizationService.GetResource("Admin.Orders.Shipments.Deleted"));
            _shipmentService.DeleteShipment(shipment);
            return RedirectToAction("Edit", new { id = orderId });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("settrackingnumber")]
        public ActionResult SetTrackingNumber(ShipmentModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(model.Id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            shipment.TrackingNumber = model.TrackingNumber;
            _shipmentService.UpdateShipment(shipment);

            return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasshipped")]
        public ActionResult SetAsShipped(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Ship(shipment, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                NotifyError(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

        [HttpPost, ActionName("ShipmentDetails")]
        [FormValueRequired("setasdelivered")]
        public ActionResult SetAsDelivered(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var shipment = _shipmentService.GetShipmentById(id);
            if (shipment == null)
                //No shipment found with the specified id
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.Deliver(shipment, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
            catch (Exception exc)
            {
                //error
                NotifyError(exc, true);
                return RedirectToAction("ShipmentDetails", new { id = shipment.Id });
            }
        }

		public ActionResult PdfPackagingSlips(bool all, string selectedIds = null)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
				return AccessDeniedView();

			if (!all && selectedIds.IsEmpty())
			{
				NotifyInfo(_localizationService.GetResource("Admin.Common.ExportNoData"));
				return RedirectToReferrer();
			}

			IList<Shipment> shipments;

			using (var scope = new DbContextScope(_services.DbContext, autoDetectChanges: false, forceNoTracking: true))
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
				NotifyInfo(_localizationService.GetResource("Admin.Common.ExportNoData"));
				return RedirectToReferrer();
			}

			if (shipments.Count > 500)
			{
				NotifyWarning(_localizationService.GetResource("Admin.Common.ExportToPdf.TooManyItems"));
				return RedirectToReferrer();
			}

			string pdfFileName = "PackagingSlips.pdf";
			if (shipments.Count == 1)
			{
				pdfFileName = "PackagingSlip-{0}.pdf".FormatInvariant(shipments[0].Id);
			}

			var model = shipments.Select(x => PrepareShipmentModel(x, true, true)).ToList();

			// TODO: (mc) this is bad for multi-document processing, where orders can originate from different stores.
			var storeId = model[0].StoreId;
			var routeValues = new RouteValueDictionary(new { storeId = storeId, area = "" });
			var pdfSettings = _services.Settings.LoadSetting<PdfSettings>(storeId);

			var settings = new PdfConvertSettings
			{
				Size = pdfSettings.LetterPageSizeEnabled ? PdfPageSize.Letter : PdfPageSize.A4,
				Margins = new PdfPageMargins { Top = 35, Bottom = 35 },
				Page = new PdfViewContent("ShipmentDetails.Print", model, this.ControllerContext),
				Header = new PdfRouteContent("PdfReceiptHeader", "Common", routeValues, this.ControllerContext),
				Footer = new PdfRouteContent("PdfReceiptFooter", "Common", routeValues, this.ControllerContext)
			};

			return new PdfResult(_pdfConverter, settings) { FileName = pdfFileName };
		}

        #endregion

        #region Order notes

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult OrderNotesSelect(int orderId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

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

            var model = new GridModel<OrderModel.OrderNote>
            {
                Data = orderNoteModels,
                Total = orderNoteModels.Count
            };

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
        public ActionResult OrderNoteAdd(int orderId, bool displayToCustomer, string message)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                return Json(new { Result = false }, JsonRequestBehavior.AllowGet);

            var orderNote = new OrderNote()
            {
                DisplayToCustomer = displayToCustomer,
                Note = message,
                CreatedOnUtc = DateTime.UtcNow,
            };
            order.OrderNotes.Add(orderNote);
            _orderService.UpdateOrder(order);

            //new order notification
            if (displayToCustomer)
            {
                //email
                _workflowMessageService.SendNewOrderNoteAddedCustomerNotification(
                    orderNote, _workContext.WorkingLanguage.Id);

            }

            return Json(new { Result = true }, JsonRequestBehavior.AllowGet);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult OrderNoteDelete(int orderId, int orderNoteId, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var order = _orderService.GetOrderById(orderId);
            if (order == null)
                throw new ArgumentException("No order found with the specified id");

            var orderNote = order.OrderNotes.Where(on => on.Id == orderNoteId).FirstOrDefault();
            if (orderNote == null)
                throw new ArgumentException("No order note found with the specified id");
            _orderService.DeleteOrderNote(orderNote);

            return OrderNotesSelect(orderId, command);
        }


        #endregion

        #region Reports

        [NonAction]
        protected IList<BestsellersReportLineModel> GetBestsellersBriefReportModel(int recordsToReturn, int orderBy)
        {
			var report = _orderReportService.BestSellersReport(0, null, null,
                null, null, null, 0, recordsToReturn, orderBy, true);

            var model = report.Select(x =>
            {
				var product = _productService.GetProductById(x.ProductId);

                var m = new BestsellersReportLineModel()
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
        public ActionResult BestsellersBriefReportByQuantity()
        {
            var model = GetBestsellersBriefReportModel(5, 1);
            return PartialView(model);
        }
        [GridAction(EnableCustomBinding = true)]
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
        public ActionResult BestsellersBriefReportByAmount()
        {
            var model = GetBestsellersBriefReportModel(5, 2);
            return PartialView(model);
        }
        [GridAction(EnableCustomBinding = true)]
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




        public ActionResult BestsellersReport()
        {
            var model = new BestsellersReportModel();

            //order statuses
            model.AvailableOrderStatuses = OrderStatus.Pending.ToSelectList(false).ToList();
            model.AvailableOrderStatuses.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //payment statuses
            model.AvailablePaymentStatuses = PaymentStatus.Pending.ToSelectList(false).ToList();
            model.AvailablePaymentStatuses.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });

            //billing countries
            foreach (var c in _countryService.GetAllCountriesForBilling())
            {
                model.AvailableCountries.Add(new SelectListItem() { Text = c.Name, Value = c.Id.ToString() });
            }
            model.AvailableCountries.Insert(0, new SelectListItem() { Text = _localizationService.GetResource("Admin.Address.SelectCountry"), Value = "0" });

            return View(model);
        }
        [GridAction(EnableCustomBinding = true)]
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

                    var m = new BestsellersReportLineModel()
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



        public ActionResult NeverSoldReport()
        {
            var model = new NeverSoldReportModel();
            return View(model);
        }
        [GridAction(EnableCustomBinding = true)]
        public ActionResult NeverSoldReportList(GridCommand command, NeverSoldReportModel model)
        {
            DateTime? startDateValue = (model.StartDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.StartDate.Value, _dateTimeHelper.CurrentTimeZone);

            DateTime? endDateValue = (model.EndDate == null) ? null
                            : (DateTime?)_dateTimeHelper.ConvertToUtcTime(model.EndDate.Value, _dateTimeHelper.CurrentTimeZone).AddDays(1);
            
            var items = _orderReportService.ProductsNeverSold(startDateValue, endDateValue,
                command.Page - 1, command.PageSize, true);
			var gridModel = new GridModel<NeverSoldReportLineModel>
			{
				Data = items.Select(x =>
				{
					var m = new NeverSoldReportLineModel()
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
			var urlHelper = new UrlHelper(Request.RequestContext);
            var report = new List<OrderAverageReportLineSummary>();

			report.Add(_orderReportService.OrderAverageReport(0, OrderStatus.Pending));
			report.Add(_orderReportService.OrderAverageReport(0, OrderStatus.Processing));
			report.Add(_orderReportService.OrderAverageReport(0, OrderStatus.Complete));
			report.Add(_orderReportService.OrderAverageReport(0, OrderStatus.Cancelled));

            var model = report.Select(x =>
            {
                return new OrderAverageReportLineSummaryModel()
                {
                    OrderStatus = x.OrderStatus.GetLocalizedEnum(_localizationService, _workContext),
                    CountTodayOrders = x.CountTodayOrders,
                    SumTodayOrders = _priceFormatter.FormatPrice(x.SumTodayOrders, true, false),
                    SumThisWeekOrders = _priceFormatter.FormatPrice(x.SumThisWeekOrders, true, false),
                    SumThisMonthOrders = _priceFormatter.FormatPrice(x.SumThisMonthOrders, true, false),
                    SumThisYearOrders = _priceFormatter.FormatPrice(x.SumThisYearOrders, true, false),
                    SumAllTimeOrders = _priceFormatter.FormatPrice(x.SumAllTimeOrders, true, false),
					Url = urlHelper.Action("List", "Order", new { OrderStatusIds = (int)x.OrderStatus })
                };
            }).ToList();

            return model;
        }

        [ChildActionOnly]
        public ActionResult OrderAverageReport()
        {
			var model = GetOrderAverageReportModel();
            return PartialView(model);
        }

        [GridAction(EnableCustomBinding = true)]
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
			var urlHelper = new UrlHelper(Request.RequestContext);

            //not paid
			var psPending = _orderReportService.GetOrderAverageReportLine(0, null, new int[] { (int)PaymentStatus.Pending }, null, null, null, null, true);
            model.Add(new OrderIncompleteReportLineModel()
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalUnpaidOrders"),
                Count = psPending.CountOrders,
                Total = _priceFormatter.FormatPrice(psPending.SumOrders, true, false),
				Url = urlHelper.Action("List", "Order", new { PaymentStatusIds = (int)PaymentStatus.Pending })
            });
            
            //not shipped
			var ssPending = _orderReportService.GetOrderAverageReportLine(0, null, null, new int[] { (int)ShippingStatus.NotYetShipped }, null, null, null, true);
            model.Add(new OrderIncompleteReportLineModel()
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalNotShippedOrders"),
                Count = ssPending.CountOrders,
                Total = _priceFormatter.FormatPrice(ssPending.SumOrders, true, false),
				Url = urlHelper.Action("List", "Order", new { ShippingStatusIds = (int)ShippingStatus.NotYetShipped })
            });

            //pending
			var osPending = _orderReportService.GetOrderAverageReportLine(0, new int[] { (int)OrderStatus.Pending }, null, null, null, null, null, true);
            model.Add(new OrderIncompleteReportLineModel()
            {
                Item = _localizationService.GetResource("Admin.SalesReport.Incomplete.TotalIncompleteOrders"),
                Count = osPending.CountOrders,
                Total = _priceFormatter.FormatPrice(osPending.SumOrders, true, false),
				Url = urlHelper.Action("List", "Order", new { OrderStatusIds = (int)OrderStatus.Pending })
            });

            return model;
        }
        [ChildActionOnly]
        public ActionResult OrderIncompleteReport()
        {
            var model = GetOrderIncompleteReportModel();
            return PartialView(model);
        }
        [GridAction(EnableCustomBinding = true)]
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
