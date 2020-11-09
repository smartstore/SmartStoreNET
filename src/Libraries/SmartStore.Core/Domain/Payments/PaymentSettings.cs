using System.Collections.Generic;
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Payments
{
    public class PaymentSettings : ISettings
    {
        public PaymentSettings()
        {
            ActivePaymentMethodSystemNames = new List<string>();
            AllowRePostingPayments = true;
            BypassPaymentMethodSelectionIfOnlyOne = false;
        }

        /// <summary>
        /// Gets or sets a system names of active payment methods
        /// </summary>
        public List<string> ActivePaymentMethodSystemNames { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to repost (complete) payments for redirection payment methods
        /// </summary>
        public bool AllowRePostingPayments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should bypass 'select payment method' page if we have only one payment method
        /// </summary>
        public bool BypassPaymentMethodSelectionIfOnlyOne { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether we should bypass the payment method info page
        /// </summary>
        public bool BypassPaymentMethodInfo { get; set; }

        /// <summary>
        /// Gets or sets the reason for automatic payment capturing
        /// </summary>
        public CapturePaymentReason? CapturePaymentReason { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the currency in which the wallet is kept.
        /// </summary>
        public int WalletCurrencyId { get; set; }
    }
}