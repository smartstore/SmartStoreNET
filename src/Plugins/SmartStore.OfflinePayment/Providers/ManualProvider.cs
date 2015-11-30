using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.OfflinePayment.Settings;
using SmartStore.Services.Payments;

namespace SmartStore.OfflinePayment
{
	[SystemName("Payments.Manual")]
	[FriendlyName("Credit Card (manual)")]
	[DisplayOrder(1)]
	public class ManualProvider : OfflinePaymentProviderBase<ManualPaymentSettings>, IConfigurable
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
				case TransactMode.AuthorizeAndCapture:
					result.NewPaymentStatus = PaymentStatus.Paid;
					break;
				default:
					{
						result.AddError(T("Common.Payment.TranactionTypeNotSupported"));
						return result;
					}
			}

			return result;
		}

		public override ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
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
				case TransactMode.AuthorizeAndCapture:
					result.NewPaymentStatus = PaymentStatus.Paid;
					break;
				default:
					{
						result.AddError(T("Common.Payment.TranactionTypeNotSupported"));
						return result;
					}
			}

			return result;
		}

		public override CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
		{
			return new CancelRecurringPaymentResult();
		}

		public override RecurringPaymentType RecurringPaymentType
		{
			get
			{
				return RecurringPaymentType.Manual;
			}
		}

		protected override string GetActionPrefix()
		{
			return "Manual";
		}

		public override bool RequiresInteraction
		{
			get
			{
				return true;
			}
		}
	}
}