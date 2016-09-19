using System;
using System.Web.Mvc;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.DevTools.Filters
{
	public class MachineNameFilter : IResultFilter
	{
		private readonly ICommonServices _services;
		private readonly Lazy<IWidgetProvider> _widgetProvider;
		private readonly ProfilerSettings _profilerSettings;

		public MachineNameFilter(
			ICommonServices services, 
			Lazy<IWidgetProvider> widgetProvider, 
			ProfilerSettings profilerSettings)
		{
			this._services = services;
			this._widgetProvider = widgetProvider;
			this._profilerSettings = profilerSettings;
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (!_profilerSettings.DisplayMachineName)
				return;

			if (filterContext.IsChildAction)
				return;
			
			// should only run on a full view rendering result
			var result = filterContext.Result as ViewResultBase;
			if (result == null)
			{
				return;
			}

			if (!_services.WorkContext.CurrentCustomer.IsAdmin() && !filterContext.HttpContext.Request.IsLocal)
			{
				return;
			}

			_widgetProvider.Value.RegisterAction(
				new[] { "body_end_html_tag_before", "admin_content_after" },
				"MachineName",
				"DevTools",
				new { area = "SmartStore.DevTools" });
		}

		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
		}

	}
}
