using System;

namespace SmartStore.Services.Catalog.Modelling
{
	public class ProductVariantQueryItem
	{
		public ProductVariantQueryItem(string value)
		{
			Value = value.EmptyNull();
		}

		public static string Prefix
		{
			get
			{
				return "pvari";
			}
		}

		public static string CreateKey(int productId, int bundleItemId, int attributeId, int variantAttributeId)
		{
			return $"{Prefix}{productId}-{bundleItemId}-{attributeId}-{variantAttributeId}";
		}

		public string Value { get; private set; }

		public int ProductId { get; set; }
		public int BundleItemId { get; set; }
		public int AttributeId { get; set; }
		public int VariantAttributeId { get; set; }

		public int VariantOptionId
		{
			get
			{
				return Value.ToInt();
			}
		}

		public int Year { get; set; }
		public int Month { get; set; }
		public int Day { get; set; }
		public DateTime? Date
		{
			get
			{
				if (Year > 0 && Month > 0 && Day > 0)
				{
					try
					{
						return new DateTime(Year, Month, Day);
					}
					catch { }
				}

				return null;
			}
		}

		public override string ToString()
		{
			return CreateKey(ProductId, BundleItemId, AttributeId, VariantAttributeId);
		}
	}
}
