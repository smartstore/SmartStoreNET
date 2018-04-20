using System;

namespace SmartStore.Services.Catalog.Modelling
{
	public class ProductVariantQueryItem
	{
		public ProductVariantQueryItem(string value)
		{
			Value = value ?? string.Empty;
		}

		/// <summary>
		/// Key used for form names.
		/// </summary>
		/// <param name="productId">Product identifier</param>
		/// <param name="bundleItemId">Bundle item identifier. 0 if not a bundle item.</param>
		/// <param name="attributeId">Product attribute identifier</param>
		/// <param name="variantAttributeId">Product variant attribute identifier</param>
		/// <returns>Key</returns>
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
		public bool IsFile { get; set; }
		public bool IsText { get; set; }

		public string Alias { get; set; }
		public string ValueAlias { get; set; }

		public override string ToString()
		{
			var key = Alias.HasValue()
				? $"{Alias}-{ProductId}-{BundleItemId}-{VariantAttributeId}"
				: CreateKey(ProductId, BundleItemId, AttributeId, VariantAttributeId);

			if (Date.HasValue)
			{
				return key + "-date";
			}
			else if (IsFile)
			{
				return key + "-file";
			}
			else if (IsText)
			{
				return key + "-text";
			}

			return key;
		}
	}
}
