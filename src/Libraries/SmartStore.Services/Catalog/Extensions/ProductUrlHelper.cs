using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Collections;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Localization;
using SmartStore.Services.Search.Modelling;

namespace SmartStore.Services.Catalog.Extensions
{
    public class ProductUrlHelper
    {
        private readonly HttpRequestBase _httpRequest;
        private readonly ICommonServices _services;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly Lazy<ILanguageService> _languageService;
        private readonly Lazy<ICatalogSearchQueryAliasMapper> _catalogSearchQueryAliasMapper;
        private readonly Lazy<LocalizationSettings> _localizationSettings;

        private readonly int _languageId;

        public ProductUrlHelper(
            HttpRequestBase httpRequest,
            ICommonServices services,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            Lazy<ILanguageService> languageService,
            Lazy<ICatalogSearchQueryAliasMapper> catalogSearchQueryAliasMapper,
            Lazy<LocalizationSettings> localizationSettings)
        {
            _httpRequest = httpRequest;
            _services = services;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _languageService = languageService;
            _catalogSearchQueryAliasMapper = catalogSearchQueryAliasMapper;
            _localizationSettings = localizationSettings;

            _languageId = _services.WorkContext.WorkingLanguage.Id;
        }

        /// <summary>
        /// URL of the product page used to create the new product URL. Created from route if <c>null</c>.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Initial query string used to create the new query string. Usually <c>null</c>.
        /// </summary>
        public QueryString InitialQuery { get; set; }

        /// <summary>
        /// Converts a query object into a URL query string
        /// </summary>
        /// <param name="query">Product variant query</param>
        /// <returns>URL query string</returns>
        public virtual string ToQueryString(ProductVariantQuery query)
        {
            var qs = InitialQuery != null
                ? new QueryString(InitialQuery)
                : new QueryString();

            // Checkout Attributes
            foreach (var item in query.CheckoutAttributes)
            {
                if (item.Date.HasValue)
                {
                    qs.Add(item.ToString(), string.Join("-", item.Date.Value.Year, item.Date.Value.Month, item.Date.Value.Day));
                }
                else
                {
                    qs.Add(item.ToString(), item.Value);
                }
            }

            // Gift cards
            foreach (var item in query.GiftCards)
            {
                qs.Add(item.ToString(), item.Value);
            }

            // Variants
            foreach (var item in query.Variants)
            {
                if (item.Alias.IsEmpty())
                {
                    item.Alias = _catalogSearchQueryAliasMapper.Value.GetVariantAliasById(item.AttributeId, _languageId);
                }

                if (item.Date.HasValue)
                {
                    qs.Add(item.ToString(), string.Join("-", item.Date.Value.Year, item.Date.Value.Month, item.Date.Value.Day));
                }
                else if (item.IsFile || item.IsText)
                {
                    qs.Add(item.ToString(), item.Value);
                }
                else
                {
                    if (item.ValueAlias.IsEmpty())
                    {
                        item.ValueAlias = _catalogSearchQueryAliasMapper.Value.GetVariantOptionAliasById(item.Value.ToInt(), _languageId);
                    }

                    var value = item.ValueAlias.HasValue()
                        ? $"{item.ValueAlias}-{item.Value}"
                        : item.Value;

                    qs.Add(item.ToString(), value);
                }
            }

            return qs.ToString(false);
        }

        /// <summary>
        /// Deserializes attributes XML into a product variant query
        /// </summary>
        /// <param name="query">Product variant query</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="attributesXml">XML formatted attributes</param>
        /// <param name="bundleItemId">Bundle item identifier</param>
        /// <param name="attributes">Product variant attributes</param>
        public virtual void DeserializeQuery(
            ProductVariantQuery query,
            int productId,
            string attributesXml,
            int bundleItemId = 0,
            ICollection<ProductVariantAttribute> attributes = null)
        {
            Guard.NotNull(query, nameof(query));

            if (productId == 0 || attributesXml.IsEmpty())
                return;

            var attributeMap = _productAttributeParser.DeserializeProductVariantAttributes(attributesXml);

            if (attributes == null)
            {
                attributes = _productAttributeService.GetProductVariantAttributesByIds(attributeMap.Keys);
            }

            foreach (var attribute in attributes)
            {
                if (attributeMap.ContainsKey(attribute.Id))
                {
                    foreach (var originalValue in attributeMap[attribute.Id])
                    {
                        var value = originalValue;
                        DateTime? date = null;

                        if (attribute.AttributeControlType == AttributeControlType.Datepicker)
                        {
                            date = originalValue.ToDateTime(new string[] { "D" }, CultureInfo.CurrentCulture, DateTimeStyles.None, null);
                            if (date == null)
                                continue;

                            value = string.Join("-", date.Value.Year, date.Value.Month, date.Value.Day);
                        }

                        var queryItem = new ProductVariantQueryItem(value);
                        queryItem.ProductId = productId;
                        queryItem.BundleItemId = bundleItemId;
                        queryItem.AttributeId = attribute.ProductAttributeId;
                        queryItem.VariantAttributeId = attribute.Id;
                        queryItem.Alias = _catalogSearchQueryAliasMapper.Value.GetVariantAliasById(attribute.ProductAttributeId, _languageId);
                        queryItem.Date = date;
                        queryItem.IsFile = attribute.AttributeControlType == AttributeControlType.FileUpload;
                        queryItem.IsText = attribute.AttributeControlType == AttributeControlType.TextBox || attribute.AttributeControlType == AttributeControlType.MultilineTextbox;

                        if (attribute.ShouldHaveValues())
                        {
                            queryItem.ValueAlias = _catalogSearchQueryAliasMapper.Value.GetVariantOptionAliasById(value.ToInt(), _languageId);
                        }

                        query.AddVariant(queryItem);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a product URL including variant query string.
        /// </summary>
        /// <param name="query">Product variant query</param>
        /// <param name="productSeName">Product SEO name</param>
        /// <returns>Product URL</returns>
        public virtual string GetProductUrl(ProductVariantQuery query, string productSeName)
        {
            if (productSeName.IsEmpty())
                return null;

            var url = Url ?? UrlHelper.GenerateUrl(
                "Product",
                null,
                null,
                new RouteValueDictionary(new { SeName = productSeName }),
                RouteTable.Routes,
                _httpRequest.RequestContext,
                false);

            return url + ToQueryString(query);
        }

        /// <summary>
        /// Creates a product URL including variant query string.
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="productSeName">Product SEO name</param>
        /// <param name="attributesXml">XML formatted attributes</param>
        /// <returns>Product URL</returns>
        public virtual string GetProductUrl(int productId, string productSeName, string attributesXml)
        {
            var query = new ProductVariantQuery();
            DeserializeQuery(query, productId, attributesXml);

            return GetProductUrl(query, productSeName);
        }

        /// <summary>
        /// Creates an absolute product URL.
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="productSeName">Product SEO name</param>
        /// <param name="attributesXml">XML formatted attributes</param>
        /// <param name="store">Store entity</param>
        /// <param name="language">Language entity</param>
        /// <returns>Absolute product URL</returns>
        public virtual string GetAbsoluteProductUrl(
            int productId,
            string productSeName,
            string attributesXml,
            Store store = null,
            Language language = null)
        {
            if (_httpRequest == null || productSeName.IsEmpty())
                return null;

            var url = Url;

            if (url.IsEmpty())
            {
                // No given URL. Create SEO friendly URL.
                var urlHelper = new LocalizedUrlHelper(_httpRequest.ApplicationPath, productSeName, false);

                store = store ?? _services.StoreContext.CurrentStore;
                language = language ?? _services.WorkContext.WorkingLanguage;

                if (_localizationSettings.Value.SeoFriendlyUrlsForLanguagesEnabled)
                {
                    var defaultSeoCode = _languageService.Value.GetDefaultLanguageSeoCode(store.Id);

                    if (language.UniqueSeoCode == defaultSeoCode && _localizationSettings.Value.DefaultLanguageRedirectBehaviour > 0)
                    {
                        urlHelper.StripSeoCode();
                    }
                    else
                    {
                        urlHelper.PrependSeoCode(language.UniqueSeoCode, true);
                    }
                }

                var storeUrl = store.Url.TrimEnd('/');

                // Prevent duplicate occurrence of application path.
                if (urlHelper.ApplicationPath.HasValue() && storeUrl.EndsWith(urlHelper.ApplicationPath, StringComparison.OrdinalIgnoreCase))
                {
                    storeUrl = storeUrl.Substring(0, storeUrl.Length - urlHelper.ApplicationPath.Length).TrimEnd('/');
                }

                url = storeUrl + urlHelper.GetAbsolutePath();
            }

            if (attributesXml.HasValue())
            {
                var query = new ProductVariantQuery();
                DeserializeQuery(query, productId, attributesXml);

                url = url + ToQueryString(query);
            }

            return url;
        }
    }
}
