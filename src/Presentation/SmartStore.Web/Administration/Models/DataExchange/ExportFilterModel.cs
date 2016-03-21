using System;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.DataExchange
{
	public class ExportFilterModel
	{
		#region All entity types

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.StoreId")]
		public int? StoreId { get; set; }
		public List<SelectListItem> AvailableStores { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.CreatedFrom")]
		public DateTime? CreatedFrom { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.CreatedTo")]
		public DateTime? CreatedTo { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.CustomerRoleIds")]
		public int[] CustomerRoleIds { get; set; }
		public List<SelectListItem> AvailableCustomerRoles { get; set; }

		public List<SelectListItem> AvailableCountries { get; set; }

		#endregion

		#region Product

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.IsPublished")]
		public bool? IsPublished { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.ProductType")]
		public ProductType? ProductType { get; set; }
		public List<SelectListItem> AvailableProductTypes { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.IdMinimum")]
		public int? IdMinimum { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.IdMaximum")]
		public int? IdMaximum { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.PriceMinimum")]
		public decimal? PriceMinimum { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.PriceMaximum")]
		public decimal? PriceMaximum { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.AvailabilityMinimum")]
		public int? AvailabilityMinimum { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.AvailabilityMaximum")]
		public int? AvailabilityMaximum { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.CategoryIds")]
		public int[] CategoryIds { get; set; }
		public List<SelectListItem> AvailableCategories { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.WithoutCategories")]
		public bool? WithoutCategories { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.ManufacturerId")]
		public int? ManufacturerId { get; set; }
		public List<SelectListItem> AvailableManufacturers { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.WithoutManufacturers")]
		public bool? WithoutManufacturers { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.ProductTagId")]
		public int? ProductTagId { get; set; }
		public List<SelectListItem> AvailableProductTags { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.FeaturedProducts")]
		public bool? FeaturedProducts { get; set; }

		#endregion

		#region Customer

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.IsActiveCustomer")]
		public bool? IsActiveCustomer { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.IsTaxExempt")]
		public bool? IsTaxExempt { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.BillingCountryIds")]
		public int[] BillingCountryIds { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.ShippingCountryIds")]
		public int[] ShippingCountryIds { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.LastActivityFrom")]
		public DateTime? LastActivityFrom { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.LastActivityTo")]
		public DateTime? LastActivityTo { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.HasSpentAtLeastAmount")]
		public decimal? HasSpentAtLeastAmount { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.HasPlacedAtLeastOrders")]
		public int? HasPlacedAtLeastOrders { get; set; }

		#endregion

		#region Order

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.OrderStatusIds")]
		public int[] OrderStatusIds { get; set; }
		public List<SelectListItem> AvailableOrderStates { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.PaymentStatusIds")]
		public int[] PaymentStatusIds { get; set; }
		public List<SelectListItem> AvailablePaymentStates { get; set; }

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.ShippingStatusIds")]
		public int[] ShippingStatusIds { get; set; }
		public List<SelectListItem> AvailableShippingStates { get; set; }

		#endregion

		#region Newsletter Subscription

		[SmartResourceDisplayName("Admin.DataExchange.Export.Filter.IsActiveSubscriber")]
		public bool? IsActiveSubscriber { get; set; }

		#endregion
	}
}