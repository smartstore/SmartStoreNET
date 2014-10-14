using SmartStore.Core.Configuration;

namespace SmartStore.PayPal.Settings
{
	public class PayPalExpressSettings : ISettings
	{
        public TransactMode TransactMode { get; set; }
		public bool UseSandbox { get; set; }
		public string ApiAccountName { get; set; }
		public string ApiAccountPassword { get; set; }
		public string Signature { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to "additional fee" is specified as percentage. true - percentage, false - fixed value.
		/// </summary>
		public bool AdditionalFeePercentage { get; set; }

		/// <summary>
		/// Additional fee
		/// </summary>
		public decimal AdditionalFee { get; set; }

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
}
