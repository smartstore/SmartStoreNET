using SmartStore.Core.Configuration;
using SmartStore.PayPal;

namespace SmartStore.PayPal.Settings
{
    public abstract class PayPalSettingsBase
    {
        public bool UseSandbox { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
        /// </summary>
        public bool AdditionalFeePercentage { get; set; }
        /// <summary>
        /// Additional fee
        /// </summary>
        public decimal AdditionalFee { get; set; }
    }

    public abstract class PayPalApiSettingsBase : PayPalSettingsBase
	{
		public TransactMode TransactMode { get; set; }
		public string ApiAccountName { get; set; }
		public string ApiAccountPassword { get; set; }
		public string Signature { get; set; }
	}

    public class PayPalDirectPaymentSettings : PayPalApiSettingsBase, ISettings
    {
		public PayPalDirectPaymentSettings()
		{
			TransactMode = TransactMode.Authorize;
            UseSandbox = true;
		}
    }

    public class PayPalExpressPaymentSettings : PayPalApiSettingsBase, ISettings 
    {
		public PayPalExpressPaymentSettings()
		{
			UseSandbox = true;
            TransactMode = TransactMode.Authorize;
		}

        /// <summary>
        /// Determines whether the checkout button is displayed beneath the cart
        /// </summary>
        public bool DisplayCheckoutButton { get; set; }

        /// <summary>
        /// Determines whether the shipment address has  to be confirmed by PayPal 
        /// </summary>
        public bool ConfirmedShipment { get; set; }

        /// <summary>
        /// Determines whether the shipment address is transmitted to PayPal
        /// </summary>
        public bool NoShipmentAddress { get; set; }

        /// <summary>
        /// Callback timeout
        /// </summary>
        public int CallbackTimeout { get; set; }

        /// <summary>
        /// Default shipping price
        /// </summary>
        public decimal DefaultShippingPrice { get; set; }
    }

    public class PayPalStandardPaymentSettings : PayPalSettingsBase, ISettings
    {
		public PayPalStandardPaymentSettings()
		{
			UseSandbox = true;
            PdtValidateOrderTotal = true;
            EnableIpn = true;
		}

        public string BusinessEmail { get; set; }
        public string PdtToken { get; set; }
        public bool PassProductNamesAndTotals { get; set; }
        public bool PdtValidateOrderTotal { get; set; }
        public bool EnableIpn { get; set; }
        public string IpnUrl { get; set; }
    }

    /// <summary>
    /// Represents payment processor transaction mode
    /// </summary>
    public enum TransactMode : int
    {
        /// <summary>
        /// Authorize
        /// </summary>
        Authorize = 1,
        /// <summary>
        /// Authorize and capture
        /// </summary>
        AuthorizeAndCapture = 2
    }
}
