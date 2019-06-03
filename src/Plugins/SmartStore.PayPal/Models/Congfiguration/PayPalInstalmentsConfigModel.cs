using SmartStore.PayPal.Settings;

namespace SmartStore.PayPal.Models
{
    public class PayPalInstalmentsConfigModel : ApiConfigurationModel
    {
        public PayPalInstalmentsConfigModel()
        {
            TransactMode = TransactMode.AuthorizeAndCapture;
        }

    }
}