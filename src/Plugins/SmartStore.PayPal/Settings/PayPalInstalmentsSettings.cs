using SmartStore.Core.Configuration;

namespace SmartStore.PayPal.Settings
{
    public class PayPalInstalmentsSettings : PayPalApiSettingsBase, ISettings
    {
        public PayPalInstalmentsSettings()
        {
            TransactMode = TransactMode.Authorize;
        }
    }
}