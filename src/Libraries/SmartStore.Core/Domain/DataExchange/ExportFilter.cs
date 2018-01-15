using System;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Core.Domain.DataExchange
{
	[Serializable]
	public class ExportFilter
	{
		/// <summary>
		/// Store identifier; 0 to load all records
		/// </summary>
		public int StoreId { get; set; }

		/// <summary>
		/// Entity created from
		/// </summary>
		public DateTime? CreatedFrom { get; set; }

		/// <summary>
		/// Entity created to
		/// </summary>
		public DateTime? CreatedTo { get; set; }

		#region Product

		/// <summary>
		/// Minimum product identifier
		/// </summary>
		public int? IdMinimum { get; set; }

		/// <summary>
		/// Maximum product identifier
		/// </summary>
		public int? IdMaximum { get; set; }

		/// <summary>
		/// Minimum price
		/// </summary>
		public decimal? PriceMinimum { get; set; }

		/// <summary>
		/// Maximum price
		/// </summary>
		public decimal? PriceMaximum { get; set; }

		/// <summary>
		/// Minimum product availability
		/// </summary>
		public int? AvailabilityMinimum { get; set; }

		/// <summary>
		/// Maximum product availability
		/// </summary>
		public int? AvailabilityMaximum { get; set; }

		/// <summary>
		/// A value indicating whether to load only published or non published products
		/// </summary>
		public bool? IsPublished { get; set; }

		/// <summary>
		/// Category identifiers
		/// </summary>
		public int[] CategoryIds { get; set; }

		/// <summary>
		/// A value indicating whether to load products without any catgory mapping
		/// </summary>
		public bool? WithoutCategories { get; set; }

		/// <summary>
		/// Manufacturer identifier
		/// </summary>
		public int? ManufacturerId { get; set; }

		/// <summary>
		/// A value indicating whether to load products without any manufacturer mapping
		/// </summary>
		public bool? WithoutManufacturers { get; set; }

		/// <summary>
		/// Identifiers of product tag
		/// </summary>
		public int? ProductTagId { get; set; }

		/// <summary>
		/// A value indicating whether to load products that are marked as featured (relates only to categories and manufacturers)
		/// </summary>
		public bool? FeaturedProducts { get; set; }

		/// <summary>
		/// Filter by product type
		/// </summary>
		public ProductType? ProductType { get; set; }

		#endregion

		#region Order

		/// <summary>
		/// Filter by order status
		/// </summary>
		public int[] OrderStatusIds { get; set; }

		/// <summary>
		/// Filter by payment status
		/// </summary>
		public int[] PaymentStatusIds { get; set; }

		/// <summary>
		/// Filter by shipping status
		/// </summary>
		public int[] ShippingStatusIds { get; set; }

		#endregion

		#region Customer

		/// <summary>
		/// Filter by active or inactive customers
		/// </summary>
		public bool? IsActiveCustomer { get; set; }

		/// <summary>
		/// Filter by tax exempt customers
		/// </summary>
		public bool? IsTaxExempt { get; set; }

		/// <summary>
		/// Identifiers of customer roles
		/// </summary>
		public int[] CustomerRoleIds { get; set; }

		/// <summary>
		/// Filter by billing country identifiers
		/// </summary>
		public int[] BillingCountryIds { get; set; }

		/// <summary>
		/// Filter by shipping country identifiers
		/// </summary>
		public int[] ShippingCountryIds { get; set; }

		/// <summary>
		/// Filter by last activity date from
		/// </summary>
		public DateTime? LastActivityFrom { get; set; }

		/// <summary>
		/// Filter by last activity date to
		/// </summary>
		public DateTime? LastActivityTo { get; set; }

		/// <summary>
		/// Filter by at least spent amount
		/// </summary>
		public decimal? HasSpentAtLeastAmount { get; set; }

		/// <summary>
		/// Filter by at least placed orders
		/// </summary>
		public int? HasPlacedAtLeastOrders { get; set; }

		#endregion

		#region Newsletter Subscription

		/// <summary>
		/// Filter by active or inactive subscriber
		/// </summary>
		public bool? IsActiveSubscriber { get; set; }

		#endregion

		#region Shopping Cart

		/// <summary>
		/// Filter by shopping cart type identifier
		/// </summary>
		public int? ShoppingCartTypeId { get; set; }

		#endregion
	}
}
