using System;
using System.Web.Mvc;
using SmartStore.Core.Themes;
using SmartStore.Services;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Admin.Infrastructure
{	
	public class PreviewModeFilter : IResultFilter
	{
		private readonly Lazy<IThemeContext> _themeContext;
		private readonly ICommonServices _services;
		private readonly Lazy<IWidgetProvider> _widgetProvider;

		public PreviewModeFilter(
			Lazy<IThemeContext> themeContext, 
			ICommonServices services, 
			Lazy<IWidgetProvider> widgetProvider)
		{
			_themeContext = themeContext;
			_services = services;
			_widgetProvider = widgetProvider;
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (filterContext.IsChildAction)
				return;

			if (!filterContext.Result.IsHtmlViewResult())
				return;

			var theme = _themeContext.Value.GetPreviewTheme();
			var storeId = _services.StoreContext.GetPreviewStore();

			if (theme == null && storeId == null)
				return;

			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
				return;

			_widgetProvider.Value.RegisterAction(
				"body_end_html_tag_before", 
				"PreviewTool", 
				"Theme",
				new { area = "Admin" });
		}
		
		public void OnResultExecuted(ResultExecutedContext filterContext)
		{
			// Noop
		}
	}
}