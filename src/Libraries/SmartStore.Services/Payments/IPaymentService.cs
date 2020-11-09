using System.Collections.Generic;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Payments
{
    public partial interface IPaymentService
    {
        /// <summary>
        /// Checks whether a payment method is active for a shop.
        /// </summary>
        bool IsPaymentMethodActive(string systemName, int storeId = 0);

        /// <summary>
        /// Checks whether a payment method is active, not filtered out and match applied rule sets.
        /// A payment method that meets these requirements appears in the checkout.
        /// </summary>
        bool IsPaymentMethodActive(
            string systemName,
            Customer customer = null,
            IList<OrganizedShoppingCartItem> cart = null,
            int storeId = 0);

        /// <summary>
        /// Loads payment methods that are active, not filtered out and match applied rule sets.
        /// </summary>
        /// <param name="customer">Filter payment methods by customer. <c>null</c> to load all.</param>
        /// <param name="cart">Filter payment methods by cart. <c>null</c> to load all.</param>
        /// <param name="storeId">Filter payment methods by store identifier. 0 to load all.</param>
        /// <param name="types">Filter payment methods by payment method types.</param>
        /// <param name="provideFallbackMethod">Provide a fallback payment method if there is no match.</param>
        /// <returns>Filtered payment methods.</returns>
        IEnumerable<Provider<IPaymentMethod>> LoadActivePaymentMethods(
            Customer customer = null,
            IList<OrganizedShoppingCartItem> cart = null,
            int storeId = 0,
            PaymentMethodType[] types = null,
            bool provideFallbackMethod = true);

        /// <summary>
        /// Load payment provider by system name
        /// </summary>
        /// <param name="systemName">System name</param>
        /// <param name="onlyWhenActive"><c>true</c> to load only active provider</param>
        /// <param name="storeId">Load records allowed only in specified store; pass 0 to load all records</param>
        /// <returns>Found payment provider</returns>
        Provider<IPaymentMethod> LoadPaymentMethodBySystemName(string systemName, bool onlyWhenActive = false, int storeId = 0);

        /// <summary>
        /// Load all payment providers
        /// </summary>
		/// <param name="storeId">Load records allowed only in specified store; pass 0 to load all records</param>
        /// <returns>Payment providers</returns>
		IEnumerable<Provider<IPaymentMethod>> LoadAllPaymentMethods(int storeId = 0);

        /// <summary>
        /// Gets all payment methods.
        /// </summary>
        /// <param name="storeId">Load records allowed only in specified store; pass 0 to load all records.</param>
        /// <returns>Dictionary of payment methods. Key is the payment method system name.</returns>
        IDictionary<string, PaymentMethod> GetAllPaymentMethods(int storeId = 0);

        /// <summary>
        /// Gets payment method extra data by system name
        /// </summary>
        /// <param name="systemName">Provider system name</param>
        /// <returns>Payment method entity</returns>
        PaymentMethod GetPaymentMethodBySystemName(string systemName);

        /// <summary>
        /// Insert payment method extra data
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        void InsertPaymentMethod(PaymentMethod paymentMethod);

        /// <summary>
        /// Updates payment method extra data
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        void UpdatePaymentMethod(PaymentMethod paymentMethod);

        /// <summary>
        /// Delete payment method extra data
        /// </summary>
        /// <param name="paymentMethod">Payment method</param>
        void DeletePaymentMethod(PaymentMethod paymentMethod);


        /// <summary>
        /// Pre process a payment
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
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest);

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        bool CanRePostProcessPayment(Order order);


        /// <summary>
        /// Gets an additional handling fee of a payment method
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Additional handling fee</returns>
		decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart, string paymentMethodSystemName);



        /// <summary>
        /// Gets a value indicating whether capture is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether capture is supported</returns>
        bool SupportCapture(string paymentMethodSystemName);

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest);



        /// <summary>
        /// Gets a value indicating whether partial refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether partial refund is supported</returns>
        bool SupportPartiallyRefund(string paymentMethodSystemName);

        /// <summary>
        /// Gets a value indicating whether refund is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether refund is supported</returns>
        bool SupportRefund(string paymentMethodSystemName);

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest);



        /// <summary>
        /// Gets a value indicating whether void is supported by payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A value indicating whether void is supported</returns>
        bool SupportVoid(string paymentMethodSystemName);

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest);



        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A recurring payment type of payment method</returns>
        RecurringPaymentType GetRecurringPaymentType(string paymentMethodSystemName);

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
        /// Gets a payment method type
        /// </summary>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>A payment method type</returns>
        PaymentMethodType GetPaymentMethodType(string paymentMethodSystemName);

        /// <summary>
        /// Gets masked credit card number
        /// </summary>
        /// <param name="creditCardNumber">Credit card number</param>
        /// <returns>Masked credit card number</returns>
        string GetMaskedCreditCardNumber(string creditCardNumber);

        /// <summary>
        /// Gets all payment filters
        /// </summary>
        /// <returns>List of payment filters</returns>
        IList<IPaymentMethodFilter> GetAllPaymentMethodFilters();
    }
}
