using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;

namespace SmartStore.Services.Catalog.Modelling
{
	public class ProductVariantQueryFactory : IProductVariantQueryFactory
	{
		protected readonly HttpContextBase _httpContext;

		public ProductVariantQueryFactory(HttpContextBase httpContext)
		{
			_httpContext = httpContext;
		}

		private void GetData(NameValueCollection data, string prefix, Action<string, string> process)
		{
			if (data != null && data.Count > 0)
			{
				var items = data.AllKeys
					.Where(x => x.EmptyNull().StartsWith(prefix))
					.SelectMany(data.GetValues, (k, v) => new { key = k.EmptyNull(), value = v.EmptyNull() });

				foreach (var item in items)
				{
					process(item.key, item.value);
				}
			}
		}

		private void GetVariants(ProductVariantQuery query, NameValueCollection data, string prefix)
		{
			GetData(data, prefix, (key, value) =>
			{
				var ids = key.Replace(prefix, "").SplitSafe("-");
				if (ids.Length > 3)
				{
					if (key.EndsWith("-day") || key.EndsWith("-month"))
						return;

					var variant = new ProductVariantQueryItem(value);
					variant.ProductId = ids[0].ToInt();
					variant.BundleItemId = ids[1].ToInt();
					variant.AttributeId = ids[2].ToInt();
					variant.VariantAttributeId = ids[3].ToInt();

					if (key.EndsWith("-year"))
					{
						variant.Year = value.ToInt();

						var dateKey = key.Replace("-year", "");
						variant.Month = data[dateKey + "-month"].ToInt();
						variant.Day = data[dateKey + "-day"].ToInt();
					}

					// TODO: get/add alias

					query.AddVariant(variant);
				}
			});
		}

		private void GetGiftCards(ProductVariantQuery query, NameValueCollection data, string prefix)
		{
			GetData(data, prefix, (key, value) =>
			{
				var elements = key.Replace(prefix, "").SplitSafe("-");
				if (elements.Length > 2)
				{
					var giftCard = new GiftCardQueryItem(elements[2], value);
					giftCard.ProductId = elements[0].ToInt();
					giftCard.BundleItemId = elements[1].ToInt();

					query.AddGiftCard(giftCard);
				}
			});
		}

		private void GetCheckoutAttributes(ProductVariantQuery query, NameValueCollection data, string prefix)
		{
			GetData(data, prefix, (key, value) =>
			{
				var ids = key.Replace(prefix, "").SplitSafe("-");
				if (ids.Length > 0)
				{
					if (key.EndsWith("-day") || key.EndsWith("-month"))
						return;

					var attribute = new CheckoutAttributeQueryItem(ids[0].ToInt(), value);

					if (key.EndsWith("-year"))
					{
						attribute.Year = value.ToInt();

						var dateKey = key.Replace("-year", "");
						attribute.Month = data[dateKey + "-month"].ToInt();
						attribute.Day = data[dateKey + "-day"].ToInt();
					}

					query.AddCheckoutAttribute(attribute);
				}
			});
		}

		public ProductVariantQuery Current { get; private set; }

		public ProductVariantQuery CreateFromQuery()
		{
			var query = new ProductVariantQuery();
			this.Current = query;

			if (_httpContext.Request == null)
				return query;

			GetVariants(query, _httpContext.Request.Form, ProductVariantQueryItem.Prefix);
			GetVariants(query, _httpContext.Request.QueryString, ProductVariantQueryItem.Prefix);

			GetGiftCards(query, _httpContext.Request.Form, GiftCardQueryItem.Prefix);
			GetGiftCards(query, _httpContext.Request.QueryString, GiftCardQueryItem.Prefix);

			GetCheckoutAttributes(query, _httpContext.Request.Form, CheckoutAttributeQueryItem.Prefix);
			GetCheckoutAttributes(query, _httpContext.Request.QueryString, CheckoutAttributeQueryItem.Prefix);

			return query;
		}
	}
}
