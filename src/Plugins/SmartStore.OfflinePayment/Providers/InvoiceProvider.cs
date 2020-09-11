using SmartStore.Core.Plugins;
using SmartStore.OfflinePayment.Settings;

namespace SmartStore.OfflinePayment
{
    [SystemName("Payments.Invoice")]
    [FriendlyName("Invoice")]
    [DisplayOrder(1)]
    public class InvoiceProvider : OfflinePaymentProviderBase<InvoicePaymentSettings>, IConfigurable
    {
        protected override string GetActionPrefix()
        {
            return "Invoice";
        }
    }
}