using SmartStore.Core.Configuration;
using SmartStore.PayPal;

namespace SmartStore.PayPal.Settings
{
	public class PayPalDirectSettings : ISettings
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
	}
}
