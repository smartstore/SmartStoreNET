using System;
using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Payments
{
    /// <summary>
    /// Provides an interface for creating payment gateways and methods
    /// </summary>
    public partial interface IPaymentMethod : IProvider, IUserEditable
    {
        #region Methods

		/// <summary>
		/// Pre process payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Pre process payment result</returns>
		PreProcessPaymentResult PreProcessPayment(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
		/// Post process payment (e.g. used by payment gateways to redirect to a third-party URL).
		/// Called after an order has been placed or when customer re-post the payment.
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest);

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
		decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart);

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest);

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest);

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest);

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest);

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest);

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        bool CanRePostProcessPayment(Order order);

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);

        Type GetControllerType();

        #endregion

        #region Properties

		/// <summary>
		/// Gets a value indicating whether the payment method is active and should be offered to customers
		/// </summary>
		bool IsActive { get; }

		/// <summary>
		/// Gets a value indicating whether the payment method requires user input
		/// before proceeding (e.g. CreditCard, DirectDebit etc.)
		/// </summary>
		bool RequiresInteraction { get; }

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        bool SupportCapture { get; }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        bool SupportPartiallyRefund { get; }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        bool SupportRefund { get; }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        bool SupportVoid { get; }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        RecurringPaymentType RecurringPaymentType { get; }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        PaymentMethodType PaymentMethodType { get; }

        #endregion
    }
}
