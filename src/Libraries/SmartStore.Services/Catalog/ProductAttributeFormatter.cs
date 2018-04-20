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
        /// Formats attributes
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <returns>Attributes</returns>
		public string FormatAttributes(Product product, string attributes)
        {
            var customer = _workContext.CurrentCustomer;
            return FormatAttributes(product, attributes, customer);
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
		/// <param name="product">Product</param>
        /// <param name="attributes">Attributes</param>
        /// <param name="customer">Customer</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="renderProductAttributes">A value indicating whether to render product attributes</param>
        /// <param name="renderGiftCardAttributes">A value indicating whether to render gift card attributes</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperink tags could be rendered (if required)</param>
        /// <returns>Attributes</returns>
        public string FormatAttributes(Product product, string attributes,
            Customer customer, string serapator = "<br />", bool htmlEncode = true, bool renderPrices = true,
            bool renderProductAttributes = true, bool renderGiftCardAttributes = true,
            bool allowHyperlinks = true)
        {
            var result = new StringBuilder();
			var languageId = _workContext.WorkingLanguage.Id;

            // Attributes
            if (renderProductAttributes)
            {
                var pvaCollection = _productAttributeParser.ParseProductVariantAttributes(attributes);
                for (int i = 0; i < pvaCollection.Count; i++)
                {
                    var pva = pvaCollection[i];
                    var valuesStr = _productAttributeParser.ParseValues(attributes, pva.Id);
                    for (int j = 0; j < valuesStr.Count; j++)
                    {
                        string valueStr = valuesStr[j];
                        string pvaAttribute = string.Empty;
                        if (!pva.ShouldHaveValues())
                        {
                            //no values
                            if (pva.AttributeControlType == AttributeControlType.MultilineTextbox)
                            {
                                //multiline textbox
                                string attributeName = pva.ProductAttribute.GetLocalized(a => a.Name, languageId);
                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = HttpUtility.HtmlEncode(attributeName);
                                pvaAttribute = string.Format("{0}: {1}", attributeName, HtmlUtils.FormatText(valueStr, false, true, false, false, false, false));
                                //we never encode multiline textbox input
                            }
                            else if (pva.AttributeControlType == AttributeControlType.FileUpload)
                            {
                                //file upload
                                Guid downloadGuid;
                                Guid.TryParse(valueStr, out downloadGuid);
                                var download = _downloadService.GetDownloadByGuid(downloadGuid);
                                if (download != null)
                                {
                                    //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                                    string attributeText = "";
                                    var fileName = string.Format("{0}{1}",
                                        download.Filename ?? download.DownloadGuid.ToString(),
                                        download.Extension);
                                    //encode (if required)
                                    if (htmlEncode)
                                        fileName = HttpUtility.HtmlEncode(fileName);
                                    if (allowHyperlinks)
                                    {
                                        //hyperlinks are allowed
                                        var downloadLink = string.Format("{0}download/getfileupload/?downloadId={1}", _webHelper.GetStoreLocation(false), download.DownloadGuid);
                                        attributeText = string.Format("<a href=\"{0}\" class=\"fileuploadattribute\">{1}</a>", downloadLink, fileName);
                                    }
                                    else
                                    {
                                        //hyperlinks aren't allowed
                                        attributeText = fileName;
                                    }
                                    string attributeName = pva.ProductAttribute.GetLocalized(a => a.Name, languageId);
                                    //encode (if required)
                                    if (htmlEncode)
                                        attributeName = HttpUtility.HtmlEncode(attributeName);
                                    pvaAttribute = string.Format("{0}: {1}", attributeName, attributeText);
                                }
                            }
                            else
                            {
                                //other attributes (textbox, datepicker)
                                pvaAttribute = string.Format("{0}: {1}", pva.ProductAttribute.GetLocalized(a => a.Name, languageId), valueStr);
                                //encode (if required)
                                if (htmlEncode)
                                    pvaAttribute = HttpUtility.HtmlEncode(pvaAttribute);
                            }
                        }
                        else
                        {
                            // Attributes with values.
                            int pvaId = 0;
                            if (int.TryParse(valueStr, out pvaId))
                            {
                                var pvaValue = _productAttributeService.GetProductVariantAttributeValueById(pvaId);
                                if (pvaValue != null)
                                {
                                    pvaAttribute = "{0}: {1}".FormatInvariant(
										pva.ProductAttribute.GetLocalized(a => a.Name, languageId),
										pvaValue.GetLocalized(a => a.Name, languageId));

                                    if (renderPrices)
                                    {
                                        decimal taxRate = decimal.Zero;
										decimal attributeValuePriceAdjustment = _priceCalculationService.GetProductVariantAttributeValuePriceAdjustment(pvaValue, product, customer, null, 1);
										decimal priceAdjustmentBase = _taxService.GetProductPrice(product, attributeValuePriceAdjustment, customer, out taxRate);
                                        decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);

										if (_shoppingCartSettings.ShowLinkedAttributeValueQuantity && pvaValue.ValueType == ProductVariantAttributeValueType.ProductLinkage &&
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

								// Encode (if required)
								if (htmlEncode)
								{
									pvaAttribute = HttpUtility.HtmlEncode(pvaAttribute);
								}
                            }
                        }

                        if (!String.IsNullOrEmpty(pvaAttribute))
                        {
                            if (i != 0 || j != 0)
                                result.Append(serapator);
                            result.Append(pvaAttribute);
                        }
                    }
                }
            }

            //gift cards
            if (renderGiftCardAttributes)
            {
                if (product.IsGiftCard)
                {
                    string giftCardRecipientName = "";
                    string giftCardRecipientEmail = "";
                    string giftCardSenderName = "";
                    string giftCardSenderEmail = "";
                    string giftCardMessage = "";
                    _productAttributeParser.GetGiftCardAttribute(attributes, out giftCardRecipientName, out giftCardRecipientEmail,
                        out giftCardSenderName, out giftCardSenderEmail, out giftCardMessage);

                    //sender
                    var giftCardFrom = product.GiftCardType == GiftCardType.Virtual ?
                        string.Format(_localizationService.GetResource("GiftCardAttribute.From.Virtual"), giftCardSenderName, giftCardSenderEmail) :
                        string.Format(_localizationService.GetResource("GiftCardAttribute.From.Physical"), giftCardSenderName);
                    //recipient
                    var giftCardFor = product.GiftCardType == GiftCardType.Virtual ?
                        string.Format(_localizationService.GetResource("GiftCardAttribute.For.Virtual"), giftCardRecipientName, giftCardRecipientEmail) :
                        string.Format(_localizationService.GetResource("GiftCardAttribute.For.Physical"), giftCardRecipientName);

                    //encode (if required)
                    if (htmlEncode)
                    {
                        giftCardFrom = HttpUtility.HtmlEncode(giftCardFrom);
                        giftCardFor = HttpUtility.HtmlEncode(giftCardFor);
                    }

                    if (!String.IsNullOrEmpty(result.ToString()))
                    {
                        result.Append(serapator);
                    }
                    result.Append(giftCardFrom);
                    result.Append(serapator);
                    result.Append(giftCardFor);
                }
            }
            return result.ToString();
        }
    }
}
