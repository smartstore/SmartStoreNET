using System.Collections.Generic;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.PayPal.Models
{
    public class PayPalPlusCheckoutModel : ModelBase
    {
        public bool UseSandbox { get; set; }
        public string BillingAddressCountryCode { get; set; }
        public string LanguageCulture { get; set; }
        public string ApprovalUrl { get; set; }
        public string ErrorMessage { get; set; }
        public string PayPalPlusPseudoMessageFlag { get; set; }
        public string FullDescription { get; set; }
        public string PayPalFee { get; set; }
        public string ThirdPartyFees { get; set; }
        public bool HasAnyFees { get; set; }

        public List<ThirdPartyPaymentMethod> ThirdPartyPaymentMethods { get; set; }

        public class ThirdPartyPaymentMethod
        {
            public string SystemName { get; set; }
            public string MethodName { get; set; }
            public string RedirectUrl { get; set; }
            public string ImageUrl { get; set; }
            public string Description { get; set; }
            public string PaymentFee { get; set; }
            public int DisplayOrder { get; set; }
        }
    }
}