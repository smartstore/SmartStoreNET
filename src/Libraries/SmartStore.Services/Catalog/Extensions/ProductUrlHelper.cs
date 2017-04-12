using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Collections;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Localization;

namespace SmartStore.Services.Catalog.Extensions
{
	public class ProductUrlHelper
	{
		private readonly HttpRequestBase _httpRequest;
		private readonly ICommonServices _services;
		private readonly IProductAttributeParser _productAttributeParser;
		private readonly Lazy<ILanguageService> _languageService;
		private readonly Lazy<LocalizationSettings> _localizationSettings;

		//private readonly int _languageId;
		private readonly QueryString _initialQuery;

		public ProductUrlHelper(
			HttpRequestBase httpRequest,
			ICommonServices services,
			IProductAttributeParser productAttributeParser,
			Lazy<ILanguageService> languageService,
			Lazy<LocalizationSettings> localizationSettings)
		{
			_httpRequest = httpRequest;
			_services = services;
			_productAttributeParser = productAttributeParser;
			_languageService = languageService;
			_localizationSettings = localizationSettings;

			//_languageId = _services.WorkContext.WorkingLanguage.Id;
			_initialQuery = QueryString.Current;
		}

		public string Url { get; set; }

		protected virtual string ToQueryString(ProductVariantQuery query, bool withInitialQuery = true)
		{
			var qs = withInitialQuery 
				? new QueryString(_initialQuery)
				: new QueryString();

			// Checkout Attributes
			foreach (var item in query.CheckoutAttributes)
			{
				var name = item.ToString();

				if (item.Date.HasValue)
				{
					qs.Add(name + "-date", string.Join("-", item.Date.Value.Year, item.Date.Value.Month, item.Date.Value.Day));
				}
				else
				{
					qs.Add(name, item.Value);
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
				var name = item.Alias.HasValue()
					? $"{item.Alias}-{item.ProductId}-{item.BundleItemId}-{item.VariantAttributeId}"
					: item.ToString();

				if (item.Date.HasValue)
				{
					// TODO: Code never reached because of ParseProductVariantAttributeValues
					qs.Add(name + "-date", string.Join("-", item.Date.Value.Year, item.Date.Value.Month, item.Date.Value.Day));
				}
				else
				{
					var value = item.ValueAlias.HasValue()
						? $"{item.ValueAlias}-{item.Value}"
						: item.Value;

					qs.Add(name, value);
				}
			}

			return qs.ToString(false);
		}

		/// <summary>
		/// Deserializes product variant query
		/// </summary>
		/// <param name="query">Product variant query</param>
		/// <param name="attributesXml">XML formatted attributes</param>
		/// <param name="productId">Product identifier</param>
		/// <param name="bundleItemId">Bundle item identifier</param>
		public virtual void DeserializeQuery(ProductVariantQuery query, int productId, string attributesXml, int bundleItemId = 0)
		{
			Guard.NotNull(query, nameof(query));

			if (attributesXml.HasValue() && productId != 0)
			{
				var attributeValues = _productAttributeParser.ParseProductVariantAttributeValues(attributesXml).ToList();

				foreach (var value in attributeValues)
				{
					query.AddVariant(new ProductVariantQueryItem(value.Id.ToString())
					{
						ProductId = productId,
						BundleItemId = bundleItemId,
						AttributeId = value.ProductVariantAttribute.ProductAttributeId,
						VariantAttributeId = value.ProductVariantAttributeId,
						Alias = value.ProductVariantAttribute.ProductAttribute.Alias,
						ValueAlias = value.Alias
					});
				}
			}
		}

		public virtual string GetProductUrl(ProductVariantQuery query, string productSeName)
		{
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
		/// Gets the URL of the product page including attributes query string
		/// </summary>
		/// <param name="attributesXml">XML formatted attributes</param>
		/// <param name="productId">Product identifier</param>
		/// <param name="productSeName">Product SEO name</param>
		/// <returns>URL of the product page including attributes query string</returns>
		public virtual string GetProductUrl(int productId, string productSeName, string attributesXml)
		{
			var query = new ProductVariantQuery();
			DeserializeQuery(query, productId, attributesXml);

			return GetProductUrl(query, productSeName);
		}

		public virtual string GetAbsoluteProductUrl(
			int productId,
			string productSeName,
			string attributesXml,
			Store store = null,
			Language language = null)
		{
			if (_httpRequest == null)
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

				url = store.Url.TrimEnd('/') + urlHelper.GetAbsolutePath();
			}

			if (attributesXml.HasValue())
			{
				var query = new ProductVariantQuery();
				DeserializeQuery(query, productId, attributesXml);

				// Ignore initial query. Could be unwanted stuff like task parameter CurrentCustomerId.
				url = url + ToQueryString(query, false);
			}

			return url;
		}
	}
}
