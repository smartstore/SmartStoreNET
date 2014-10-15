using System;
using System.Web.Routing;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Localization;
using SmartStore.OfflinePayment.Controllers;
using SmartStore.Services.Payments;

namespace SmartStore.OfflinePayment
{
	public abstract class OfflinePaymentProviderBase : PaymentMethodBase
    {

		protected OfflinePaymentProviderBase()
		{
			T = NullLocalizer.Instance;
		}

		public Localizer T { get; set; }

		public override Type GetControllerType()
		{
			return typeof(OfflinePaymentController);
		}

		public override PaymentMethodType PaymentMethodType
		{
			get
			{
				return PaymentMethodType.Standard;
			}
		}

		public override bool CanRePostProcessPayment(Order order)
		{
			return false;
		}

		protected abstract string GetActionPrefix();

		public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "{0}Configure".FormatInvariant(GetActionPrefix());
			controllerName = "OfflinePayment";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.OfflinePayment" } };
		}

		public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "{0}PaymentInfo".FormatInvariant(GetActionPrefix());
			controllerName = "OfflinePayment";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.OfflinePayment" } };
		}

		public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
		{
			var result = new CapturePaymentResult();
			result.AddError(T("Common.Payment.NoCaptureSupport"));
			return result;
		}

		public override RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
		{
			var result = new RefundPaymentResult();
			result.AddError(T("Common.Payment.NoRefundSupport"));
			return result;
		}

		public override VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
		{
			var result = new VoidPaymentResult();
			result.AddError(T("Common.Payment.NoVoidSupport"));
			return result;
		}

		public override ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = new ProcessPaymentResult();
			result.AddError(T("Common.Payment.NoRecurringPaymentSupport"));
			return result;
		}

		public override CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
		{
			var result = new CancelRecurringPaymentResult();
			result.AddError(T("Common.Payment.NoRecurringPaymentSupport"));
			return result;
		}

		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = new ProcessPaymentResult();
			result.NewPaymentStatus = PaymentStatus.Pending;
			return result;
		}

	}
}