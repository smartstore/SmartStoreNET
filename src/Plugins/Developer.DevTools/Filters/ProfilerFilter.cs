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
using SmartStore.Plugin.Developer.DevTools.Services;
using SmartStore.Core;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Plugin.Developer.DevTools.Filters
{
	public class ProfilerFilter : IActionFilter, IResultFilter
	{
		private readonly IProfilerService _profiler;
		private readonly ICommonServices _services;
		private readonly IWidgetProvider _widgetProvider;

		public ProfilerFilter(IProfilerService profiler, ICommonServices services, IWidgetProvider widgetProvider)
		{
			this._profiler = profiler;
			this._services = services;
			this._widgetProvider = widgetProvider;
		}

		public void OnActionExecuting(ActionExecutingContext filterContext)
		{
			var tokens = filterContext.RouteData.DataTokens;
			string area = tokens.ContainsKey("area") && !string.IsNullOrEmpty(tokens["area"].ToString()) ?
				string.Concat(tokens["area"], ".") :
				string.Empty;
			string controller = string.Concat(filterContext.Controller.ToString().Split('.').Last(), ".");
			string action = filterContext.ActionDescriptor.ActionName;
			this._profiler.StepStart("ActionFilter", "Controller: " + area + controller + action);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			this._profiler.StepStop("ActionFilter");
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
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
				_widgetProvider.RegisterAction(
					"head_html_tag",
					"MiniProfiler",
					"MyCheckout",
					new { Namespaces = "SmartStore.Plugin.Developer.DevTools.Controllers", area = "Developer.DevTools" });
			}

			this._profiler.StepStart("ResultFilter", string.Format("Result: {0}", filterContext.Result));
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
			// should only run on a full view rendering result
			if (!(filterContext.Result is ViewResultBase))
			{
				return;
			}

			if (!this.ShouldProfile(filterContext.HttpContext))
			{
				return;
			}

			this._profiler.StepStop("ResultFilter");
		}

		private bool ShouldProfile(HttpContextBase ctx)
		{
			if (_services.WorkContext.IsAdmin)
			{
				return false;
			}

			if (!_services.WorkContext.CurrentCustomer.IsAdmin())
			{
				return ctx.Request.IsLocal;
			}

			return true;
		}

	}
}
