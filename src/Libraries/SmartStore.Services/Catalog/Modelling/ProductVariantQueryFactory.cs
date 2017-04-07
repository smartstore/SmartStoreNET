using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using SmartStore.Collections;
using SmartStore.Services.Search.Modelling;

namespace SmartStore.Services.Catalog.Modelling
{
	public class ProductVariantQueryFactory : IProductVariantQueryFactory
	{
		internal static readonly Regex IsVariantKey = new Regex(@"pvari[0-9]+-[0-9]+-[0-9]+-[0-9]+", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		internal static readonly Regex IsGiftCardKey = new Regex(@"giftcard[0-9]+-[0-9]+-\.\w+$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		internal static readonly Regex IsCheckoutAttributeKey = new Regex(@"cattr[0-9]+$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		internal static readonly Regex IsVariantAliasKey = new Regex(@"\w+-[0-9]+-[0-9]+-[0-9]+$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

		protected readonly HttpContextBase _httpContext;
		protected readonly ICommonServices _services;
		protected readonly ICatalogSearchQueryAliasMapper _catalogSearchQueryAliasMapper;
		private Multimap<string, string> _queryItems;

		public ProductVariantQueryFactory(
			HttpContextBase httpContext,
			ICommonServices services,
			ICatalogSearchQueryAliasMapper catalogSearchQueryAliasMapper)
		{
			_httpContext = httpContext;
			_services = services;
			_catalogSearchQueryAliasMapper = catalogSearchQueryAliasMapper;
		}

		protected Multimap<string, string> QueryItems
		{
			get
			{
				if (_queryItems == null)
				{
					_queryItems = new Multimap<string, string>();

					if (_httpContext.Request != null)
					{
						var form = _httpContext.Request.Form;
						var query = _httpContext.Request.QueryString;

						if (form != null)
						{
							form.AllKeys.Each(key => _queryItems.AddRange(key, form[key].SplitSafe(",")));
						}

						if (query != null)
						{
							query.AllKeys.Each(key => _queryItems.AddRange(key, query[key].SplitSafe(",")));
						}
					}
				}

				return _queryItems;
			}
		}

		protected virtual void ConvertVariant(ProductVariantQuery query, string key, string value)
		{
			if (key.EndsWith("-day") || key.EndsWith("-month"))
				return;

			var ids = key.Replace("pvari", "").SplitSafe("-");
			if (ids.Length > 3)
			{
				var variant = new ProductVariantQueryItem(value);
				variant.ProductId = ids[0].ToInt();
				variant.BundleItemId = ids[1].ToInt();
				variant.AttributeId = ids[2].ToInt();
				variant.VariantAttributeId = ids[3].ToInt();

				if (key.EndsWith("-year"))
				{
					variant.Year = value.ToInt();

					var dateKey = key.Replace("-year", "");
					variant.Month = QueryItems[dateKey + "-month"].FirstOrDefault()?.ToInt() ?? 0;
					variant.Day = QueryItems[dateKey + "-day"].FirstOrDefault()?.ToInt() ?? 0;
				}

				query.AddVariant(variant);
			}
		}

		protected virtual void ConvertGiftCard(ProductVariantQuery query, string key, string value)
		{
			var elements = key.Replace("giftcard", "").SplitSafe("-");
			if (elements.Length > 2)
			{
				var giftCard = new GiftCardQueryItem(elements[2], value);
				giftCard.ProductId = elements[0].ToInt();
				giftCard.BundleItemId = elements[1].ToInt();

				query.AddGiftCard(giftCard);
			}
		}

		protected virtual void ConvertCheckoutAttribute(ProductVariantQuery query, string key, string value)
		{
			if (key.EndsWith("-day") || key.EndsWith("-month"))
				return;

			var ids = key.Replace("cattr", "").SplitSafe("-");
			if (ids.Length > 0)
			{
				var attribute = new CheckoutAttributeQueryItem(ids[0].ToInt(), value);

				if (key.EndsWith("-year"))
				{
					attribute.Year = value.ToInt();

					var dateKey = key.Replace("-year", "");
					attribute.Month = QueryItems[dateKey + "-month"].FirstOrDefault()?.ToInt() ?? 0;
					attribute.Day = QueryItems[dateKey + "-day"].FirstOrDefault()?.ToInt() ?? 0;
				}

				query.AddCheckoutAttribute(attribute);
			}
		}

		protected virtual bool ConvertVariantAlias(ProductVariantQuery query, string key, ICollection<string> values, int languageId)
		{
			var ids = key.SplitSafe("-");
			if (ids.Length < 4)
				return false;

			var attributeId = _catalogSearchQueryAliasMapper.GetVariantIdByAlias(ids[0], languageId);
			if (attributeId == 0)
				return false;

			var result = false;
			var productId = ids[1].ToInt();
			var bundleItemId = ids[2].ToInt();
			var variantAttributeId = ids[3].ToInt();

			foreach (var value in values)
			{
				var optionId = _catalogSearchQueryAliasMapper.GetVariantOptionIdByAlias(value, attributeId, languageId);

				var variant = new ProductVariantQueryItem(optionId == 0 ? value : optionId.ToString());
				variant.ProductId = productId;
				variant.BundleItemId = bundleItemId;
				variant.AttributeId = attributeId;
				variant.VariantAttributeId = variantAttributeId;

				query.AddVariant(variant);
				result = true;
			}

			return result;
		}

		protected virtual void ConvertItems(HttpRequestBase request, ProductVariantQuery query, string key, ICollection<string> values)
		{
		}

		public ProductVariantQuery Current { get; private set; }

		public ProductVariantQuery CreateFromQuery()
		{
			var query = new ProductVariantQuery();
			Current = query;

			if (_httpContext.Request == null)
				return query;

			var languageId = _services.WorkContext.WorkingLanguage.Id;

			foreach (var item in QueryItems)
			{
				if (IsVariantKey.IsMatch(item.Key))
				{
					item.Value.Each(value => ConvertVariant(query, item.Key, value));
				}
				else if (IsGiftCardKey.IsMatch(item.Key))
				{
					item.Value.Each(value => ConvertGiftCard(query, item.Key, value));
				}
				else if (IsCheckoutAttributeKey.IsMatch(item.Key))
				{
					item.Value.Each(value => ConvertCheckoutAttribute(query, item.Key, value));
				}
				else if (IsVariantAliasKey.IsMatch(item.Key) && ConvertVariantAlias(query, item.Key, item.Value, languageId))
				{
				}
				else
				{
					ConvertItems(_httpContext.Request, query, item.Key, item.Value);
				}
			}

			return query;
		}
	}
}
