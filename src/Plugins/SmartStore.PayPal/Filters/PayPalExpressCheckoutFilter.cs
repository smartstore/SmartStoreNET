using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Domain.Customers;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal.Filters
{
    public class PayPalExpressCheckoutFilter : IActionFilter
    {
        private static readonly string[] s_interceptableActions = new string[] { "PaymentMethod" };

        private readonly IGenericAttributeService _genericAttributeService;
        private readonly HttpContextBase _httpContext;
        private readonly ICommonServices _services;

        public PayPalExpressCheckoutFilter(
            IGenericAttributeService genericAttributeService,
            HttpContextBase httpContext,
            ICommonServices services)
        {
            _genericAttributeService = genericAttributeService;
            _httpContext = httpContext;
            _services = services;
        }

        private static bool IsInterceptableAction(string actionName)
        {
            return s_interceptableActions.Contains(actionName, StringComparer.OrdinalIgnoreCase);
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext == null || filterContext.ActionDescriptor == null || filterContext.HttpContext == null || filterContext.HttpContext.Request == null)
                return;

            var attr = Convert.ToBoolean(filterContext.HttpContext.GetCheckoutState().CustomProperties.Get("PayPalExpressButtonUsed"));

            //verify paypalexpressprovider was used
            if (attr == true)
            {
                var store = _services.StoreContext.CurrentStore;
                var customer = _services.WorkContext.CurrentCustomer;

                _genericAttributeService.SaveAttribute<string>(customer, SystemCustomerAttributeNames.SelectedPaymentMethod, PayPalExpressProvider.SystemName, store.Id);

                var paymentRequest = _httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest;
                if (paymentRequest == null)
                {
                    _httpContext.Session["OrderPaymentInfo"] = new ProcessPaymentRequest();
                }

                //delete property for backward navigation
                _httpContext.GetCheckoutState().CustomProperties.Remove("PayPalExpressButtonUsed");

                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "Controller", "Checkout" },
                        { "Action", "Confirm" },
                        { "area", null }
                    });
            }
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
        }
    }
}