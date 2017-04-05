using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmartStore.Services.Catalog.Modelling
{
	public class ProductVariantQuery
	{
		private readonly List<ProductVariantQueryItem> _variants;

		public ProductVariantQuery()
		{
			_variants = new List<ProductVariantQueryItem>();
		}

		public IReadOnlyList<ProductVariantQueryItem> Variants
		{
			get
			{
				return _variants;
			}
		}

		public ProductVariantQueryItem GetVariant(int productId, int bundleItemId, int attributeId, int variantAttributeId)
		{
			return _variants.FirstOrDefault(x => 
				x.ProductId == productId && 
				x.BundleItemId == bundleItemId && 
				x.AttributeId == attributeId && 
				x.VariantAttributeId == variantAttributeId);
		}

		public void AddVariant(ProductVariantQueryItem item)
		{
			_variants.Add(item);
		}

		public override string ToString()
		{
			return string.Join("&", Variants.Select(x => string.Concat(x.ToString(), "=", HttpUtility.UrlEncode(x.Value))));
		}
	}
}
