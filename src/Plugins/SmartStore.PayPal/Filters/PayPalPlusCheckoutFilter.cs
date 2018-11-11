using System;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Filters
{
	public class PayPalPlusCheckoutFilter : IActionFilter
	{
		private readonly ICommonServices _services;
		private readonly IPaymentService _paymentService;
		private readonly Lazy<IGenericAttributeService> _genericAttributeService;

		public PayPalPlusCheckoutFilter(
			ICommonServices services,
			IPaymentService paymentService,
			Lazy<IGenericAttributeService> genericAttributeService)
		{
			_services = services;
			_paymentService = paymentService;
			_genericAttributeService = genericAttributeService;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext == null || filterContext.ActionDescriptor == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
				return;

			var store = _services.StoreContext.CurrentStore;

			if (!_paymentService.IsPaymentMethodActive(PayPalPlusProvider.SystemName, store.Id))
				return;

			_genericAttributeService.Value.SaveAttribute(_services.WorkContext.CurrentCustomer, SystemCustomerAttributeNames.SelectedPaymentMethod, PayPalPlusProvider.SystemName, store.Id);

			var routeValues = new RouteValueDictionary(new { action = "PaymentWall", controller = "PayPalPlus" });

			filterContext.Result = new RedirectToRouteResult("SmartStore.PayPalPlus", routeValues);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{

		}
	}
}