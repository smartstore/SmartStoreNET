using SmartStore.Core.Plugins;
using SmartStore.OfflinePayment.Settings;

namespace SmartStore.OfflinePayment
{
    [SystemName("Payments.PayInStore")]
    [FriendlyName("Pay In Store")]
    [DisplayOrder(1)]
    public class PayInStoreProvider : OfflinePaymentProviderBase<PayInStorePaymentSettings>, IConfigurable
    {
        protected override string GetActionPrefix()
        {
            return "PayInStore";
        }
    }
}