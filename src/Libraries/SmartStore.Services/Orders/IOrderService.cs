using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Order service interface
    /// </summary>
    public partial interface IOrderService
    {
        #region Orders

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>Order</returns>
        Order GetOrderById(int orderId);

        /// <summary>
        /// Gets an order either by it's formatted number or by id
        /// </summary>
        /// <param name="orderNumber">
        /// The order number. If no order can be found, this param gets
        /// converted to <c>int</c> and - if successfull - an order with
        /// an <c>int Id</c> is fetched.
        /// </param>
        /// <returns>Order</returns>
        Order GetOrderByNumber(string orderNumber);

        /// <summary>
        /// Get orders by identifiers
        /// </summary>
        /// <param name="orderIds">Order identifiers</param>
        /// <returns>Order</returns>
        IList<Order> GetOrdersByIds(int[] orderIds);

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderGuid">The order identifier</param>
        /// <returns>Order</returns>
        Order GetOrderByGuid(Guid orderGuid);

		/// <summary>
		/// Get order by payment authorization data
		/// </summary>
		/// <param name="paymentMethodSystemName">System name of the payment method</param>
		/// <param name="authorizationTransactionId">Authorization transaction Id</param>
		/// <returns>Order entity</returns>
		Order GetOrderByPaymentAuthorization(string paymentMethodSystemName, string authorizationTransactionId);

		/// <summary>
		/// Get order by payment capture data
		/// </summary>
		/// <param name="paymentMethodSystemName">System name of the payment method</param>
		/// <param name="captureTransactionId">Capture transaction Id</param>
		/// <returns>Order entity</returns>
		Order GetOrderByPaymentCapture(string paymentMethodSystemName, string captureTransactionId);

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        void DeleteOrder(Order order);

		/// <summary>
		/// Get orders
		/// </summary>
		/// <param name="storeId">Store identifier; null to load all orders</param>
		/// <param name="customerId">Customer identifier; null to load all orders</param>
		/// <param name="startTime">Order start time; null to load all orders</param>
		/// <param name="endTime">Order end time; null to load all orders</param>
		/// <param name="orderStatusIds">Filter by order status</param>
		/// <param name="paymentStatusIds">Filter by payment status</param>
		/// <param name="shippingStatusIds">Filter by shipping status</param>
		/// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
		/// <param name="orderNumber">Filter by order number</param>
		/// <param name="billingName">Billing name. Leave empty to load all records.</param>
		/// <returns>Order query</returns>
		IQueryable<Order> GetOrders(
			int storeId,
			int customerId,
			DateTime? startTime,
			DateTime? endTime,
			int[] orderStatusIds,
			int[] paymentStatusIds,
			int[] shippingStatusIds,
			string billingEmail,
			string orderNumber,
			string billingName = null);

        /// <summary>
        /// Search orders
        /// </summary>
		/// <param name="storeId">Store identifier; null to load all orders</param>
		/// <param name="customerId">Customer identifier; null to load all orders</param>
        /// <param name="startTime">Order start time; null to load all orders</param>
        /// <param name="endTime">Order end time; null to load all orders</param>
		/// <param name="orderStatusIds">Filter by order status</param>
		/// <param name="paymentStatusIds">Filter by payment status</param>
		/// <param name="shippingStatusIds">Filter by shipping status</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="orderGuid">Search by order GUID (Global unique identifier) or part of GUID. Leave empty to load all records.</param>
		/// <param name="orderNumber">Filter by order number</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
		/// <param name="billingName">Billing name. Leave empty to load all records.</param>
        /// <returns>Order collection</returns>
		IPagedList<Order> SearchOrders(int storeId, int customerId, DateTime? startTime, DateTime? endTime,
			int[] orderStatusIds, int[] paymentStatusIds, int[] shippingStatusIds,
			string billingEmail, string orderGuid, string orderNumber, int pageIndex, int pageSize, string billingName = null);

        /// <summary>
        /// Gets all orders by affiliate identifier
        /// </summary>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Orders</returns>
        IPagedList<Order> GetAllOrders(int affiliateId, int pageIndex, int pageSize);

        /// <summary>
        /// Load all orders
        /// </summary>
        /// <returns>Order collection</returns>
        IList<Order> LoadAllOrders();

        /// <summary>
        /// Gets all orders by affiliate identifier
        /// </summary>
        /// <param name="affiliateId">Affiliate identifier</param>
        /// <returns>Order collection</returns>
        IList<Order> GetOrdersByAffiliateId(int affiliateId);

        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        void InsertOrder(Order order);

        /// <summary>
        /// Updates the order
        /// </summary>
        /// <param name="order">The order</param>
        void UpdateOrder(Order order);

        /// <summary>
        /// Deletes an order note
        /// </summary>
        /// <param name="orderNote">The order note</param>
        void DeleteOrderNote(OrderNote orderNote);

        /// <summary>
        /// Get an order by authorization transaction ID and payment method system name
        /// </summary>
        /// <param name="authorizationTransactionId">Authorization transaction ID</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Order</returns>
        Order GetOrderByAuthorizationTransactionIdAndPaymentMethod(string authorizationTransactionId, string paymentMethodSystemName);

		/// <summary>
		/// Shortcut to add an order
		/// </summary>
		/// <param name="order">Order</param>
		/// <param name="note">Order note</param>
		/// <param name="displayToCustomer">Whether to display the note to the customer</param>
		void AddOrderNote(Order order, string note, bool displayToCustomer = false);

		#endregion

		#region Orders items

		/// <summary>
		/// Gets an order item
		/// </summary>
		/// <param name="orderItemId">Order item identifier</param>
		/// <returns>Order item</returns>
		OrderItem GetOrderItemById(int orderItemId);

        /// <summary>
        /// Gets an order item
        /// </summary>
        /// <param name="orderItemGuid">Order item identifier</param>
        /// <returns>Order item</returns>
        OrderItem GetOrderItemByGuid(Guid orderItemGuid);

        /// <summary>
        /// Gets all order items
        /// </summary>
        /// <param name="orderId">Order identifier; null to load all records</param>
        /// <param name="customerId">Customer identifier; null to load all records</param>
        /// <param name="startTime">Order start time; null to load all records</param>
        /// <param name="endTime">Order end time; null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Order shippment status; null to load all records</param>
        /// <param name="loadDownloableProductsOnly">Value indicating whether to load downloadable products only</param>
        /// <returns>Order collection</returns>
        IList<OrderItem> GetAllOrderItems(int? orderId,
           int? customerId, DateTime? startTime, DateTime? endTime,
           OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss,
           bool loadDownloableProductsOnly = false);

		/// <summary>
		/// Get order items by order identifiers
		/// </summary>
		/// <param name="orderIds">Order identifiers</param>
		/// <returns>Order items</returns>
		Multimap<int, OrderItem> GetOrderItemsByOrderIds(int[] orderIds);

        /// <summary>
        /// Delete an order item
        /// </summary>
        /// <param name="orderItem">The order item</param>
        void DeleteOrderItem(OrderItem orderItem);

        #endregion

        #region Recurring payments

        /// <summary>
        /// Deletes a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        void DeleteRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Gets a recurring payment
        /// </summary>
        /// <param name="recurringPaymentId">The recurring payment identifier</param>
        /// <returns>Recurring payment</returns>
        RecurringPayment GetRecurringPaymentById(int recurringPaymentId);

        /// <summary>
        /// Inserts a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        void InsertRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Updates the recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        void UpdateRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Search recurring payments
        /// </summary>
        /// <param name="customerId">The customer identifier; 0 to load all records</param>
		/// <param name="storeId">The store identifier; 0 to load all records</param>
        /// <param name="initialOrderId">The initial order identifier; 0 to load all records</param>
        /// <param name="initialOrderStatus">Initial order status identifier; null to load all records</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Recurring payment collection</returns>
		IList<RecurringPayment> SearchRecurringPayments(int storeId, 
			int customerId, int initialOrderId, OrderStatus? initialOrderStatus, bool showHidden = false);

        #endregion

        #region Return requests

        /// <summary>
        /// Deletes a return request
        /// </summary>
        /// <param name="returnRequest">Return request</param>
        void DeleteReturnRequest(ReturnRequest returnRequest);

        /// <summary>
        /// Gets a return request
        /// </summary>
        /// <param name="returnRequestId">Return request identifier</param>
        /// <returns>Return request</returns>
        ReturnRequest GetReturnRequestById(int returnRequestId);
        
        /// <summary>
        /// Search return requests
        /// </summary>
		/// <param name="storeId">Store identifier; 0 to load all entries</param>
        /// <param name="customerId">Customer identifier; null to load all entries</param>
        /// <param name="orderItemId">Order item identifier; null to load all entries</param>
        /// <param name="rs">Return request status; null to load all entries</param>
		/// <param name="pageIndex">Page index</param>
		/// <param name="pageSize">Page size</param>
		/// <param name="id">Return Request Id</param>
        /// <returns>Return requests</returns>
		IPagedList<ReturnRequest> SearchReturnRequests(int storeId, int customerId, int orderItemId, ReturnRequestStatus? rs, int pageIndex, int pageSize, int id = 0);
        
        #endregion
    }
}
