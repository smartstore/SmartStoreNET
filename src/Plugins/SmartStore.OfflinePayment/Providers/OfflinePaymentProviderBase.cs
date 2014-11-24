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

		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = new ProcessPaymentResult();
			result.NewPaymentStatus = PaymentStatus.Pending;
			return result;
		}

	}
}