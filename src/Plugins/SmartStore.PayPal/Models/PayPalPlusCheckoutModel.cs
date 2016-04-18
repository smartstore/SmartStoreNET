using System.Collections.Generic;
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
		public string ErrorMessage { get; set; }

		public List<ThirdPartyPaymentMethod> ThirdPartyPaymentMethods { get; set; }

		public class ThirdPartyPaymentMethod
		{
			public string RedirectUrl { get; set; }
			public string MethodName { get; set; }
			public string ImageUrl { get; set; }
			public string Description { get; set; }
		}
	}
}