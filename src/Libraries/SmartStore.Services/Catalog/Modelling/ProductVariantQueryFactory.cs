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

		private void GetVariants(ProductVariantQuery query, NameValueCollection data, string prefix)
		{
			if (data == null || data.Count == 0)
				return;

			var items = data.AllKeys
				.Where(x => x.EmptyNull().StartsWith(prefix))
				.SelectMany(data.GetValues, (k, v) => new { key = k.EmptyNull(), value = v.EmptyNull() });

			foreach (var item in items)
			{
				var ids = item.key.Replace(prefix, "").SplitSafe("-");
				if (ids.Length > 3)
				{
					if (item.key.EndsWith("-day") || item.key.EndsWith("-month"))
						continue;

					var variant = new ProductVariantQueryItem(item.value);
					variant.ProductId = ids[0].ToInt();
					variant.BundleItemId = ids[1].ToInt();
					variant.AttributeId = ids[2].ToInt();
					variant.VariantAttributeId = ids[3].ToInt();

					if (item.key.EndsWith("-year"))
					{
						variant.Year = item.value.ToInt();

						var dateKey = item.key.Replace("-year", "");
						variant.Month = data[dateKey + "-month"].ToInt();
						variant.Day = data[dateKey + "-day"].ToInt();
					}

					// TODO: get/add alias

					query.AddVariant(variant);
				}
			}
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


			return query;
		}
	}
}
