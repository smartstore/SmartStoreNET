using System;

namespace SmartStore.Services.Catalog.Modelling
{
	public class ProductVariantQueryItem
	{
		public ProductVariantQueryItem(string value)
		{
			Value = value.EmptyNull();
		}

		public static string CreateKey(int productId, int bundleItemId, int attributeId, int variantAttributeId)
		{
			return $"pvari{productId}-{bundleItemId}-{attributeId}-{variantAttributeId}";
		}

		public string Value { get; private set; }

		public int ProductId { get; set; }
		public int BundleItemId { get; set; }
		public int AttributeId { get; set; }
		public int VariantAttributeId { get; set; }
		public DateTime? Date { get; set; }

		public string Alias { get; set; }
		public string ValueAlias { get; set; }

		public override string ToString()
		{
			return CreateKey(ProductId, BundleItemId, AttributeId, VariantAttributeId);
		}
	}
}
