﻿using System.Collections.Generic;
using System.Collections.Specialized;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Orders
{
	public class AddToCartContext
	{
		public AddToCartContext()
		{
			Warnings = new List<string>();
			CustomerEnteredPrice = decimal.Zero;
			ChildItems = new List<ShoppingCartItem>();
		}

		public List<string> Warnings { get; set; }

		public ShoppingCartItem Item { get; set; }
		public List<ShoppingCartItem> ChildItems { get; set; }
		public ProductBundleItem BundleItem { get; set; }

		public Customer Customer { get; set; }
		public Product Product { get; set; }
		public ShoppingCartType CartType { get; set; }
		public NameValueCollection AttributeForm { get; set; }
		public string Attributes { get; set; }
		public decimal CustomerEnteredPrice { get; set; }
		public int Quantity { get; set; }
		public bool AddRequiredProducts { get; set; }
		public int? StoreId { get; set; }

		public int BundleItemId
		{
			get
			{
				return (BundleItem == null ? 0 : BundleItem.Id);
			}
		}
	}
}
