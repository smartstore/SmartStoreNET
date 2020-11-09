using SmartStore.Core.Plugins;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Services;

namespace SmartStore.OfflinePayment
{

    public partial class Plugin : BasePlugin
    {
        private readonly ICommonServices _services;

        public Plugin(ICommonServices services)
        {
            this._services = services;
        }

        public override void Install()
        {
            var settings = _services.Settings;
            var loc = _services.Localization;

            // add settings
            settings.SaveSetting(new CashOnDeliveryPaymentSettings
            {
                DescriptionText = "@Plugins.Payment.CashOnDelivery.PaymentInfoDescription"
            });
            settings.SaveSetting(new InvoicePaymentSettings
            {
                DescriptionText = "@Plugins.Payment.Invoice.PaymentInfoDescription"
            });
            settings.SaveSetting(new PayInStorePaymentSettings
            {
                DescriptionText = "@Plugins.Payment.PayInStore.PaymentInfoDescription"
            });
            settings.SaveSetting(new PrepaymentPaymentSettings
            {
                DescriptionText = "@Plugins.Payment.Prepayment.PaymentInfoDescription"
            });
            settings.SaveSetting(new ManualPaymentSettings
            {
                DescriptionText = "@Plugins.Payments.Manual.PaymentInfoDescription",
                TransactMode = TransactMode.Pending
            });
            settings.SaveSetting(new DirectDebitPaymentSettings
            {
                DescriptionText = "@Plugins.Payments.DirectDebit.PaymentInfoDescription"
            });
            settings.SaveSetting(new PurchaseOrderNumberPaymentSettings
            {
                DescriptionText = "@Plugins.Payments.PurchaseOrderNumber.PaymentInfoDescription",
                TransactMode = TransactMode.Pending
            });

            // add resources
            loc.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        public override void Uninstall()
        {
            var settings = _services.Settings;
            var loc = _services.Localization;

            // delete settings
            settings.DeleteSetting<CashOnDeliveryPaymentSettings>();
            settings.DeleteSetting<InvoicePaymentSettings>();
            settings.DeleteSetting<PayInStorePaymentSettings>();
            settings.DeleteSetting<PrepaymentPaymentSettings>();
            settings.DeleteSetting<ManualPaymentSettings>();
            settings.DeleteSetting<DirectDebitPaymentSettings>();

            // delete resources
            loc.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            loc.DeleteLocaleStringResources("Plugins.Payment.CashOnDelivery");
            loc.DeleteLocaleStringResources("Plugins.Payment.Invoice");
            loc.DeleteLocaleStringResources("Plugins.Payment.PayInStore");
            loc.DeleteLocaleStringResources("Plugins.Payment.Prepayment");
            loc.DeleteLocaleStringResources("Plugins.Payments.Manual");
            loc.DeleteLocaleStringResources("Plugins.Payments.DirectDebit");

            base.Uninstall();
        }
    }
}