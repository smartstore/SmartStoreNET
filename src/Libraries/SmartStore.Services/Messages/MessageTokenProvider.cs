using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tax;
using SmartStore.Core.Events;
using SmartStore.Core.Html;
using SmartStore.Core.Plugins;
using SmartStore.Services.Catalog;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Topics;

namespace SmartStore.Services.Messages
{
    public partial class MessageTokenProvider : IMessageTokenProvider
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly ILocalizationService _localizationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly ICurrencyService _currencyService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IDownloadService _downloadService;
        private readonly IOrderService _orderService;
		private readonly IProviderManager _providerManager;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly StoreInformationSettings _storeSettings;
        private readonly MessageTemplatesSettings _templatesSettings;
        private readonly EmailAccountSettings _emailAccountSettings;
        private readonly CatalogSettings _catalogSettings;
        private readonly TaxSettings _taxSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly CompanyInformationSettings _companyInfoSettings;
        private readonly BankConnectionSettings _bankConnectionSettings;
        private readonly ContactDataSettings _contactDataSettings;
        private readonly ITopicService _topicService;
        private readonly ShoppingCartSettings _shoppingCartSettings;
		private readonly IDeliveryTimeService _deliveryTimeService;
        private readonly IQuantityUnitService _quantityUnitService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;
        private readonly IGenericAttributeService _attrService;

        #endregion

        #region Ctor

        public MessageTokenProvider(ILanguageService languageService,
            ILocalizationService localizationService, IDateTimeHelper dateTimeHelper,
            IEmailAccountService emailAccountService,
            IPriceFormatter priceFormatter, ICurrencyService currencyService, IWebHelper webHelper,
            IWorkContext workContext, IStoreContext storeContext,
			IDownloadService downloadService, ShoppingCartSettings shoppingCartSettings,
            IOrderService orderService, IProviderManager providerManager,
            IProductAttributeParser productAttributeParser,
            StoreInformationSettings storeSettings, MessageTemplatesSettings templatesSettings,
            EmailAccountSettings emailAccountSettings, CatalogSettings catalogSettings,
            TaxSettings taxSettings, IEventPublisher eventPublisher,
            CompanyInformationSettings companyInfoSettings, BankConnectionSettings bankConnectionSettings,
            ContactDataSettings contactDataSettings, ITopicService topicService,
            IDeliveryTimeService deliveryTimeService, IQuantityUnitService quantityUnitService,
            IUrlRecordService urlRecordService, IStoreService storeService,
            IGenericAttributeService attrService)
        {
            this._languageService = languageService;
            this._localizationService = localizationService;
            this._dateTimeHelper = dateTimeHelper;
            this._emailAccountService = emailAccountService;
            this._priceFormatter = priceFormatter;
            this._currencyService = currencyService;
            this._webHelper = webHelper;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._downloadService = downloadService;
            this._orderService = orderService;
			this._providerManager = providerManager;
            this._productAttributeParser = productAttributeParser;
            this._storeSettings = storeSettings;
            this._templatesSettings = templatesSettings;
            this._emailAccountSettings = emailAccountSettings;
            this._catalogSettings = catalogSettings;
            this._taxSettings = taxSettings;
            this._eventPublisher = eventPublisher;
            this._companyInfoSettings = companyInfoSettings;
            this._bankConnectionSettings = bankConnectionSettings;
            this._contactDataSettings = contactDataSettings;
            this._topicService = topicService;
            this._shoppingCartSettings = shoppingCartSettings;
			this._deliveryTimeService = deliveryTimeService;
            this._quantityUnitService = quantityUnitService;
            this._urlRecordService = urlRecordService;
            this._storeService = storeService;
            this._attrService = attrService;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>HTML table of products</returns>
        protected virtual string ProductListToHtmlTable(Order order, int languageId)
        {
            var result = "";

            var language = _languageService.GetLanguageById(languageId);

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Price", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Total", languageId)));
            sb.AppendLine("</tr>");

            var table = order.OrderItems.ToList();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var orderItem = table[i];
                var product = orderItem.Product;
                if (product == null)
                    continue;

				DeliveryTime deliveryTime = null;

				// merging attribute combination data required?
				if (_catalogSettings.ShowProductSku || (_shoppingCartSettings.ShowDeliveryTimes && product.IsShipEnabled))
				{
					product.MergeWithCombination(orderItem.AttributesXml, _productAttributeParser);
				}

				if (_shoppingCartSettings.ShowDeliveryTimes && product.IsShipEnabled)
				{
					deliveryTime = _deliveryTimeService.GetDeliveryTimeById(product.DeliveryTimeId ?? 0);
				}

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
				string productName = product.GetLocalized(x => x.Name, languageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + HttpUtility.HtmlEncode(productName));
                //add download link
                if (_downloadService.IsDownloadAllowed(orderItem))
                {
                    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                    string downloadUrl = string.Format("{0}download/getdownload/{1}", _webHelper.GetStoreLocation(false), orderItem.OrderItemGuid);
                    string downloadLink = string.Format("<a class=\"link\" href=\"{0}\">{1}</a>", downloadUrl, _localizationService.GetResource("Messages.Order.Product(s).Download", languageId));
                    sb.AppendLine("&nbsp;&nbsp;(");
                    sb.AppendLine(downloadLink);
                    sb.AppendLine(")");
                }

                //deliverytime
				if (deliveryTime != null)
                {
					string deliveryTimeName = HttpUtility.HtmlEncode(deliveryTime.GetLocalized(x => x.Name));

                    sb.AppendLine("<br />");
                    sb.AppendLine("<div class=\"delivery-time\">");
                    sb.AppendLine("<span class=\"delivery-time-label\">" + _localizationService.GetResource("Products.DeliveryTime") + "</span>");
                    sb.AppendLine("<span class=\"delivery-time-color\" style=\"background-color:" + deliveryTime.ColorHexValue + "\" title=\"" + deliveryTimeName + "\"></span>");
                    sb.AppendLine("<span class=\"delivery-time-value\">" + deliveryTimeName + "</span>");
                    sb.AppendLine("</div>");
                }

                //attributes
                if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                //sku
                if (_catalogSettings.ShowProductSku)
                {
                    if (!String.IsNullOrEmpty(product.Sku))
                    {
                        sb.AppendLine("<br />");
						sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(product.Sku)));
                    }
                }
                sb.AppendLine("</td>");

                string unitPriceStr = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var unitPriceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceExclTax, order.CurrencyRate);
                            unitPriceStr = _priceFormatter.FormatPrice(unitPriceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var unitPriceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.UnitPriceInclTax, order.CurrencyRate);
                            unitPriceStr = _priceFormatter.FormatPrice(unitPriceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                        }
                        break;
                }
                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: right;\">{0}</td>", unitPriceStr));

                var quantityUnit = _quantityUnitService.GetQuantityUnitById(product.QuantityUnitId);
                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0} {1}</td>", 
                    orderItem.Quantity, 
                    quantityUnit == null ? "" : quantityUnit.GetLocalized(x => x.Name)));

                string priceStr = string.Empty;
                switch (order.CustomerTaxDisplayType)
                {
                    case TaxDisplayType.ExcludingTax:
                        {
                            var priceExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceExclTax, order.CurrencyRate);
                            priceStr = _priceFormatter.FormatPrice(priceExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                        }
                        break;
                    case TaxDisplayType.IncludingTax:
                        {
                            var priceInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(orderItem.PriceInclTax, order.CurrencyRate);
                            priceStr = _priceFormatter.FormatPrice(priceInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                        }
                        break;
                }
                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: right;\">{0}</td>", priceStr));

                sb.AppendLine("</tr>");
            }
            #endregion

            #region Checkout Attributes

            if (!String.IsNullOrEmpty(order.CheckoutAttributeDescription))
            {
                sb.AppendLine("<tr><td style=\"text-align:right;\" colspan=\"1\">&nbsp;</td><td colspan=\"3\" style=\"text-align:right\">");
                sb.AppendLine(HtmlUtils.ConvertPlainTextToTable(HtmlUtils.ConvertHtmlToPlainText(order.CheckoutAttributeDescription)));
                sb.AppendLine("</td></tr>");
            }

            #endregion

            #region Totals

            string cusSubTotal = string.Empty;
            bool dislaySubTotalDiscount = false;
            string cusSubTotalDiscount = string.Empty;
            string cusShipTotal = string.Empty;
            string cusPaymentMethodAdditionalFee = string.Empty;
            var taxRates = new SortedDictionary<decimal, decimal>();
            string cusTaxTotal = string.Empty;
            string cusDiscount = string.Empty;
            string cusTotal = string.Empty;
            //subtotal, shipping, payment method fee
            switch (order.CustomerTaxDisplayType)
            {
                case TaxDisplayType.ExcludingTax:
                    {
                        //subtotal
                        var orderSubtotalExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalExclTax, order.CurrencyRate);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountExclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountExclTaxInCustomerCurrency > decimal.Zero)
                        {
                            cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                            dislaySubTotalDiscount = true;
                        }
                        //shipping
                        var orderShippingExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingExclTax, order.CurrencyRate);
                        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                        //payment method additional fee
                        var paymentMethodAdditionalFeeExclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeExclTax, order.CurrencyRate);
                        cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeExclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, false);
                    }
                    break;
                case TaxDisplayType.IncludingTax:
                    {
                        //subtotal
                        var orderSubtotalInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubtotalInclTax, order.CurrencyRate);
                        cusSubTotal = _priceFormatter.FormatPrice(orderSubtotalInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                        //discount (applied to order subtotal)
                        var orderSubTotalDiscountInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderSubTotalDiscountInclTax, order.CurrencyRate);
                        if (orderSubTotalDiscountInclTaxInCustomerCurrency > decimal.Zero)
                        {
                            cusSubTotalDiscount = _priceFormatter.FormatPrice(-orderSubTotalDiscountInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                            dislaySubTotalDiscount = true;
                        }
                        //shipping
                        var orderShippingInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderShippingInclTax, order.CurrencyRate);
                        cusShipTotal = _priceFormatter.FormatShippingPrice(orderShippingInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                        //payment method additional fee
                        var paymentMethodAdditionalFeeInclTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.PaymentMethodAdditionalFeeInclTax, order.CurrencyRate);
                        cusPaymentMethodAdditionalFee = _priceFormatter.FormatPaymentMethodAdditionalFee(paymentMethodAdditionalFeeInclTaxInCustomerCurrency, true, order.CustomerCurrencyCode, language, true);
                    }
                    break;
            }

            //shipping
            bool dislayShipping = order.ShippingStatus != ShippingStatus.ShippingNotRequired;

            //payment method fee
            bool displayPaymentMethodFee = true;
            if (order.PaymentMethodAdditionalFeeExclTax == decimal.Zero)
            {
                displayPaymentMethodFee = false;
            }

            //tax
            bool displayTax = true;
            bool displayTaxRates = true;
            if (_taxSettings.HideTaxInOrderSummary && order.CustomerTaxDisplayType == TaxDisplayType.IncludingTax)
            {
                displayTax = false;
                displayTaxRates = false;
            }
            else
            {
                if (order.OrderTax == 0 && _taxSettings.HideZeroTax)
                {
                    displayTax = false;
                    displayTaxRates = false;
                }
                else
                {
                    taxRates = new SortedDictionary<decimal, decimal>();
                    foreach (var tr in order.TaxRatesDictionary)
                        taxRates.Add(tr.Key, _currencyService.ConvertCurrency(tr.Value, order.CurrencyRate));

                    displayTaxRates = _taxSettings.DisplayTaxRates && taxRates.Count > 0;
                    displayTax = !displayTaxRates;

                    var orderTaxInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTax, order.CurrencyRate);
                    string taxStr = _priceFormatter.FormatPrice(orderTaxInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);
                    cusTaxTotal = taxStr;
                }
            }

            //discount
            bool dislayDiscount = false;
            if (order.OrderDiscount > decimal.Zero)
            {
                var orderDiscountInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderDiscount, order.CurrencyRate);
                cusDiscount = _priceFormatter.FormatPrice(-orderDiscountInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);
                dislayDiscount = true;
            }

            //total
            var orderTotalInCustomerCurrency = _currencyService.ConvertCurrency(order.OrderTotal, order.CurrencyRate);
            cusTotal = _priceFormatter.FormatPrice(orderTotalInCustomerCurrency, true, order.CustomerCurrencyCode, false, language);




            //subtotal
            //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));
            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"padding:8px 0;border-top:1px solid #ddd;\"><strong>{0}</strong></td> <td style=\"padding:8px;border-top:1px solid #ddd;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.SubTotal", languageId), cusSubTotal));

            //discount (applied to order subtotal)
            if (dislaySubTotalDiscount)
            {
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"padding:8px 0;border-top:1px solid #ddd;\"><strong>{0}</strong></td> <td style=\"padding:8px;border-top:1px solid #ddd;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.SubTotalDiscount", languageId), cusSubTotalDiscount));
            }


            //shipping
            if (dislayShipping)
            {
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"padding:8px 0;border-top:1px solid #ddd;\"><strong>{0}</strong></td> <td style=\"padding:8px;border-top:1px solid #ddd;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.Shipping", languageId), cusShipTotal));
            }

            //payment method fee
            if (displayPaymentMethodFee)
            {
                string paymentMethodFeeTitle = _localizationService.GetResource("Messages.Order.PaymentMethodAdditionalFee", languageId);
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, paymentMethodFeeTitle, cusPaymentMethodAdditionalFee));
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"padding:8px 0;border-top:1px solid #ddd;\"><strong>{0}</strong></td> <td style=\"padding:8px;border-top:1px solid #ddd;\">{1}</td></tr>", paymentMethodFeeTitle, cusPaymentMethodAdditionalFee));
            }

            //tax
            if (displayTax)
            {
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"padding:8px 0;border-top:1px solid #ddd;\"><strong>{0}</strong></td> <td style=\"padding:8px;border-top:1px solid #ddd;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.Tax", languageId), cusTaxTotal));
            }
            if (displayTaxRates)
            {
                foreach (var item in taxRates)
                {
                    string taxRate = String.Format(_localizationService.GetResource("Messages.Order.TaxRateLine"), _priceFormatter.FormatTaxRate(item.Key));
                    string taxValue = _priceFormatter.FormatPrice(item.Value, true, order.CustomerCurrencyCode, false, language);
                    //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, taxRate, taxValue));
                    sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"padding:8px 0;border-top:1px solid #ddd;\"><strong>{0}</strong></td> <td style=\"padding:8px;border-top:1px solid #ddd;\">{1}</td></tr>", taxRate, taxValue));
                }
            }

            //discount
            if (dislayDiscount)
            {
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.TotalDiscount", languageId), cusDiscount));
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"padding:8px 0;border-top:1px solid #ddd;\"><strong>{0}</strong></td> <td style=\"padding:8px;border-top:1px solid #ddd;\">{1}</td></tr>", _localizationService.GetResource("Messages.Order.TotalDiscount", languageId), cusDiscount));
            }

            //gift cards
            var gcuhC = order.GiftCardUsageHistory;
            foreach (var gcuh in gcuhC)
            {
                string giftCardText = String.Format(_localizationService.GetResource("Messages.Order.GiftCardInfo", languageId), HttpUtility.HtmlEncode(gcuh.GiftCard.GiftCardCouponCode));
                string giftCardAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(gcuh.UsedValue, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language);
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, giftCardText, giftCardAmount));
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"padding:8px 0;border-top:1px solid #ddd;\"><strong>{0}</strong></td> <td style=\"padding:8px;border-top:1px solid #ddd;\">{1}</td></tr>", giftCardText, giftCardAmount));
            }

            //reward points
            if (order.RedeemedRewardPointsEntry != null)
            {
                string rpTitle = string.Format(_localizationService.GetResource("Messages.Order.RewardPoints", languageId), -order.RedeemedRewardPointsEntry.Points);
                string rpAmount = _priceFormatter.FormatPrice(-(_currencyService.ConvertCurrency(order.RedeemedRewardPointsEntry.UsedAmount, order.CurrencyRate)), true, order.CustomerCurrencyCode, false, language);
                //sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{1}</strong></td> <td style=\"background-color: {0};padding:0.6em 0.4 em;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, rpTitle, rpAmount));
                sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"padding:8px 0;border-top:1px solid #ddd;\"><strong>{0}</strong></td> <td style=\"padding:8px;border-top:1px solid #ddd;\">{1}</td></tr>", rpTitle, rpAmount));
            }

            //total
            sb.AppendLine(string.Format("<tr style=\"text-align:right;\"><td>&nbsp;</td><td colspan=\"2\" style=\"background-color: {0};padding:8px 0;border-top:1px solid #ddd;\"><strong>{1}</strong></td><td style=\"background-color: {0};padding:8px;border-top:1px solid #ddd;\"><strong>{2}</strong></td></tr>", _templatesSettings.Color3, _localizationService.GetResource("Messages.Order.OrderTotal", languageId), cusTotal));
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }

        /// <summary>
        /// Convert a collection to a HTML table
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>HTML table of products</returns>
        protected virtual string ProductListToHtmlTable(Shipment shipment, int languageId)
        {
            var result = "";

            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\">");

            #region Products
            sb.AppendLine(string.Format("<tr style=\"background-color:{0};text-align:center;\">", _templatesSettings.Color1));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Name", languageId)));
            sb.AppendLine(string.Format("<th>{0}</th>", _localizationService.GetResource("Messages.Order.Product(s).Quantity", languageId)));
            sb.AppendLine("</tr>");

            var table = shipment.ShipmentItems.ToList();
            for (int i = 0; i <= table.Count - 1; i++)
            {
                var si = table[i];
                var orderItem = _orderService.GetOrderItemById(si.OrderItemId);
                if (orderItem == null)
                    continue;

                var product = orderItem.Product;
                if (product == null)
                    continue;

                sb.AppendLine(string.Format("<tr style=\"background-color: {0};text-align: center;\">", _templatesSettings.Color2));
                //product name
				string productName = product.GetLocalized(x => x.Name, languageId);

                sb.AppendLine("<td style=\"padding: 0.6em 0.4em;text-align: left;\">" + HttpUtility.HtmlEncode(productName));
                //attributes
                if (!String.IsNullOrEmpty(orderItem.AttributeDescription))
                {
                    sb.AppendLine("<br />");
                    sb.AppendLine(orderItem.AttributeDescription);
                }
                //sku
                if (_catalogSettings.ShowProductSku)
                {
                    product.MergeWithCombination(orderItem.AttributesXml, _productAttributeParser);

                    if (!String.IsNullOrEmpty(product.Sku))
                    {
                        sb.AppendLine("<br />");
						sb.AppendLine(string.Format(_localizationService.GetResource("Messages.Order.Product(s).SKU", languageId), HttpUtility.HtmlEncode(product.Sku)));
                    }
                }
                sb.AppendLine("</td>");

                sb.AppendLine(string.Format("<td style=\"padding: 0.6em 0.4em;text-align: center;\">{0}</td>", si.Quantity));

                sb.AppendLine("</tr>");
            }
            #endregion

            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }

        protected virtual string TopicToHtml(string systemName, int languageId)
        {
            var result = "";
            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" style=\"width:100%;\" class=\"legal-infos\">");

			//load by store
			var topic = _topicService.GetTopicBySystemName(systemName, _storeContext.CurrentStore.Id);
			if (topic == null)
				//not found. let's find topic assigned to all stores
				topic = _topicService.GetTopicBySystemName(systemName, 0);

            if (topic == null)
                return string.Empty;

            sb.AppendLine("<tr><td style=\"color:#aaa\">");
            sb.AppendLine(topic.Title);
            sb.AppendLine("<td/><tr/><tr><td>");
            sb.AppendLine(topic.Body);
            sb.AppendLine("<td/><tr/>");
            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }

        protected virtual string GetSupplierIdentification()
        {
            var result = "";
            var sb = new StringBuilder();
            sb.AppendLine("<table border=\"0\" class=\"supplier-identification\">");
            sb.AppendLine("<tr valign=\"top\">");
			
            sb.AppendLine("<td class=\"smaller\" width=\"33%\">");

            sb.AppendLine(String.Format("{0} <br>", _companyInfoSettings.CompanyName ));

            if (!String.IsNullOrEmpty(_companyInfoSettings.Salutation)) 
            {
                sb.AppendLine(_companyInfoSettings.Salutation);
            }
            if (!String.IsNullOrEmpty(_companyInfoSettings.Title)) 
            {
                sb.AppendLine(_companyInfoSettings.Title);
            }
            if (!String.IsNullOrEmpty(_companyInfoSettings.Firstname)) 
            {
                sb.AppendLine(String.Format("{0} ", _companyInfoSettings.Firstname));
            }
            if (!String.IsNullOrEmpty(_companyInfoSettings.Lastname)) 
            {
                sb.AppendLine(_companyInfoSettings.Lastname);
            }
            sb.AppendLine("<br>");

            if (!String.IsNullOrEmpty(_companyInfoSettings.Street)) 
            {
                sb.AppendLine(String.Format("{0} {1}<br>", _companyInfoSettings.Street, _companyInfoSettings.Street2));
            }
			if (!String.IsNullOrEmpty(_companyInfoSettings.ZipCode) || !String.IsNullOrEmpty(_companyInfoSettings.City)) 
            {
                sb.AppendLine(String.Format("{0} {1}<br>", _companyInfoSettings.ZipCode, _companyInfoSettings.City));
            }	
			if (!String.IsNullOrEmpty(_companyInfoSettings.CountryName)) 
            {
                sb.AppendLine(_companyInfoSettings.CountryName);

                if(!String.IsNullOrEmpty(_companyInfoSettings.Region))
                {
                    sb.AppendLine(String.Format(", {0}", _companyInfoSettings.Region));
                }
                sb.AppendLine("<br>");
            }				

            sb.AppendLine("<td/>");

            sb.AppendLine("<td class=\"smaller\" width=\"33%\">");
            
            if (!String.IsNullOrEmpty(_storeContext.CurrentStore.Url)) 
            {
				sb.AppendLine(String.Format("Url: <a href=\"{0}\">{0}</a><br>", _storeContext.CurrentStore.Url));
            }
            if (!String.IsNullOrEmpty(_contactDataSettings.CompanyEmailAddress)) 
            {
                sb.AppendLine(String.Format("Mail: {0}<br>", _contactDataSettings.CompanyEmailAddress));
            }
			if (!String.IsNullOrEmpty(_contactDataSettings.CompanyTelephoneNumber)) 
            {
                sb.AppendLine(String.Format("Fon: {0}<br>", _contactDataSettings.CompanyTelephoneNumber));
            }
            if (!String.IsNullOrEmpty(_contactDataSettings.CompanyFaxNumber)) 
            {
                sb.AppendLine(String.Format("Fax: {0}<br>", _contactDataSettings.CompanyFaxNumber));
            }

            sb.AppendLine("<td/>");

            sb.AppendLine("<td class=\"smaller\" width=\"34%\">");

            if (!String.IsNullOrEmpty(_bankConnectionSettings.Bankname)) 
            {
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.Bankname));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.Bankcode)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.Bankcode));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.AccountNumber)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.AccountNumber));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.AccountHolder)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.AccountHolder));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.Iban)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.Iban));
            }
            if (!String.IsNullOrEmpty(_bankConnectionSettings.Bic)) 
            {
                //TODO: caption
                sb.AppendLine(String.Format("{0}<br>", _bankConnectionSettings.Bic));
            }

            sb.AppendLine("<td/>");

            sb.AppendLine("<tr/>");
            sb.AppendLine("</table>");
            result = sb.ToString();
            return result;
        }

        #endregion

        #region Methods

		public virtual void AddStoreTokens(IList<Token> tokens, Store store)
        {
			tokens.Add(new Token("Store.Name", store.Name));
			tokens.Add(new Token("Store.URL", store.Url, true));
			var defaultEmailAccount = _emailAccountService.GetDefaultEmailAccount();
            tokens.Add(new Token("Store.SupplierIdentification", GetSupplierIdentification(), true));
            tokens.Add(new Token("Store.Email", defaultEmailAccount.Email));
        }

        public virtual void AddCompanyTokens(IList<Token> tokens)
        {
            tokens.Add(new Token("Company.CompanyName", _companyInfoSettings.CompanyName));
            tokens.Add(new Token("Company.Salutation", _companyInfoSettings.Salutation));
            tokens.Add(new Token("Company.Title", _companyInfoSettings.Title));
            tokens.Add(new Token("Company.Firstname", _companyInfoSettings.Firstname));
            tokens.Add(new Token("Company.Lastname", _companyInfoSettings.Lastname));
            tokens.Add(new Token("Company.CompanyManagementDescription", _companyInfoSettings.CompanyManagementDescription));
            tokens.Add(new Token("Company.CompanyManagement", _companyInfoSettings.CompanyManagement));
            tokens.Add(new Token("Company.Street", _companyInfoSettings.Street));
            tokens.Add(new Token("Company.Street2", _companyInfoSettings.Street2));
            tokens.Add(new Token("Company.ZipCode", _companyInfoSettings.ZipCode));
            tokens.Add(new Token("Company.City", _companyInfoSettings.City));
            tokens.Add(new Token("Company.CountryName", _companyInfoSettings.CountryName));
            tokens.Add(new Token("Company.Region", _companyInfoSettings.Region));
            tokens.Add(new Token("Company.VatId", _companyInfoSettings.VatId));
            tokens.Add(new Token("Company.CommercialRegister", _companyInfoSettings.CommercialRegister));
            tokens.Add(new Token("Company.TaxNumber", _companyInfoSettings.TaxNumber));
        }

        public virtual void AddBankConnectionTokens(IList<Token> tokens)
        {
            tokens.Add(new Token("Bank.Bankname", _bankConnectionSettings.Bankname));
            tokens.Add(new Token("Bank.Bankcode", _bankConnectionSettings.Bankcode));
            tokens.Add(new Token("Bank.AccountNumber", _bankConnectionSettings.AccountNumber));
            tokens.Add(new Token("Bank.AccountHolder", _bankConnectionSettings.AccountHolder));
            tokens.Add(new Token("Bank.Iban", _bankConnectionSettings.Iban));
            tokens.Add(new Token("Bank.Bic", _bankConnectionSettings.Bic));
        }

        public virtual void AddContactDataTokens(IList<Token> tokens)
        {
            tokens.Add(new Token("Contact.CompanyTelephoneNumber", _contactDataSettings.CompanyTelephoneNumber));
            tokens.Add(new Token("Contact.HotlineTelephoneNumber", _contactDataSettings.HotlineTelephoneNumber));
            tokens.Add(new Token("Contact.MobileTelephoneNumber", _contactDataSettings.MobileTelephoneNumber));
            tokens.Add(new Token("Contact.CompanyFaxNumber", _contactDataSettings.CompanyFaxNumber));
            tokens.Add(new Token("Contact.CompanyEmailAddress", _contactDataSettings.CompanyEmailAddress));
            tokens.Add(new Token("Contact.WebmasterEmailAddress", _contactDataSettings.WebmasterEmailAddress));
            tokens.Add(new Token("Contact.SupportEmailAddress", _contactDataSettings.SupportEmailAddress));
            tokens.Add(new Token("Contact.ContactEmailAddress", _contactDataSettings.ContactEmailAddress));
        }

        public virtual void AddOrderTokens(IList<Token> tokens, Order order, int languageId)
        {
			tokens.Add(new Token("Order.ID", order.Id.ToString()));
			tokens.Add(new Token("Order.OrderNumber", order.GetOrderNumber()));

            tokens.Add(new Token("Order.CustomerFullName", string.Format("{0} {1}", order.BillingAddress.FirstName, order.BillingAddress.LastName)));
            tokens.Add(new Token("Order.CustomerEmail", order.BillingAddress.Email));

            tokens.Add(new Token("Order.BillingFirstName", order.BillingAddress.FirstName));
            tokens.Add(new Token("Order.BillingLastName", order.BillingAddress.LastName));
            tokens.Add(new Token("Order.BillingPhoneNumber", order.BillingAddress.PhoneNumber));
            tokens.Add(new Token("Order.BillingEmail", order.BillingAddress.Email));
            tokens.Add(new Token("Order.BillingFaxNumber", order.BillingAddress.FaxNumber));
            tokens.Add(new Token("Order.BillingCompany", order.BillingAddress.Company));
            tokens.Add(new Token("Order.BillingAddress1", order.BillingAddress.Address1));
            tokens.Add(new Token("Order.BillingAddress2", order.BillingAddress.Address2));
            tokens.Add(new Token("Order.BillingCity", order.BillingAddress.City));
            tokens.Add(new Token("Order.BillingStateProvince", order.BillingAddress.StateProvince != null ? order.BillingAddress.StateProvince.GetLocalized(x => x.Name) : ""));
            tokens.Add(new Token("Order.BillingZipPostalCode", order.BillingAddress.ZipPostalCode));
            tokens.Add(new Token("Order.BillingCountry", order.BillingAddress.Country != null ? order.BillingAddress.Country.GetLocalized(x => x.Name) : ""));

            tokens.Add(new Token("Order.ShippingMethod", order.ShippingMethod));
            tokens.Add(new Token("Order.ShippingFirstName", order.ShippingAddress != null ? order.ShippingAddress.FirstName : ""));
            tokens.Add(new Token("Order.ShippingLastName", order.ShippingAddress != null ? order.ShippingAddress.LastName : ""));
            tokens.Add(new Token("Order.ShippingPhoneNumber", order.ShippingAddress != null ? order.ShippingAddress.PhoneNumber : ""));
            tokens.Add(new Token("Order.ShippingEmail", order.ShippingAddress != null ? order.ShippingAddress.Email : ""));
            tokens.Add(new Token("Order.ShippingFaxNumber", order.ShippingAddress != null ? order.ShippingAddress.FaxNumber : ""));
            tokens.Add(new Token("Order.ShippingCompany", order.ShippingAddress != null ? order.ShippingAddress.Company : ""));
            tokens.Add(new Token("Order.ShippingAddress1", order.ShippingAddress != null ? order.ShippingAddress.Address1 : ""));
            tokens.Add(new Token("Order.ShippingAddress2", order.ShippingAddress != null ? order.ShippingAddress.Address2 : ""));
            tokens.Add(new Token("Order.ShippingCity", order.ShippingAddress != null ? order.ShippingAddress.City : ""));
            tokens.Add(new Token("Order.ShippingStateProvince", order.ShippingAddress != null && order.ShippingAddress.StateProvince != null ? order.ShippingAddress.StateProvince.GetLocalized(x => x.Name) : ""));
            tokens.Add(new Token("Order.ShippingZipPostalCode", order.ShippingAddress != null ? order.ShippingAddress.ZipPostalCode : ""));
            tokens.Add(new Token("Order.ShippingCountry", order.ShippingAddress != null && order.ShippingAddress.Country != null ? order.ShippingAddress.Country.GetLocalized(x => x.Name) : ""));

			var paymentMethod = _providerManager.GetProvider<IPaymentMethod>(order.PaymentMethodSystemName);
			var paymentMethodName = paymentMethod != null ? GetLocalizedValue(paymentMethod.Metadata, "FriendlyName", x => x.FriendlyName) : order.PaymentMethodSystemName;

            tokens.Add(new Token("Order.PaymentMethod", paymentMethodName));
            tokens.Add(new Token("Order.VatNumber", order.VatNumber));
            tokens.Add(new Token("Order.Product(s)", ProductListToHtmlTable(order, languageId), true));
            tokens.Add(new Token("Order.CustomerComment", order.CustomerOrderComment, true));

            var language = _languageService.GetLanguageById(languageId);
            if (language != null && !String.IsNullOrEmpty(language.LanguageCulture))
            {
                DateTime createdOn = _dateTimeHelper.ConvertToUserTime(order.CreatedOnUtc, TimeZoneInfo.Utc, _dateTimeHelper.GetCustomerTimeZone(order.Customer));
                tokens.Add(new Token("Order.CreatedOn", createdOn.ToString("D", new CultureInfo(language.LanguageCulture))));
            }
            else
            {
                tokens.Add(new Token("Order.CreatedOn", order.CreatedOnUtc.ToString("D")));
            }

            // TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            tokens.Add(new Token("Order.OrderURLForCustomer", string.Format("{0}order/details/{1}", _webHelper.GetStoreLocation(false), order.Id), true));

            tokens.Add(new Token("Order.Disclaimer", TopicToHtml("Disclaimer", languageId), true));
            tokens.Add(new Token("Order.ConditionsOfUse", TopicToHtml("ConditionsOfUse", languageId), true));

            //event notification
            _eventPublisher.EntityTokensAdded(order, tokens);
        }

		private string GetLocalizedValue(ProviderMetadata metadata, string propertyName, Expression<Func<ProviderMetadata, string>> fallback)
		{
			// TODO: (mc) this actually belongs to PluginMediator, but we simply cannot add a dependency to framework from here. Refactor later!
			
			Guard.ArgumentNotNull(() => metadata);

			string systemName = metadata.SystemName;
			var languageId = _workContext.WorkingLanguage.Id;
			var resourceName = metadata.ResourceKeyPattern.FormatInvariant(metadata.SystemName, propertyName);
			string result = _localizationService.GetResource(resourceName, languageId, false, "", true);

			if (result.IsEmpty())
				result = fallback.Compile()(metadata);

			return result;
		}

        public virtual void AddShipmentTokens(IList<Token> tokens, Shipment shipment, int languageId)
        {
            tokens.Add(new Token("Shipment.ShipmentNumber", shipment.Id.ToString()));
            tokens.Add(new Token("Shipment.TrackingNumber", shipment.TrackingNumber));
            tokens.Add(new Token("Shipment.Product(s)", ProductListToHtmlTable(shipment, languageId), true));
            tokens.Add(new Token("Shipment.URLForCustomer", string.Format("{0}order/shipmentdetails/{1}", _webHelper.GetStoreLocation(false), shipment.Id), true));

            //event notification
            _eventPublisher.EntityTokensAdded(shipment, tokens);
        }

        public virtual void AddOrderNoteTokens(IList<Token> tokens, OrderNote orderNote)
        {
            tokens.Add(new Token("Order.NewNoteText", orderNote.FormatOrderNoteText(), true));

            //event notification
            _eventPublisher.EntityTokensAdded(orderNote, tokens);
        }

        public virtual void AddRecurringPaymentTokens(IList<Token> tokens, RecurringPayment recurringPayment)
        {
            tokens.Add(new Token("RecurringPayment.ID", recurringPayment.Id.ToString()));

            //event notification
            _eventPublisher.EntityTokensAdded(recurringPayment, tokens);
        }

        public virtual void AddReturnRequestTokens(IList<Token> tokens, ReturnRequest returnRequest, OrderItem orderItem)
        {
            tokens.Add(new Token("ReturnRequest.ID", returnRequest.Id.ToString()));
            tokens.Add(new Token("ReturnRequest.Product.Quantity", returnRequest.Quantity.ToString()));
            tokens.Add(new Token("ReturnRequest.Product.Name", orderItem.Product.Name));
            tokens.Add(new Token("ReturnRequest.Reason", returnRequest.ReasonForReturn));
            tokens.Add(new Token("ReturnRequest.RequestedAction", returnRequest.RequestedAction));
            tokens.Add(new Token("ReturnRequest.CustomerComment", HtmlUtils.FormatText(returnRequest.CustomerComments, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.StaffNotes", HtmlUtils.FormatText(returnRequest.StaffNotes, false, true, false, false, false, false), true));
            tokens.Add(new Token("ReturnRequest.Status", returnRequest.ReturnRequestStatus.GetLocalizedEnum(_localizationService, _workContext)));

            //event notification
            _eventPublisher.EntityTokensAdded(returnRequest, tokens);
        }

        public virtual void AddGiftCardTokens(IList<Token> tokens, GiftCard giftCard)
        {
            tokens.Add(new Token("GiftCard.SenderName", giftCard.SenderName));
            tokens.Add(new Token("GiftCard.SenderEmail", giftCard.SenderEmail));
            tokens.Add(new Token("GiftCard.RecipientName", giftCard.RecipientName));
            tokens.Add(new Token("GiftCard.RecipientEmail", giftCard.RecipientEmail));
            tokens.Add(new Token("GiftCard.Amount", _priceFormatter.FormatPrice(giftCard.Amount, true, false)));
            tokens.Add(new Token("GiftCard.CouponCode", giftCard.GiftCardCouponCode));

            var giftCardMesage = !String.IsNullOrWhiteSpace(giftCard.Message) ?
                HtmlUtils.FormatText(giftCard.Message, false, true, false, false, false, false) : "";

            tokens.Add(new Token("GiftCard.Message", giftCardMesage, true));

            //event notification
            _eventPublisher.EntityTokensAdded(giftCard, tokens);
        }

        public virtual void AddCustomerTokens(IList<Token> tokens, Customer customer)
        {
			tokens.Add(new Token("Customer.ID", customer.Id.ToString()));
			tokens.Add(new Token("Customer.Email", customer.Email));
            tokens.Add(new Token("Customer.Username", customer.Username));
            tokens.Add(new Token("Customer.FullName", customer.GetFullName()));
			tokens.Add(new Token("Customer.VatNumber", customer.GetAttribute<string>(SystemCustomerAttributeNames.VatNumber)));
			tokens.Add(new Token("Customer.VatNumberStatus", ((VatNumberStatus)customer.GetAttribute<int>(SystemCustomerAttributeNames.VatNumberStatusId)).ToString()));

            //note: we do not use SEO friendly URLS because we can get errors caused by having .(dot) in the URL (from the emauk address)
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
			string passwordRecoveryUrl = string.Format("{0}customer/passwordrecoveryconfirm?token={1}&email={2}", _webHelper.GetStoreLocation(), 
				customer.GetAttribute<string>(SystemCustomerAttributeNames.PasswordRecoveryToken), HttpUtility.UrlEncode(customer.Email));

			string accountActivationUrl = string.Format("{0}customer/activation?token={1}&email={2}", _webHelper.GetStoreLocation(), 
				customer.GetAttribute<string>(SystemCustomerAttributeNames.AccountActivationToken), HttpUtility.UrlEncode(customer.Email));

            var wishlistUrl = string.Format("{0}wishlist/{1}", _webHelper.GetStoreLocation(), customer.CustomerGuid);
            tokens.Add(new Token("Customer.PasswordRecoveryURL", passwordRecoveryUrl, true));
            tokens.Add(new Token("Customer.AccountActivationURL", accountActivationUrl, true));
            tokens.Add(new Token("Wishlist.URLForCustomer", wishlistUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(customer, tokens);
        }

        public virtual void AddNewsLetterSubscriptionTokens(IList<Token> tokens, NewsLetterSubscription subscription)
        {
            tokens.Add(new Token("NewsLetterSubscription.Email", subscription.Email));

            const string urlFormat = "{0}newsletter/subscriptionactivation/{1}/{2}";

            var activationUrl = String.Format(urlFormat, _webHelper.GetStoreLocation(false), subscription.NewsLetterSubscriptionGuid, "true");
            tokens.Add(new Token("NewsLetterSubscription.ActivationUrl", activationUrl, true));

            var deActivationUrl = String.Format(urlFormat, _webHelper.GetStoreLocation(false), subscription.NewsLetterSubscriptionGuid, "false");
            tokens.Add(new Token("NewsLetterSubscription.DeactivationUrl", deActivationUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(subscription, tokens);
        }

        public virtual void AddProductReviewTokens(IList<Token> tokens, ProductReview productReview)
        {
            tokens.Add(new Token("ProductReview.ProductName", productReview.Product.Name));

            //event notification
            _eventPublisher.EntityTokensAdded(productReview, tokens);
        }

        public virtual void AddBlogCommentTokens(IList<Token> tokens, BlogComment blogComment)
        {
            tokens.Add(new Token("BlogComment.BlogPostTitle", blogComment.BlogPost.Title));

            //event notification
            _eventPublisher.EntityTokensAdded(blogComment, tokens);
        }

        public virtual void AddNewsCommentTokens(IList<Token> tokens, NewsComment newsComment)
        {
            tokens.Add(new Token("NewsComment.NewsTitle", newsComment.NewsItem.Title));

            //event notification
            _eventPublisher.EntityTokensAdded(newsComment, tokens);
        }

		public virtual void AddProductTokens(IList<Token> tokens, Product product, int languageId)
        {
			tokens.Add(new Token("Product.ID", product.Id.ToString()));
			tokens.Add(new Token("Product.Sku", product.Sku));
			tokens.Add(new Token("Product.Name", product.GetLocalized(x => x.Name, languageId)));
			tokens.Add(new Token("Product.ShortDescription", product.GetLocalized(x => x.ShortDescription, languageId), true));
			tokens.Add(new Token("Product.StockQuantity", product.StockQuantity.ToString()));

            // TODO: add a method for getting URL (use routing because it handles all SEO friendly URLs)
            var productUrl = string.Format("{0}{1}", _webHelper.GetStoreLocation(false), product.GetSeName());
            tokens.Add(new Token("Product.ProductURLForCustomer", productUrl, true));

            //event notification
            _eventPublisher.EntityTokensAdded(product, tokens);
        }

        public virtual void AddForumTopicTokens(IList<Token> tokens, ForumTopic forumTopic,
            int? friendlyForumTopicPageIndex = null, int? appendedPostIdentifierAnchor = null)
        {
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            string topicUrl = null;
            if (friendlyForumTopicPageIndex.HasValue && friendlyForumTopicPageIndex.Value > 1)
                topicUrl = string.Format("{0}boards/topic/{1}/{2}/page/{3}", _webHelper.GetStoreLocation(false), forumTopic.Id, forumTopic.GetSeName(), friendlyForumTopicPageIndex.Value);
            else
                topicUrl = string.Format("{0}boards/topic/{1}/{2}", _webHelper.GetStoreLocation(false), forumTopic.Id, forumTopic.GetSeName());
            if (appendedPostIdentifierAnchor.HasValue && appendedPostIdentifierAnchor.Value > 0)
                topicUrl = string.Format("{0}#{1}", topicUrl, appendedPostIdentifierAnchor.Value);
            tokens.Add(new Token("Forums.TopicURL", topicUrl, true));
            tokens.Add(new Token("Forums.TopicName", forumTopic.Subject));

            //event notification
            _eventPublisher.EntityTokensAdded(forumTopic, tokens);
        }

        public virtual void AddForumPostTokens(IList<Token> tokens, ForumPost forumPost)
        {
            tokens.Add(new Token("Forums.PostAuthor", forumPost.Customer.FormatUserName()));
            tokens.Add(new Token("Forums.PostBody", forumPost.FormatPostText(), true));

            //event notification
            _eventPublisher.EntityTokensAdded(forumPost, tokens);
        }

		public virtual void AddForumTokens(IList<Token> tokens, Forum forum, int languageId)
        {
            //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
            var forumUrl = string.Format("{0}boards/forum/{1}/{2}", _webHelper.GetStoreLocation(false), forum.Id, forum.GetSeName(languageId));
            tokens.Add(new Token("Forums.ForumURL", forumUrl, true));
            tokens.Add(new Token("Forums.ForumName", forum.GetLocalized(x => x.Name, languageId)));

            //event notification
            _eventPublisher.EntityTokensAdded(forum, tokens);
        }

        public virtual void AddPrivateMessageTokens(IList<Token> tokens, PrivateMessage privateMessage)
        {
            tokens.Add(new Token("PrivateMessage.Subject", privateMessage.Subject));
            tokens.Add(new Token("PrivateMessage.Text", privateMessage.FormatPrivateMessageText(), true));

            //event notification
            _eventPublisher.EntityTokensAdded(privateMessage, tokens);
        }

        public virtual void AddBackInStockTokens(IList<Token> tokens, BackInStockSubscription subscription)
        {
            var customerLangId = subscription.Customer.GetAttribute<int>(
                        SystemCustomerAttributeNames.LanguageId,
                        _attrService,
                        _storeContext.CurrentStore.Id);

            var store = _storeService.GetStoreById(subscription.StoreId);
            var productLink = "{0}{1}".FormatWith(store.Url, subscription.Product.GetSeName(customerLangId, _urlRecordService, _languageService));

            tokens.Add(new Token("BackInStockSubscription.ProductName", "<a href='{0}'>{1}</a>".FormatWith(productLink, subscription.Product.Name), true));

            //event notification
            _eventPublisher.EntityTokensAdded(subscription, tokens);
        }

        /// <summary>
        /// Gets list of allowed (supported) message tokens for campaigns
        /// </summary>
        /// <returns>List of allowed (supported) message tokens for campaigns</returns>
        public virtual string[] GetListOfCampaignAllowedTokens()
        {
            var allowedTokens = new List<string>()
            {
                "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%NewsLetterSubscription.Email%",
                "%NewsLetterSubscription.ActivationUrl%",
                "%NewsLetterSubscription.DeactivationUrl%",
                "%Store.SupplierIdentification%",
            };
            return allowedTokens.ToArray();
        }

        public virtual string[] GetListOfAllowedTokens()
        {
            var allowedTokens = new List<string>()
            {
                "%Store.Name%",
                "%Store.URL%",
                "%Store.Email%",
                "%Order.OrderNumber%",
                "%Order.CustomerFullName%",
                "%Order.CustomerEmail%",
                "%Order.BillingFirstName%",
                "%Order.BillingLastName%",
                "%Order.BillingPhoneNumber%",
                "%Order.BillingEmail%",
                "%Order.BillingFaxNumber%",
                "%Order.BillingCompany%",
                "%Order.BillingAddress1%",
                "%Order.BillingAddress2%",
                "%Order.BillingCity%",
                "%Order.BillingStateProvince%",
                "%Order.BillingZipPostalCode%",
                "%Order.BillingCountry%",
                "%Order.ShippingMethod%",
                "%Order.ShippingFirstName%",
                "%Order.ShippingLastName%",
                "%Order.ShippingPhoneNumber%",
                "%Order.ShippingEmail%",
                "%Order.ShippingFaxNumber%",
                "%Order.ShippingCompany%",
                "%Order.ShippingAddress1%",
                "%Order.ShippingAddress2%",
                "%Order.ShippingCity%",
                "%Order.ShippingStateProvince%",
                "%Order.ShippingZipPostalCode%", 
                "%Order.ShippingCountry%",
                "%Order.PaymentMethod%",
                "%Order.VatNumber%", 
                "%Order.CustomerComment%", 
                "%Order.Product(s)%",
                "%Order.CreatedOn%",
                "%Order.OrderURLForCustomer%",
                "%Order.NewNoteText%",
				"%Product.ID%",
				"%Product.Sku%",
                "%Product.Name%",
                "%Product.ShortDescription%", 
                "%Product.ProductURLForCustomer%",
                "%Product.StockQuantity%",
                "%RecurringPayment.ID%",
                "%Shipment.ShipmentNumber%",
                "%Shipment.TrackingNumber%",
                "%Shipment.Product(s)%",
                "%Shipment.URLForCustomer%",
                "%ReturnRequest.ID%", 
                "%ReturnRequest.Product.Quantity%",
                "%ReturnRequest.Product.Name%", 
                "%ReturnRequest.Reason%", 
                "%ReturnRequest.RequestedAction%", 
                "%ReturnRequest.CustomerComment%", 
                "%ReturnRequest.StaffNotes%",
                "%ReturnRequest.Status%",
                "%GiftCard.SenderName%", 
                "%GiftCard.SenderEmail%",
                "%GiftCard.RecipientName%", 
                "%GiftCard.RecipientEmail%", 
                "%GiftCard.Amount%", 
                "%GiftCard.CouponCode%",
                "%GiftCard.Message%",
                "%Customer.Email%", 
                "%Customer.Username%", 
                "%Customer.FullName%", 
                "%Customer.VatNumber%",
                "%Customer.VatNumberStatus%", 
                "%Customer.PasswordRecoveryURL%", 
                "%Customer.AccountActivationURL%", 
                "%Wishlist.URLForCustomer%", 
                "%NewsLetterSubscription.Email%", 
                "%NewsLetterSubscription.ActivationUrl%",
                "%NewsLetterSubscription.DeactivationUrl%", 
                "%ProductReview.ProductName%", 
                "%BlogComment.BlogPostTitle%", 
                "%NewsComment.NewsTitle%",
                "%Forums.TopicURL%",
                "%Forums.TopicName%", 
                "%Forums.PostAuthor%",
                "%Forums.PostBody%",
                "%Forums.ForumURL%", 
                "%Forums.ForumName%", 
                "%PrivateMessage.Subject%", 
                "%PrivateMessage.Text%",
                "%BackInStockSubscription.ProductName%",
                "%Order.Disclaimer%",
                "%Order.ConditionsOfUse%",
                "%Company.CompanyName%",
                "%Company.Salutation%",
                "%Company.Title%",
                "%Company.Firstname%",
                "%Company.Lastname%",
                "%Company.CompanyManagementDescription%",
                "%Company.CompanyManagement%",
                "%Company.Street%",
                "%Company.Street2%",
                "%Company.ZipCode%",
                "%Company.City%",
                "%Company.CountryName%",
                "%Company.Region%",
                "%Company.VatId%",
                "%Company.CommercialRegister%",
                "%Company.TaxNumber%",
                "%Bank.Bankname%",
                "%Bank.Bankcode%",
                "%Bank.AccountNumber%",
                "%Bank.AccountHolder%",
                "%Bank.Iban%",
                "%Bank.Bic%",
                "%Contact.CompanyTelephoneNumber%",
                "%Contact.HotlineTelephoneNumber%",
                "%Contact.MobileTelephoneNumber%",
                "%Contact.CompanyFaxNumber%",
                "%Contact.CompanyEmailAddress%",
                "%Contact.WebmasterEmailAddress%",
                "%Contact.SupportEmailAddress%",
                "%Contact.ContactEmailAddress%",
                "%Store.SupplierIdentification%",
                
            };
            return allowedTokens.ToArray();
        }

        #endregion
    }
}
