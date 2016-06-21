using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using SmartStore.Core.Data;

namespace SmartStore.DevTools.OutputCache
{
	public class DonutHoleFilter : IActionFilter
	{
		private readonly IOutputCacheControlPolicy _policy;

		public DonutHoleFilter()
		{
			_policy = new OutputCacheControlPolicy();
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!filterContext.IsChildAction)
				return;

			if (!filterContext.HttpContext.Items.Contains("OutputCache.IsCachingRequest"))
				return;

			if (filterContext.RouteData.Values.ContainsKey("OutputCache.InvokingChildAction"))
				return;

			if (!DataSettings.DatabaseIsInstalled())
				return;

			var routeKey = CacheHelpers.GetRouteKey(filterContext);

			// Get out if we are unable to generate a route key (Area/Controller/Action)
			if (string.IsNullOrEmpty(routeKey))
				return;

			var cacheableRoute = _policy.GetCacheableRoute(routeKey);

			// do not donut cache when child action route is not in white list
			if (cacheableRoute == null)
				return;

			var data = GetActionData(filterContext);

			filterContext.Result = new ContentResult
			{
				Content = string.Format("<!--Donut#{0}#-->{1}<!--EndDonut-->", JsonConvert.SerializeObject(data), "ReplaceMe")
			};
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			//
		}

		private ChildActionData GetActionData(ActionExecutingContext context)
		{
			return new ChildActionData
			{
				ActionName = context.ActionDescriptor.ActionName,
				ControllerName = context.ActionDescriptor.ControllerDescriptor.ControllerName,
				RouteValues = context.RouteData.Values
			};
		}
	}
}