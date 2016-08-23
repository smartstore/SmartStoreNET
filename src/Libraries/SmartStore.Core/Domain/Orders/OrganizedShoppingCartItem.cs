using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Core.Domain.Orders
{
	public partial class OrganizedShoppingCartItem
	{
		public OrganizedShoppingCartItem(ShoppingCartItem item)
		{
			Guard.NotNull(item, nameof(item));

			Item = item;	// must not be null
			ChildItems = new List<OrganizedShoppingCartItem>();
			BundleItemData = new ProductBundleItemData(item.BundleItem);
			CustomProperties = new Dictionary<string, object>();
		}

		public ShoppingCartItem Item { get; private set; }
		public IList<OrganizedShoppingCartItem> ChildItems { get; set; }
		public ProductBundleItemData BundleItemData { get; set; }

		/// <summary>
		/// Use this dictionary for any custom data required along cart processing
		/// </summary>
		public Dictionary<string, object> CustomProperties { get; set; }
	}
}
