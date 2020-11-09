using SmartStore.Core.Plugins;
using SmartStore.OfflinePayment.Settings;

namespace SmartStore.OfflinePayment
{
    [SystemName("Payments.Prepayment")]
    [FriendlyName("Prepayment")]
    [DisplayOrder(1)]
    public class PrepaymentProvider : OfflinePaymentProviderBase<PrepaymentPaymentSettings>, IConfigurable
    {
        protected override string GetActionPrefix()
        {
            return "Prepayment";
        }
    }
}