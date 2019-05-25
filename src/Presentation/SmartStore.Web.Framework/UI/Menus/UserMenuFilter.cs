using System;
using System.Web.Mvc;
using SmartStore.Services.Cms;

namespace SmartStore.Web.Framework.UI
{
    public class UserMenuFilter : IResultFilter
    {
		private readonly IWidgetProvider _widgetProvider;
		private readonly IMenuStorage _menuStorage;

        public UserMenuFilter(IWidgetProvider widgetProvider, IMenuStorage menuStorage)
        {
            _widgetProvider = widgetProvider;
			_menuStorage = menuStorage;
        }

		public void OnResultExecuting(ResultExecutingContext filterContext)
        {
			if (filterContext.IsChildAction || !filterContext.Result.IsHtmlViewResult())
			{
				return;
			}

			ProcessUserMenus();
		}

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

		/// <summary>
		/// Registers actions to render user menus in widget zones.
		/// </summary>
		private void ProcessUserMenus()
		{
			var menusInfo = _menuStorage.GetUserMenuInfos();

			foreach (var info in menusInfo)
			{
				_widgetProvider.RegisterAction(
					info.WidgetZones,
					"Menu",
					"Common",
					new { area = "", name = info.SystemName, template = info.Template },
					info.DisplayOrder);
			}
		}
	}
}
