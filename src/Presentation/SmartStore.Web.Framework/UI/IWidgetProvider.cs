using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI
{
	public interface IWidgetProvider
	{
		void RegisterAction(string widgetZone, string actionName, string controllerName, RouteValueDictionary routeValues, int order = 0);

		IEnumerable<WidgetRouteInfo> GetWidgets(string widgetZone);
	}

	public static class IWidgetProviderExtensions
	{
		public static void RegisterAction(this IWidgetProvider provider, string widgetZone, string actionName, string controllerName, object routeValues, int order = 0)
		{
			provider.RegisterAction(widgetZone, actionName, controllerName, new RouteValueDictionary(routeValues), order);
		}
	}
}
