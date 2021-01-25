using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Payments;

namespace SmartStore.Services.Orders
{
    /// <summary>
    /// Order processing service interface
    /// </summary>
    public partial interface IOrderProcessingService
    {
        /// <summary>
        /// Checks order status
        /// </summary>
        /// <param name="order">Order to be validated</param>
        void CheckOrderStatus(Order order);

        /// <summary>
        /// Checks whether orders are placed at too short intervals.
        /// </summary>
        /// <param name="customer">Customer.</param>
        /// <param name="store">Store</param>
        /// <returns><c>true</c> valid interval, <c>false</c> invalid interval.</returns>
        bool IsMinimumOrderPlacementIntervalValid(Customer customer, Store store);

        /// <summary>
        /// Checks whether an order can be placed.
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request.</param>
        /// <returns>List of warning messages. Empty list if an order can be placed.</returns>
        IList<string> GetOrderPlacementWarnings(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Checks whether an order can be placed.
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request.</param>
        /// <param name="initialOrder">Initial order if any (recurring payment).</param>
        /// <param name="customer">Customer placing the order.</param>
        /// <returns>List of warning messages. Empty list if an order can be placed.</returns>
        IList<string> GetOrderPlacementWarnings(
            ProcessPaymentRequest processPaymentRequest,
            Order initialOrder,
            Customer customer,
            out IList<OrganizedShoppingCartItem> cart);

        /// <summary>
        /// Places an order
        /// </summary>
        /// <param name="processPaymentRequest">Process payment request</param>
        /// <returns>Place order result</returns>
        PlaceOrderResult PlaceOrder(ProcessPaymentRequest processPaymentRequest, Dictionary<string, string> extraData);

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        void DeleteOrder(Order order);

        /// <summary>
        /// Process next recurring psayment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        void ProcessNextRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        IList<string> CancelRecurringPayment(RecurringPayment recurringPayment);

        /// <summary>
        /// Gets a value indicating whether a customer can cancel recurring payment
        /// </summary>
        /// <param name="customerToValidate">Customer</param>
        /// <param name="recurringPayment">Recurring Payment</param>
        /// <returns>value indicating whether a customer can cancel recurring payment</returns>
        bool CanCancelRecurringPayment(Customer customerToValidate, RecurringPayment recurringPayment);

        /// <summary>
        /// Send a shipment
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        void Ship(Shipment shipment, bool notifyCustomer);

        /// <summary>
        /// Marks a shipment as delivered
        /// </summary>
        /// <param name="shipment">Shipment</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        void Deliver(Shipment shipment, bool notifyCustomer);

        /// <summary>
        /// Gets a value indicating whether cancel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether cancel is allowed</returns>
        bool CanCancelOrder(Order order);

        /// <summary>
        /// Cancels order
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="notifyCustomer">True to notify customer</param>
        void CancelOrder(Order order, bool notifyCustomer);

        /// <summary>
        /// Auto update order details
        /// </summary>
        /// <param name="context">Context parameters</param>
        void AutoUpdateOrderDetails(AutoUpdateOrderItemContext context);

        /// <summary>
        /// Gets a value indicating whether order can be marked as authorized
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as authorized</returns>
        bool CanMarkOrderAsAuthorized(Order order);

        /// <summary>
        /// Marks order as authorized
        /// </summary>
        /// <param name="order">Order</param>
        void MarkAsAuthorized(Order order);

        /// <summary>
        /// Gets a value indicating whether the order can be marked as completed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether the order can be marked as completed</returns>
        bool CanCompleteOrder(Order order);

        /// <summary>
        /// Marks the order as completed
        /// </summary>
        /// <param name="order">Order</param>
        void CompleteOrder(Order order);

        /// <summary>
        /// Gets a value indicating whether capture from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether capture from admin panel is allowed</returns>
        bool CanCapture(Order order);

        /// <summary>
        /// Capture an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        IList<string> Capture(Order order);

        /// <summary>
        /// Gets a value indicating whether order can be marked as paid
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as paid</returns>
        bool CanMarkOrderAsPaid(Order order);

        /// <summary>
        /// Marks order as paid
        /// </summary>
        /// <param name="order">Order</param>
        void MarkOrderAsPaid(Order order);

        /// <summary>
        /// Gets a value indicating whether refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        bool CanRefund(Order order);

        /// <summary>
        /// Refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        IList<string> Refund(Order order);

        /// <summary>
        /// Gets a value indicating whether order can be marked as refunded
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as refunded</returns>
        bool CanRefundOffline(Order order);

        /// <summary>
        /// Refunds an order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        void RefundOffline(Order order);

        /// <summary>
        /// Gets a value indicating whether partial refund from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether refund from admin panel is allowed</returns>
        bool CanPartiallyRefund(Order order, decimal amountToRefund);

        /// <summary>
        /// Partially refunds an order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A list of errors; empty list if no errors</returns>
        IList<string> PartiallyRefund(Order order, decimal amountToRefund);

        /// <summary>
        /// Gets a value indicating whether order can be marked as partially refunded
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        /// <returns>A value indicating whether order can be marked as partially refunded</returns>
        bool CanPartiallyRefundOffline(Order order, decimal amountToRefund);

        /// <summary>
        /// Partially refunds an order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="amountToRefund">Amount to refund</param>
        void PartiallyRefundOffline(Order order, decimal amountToRefund);

        /// <summary>
        /// Gets a value indicating whether void from admin panel is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether void from admin panel is allowed</returns>
        bool CanVoid(Order order);

        /// <summary>
        /// Voids order (from admin panel)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Voided order</returns>
        IList<string> Void(Order order);

        /// <summary>
        /// Gets a value indicating whether order can be marked as voided
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether order can be marked as voided</returns>
        bool CanVoidOffline(Order order);

        /// <summary>
        /// Voids order (offline)
        /// </summary>
        /// <param name="order">Order</param>
        void VoidOffline(Order order);

        /// <summary>
        /// Place order items in current user shopping cart.
        /// </summary>
        /// <param name="order">The order</param>
        void ReOrder(Order order);

        /// <summary>
        /// Check whether return request is allowed
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        bool IsReturnRequestAllowed(Order order);

        /// <summary>
        /// Valdiate minimum order amount.
        /// Gets min order amount from customer role.
        /// When no min order amount is defined in customer role, default order settings are used as fallback if present.
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - OK; false - minimum order amount is not reached</returns>
        (bool valid, decimal orderTotalMinimum) IsAboveOrderTotalMinimum(IList<OrganizedShoppingCartItem> cart, int[] customerRoleIds);

        /// <summary>
        /// Valdiate maximum order amount.
        /// Gets max order amount from customer role.
        /// When no max order amount is defined in customer role, default order settings are used as fallback if present.
        /// </summary>
        /// <param name="cart">Shopping cart, customer role ids</param>
        /// <returns>true - OK; false - maximum order amount is exceeded</returns>
        (bool valid, decimal orderTotalMaximum) IsBelowOrderTotalMaximum(IList<OrganizedShoppingCartItem> cart, int[] customerRoleIds);

        /// <summary>
        /// Adds a shipment to an order.
        /// </summary>
        /// <param name="order">Order.</param>
        /// <param name="trackingNumber">Tracking number.</param>
        /// <param name="trackingUrl">Tracking URL.</param>
        /// <param name="quantities">Quantities by order item identifiers. <c>null</c> to use the remaining total number of products for each order item.</param>
        /// <returns>New shipment, <c>null</c> if no shipment was added.</returns>
        Shipment AddShipment(
            Order order,
            string trackingNumber,
            string trackingUrl,
            Dictionary<int, int> quantities);
    }
}
