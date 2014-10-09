using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using System.Web.Mvc;
using SmartStore.Core.Logging;
using SmartStore.Core.Localization;
using SmartStore.Utilities;

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
