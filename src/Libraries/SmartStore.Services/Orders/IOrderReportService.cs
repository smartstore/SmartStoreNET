using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Order report service interface
    /// </summary>
    public partial interface IOrderReportService
    {
        /// <summary>
        /// Get order average report
        /// </summary>
		/// <param name="storeId">Store identifier</param>
		/// <param name="orderStatusIds">Filter by order status</param>
		/// <param name="paymentStatusIds">Filter by payment status</param>
		/// <param name="shippingStatusIds">Filter by shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="ignoreCancelledOrders">A value indicating whether to ignore cancelled orders</param>
        /// <returns>Result</returns>
		OrderAverageReportLine GetOrderAverageReportLine(int storeId, int[] orderStatusIds,
			int[] paymentStatusIds, int[] shippingStatusIds, DateTime? startTimeUtc,
			DateTime? endTimeUtc, string billingEmail, bool ignoreCancelledOrders = false);
        
        /// <summary>
        /// Get order average report
        /// </summary>
		/// <param name="storeId">Store identifier</param>
        /// <param name="os">Order status</param>
        /// <returns>Result</returns>
		OrderAverageReportLineSummary OrderAverageReport(int storeId, OrderStatus os);

        /// <summary>
        /// Get best sellers report
        /// </summary>
		/// <param name="storeId">Store identifier</param>
        /// <param name="startTime">Order start time; null to load all</param>
        /// <param name="endTime">Order end time; null to load all</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Shipping status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="recordsToReturn">Records to return</param>
        /// <param name="orderBy">1 - order by quantity, 2 - order by total amount</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Result</returns>
		IList<BestsellersReportLine> BestSellersReport(int storeId,
			DateTime? startTime, DateTime? endTime,
			OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss,
            int billingCountryId = 0, int recordsToReturn = 5,
			int orderBy = 1, bool showHidden = false);
        
        /// <summary>
        /// Gets a list of product identifiers purchased by other customers who purchased the above
        /// </summary>
		/// <param name="storeId">Store identifier</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="recordsToReturn">Records to return</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Product collection</returns>
		int[] GetAlsoPurchasedProductsIds(int storeId, int productId, int recordsToReturn = 5, bool showHidden = false);

        /// <summary>
        /// Gets a list of products that were never sold
        /// </summary>
        /// <param name="startTime">Order start time; null to load all</param>
        /// <param name="endTime">Order end time; null to load all</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Products</returns>
        IPagedList<Product> ProductsNeverSold(DateTime? startTime,
            DateTime? endTime, int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Get profit report
        /// </summary>
		/// <param name="storeId">Store identifier</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
		/// <param name="orderStatusIds">Filter by order status</param>
		/// <param name="paymentStatusIds">Filter by payment status</param>
		/// <param name="shippingStatusIds">Filter by shipping status</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <returns>Result</returns>
		decimal ProfitReport(int storeId, int[] orderStatusIds, int[] paymentStatusIds, int[] shippingStatusIds,
            DateTime? startTimeUtc, DateTime? endTimeUtc, string billingEmail);
    }
}
