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
			{
				return;
			}

			var store = _services.StoreContext.CurrentStore;
			var customer = _services.WorkContext.CurrentCustomer;

			var paymentProvider = _paymentService.LoadPaymentMethodBySystemName(PayPalPlusProvider.SystemName, true, store.Id);
			if (paymentProvider == null)
			{
				return;
			}

			var cart = customer.GetCartItems(ShoppingCartType.ShoppingCart, store.Id);

			var filterRequest = new PaymentFilterRequest
			{
				PaymentMethod = paymentProvider,
				Customer = customer,
				StoreId = store.Id,
				Cart = cart
			};

			if (_paymentService.IsPaymentMethodFiltered(filterRequest))
			{
				return;
			}

			// Skip payment if the cart total is zero. PayPal would return an error "Amount cannot be zero".
			decimal? cartTotal = _orderTotalCalculationService.Value.GetShoppingCartTotal(cart, true);
			if (cartTotal.HasValue && cartTotal.Value == decimal.Zero)
			{
				var urlHelper = new UrlHelper(filterContext.HttpContext.Request.RequestContext);
				var url = urlHelper.Action("Confirm", "Checkout", new { area = "" });

				filterContext.Result = new RedirectResult(url, false);
			}
			else
			{
				_genericAttributeService.Value.SaveAttribute(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, PayPalPlusProvider.SystemName, store.Id);

				var routeValues = new RouteValueDictionary(new { action = "PaymentWall", controller = "PayPalPlus" });
				filterContext.Result = new RedirectToRouteResult("SmartStore.PayPalPlus", routeValues);
			}
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
		}
	}
}