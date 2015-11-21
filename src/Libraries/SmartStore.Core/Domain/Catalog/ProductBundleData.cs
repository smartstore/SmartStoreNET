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
}
