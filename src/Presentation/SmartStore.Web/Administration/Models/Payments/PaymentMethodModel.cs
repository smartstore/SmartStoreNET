using SmartStore.Core;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Plugins;

namespace SmartStore.Admin.Models.Payments
{
	public class PaymentMethodModel : ProviderModel, IActivatable
    {
        public bool IsActive { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.SupportCapture")]
        public bool SupportCapture { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.SupportPartiallyRefund")]
        public bool SupportPartiallyRefund { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.SupportRefund")]
        public bool SupportRefund { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.SupportVoid")]
        public bool SupportVoid { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.Fields.RecurringPaymentType")]
        public string RecurringPaymentType { get; set; }
    }
}