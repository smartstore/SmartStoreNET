using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Services.Orders;
using static SmartStore.Services.Customers.CustomerReportService;

namespace SmartStore.Services.Customers
{
    /// <summary>
    /// Customer report service interface.
    /// </summary>
    public partial interface ICustomerReportService
    {
        /// <summary>
        /// Get best customers.
        /// </summary>
        /// <param name="startTime">Order start time. <c>null</c> to load all customers.</param>
        /// <param name="endTime">Order end time. <c>null</c> to load all customers.</param>
        /// <param name="orderStatus">Order status. <c>null</c> to load all customers.</param>
        /// <param name="paymentStatus">Order payment status. <c>null</c> to load all customers.</param>
        /// <param name="shippingStatus">Order shippment status. <c>null</c> to load all customers.</param>
        /// <param name="sorting">Sorting of report items.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <returns>Report</returns>
        IPagedList<TopCustomerReportLine> GetTopCustomersReport(
            DateTime? startTime,
            DateTime? endTime,
            OrderStatus? orderStatus,
            PaymentStatus? paymentStatus,
            ShippingStatus? shippingStatus,
            ReportSorting sorting,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a report of customers registered in the last days
        /// </summary>
        /// <param name="days">Customers registered in the last days</param>
        /// <returns>Number of registered customers</returns>
        int GetRegisteredCustomersReport(int days);

        /// <summary>
        /// Get customers registrations sorted by date
        /// </summary>
        /// <returns>Customer registrations</returns>
        List<RegistredCustomersDate> GetRegisteredCustomersDate();

        /// <summary>
        /// Get customer registration count
        /// </summary>
        /// <param name="startTimeUtc">Start time UTC</param>
        /// <param name="endTimeUtc">End time UTC</param>
        /// <returns>Number of registrations</returns>
        int GetCustomerRegistrations(DateTime? startTimeUtc, DateTime? endTimeUtc);
    }
}