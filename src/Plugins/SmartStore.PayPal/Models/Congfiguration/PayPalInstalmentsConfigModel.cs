using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework;

namespace SmartStore.PayPal.Models
{
    public class PayPalInstalmentsConfigModel : ApiConfigurationModel
    {
        public PayPalInstalmentsConfigModel()
        {
            TransactMode = TransactMode.AuthorizeAndCapture;
        }

        [SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.OrderAmountMin")]
        public decimal OrderAmountMin { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.PayPalInstalments.OrderAmountMax")]
        public decimal OrderAmountMax { get; set; }
    }
}