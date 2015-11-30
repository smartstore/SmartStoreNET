using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace SmartStore.Core.Domain.Catalog
{
	public partial class ProductBundleItemData
	{
		public ProductBundleItemData(ProductBundleItem item)
		{
			Item = item;	// can be null... test with IsValid
		}

		public ProductBundleItem Item { get; private set; }
		public decimal AdditionalCharge { get; set; }
	}

	public partial class ProductBundleItemOrderData
	{
		public int BundleItemId { get; set; }
		public int ProductId { get; set; }
		public string Sku { get; set; }
		public string ProductName { get; set; }
		public string ProductSeName { get; set; }
		public bool VisibleIndividually { get; set; }
		public int Quantity { get; set; }
		public decimal PriceWithDiscount { get; set; }
		public int DisplayOrder { get; set; }
		public string AttributesXml { get; set; }
		public string AttributesInfo { get; set; }
		public bool PerItemShoppingCart { get; set; }
	}

	public class ProductBundleDataListTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			if (sourceType == typeof(string))
				return true;

			return base.CanConvertFrom(context, sourceType);
		}
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value is string)
			{
				List<ProductBundleItemOrderData> bundleData = null;
				string rawValue = value as string;

				if (rawValue.HasValue())
				{
					try
					{
						using (var reader = new StringReader(rawValue))
						{
							var xml = new XmlSerializer(typeof(List<ProductBundleItemOrderData>));
							bundleData = (List<ProductBundleItemOrderData>)xml.Deserialize(reader);
						}
					}
					catch { }
				}
				return bundleData;
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (destinationType == typeof(string))
			{
				var bundleData = value as List<ProductBundleItemOrderData>;

				if (bundleData == null)
					return "";

				var sb = new StringBuilder();
				using (var writer = new StringWriter(sb))
				{
					var xml = new XmlSerializer(typeof(List<ProductBundleItemOrderData>));
					xml.Serialize(writer, value);
					return sb.ToString();
				}
			}
			return base.ConvertTo(context, culture, value, destinationType);
		}
	}
}
