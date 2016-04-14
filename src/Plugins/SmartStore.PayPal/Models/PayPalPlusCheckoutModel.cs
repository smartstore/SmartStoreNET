using SmartStore.Web.Framework.Modelling;

namespace SmartStore.PayPal.Models
{
	public class PayPalPlusCheckoutModel : ModelBase
	{
		public bool UseSandbox { get; set; }
		public bool HasPaymentFee { get; set; }
		public string BillingAddressCountryCode { get; set; }
		public string LanguageCulture { get; set; }
		public string ApprovalUrl { get; set; }
	}
}