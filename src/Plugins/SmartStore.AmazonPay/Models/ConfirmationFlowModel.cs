using SmartStore.Web.Framework.Modelling;

namespace SmartStore.AmazonPay.Models
{
    public class ConfirmationFlowModel : ModelBase
    {
        public string WidgetUrl { get; set; }
        public string RedirectUrl { get; set; }

        public string SellerId { get; set; }
        public string OrderReferenceId { get; set; }
        public bool TriggerPostOrderFlow { get; set; }
    }
}