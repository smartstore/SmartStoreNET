using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Dashboard;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Services.Orders
{
    public enum ReportSorting
    {
        ByQuantityAsc = 0,
        ByQuantityDesc,
        ByAmountAsc,
        ByAmountDesc
    }

    /// <summary>
    /// Order report service interface
    /// </summary>
    public partial interface IOrderReportService
    {
        /// <summary>
        /// Get order average report.
        /// </summary>
		/// <param name="storeId">Store identifier</param>
		/// <param name="orderStatusIds">Filter by order status</param>
		/// <param name="paymentStatusIds">Filter by payment status</param>
		/// <param name="shippingStatusIds">Filter by shipping status</param>
        /// <param name="startTimeUtc">Start date</param>
        /// <param name="endTimeUtc">End date</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="ignoreCancelledOrders">A value indicating whether to ignore cancelled orders</param>
        /// <returns>Order average report line.</returns>
		OrderAverageReportLine GetOrderAverageReportLine(int storeId,
            int[] orderStatusIds,
            int[] paymentStatusIds,
            int[] shippingStatusIds,
            DateTime? startTimeUtc,
            DateTime? endTimeUtc,
            string billingEmail,
            bool ignoreCancelledOrders = false);

        /// <summary>
        /// Get order average report.
        /// </summary>
        /// <param name="orderQuery">Order queryable.</param>
        /// <returns>Order average report line.</returns>
        OrderAverageReportLine GetOrderAverageReportLine(IQueryable<Order> orderQuery);

        /// <summary>
        /// Get order average report.
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="orderStatus">Order status</param>
        /// <returns>Order average report.</returns>
        OrderAverageReportLineSummary OrderAverageReport(int storeId, OrderStatus orderStatus);

        /// <summary>
        /// Get best sellers report.
        /// </summary>
		/// <param name="storeId">Store identifier</param>
        /// <param name="startTime">Order start time; null to load all</param>
        /// <param name="endTime">Order end time; null to load all</param>
        /// <param name="orderStatus">Order status; null to load all records</param>
        /// <param name="paymentStatus">Order payment status; null to load all records</param>
        /// <param name="shippingStatus">Shipping status; null to load all records</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all records</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="sorting">Sorting of report items.</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Best selling products.</returns>
		IPagedList<BestsellersReportLine> BestSellersReport(
            int storeId,
            DateTime? startTime,
            DateTime? endTime,
            OrderStatus? orderStatus,
            PaymentStatus? paymentStatus,
            ShippingStatus? shippingStatus,
            int billingCountryId = 0,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            ReportSorting sorting = ReportSorting.ByQuantityDesc,
            bool showHidden = false);

        /// <summary>
        /// Gets a the count of purchases for a product
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Purchase count</returns>
        int GetPurchaseCount(int productId);

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
        IPagedList<Product> ProductsNeverSold(DateTime? startTime, DateTime? endTime, int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Get order profit.
        /// </summary>
        /// <param name="orderQuery">Order queryable.</param>
        /// <returns>Order profit.</returns>
        decimal GetProfit(IQueryable<Order> orderQuery);


        /// <summary>
        /// Get paged list of incomplete orders
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="startTimeUtc">Start time limitation</param>
        /// <param name="endTimeUtc">End time limitation</param>
        /// <returns>List of incomplete orders</returns>
        IPagedList<OrderDataPoint> GetIncompleteOrders(int storeId, DateTime? startTimeUtc, DateTime? endTimeUtc);

        /// <summary>
        /// Get paged list of orders as ChartDataPoints
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="startTimeUtc">Start time UTC</param>
        /// <param name="endTimeUtc">End time UTC</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns></returns>
        IPagedList<OrderDataPoint> GetOrdersDashboardData(int storeId, DateTime? startTimeUtc, DateTime? endTimeUtc, int pageIndex, int pageSize);

        /// <summary>
        /// Get orders total
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="startTimeUtc">Start time UTC</param>
        /// <param name="endTimeUtc">End time UTC</param>
        /// <returns></returns>
        decimal GetOrdersTotal(int storeId, DateTime? startTimeUtc, DateTime? endTimeUtc);
    }
}