﻿using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.AmazonPay.Extensions;
using SmartStore.AmazonPay.Services;

namespace SmartStore.AmazonPay.Filters
{
	public class AmazonPayCheckoutFilter : IActionFilter
	{
		private static readonly string[] s_interceptableActions = new string[] { "BillingAddress", "ShippingAddress", "ShippingMethod", "PaymentMethod" };
		private readonly Lazy<IAmazonPayService> _apiService;

		public AmazonPayCheckoutFilter(Lazy<IAmazonPayService> apiService)
		{
			_apiService = apiService;
		}

		private static bool IsInterceptableAction(string actionName)
		{
			return s_interceptableActions.Contains(actionName, StringComparer.OrdinalIgnoreCase);
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext == null || filterContext.ActionDescriptor == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
				return;

			if (!filterContext.HttpContext.HasAmazonPayState())
				return;

			string actionName = filterContext.ActionDescriptor.ActionName;

			if (!IsInterceptableAction(actionName))
				return;

			if (actionName.IsCaseInsensitiveEqual("ShippingMethod"))
			{
				var model = _apiService.Value.ProcessPluginRequest(AmazonPayRequestType.ShippingMethod, filterContext.Controller.TempData);

				if (model.Result == AmazonPayResultType.Redirect) // shipping to selected address not possible
				{
					var urlHelper = new UrlHelper(filterContext.HttpContext.Request.RequestContext);
					var url = urlHelper.Action("ShippingAddress", "Checkout", new { area = "" });

					filterContext.Result = new RedirectResult(url, false);
				}
			}
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (filterContext == null || filterContext.ActionDescriptor == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
				return;

			if (!filterContext.HttpContext.HasAmazonPayState())
				return;

			string actionName = filterContext.ActionDescriptor.ActionName;

			if (!IsInterceptableAction(actionName))
				return;

			if (actionName.IsCaseInsensitiveEqual("ShippingMethod"))
				return;

			var routeValues = new RouteValueDictionary(new { action = actionName, controller = "AmazonPayCheckout" });

			filterContext.Result = new RedirectToRouteResult("SmartStore.AmazonPay", routeValues);
		}
	}
}