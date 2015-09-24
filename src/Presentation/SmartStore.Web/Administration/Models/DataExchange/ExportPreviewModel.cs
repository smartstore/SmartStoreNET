using System;
using System.Collections.Generic;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.DataExchange
{
	public class ExportPreviewModel : EntityModelBase
	{
		public string Name { get; set; }
		public string ThumbnailUrl { get; set; }
		public int GridPageSize { get; set; }
		public int TotalRecords { get; set; }
		public ExportEntityType EntityType { get; set; }
		public bool LogFileExists { get; set; }
	}

	public class ExportPreviewProductModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
		public int ProductTypeId { get; set; }
		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.ProductType")]
		public string ProductTypeName { get; set; }
		public string ProductTypeLabelHint { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Sku")]
		public string Sku { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Price")]
		public decimal Price { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.StockQuantity")]
		public int StockQuantity { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.Published")]
		public bool Published { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Products.Fields.AdminComment")]
		public string AdminComment { get; set; }
	}

	public class ExportPreviewOrderModel : EntityModelBase
	{
		public bool HasNewPaymentNotification { get; set; }

		[SmartResourceDisplayName("Admin.Orders.Fields.OrderNumber")]
		public string OrderNumber { get; set; }

		[SmartResourceDisplayName("Admin.Orders.Fields.OrderStatus")]
		public string OrderStatus { get; set; }

		[SmartResourceDisplayName("Admin.Orders.Fields.PaymentStatus")]
		public string PaymentStatus { get; set; }

		[SmartResourceDisplayName("Admin.Orders.Fields.ShippingStatus")]
		public string ShippingStatus { get; set; }

		[SmartResourceDisplayName("Admin.Orders.Fields.CustomerEmail")]
		public string CustomerEmail { get; set; }

		[SmartResourceDisplayName("Admin.Orders.Fields.Store")]
		public string StoreName { get; set; }

		[SmartResourceDisplayName("Admin.Orders.Fields.CreatedOn")]
		public DateTime CreatedOn { get; set; }

		[SmartResourceDisplayName("Admin.Orders.Fields.OrderTotal")]
		public decimal OrderTotal { get; set; }
	}
}