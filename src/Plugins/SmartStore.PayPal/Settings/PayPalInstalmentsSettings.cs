using SmartStore.Core.Configuration;

namespace SmartStore.PayPal.Settings
{
    public class PayPalInstalmentsSettings : PayPalApiSettingsBase, ISettings
    {
        public PayPalInstalmentsSettings()
        {
            TransactMode = TransactMode.Authorize;
            OrderAmountMin = 99.0M;
            OrderAmountMax = 5000.0M;
        }

        public decimal OrderAmountMin { get; set; }
        public decimal OrderAmountMax { get; set; }
    }
}