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
			this._profiler.Value.StepStart("ActionFilter", "Controller: " + area + controller + action);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (!_profilerSettings.EnableMiniProfilerInPublicStore)
				return;

			this._profiler.Value.StepStop("ActionFilter");
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
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

			if (!filterContext.IsChildAction)
			{
				_widgetProvider.Value.RegisterAction(
					"head_html_tag",
					"MiniProfiler",
					"DevTools",
					new { Namespaces = "SmartStore.DevTools.Controllers", area = "SmartStore.DevTools" });
			}

			this._profiler.Value.StepStart("ResultFilter", string.Format("Result: {0}", filterContext.Result));
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
