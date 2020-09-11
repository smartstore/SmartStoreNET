using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Logging;

namespace SmartStore.DevTools.Filters
{
    public class SampleCheckoutFilter : IActionFilter
    {
        private readonly INotifier _notifier;

        public SampleCheckoutFilter(INotifier notifier)
        {
            this._notifier = notifier;
        }

        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var routeValues = new { action = "MyBillingAddress" };
            filterContext.Result = new RedirectToRouteResult("Developer.DevTools.MyCheckout", new RouteValueDictionary(routeValues));
        }
    }
}
