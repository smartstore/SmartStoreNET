﻿using System;
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
using SmartStore.Services.Common;

namespace SmartStore.DevTools.Filters
{
	public class ProfilerFilter : IActionFilter, IResultFilter
	{
		private readonly ICommonServices _services;
		private readonly Lazy<IWidgetProvider> _widgetProvider;
		private readonly ProfilerSettings _profilerSettings;
		private readonly IMobileDeviceHelper _mobileDeviceHelper;

		public ProfilerFilter(
			ICommonServices services, 
			Lazy<IWidgetProvider> widgetProvider, 
			ProfilerSettings profilerSettings,
			IMobileDeviceHelper mobileDeviceHelper)
		{
			this._services = services;
			this._widgetProvider = widgetProvider;
			this._profilerSettings = profilerSettings;
			this._mobileDeviceHelper = mobileDeviceHelper;
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

			_services.Chronometer.StepStart("ActionFilter", "Action: " + area + controller + action);
		}

		public void OnActionExecuted(ActionExecutedContext filterContext)
		{
			if (!_profilerSettings.EnableMiniProfilerInPublicStore)
				return;

			if (!filterContext.Result.IsHtmlViewResult())
			{
				_services.Chronometer.StepStop("ActionFilter");
			}
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (!_profilerSettings.EnableMiniProfilerInPublicStore)
				return;
			
			// should only run on a full view rendering result
			if (!filterContext.Result.IsHtmlViewResult())
			{
				return;
			}

			if (!this.ShouldProfile(filterContext.HttpContext))
			{
				return;
			}

			var viewResult = filterContext.Result as ViewResultBase;

			string viewName = viewResult?.ViewName;
			if (viewName.IsEmpty())
			{
				string action = (filterContext.RouteData.Values["action"] as string).EmptyNull();
				viewName = action;
			}

			_services.Chronometer.StepStart("ResultFilter", string.Format("{0}: {1}", viewResult is PartialViewResult ? "Partial" : "View", viewName));

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
			if (!filterContext.Result.IsHtmlViewResult())
			{
				return;
			}

			if (!this.ShouldProfile(filterContext.HttpContext))
			{
				return;
			}

			_services.Chronometer.StepStop("ResultFilter");
			_services.Chronometer.StepStop("ActionFilter");
		}

		private bool ShouldProfile(HttpContextBase ctx)
		{
			if (_mobileDeviceHelper.IsMobileDevice())
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
