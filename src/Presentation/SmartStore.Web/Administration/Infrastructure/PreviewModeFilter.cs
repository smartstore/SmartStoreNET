using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Services;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Theming;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Admin.Infrastructure
{
	
	public class PreviewModeFilter : IResultFilter
	{
		private readonly IThemeContext _themeContext;
		private readonly ICommonServices _services;
		private readonly Lazy<IWidgetProvider> _widgetProvider;

		public PreviewModeFilter(IThemeContext themeContext, ICommonServices services, Lazy<IWidgetProvider> widgetProvider)
		{
			this._themeContext = themeContext;
			this._services = services;
			this._widgetProvider = widgetProvider;
		}

		public void OnResultExecuting(ResultExecutingContext filterContext)
		{
			if (!(filterContext.Result is ViewResult))
				return;

			var theme = _themeContext.GetPreviewTheme();
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