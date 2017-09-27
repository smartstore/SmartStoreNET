using System;
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
		internal static readonly Regex IsVariantAliasKey = new Regex(@"\w+-[0-9]+-[0-9]+-[0-9]+", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		internal static readonly Regex IsGiftCardKey = new Regex(@"giftcard[0-9]+-[0-9]+-\.\w+$", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
		internal static readonly Regex IsCheckoutAttributeKey = new Regex(@"cattr[0-9]+", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
							foreach (var key in form.AllKeys)
							{
								if (key.HasValue())
								{
									_queryItems.AddRange(key, form[key].SplitSafe(","));
								}
							}
						}

						if (query != null)
						{
							foreach (var key in query.AllKeys)
							{
								if (key.HasValue())
								{
									_queryItems.AddRange(key, query[key].SplitSafe(","));
								}
							}
						}
					}
				}

				return _queryItems;
			}
		}

		private DateTime? ConvertToDate(string key, string value)
		{
			var year = 0;
			var month = 0;
			var day = 0;

			if (key.EndsWith("-date"))
			{
				// Convert from one query string item.
				var dateItems = value.SplitSafe("-");
				year = dateItems.SafeGet(0).ToInt();
				month = dateItems.SafeGet(1).ToInt();
				day = dateItems.SafeGet(2).ToInt();
			}
			else if (key.EndsWith("-year"))
			{
				// Convert from three form controls.
				var dateKey = key.Replace("-year", "");
				year = value.ToInt();
				month = QueryItems[dateKey + "-month"].FirstOrDefault()?.ToInt() ?? 0;
				day = QueryItems[dateKey + "-day"].FirstOrDefault()?.ToInt() ?? 0;
			}

			if (year > 0 && month > 0 && day > 0)
			{
				try
				{
					return new DateTime(year, month, day);
				}
				catch { }
			}

			return null;
		}

		protected virtual void ConvertVariant(ProductVariantQuery query, string key, ICollection<string> values)
		{
			var ids = key.Replace("pvari", "").SplitSafe("-");
			if (ids.Length < 4)
				return;

			var isDate = key.EndsWith("-date") || key.EndsWith("-year");
			var isFile = key.EndsWith("-file");
			var isText = key.EndsWith("-text");

			if (isDate || isFile || isText)
			{
				var value = isText ? string.Join(",", values) : values.First();
				var variant = new ProductVariantQueryItem(value);
				variant.ProductId = ids[0].ToInt();
				variant.BundleItemId = ids[1].ToInt();
				variant.AttributeId = ids[2].ToInt();
				variant.VariantAttributeId = ids[3].ToInt();
				variant.IsFile = isFile;
				variant.IsText = isText;

				if (isDate)
				{
					variant.Date = ConvertToDate(key, value);
				}

				query.AddVariant(variant);
			}
			else
			{
				foreach (var value in values)
				{
					var variant = new ProductVariantQueryItem(value);
					variant.ProductId = ids[0].ToInt();
					variant.BundleItemId = ids[1].ToInt();
					variant.AttributeId = ids[2].ToInt();
					variant.VariantAttributeId = ids[3].ToInt();

					query.AddVariant(variant);
				}
			}
		}

		protected virtual void ConvertVariantAlias(ProductVariantQuery query, string key, ICollection<string> values, int languageId)
		{
			var ids = key.SplitSafe("-");
			var len = ids.Length;
			if (len < 4)
				return;

			var isDate = key.EndsWith("-date") || key.EndsWith("-year");
			var isFile = key.EndsWith("-file");
			var isText = key.EndsWith("-text");

			if (isDate || isFile || isText)
			{
				ids = ids.Take(len - 1).ToArray();
				len = ids.Length;
			}

			var alias = string.Join("-", ids.Take(len - 3));
			var attributeId = _catalogSearchQueryAliasMapper.GetVariantIdByAlias(alias, languageId);
			if (attributeId == 0)
				return;

			var productId = ids.SafeGet(len - 3).ToInt();
			var bundleItemId = ids.SafeGet(len - 2).ToInt();
			var variantAttributeId = ids.SafeGet(len - 1).ToInt();

			if (productId == 0 || variantAttributeId == 0)
				return;

			if (isDate || isFile || isText)
			{
				var value = isText ? string.Join(",", values) : values.First();
				var variant = new ProductVariantQueryItem(value);
				variant.ProductId = productId;
				variant.BundleItemId = bundleItemId;
				variant.AttributeId = attributeId;
				variant.VariantAttributeId = variantAttributeId;
				variant.Alias = alias;
				variant.IsFile = isFile;
				variant.IsText = isText;

				if (isDate)
				{
					variant.Date = ConvertToDate(key, value);
				}

				query.AddVariant(variant);
			}
			else
			{
				foreach (var value in values)
				{
					// We cannot use GetVariantOptionIdByAlias. It doesn't necessarily provide a ProductVariantAttributeValue.Id associated with this product.
					//var optionId = _catalogSearchQueryAliasMapper.GetVariantOptionIdByAlias(value, attributeId, languageId);
					var optionId = 0;
					string valueAlias = null;

					var valueIds = value.SplitSafe("-");
					if (valueIds.Length >= 2)
					{
						optionId = valueIds.SafeGet(valueIds.Length - 1).ToInt();
						valueAlias = string.Join("-", valueIds.Take(valueIds.Length - 1));
					}

					var variant = new ProductVariantQueryItem(optionId == 0 ? value : optionId.ToString());
					variant.ProductId = productId;
					variant.BundleItemId = bundleItemId;
					variant.AttributeId = attributeId;
					variant.VariantAttributeId = variantAttributeId;
					variant.Alias = alias;

					if (optionId != 0)
					{
						variant.ValueAlias = valueAlias;
					}

					query.AddVariant(variant);
				}
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

		protected virtual void ConvertCheckoutAttribute(ProductVariantQuery query, string key, ICollection<string> values)
		{
			var ids = key.Replace("cattr", "").SplitSafe("-");
			if (ids.Length <= 0)
				return;

			var attributeId = ids[0].ToInt();
			var isDate = key.EndsWith("-date") || key.EndsWith("-year");
			var isFile = key.EndsWith("-file");
			var isText = key.EndsWith("-text");

			if (isDate || isFile || isText)
			{
				var value = isText ? string.Join(",", values) : values.First();
				var attribute = new CheckoutAttributeQueryItem(attributeId, value);
				attribute.IsFile = isFile;
				attribute.IsText = isText;

				if (isDate)
				{
					attribute.Date = ConvertToDate(key, value);
				}

				query.AddCheckoutAttribute(attribute);
			}
			else
			{
				foreach (var value in values)
				{
					query.AddCheckoutAttribute(new CheckoutAttributeQueryItem(attributeId, value));
				}
			}
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
				if (!item.Value.Any() || item.Key.EndsWith("-day") || item.Key.EndsWith("-month"))
				{
					continue;
				}

				if (IsVariantKey.IsMatch(item.Key))
				{
					ConvertVariant(query, item.Key, item.Value);
				}
				else if (IsGiftCardKey.IsMatch(item.Key))
				{
					item.Value.Each(value => ConvertGiftCard(query, item.Key, value));
				}
				else if (IsCheckoutAttributeKey.IsMatch(item.Key))
				{
					ConvertCheckoutAttribute(query, item.Key, item.Value);
				}
				else if (IsVariantAliasKey.IsMatch(item.Key))
				{
					ConvertVariantAlias(query, item.Key, item.Value, languageId);
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
