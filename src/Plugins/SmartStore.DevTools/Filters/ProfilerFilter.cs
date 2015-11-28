using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Logging;
using SmartStore.Core.Localization;
using SmartStore.DevTools.Services;
using SmartStore.Core;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.DevTools.Filters
{
	public class ProfilerFilter : IActionFilter, IResultFilter
	{
		private readonly Lazy<IProfilerService> _profiler;
		private readonly ICommonServices _services;
		private readonly Lazy<IWidgetProvider> _widgetProvider;
		private readonly ProfilerSettings _profilerSettings;

		public ProfilerFilter(
			Lazy<IProfilerService> profiler, 
			ICommonServices services, 
			Lazy<IWidgetProvider> widgetProvider, 
			ProfilerSettings profilerSettings)
		{
			this._profiler = profiler;
			this._services = services;
			this._widgetProvider = widgetProvider;
			this._profilerSettings = profilerSettings;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (!_profilerSettings.EnableMiniProfilerInPublicStore)
				return;
			
			var tokens = filterContext.RouteData.DataTokens;
			string area = tokens.ContainsKey("area") && !string.IsNullOrEmpty(tokens["area"].ToString()) ?
				string.Concat(tokens["area"], ".") :
				string.Empty;
			string controller = string.Concat(filterContext.Controller.ToString().Split('.').Last(), ".");
			string action = filterContext.ActionDescriptor.ActionName;
			this._profiler.Value.StepStart("ActionFilter", "Action: " + area + controller + action);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (!_profilerSettings.EnableMiniProfilerInPublicStore)
				return;

			if (!(filterContext.Result is ViewResultBase))
			{
				this._profiler.Value.StepStop("ActionFilter");
			}
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (!_profilerSettings.EnableMiniProfilerInPublicStore)
				return;
			
			// should only run on a full view rendering result
			var result = filterContext.Result as ViewResultBase;
			if (result == null)
			{
				return;
			}

			if (!this.ShouldProfile(filterContext.HttpContext))
			{
				return;
			}

			var viewName = result.ViewName;
			if (viewName.IsEmpty())
			{
				string action = (filterContext.RouteData.Values["action"] as string).EmptyNull();
				viewName = action;
			}

			this._profiler.Value.StepStart("ResultFilter", string.Format("{0}: {1}", result is PartialViewResult ? "Partial" : "View", viewName));

			if (!filterContext.IsChildAction)
			{
				_widgetProvider.Value.RegisterAction(
					"head_html_tag",
					"MiniProfiler",
					"DevTools",
					new { area = "SmartStore.DevTools" });
			}
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
			if (!_profilerSettings.EnableMiniProfilerInPublicStore)
				return;
			
			// should only run on a full view rendering result
			if (!(filterContext.Result is ViewResultBase))
			{
				return;
			}

			if (!this.ShouldProfile(filterContext.HttpContext))
			{
				return;
			}

			this._profiler.Value.StepStop("ResultFilter");
			this._profiler.Value.StepStop("ActionFilter");
		}

		private bool ShouldProfile(HttpContextBase ctx)
		{
			if (!_services.WorkContext.CurrentCustomer.IsAdmin())
			{
				return ctx.Request.IsLocal;
			}

			return true;
		}

	}
}
