using System;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

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
		public bool UsernamesEnabled { get; set; }
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

	public class ExportPreviewCategoryModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.Catalog.Products.Categories.Fields.Category")]
		public string Breadcrumb { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Categories.Fields.FullName")]
		public string FullName { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Alias")]
		public string Alias { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Categories.Fields.Published")]
		public bool Published { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Categories.Fields.DisplayOrder")]
		public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
	}

	public class ExportPreviewManufacturerModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Name")]
		public string Name { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.Published")]
		public bool Published { get; set; }

		[SmartResourceDisplayName("Admin.Catalog.Manufacturers.Fields.DisplayOrder")]
		public int DisplayOrder { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store.LimitedTo")]
		public bool LimitedToStores { get; set; }
	}

	public class ExportPreviewCustomerModel : EntityModelBase
	{
		public bool UsernamesEnabled { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.Username")]
		public string Username { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.FullName")]
		public string FullName { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.Email")]
		public string Email { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.CustomerRoles")]
		public string CustomerRoleNames { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.Active")]
		public bool Active { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.CreatedOn")]
		public DateTime CreatedOn { get; set; }

		[SmartResourceDisplayName("Admin.Customers.Customers.Fields.LastActivityDate")]
		public DateTime LastActivityDate { get; set; }
	}

	public class ExportPreviewNewsLetterSubscriptionModel : EntityModelBase
	{
		[SmartResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Email")]
		public string Email { get; set; }

		[SmartResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.Active")]
		public bool Active { get; set; }

		[SmartResourceDisplayName("Admin.Promotions.NewsLetterSubscriptions.Fields.CreatedOn")]
		public DateTime CreatedOn { get; set; }

		[SmartResourceDisplayName("Admin.Common.Store")]
		public string StoreName { get; set; }
	}
}