using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.OfflinePayment
{
    [SystemName("SmartStore.PurchaseOrderNumber")]
    [FriendlyName("Purchase Order Number")]
    [DisplayOrder(10)]
    public class PurchaseOrderNumberProvider : OfflinePaymentProviderBase<PurchaseOrderNumberPaymentSettings>, IConfigurable
    {
        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            var settings = CommonServices.Settings.LoadSetting<ManualPaymentSettings>(processPaymentRequest.StoreId);

            result.AllowStoringCreditCardNumber = true;
            switch (settings.TransactMode)
            {
                case TransactMode.Pending:
                    result.NewPaymentStatus = PaymentStatus.Pending;
                    break;
                case TransactMode.Authorize:
                    result.NewPaymentStatus = PaymentStatus.Authorized;
                    break;
                case TransactMode.Paid:
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    break;
                default:
                    result.AddError(T("Common.Payment.TranactionTypeNotSupported"));
                    return result;
            }

            return result;
        }

        public override bool RequiresInteraction => true;

        protected override string GetActionPrefix()
        {
            return "PurchaseOrderNumber";
        }
    }
}
