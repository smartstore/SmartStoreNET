using System;
using System.Collections.Generic;

namespace SmartStore.Services.Orders
{
    [Serializable]
    public partial class CheckoutState
    {
        public CheckoutState()
        {
            CustomProperties = new Dictionary<string, object>();
            PaymentData = new Dictionary<string, object>();
        }

        public static string CheckoutStateSessionKey => "SmCheckoutState";

        /// <summary>
        /// The payment summary as displayed on the checkout confirmation page
        /// </summary>
        public string PaymentSummary { get; set; }

        /// <summary>
        /// Indicates whether the payment method selection page was skipped
        /// </summary>
        public bool IsPaymentSelectionSkipped { get; set; }

        /// <summary>
        /// Use this dictionary for any custom data required along checkout flow
        /// </summary>
        public IDictionary<string, object> CustomProperties { get; set; }

        /// <summary>
        /// Payment data entered on payment method selection page
        /// </summary>
        public IDictionary<string, object> PaymentData { get; set; }
    }
}
