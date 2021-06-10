using System;
using System.Text;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Html;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Tax;

namespace SmartStore.Services.Catalog
{
    /// <summary>
    /// Product attribute formatter
    /// </summary>
    public partial class ProductAttributeFormatter : IProductAttributeFormatter
    {
        private readonly IWorkContext _workContext;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly ITaxService _taxService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDownloadService _downloadService;
        private readonly IWebHelper _webHelper;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly CatalogSettings _catalogSettings;

        public ProductAttributeFormatter(IWorkContext workContext,
            IProductAttributeService productAttributeService,
            IProductAttributeParser productAttributeParser,
            IPriceCalculationService priceCalculationService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            ITaxService taxService,
            IPriceFormatter priceFormatter,
            IDownloadService downloadService,
            IWebHelper webHelper,
            ShoppingCartSettings shoppingCartSettings,
            CatalogSettings catalogSettings)
        {
            _workContext = workContext;
            _productAttributeService = productAttributeService;
            _productAttributeParser = productAttributeParser;
            _priceCalculationService = priceCalculationService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _taxService = taxService;
            _priceFormatter = priceFormatter;
            _downloadService = downloadService;
            _webHelper = webHelper;
            _shoppingCartSettings = shoppingCartSettings;
            _catalogSettings = catalogSettings;
        }

        /// <summary>
        /// Formats attributes.
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <returns>Formatted attributes.</returns>
		public string FormatAttributes(Product product, string attributes)
        {
            return FormatAttributes(product, attributes, _workContext.CurrentCustomer);
        }

        /// <summary>
        /// Formats attributes.
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="customer">Customer</param>
        /// <param name="separator">Separator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="renderProductAttributes">A value indicating whether to render product attributes</param>
        /// <param name="renderGiftCardAttributes">A value indicating whether to render gift card attributes</param>
        /// <param name="allowHyperlinks">A value indicating whether to render HTML hyperlinks</param>
        /// <returns>Formatted attributes.</returns>
        public string FormatAttributes(
            Product product,
            string attributes,
            Customer customer,
            string separator = "<br />",
            bool htmlEncode = true,
            bool renderPrices = true,
            bool renderProductAttributes = true,
            bool renderGiftCardAttributes = true,
            bool allowHyperlinks = true)
        {
            var result = new StringBuilder();

            // Attributes.
            if (renderProductAttributes)
            {
                var languageId = _workContext.WorkingLanguage.Id;
                var pvaCollection = _productAttributeParser.ParseProductVariantAttributes(attributes);

                for (var i = 0; i < pvaCollection.Count; ++i)
                {
                    var pva = pvaCollection[i];
                    var valuesStr = _productAttributeParser.ParseValues(attributes, pva.Id);

                    for (var j = 0; j < valuesStr.Count; ++j)
                    {
                        var valueStr = valuesStr[j];
                        var pvaAttribute = string.Empty;

                        if (!pva.ShouldHaveValues())
                        {
                            // No values.
                            if (pva.AttributeControlType == AttributeControlType.MultilineTextbox)
                            {
                                // Multiline textbox.
                                string attributeName = pva.ProductAttribute.GetLocalized(a => a.Name, languageId);

                                if (htmlEncode)
                                {
                                    attributeName = HttpUtility.HtmlEncode(attributeName);
                                }

                                pvaAttribute = string.Format("{0}: {1}", attributeName, HtmlUtils.ConvertPlainTextToHtml(valueStr.HtmlEncode()));
                                // We never encode multiline textbox input.
                            }
                            else if (pva.AttributeControlType == AttributeControlType.FileUpload)
                            {
                                Guid.TryParse(valueStr, out var downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);

                                if (download?.MediaFile != null)
                                {
                                    // TODO: add a method for getting URL (use routing because it handles all SEO friendly URLs).
                                    var attributeText = "";
                                    var fileName = download.MediaFile.Name;

                                    if (htmlEncode)
                                    {
                                        fileName = HttpUtility.HtmlEncode(fileName);
                                    }

                                    if (allowHyperlinks)
                                    {
                                        var downloadLink = string.Format("{0}download/getfileupload/?downloadId={1}", _webHelper.GetStoreLocation(), download.DownloadGuid);
                                        attributeText = string.Format("<a href=\"{0}\" class=\"fileuploadattribute\">{1}</a>", downloadLink, fileName);
                                    }
                                    else
                                    {
                                        attributeText
                                            = fileName;
                                    }
                                    string attributeName = pva.ProductAttribute.GetLocalized(a => a.Name, languageId);

                                    if (htmlEncode)
                                    {
                                        attributeName = HttpUtility.HtmlEncode(attributeName);
                                    }

                                    pvaAttribute = string.Format("{0}: {1}", attributeName, attributeText);
                                }
                            }
                            else
                            {
                                // Other attributes (textbox, datepicker).
                                pvaAttribute = string.Format("{0}: {1}", pva.ProductAttribute.GetLocalized(a => a.Name, languageId), valueStr);

                                if (htmlEncode)
                                {
                                    pvaAttribute = HttpUtility.HtmlEncode(pvaAttribute);
                                }
                            }
                        }
                        else
                        {
                            // Attributes with values.
                            if (int.TryParse(valueStr, out var pvaId))
                            {
                                var pvaValue = _productAttributeService.GetProductVariantAttributeValueById(pvaId);
                                if (pvaValue != null)
                                {
                                    pvaAttribute = "{0}: {1}".FormatInvariant(
                                        pva.ProductAttribute.GetLocalized(a => a.Name, languageId),
                                        pvaValue.GetLocalized(a => a.Name, languageId));

                                    if (renderPrices)
                                    {
                                        var attributeValuePriceAdjustment = _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(pvaValue, product, customer, null, 1);
                                        var priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, customer, out var taxRate);
                                        var priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);

                                        if (_shoppingCartSettings.ShowLinkedAttributeValueQuantity &&
                                            pvaValue.ValueType == ProductVariantAttributeValueType.ProductLinkage &&
                                            pvaValue.Quantity > 1)
                                        {
                                            pvaAttribute += string.Format(" × {0}", pvaValue.Quantity);
                                        }

                                        if (_catalogSettings.ShowVariantCombinationPriceAdjustment)
                                        {
                                            if (priceAdjustmentBase > 0)
                                            {
                                                pvaAttribute += " (+{0})".FormatInvariant(_priceFormatter.FormatPrice(priceAdjustment, true, false));
                                            }
                                            else if (priceAdjustmentBase < decimal.Zero)
                                            {
                                                pvaAttribute += " (-{0})".FormatInvariant(_priceFormatter.FormatPrice(-priceAdjustment, true, false));
                                            }
                                        }
                                    }
                                }

                                if (htmlEncode)
                                {
                                    pvaAttribute = HttpUtility.HtmlEncode(pvaAttribute);
                                }
                            }
                        }

                        if (!string.IsNullOrEmpty(pvaAttribute))
                        {
                            if (i != 0 || j != 0)
                            {
                                result.Append(separator);
                            }
                            result.Append(pvaAttribute);
                        }
                    }
                }
            }

            // Gift cards.
            if (renderGiftCardAttributes)
            {
                if (product.IsGiftCard)
                {
                    _productAttributeParser.GetGiftCardAttribute(attributes, out var giftCardRecipientName, out var giftCardRecipientEmail,
                        out var giftCardSenderName, out var giftCardSenderEmail, out var giftCardMessage);

                    // Sender.
                    var giftCardFrom = product.GiftCardType == GiftCardType.Virtual
                        ? string.Format(_localizationService.GetResource("GiftCardAttribute.From.Virtual"), giftCardSenderName, giftCardSenderEmail)
                        : string.Format(_localizationService.GetResource("GiftCardAttribute.From.Physical"), giftCardSenderName);

                    // Recipient.
                    var giftCardFor = product.GiftCardType == GiftCardType.Virtual
                        ? string.Format(_localizationService.GetResource("GiftCardAttribute.For.Virtual"), giftCardRecipientName, giftCardRecipientEmail)
                        : string.Format(_localizationService.GetResource("GiftCardAttribute.For.Physical"), giftCardRecipientName);

                    if (htmlEncode)
                    {
                        giftCardFrom = HttpUtility.HtmlEncode(giftCardFrom);
                        giftCardFor = HttpUtility.HtmlEncode(giftCardFor);
                    }

                    if (!string.IsNullOrEmpty(result.ToString()))
                    {
                        result.Append(separator);
                    }
                    result.Append(giftCardFrom);
                    result.Append(separator);
                    result.Append(giftCardFor);
                }
            }

            return result.ToString();
        }
    }
}
