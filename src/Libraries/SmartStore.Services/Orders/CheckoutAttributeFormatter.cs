using System;
using System.Text;
using System.Web;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Html;
using SmartStore.Services.Catalog;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Tax;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Checkout attribute helper
    /// </summary>
    public partial class CheckoutAttributeFormatter : ICheckoutAttributeFormatter
    {
        private readonly IWorkContext _workContext;
        private readonly ICheckoutAttributeService _checkoutAttributeService;
        private readonly ICheckoutAttributeParser _checkoutAttributeParser;
        private readonly ICurrencyService _currencyService;
        private readonly ITaxService _taxService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDownloadService _downloadService;
        private readonly IWebHelper _webHelper;

        public CheckoutAttributeFormatter(IWorkContext workContext,
            ICheckoutAttributeService checkoutAttributeService,
            ICheckoutAttributeParser checkoutAttributeParser,
            ICurrencyService currencyService,
            ITaxService taxService,
            IPriceFormatter priceFormatter,
            IDownloadService downloadService,
            IWebHelper webHelper)
        {
            this._workContext = workContext;
            this._checkoutAttributeService = checkoutAttributeService;
            this._checkoutAttributeParser = checkoutAttributeParser;
            this._currencyService = currencyService;
            this._taxService = taxService;
            this._priceFormatter = priceFormatter;
            this._downloadService = downloadService;
            this._webHelper = webHelper;
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <returns>Attributes</returns>
        public string FormatAttributes(string attributes)
        {
            var customer = _workContext.CurrentCustomer;
            return FormatAttributes(attributes, customer);
        }

        /// <summary>
        /// Formats attributes
        /// </summary>
        /// <param name="attributes">Attributes</param>
        /// <param name="customer">Customer</param>
        /// <param name="serapator">Serapator</param>
        /// <param name="htmlEncode">A value indicating whether to encode (HTML) values</param>
        /// <param name="renderPrices">A value indicating whether to render prices</param>
        /// <param name="allowHyperlinks">A value indicating whether to HTML hyperink tags could be rendered (if required)</param>
        /// <returns>Attributes</returns>
        public string FormatAttributes(string attributes,
            Customer customer,
            string serapator = "<br />",
            bool htmlEncode = true,
            bool renderPrices = true,
            bool allowHyperlinks = true)
        {
            var result = new StringBuilder();

            var caCollection = _checkoutAttributeParser.ParseCheckoutAttributes(attributes);

            if (caCollection.Count <= 0)
            {
                return null;
            }
            for (int i = 0; i < caCollection.Count; i++)
            {
                var ca = caCollection[i];
                var valuesStr = _checkoutAttributeParser.ParseValues(attributes, ca.Id);
                for (int j = 0; j < valuesStr.Count; j++)
                {
                    string valueStr = valuesStr[j];
                    string caAttribute = "";
                    if (!ca.ShouldHaveValues())
                    {
                        //no values
                        if (ca.AttributeControlType == AttributeControlType.MultilineTextbox)
                        {
                            //multiline textbox
                            string attributeName = ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage);
                            //encode (if required)
                            if (htmlEncode)
                                attributeName = HttpUtility.HtmlEncode(attributeName);

                            caAttribute = string.Format("{0}: {1}", attributeName,
                                HtmlUtils.ConvertPlainTextToHtml(valueStr.EmptyNull().Replace(":", "").HtmlEncode()));

                            //we never encode multiline textbox input
                        }
                        else if (ca.AttributeControlType == AttributeControlType.FileUpload)
                        {
                            //file upload
                            Guid downloadGuid;
                            Guid.TryParse(valueStr, out downloadGuid);
                            var download = _downloadService.GetDownloadByGuid(downloadGuid);
                            if (download?.MediaFile != null)
                            {
                                //TODO add a method for getting URL (use routing because it handles all SEO friendly URLs)
                                string attributeText = "";
                                var fileName = download.MediaFile.Name;
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
                                string attributeName = ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage);
                                //encode (if required)
                                if (htmlEncode)
                                    attributeName = HttpUtility.HtmlEncode(attributeName);
                                caAttribute = string.Format("{0}: {1}", attributeName, attributeText);
                            }
                        }
                        else
                        {
                            //other attributes (textbox, datepicker)
                            caAttribute = string.Format("{0}: {1}", ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage), valueStr);
                            //encode (if required)
                            if (htmlEncode)
                                caAttribute = HttpUtility.HtmlEncode(caAttribute);
                        }
                    }
                    else
                    {
                        int caId = 0;
                        if (int.TryParse(valueStr, out caId))
                        {
                            var caValue = _checkoutAttributeService.GetCheckoutAttributeValueById(caId);
                            if (caValue != null)
                            {
                                caAttribute = string.Format("{0}: {1}", ca.GetLocalized(a => a.Name, _workContext.WorkingLanguage), caValue.GetLocalized(a => a.Name, _workContext.WorkingLanguage));
                                if (renderPrices)
                                {
                                    decimal priceAdjustmentBase = _taxService.GetCheckoutAttributePrice(caValue, customer);
                                    decimal priceAdjustment = _currencyService.ConvertFromPrimaryStoreCurrency(priceAdjustmentBase, _workContext.WorkingCurrency);
                                    if (priceAdjustmentBase > 0)
                                    {
                                        string priceAdjustmentStr = _priceFormatter.FormatPrice(priceAdjustment);
                                        caAttribute += string.Format(" [+{0}]", priceAdjustmentStr);
                                    }
                                }
                            }
                            //encode (if required)
                            if (htmlEncode)
                                caAttribute = HttpUtility.HtmlEncode(caAttribute);
                        }
                    }

                    if (!String.IsNullOrEmpty(caAttribute))
                    {
                        if (i != 0 || j != 0)
                            result.Append(serapator);
                        result.Append(caAttribute);
                    }
                }
            }

            return result.ToString();
        }

    }
}
