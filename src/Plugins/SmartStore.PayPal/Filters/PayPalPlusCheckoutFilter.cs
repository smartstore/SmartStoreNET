using System;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Filters
{
	public class PayPalPlusCheckoutFilter : IActionFilter
	{
		private readonly ICommonServices _services;
		private readonly IPaymentService _paymentService;
		private readonly Lazy<IGenericAttributeService> _genericAttributeService;
		private readonly Lazy<IOrderTotalCalculationService> _orderTotalCalculationService;

		public PayPalPlusCheckoutFilter(
			ICommonServices services,
			IPaymentService paymentService,
			Lazy<IGenericAttributeService> genericAttributeService,
			Lazy<IOrderTotalCalculationService> orderTotalCalculationService)
		{
			_services = services;
			_paymentService = paymentService;
			_genericAttributeService = genericAttributeService;
			_orderTotalCalculationService = orderTotalCalculationService;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext == null || filterContext.ActionDescriptor == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
				return;

			var store = _services.StoreContext.CurrentStore;
			var customer = _services.WorkContext.CurrentCustomer;

			if (!_paymentService.IsPaymentMethodActive(PayPalPlusProvider.SystemName, store.Id))
				return;

			// skip payment if the cart total is zero. paypal would return an error "Amount cannot be zero".
			var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

			var cartTotal = _orderTotalCalculationService.Value.GetShoppingCartTotal(cart, true);
			if (cartTotal.HasValue && cartTotal.Value == decimal.Zero)
				return;

			_genericAttributeService.Value.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, PayPalPlusProvider.SystemName, store.Id);

			var routeValues = new RouteValueDictionary(new { action = "PaymentWall", controller = "PayPalPlus" });

			filterContext.Result = new RedirectToRouteResult("SmartStore.PayPalPlus", routeValues);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}
	}
}