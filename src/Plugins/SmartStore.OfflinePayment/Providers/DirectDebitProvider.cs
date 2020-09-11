using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.OfflinePayment
{
    [SystemName("Payments.DirectDebit")]
    [FriendlyName("Direct Debit")]
    [DisplayOrder(1)]
    public class DirectDebitProvider : OfflinePaymentProviderBase<DirectDebitPaymentSettings>, IConfigurable
    {
        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AllowStoringDirectDebit = true;
            result.NewPaymentStatus = PaymentStatus.Pending;
            return result;
        }

        protected override string GetActionPrefix()
        {
            return "DirectDebit";
        }

        public override bool RequiresInteraction => true;
    }
}