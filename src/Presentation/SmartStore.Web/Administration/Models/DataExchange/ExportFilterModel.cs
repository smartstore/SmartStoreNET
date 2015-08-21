using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.DataExchange
{
	public abstract class ExportFilterModelBase
	{
		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.StoreId")]
		public int? StoreId { get; set; }
		public List<SelectListItem> AvailableStores { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.CreatedFrom")]
		public DateTime? CreatedFrom { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.CreatedTo")]
		public DateTime? CreatedTo { get; set; }
	}


	public class ExportProductFilterModel : ExportFilterModelBase
	{
		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.IsPublished")]
		public bool? IsPublished { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.ProductType")]
		public ProductType? ProductType { get; set; }
		public List<SelectListItem> AvailableProductTypes { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.IdMinimum")]
		public int? IdMinimum { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.IdMaximum")]
		public int? IdMaximum { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.PriceMinimum")]
		public decimal? PriceMinimum { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.PriceMaximum")]
		public decimal? PriceMaximum { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.AvailabilityMinimum")]
		public int? AvailabilityMinimum { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.AvailabilityMaximum")]
		public int? AvailabilityMaximum { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.CategoryIds")]
		public int[] CategoryIds { get; set; }
		public List<SelectListItem> AvailableCategories { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.WithoutCategories")]
		public bool? WithoutCategories { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.ManufacturerId")]
		public int? ManufacturerId { get; set; }
		public List<SelectListItem> AvailableManufacturers { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.WithoutManufacturers")]
		public bool? WithoutManufacturers { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.ProductTagId")]
		public int? ProductTagId { get; set; }
		public List<SelectListItem> AvailableProductTags { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.FeaturedProducts")]
		public bool? FeaturedProducts { get; set; }
	}


	public class ExportOrderFilterModel : ExportFilterModelBase
	{
		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.OrderStatus")]
		public OrderStatus[] OrderStatus { get; set; }
		public List<SelectListItem> AvailableOrderStates { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.PaymentStatus")]
		public PaymentStatus[] PaymentStatus { get; set; }
		public List<SelectListItem> AvailablePaymentStates { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.ShippingStatus")]
		public ShippingStatus[] ShippingStatus { get; set; }
		public List<SelectListItem> AvailableShippingStates { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Export.Filter.CustomerRoleIds")]
		public int[] CustomerRoleIds { get; set; }
		public List<SelectListItem> AvailableCustomerRoles { get; set; }
	}
}