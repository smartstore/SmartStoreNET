using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
	public class WidgetProvider : IWidgetProvider
	{
		private readonly Multimap<string, WidgetRouteInfo> _widgetMap = new Multimap<string, WidgetRouteInfo>();
		
		public void RegisterAction(string widgetZone, string actionName, string controllerName, RouteValueDictionary routeValues, int order = 0)
		{
			Guard.ArgumentNotEmpty(() => widgetZone);
			Guard.ArgumentNotEmpty(() => actionName);
			Guard.ArgumentNotEmpty(() => controllerName);

			var routeInfo = new WidgetRouteInfo 
			{ 
				ActionName = actionName,
				ControllerName = controllerName,
				RouteValues = routeValues ?? new RouteValueDictionary(),
				Order = order
			};

			_widgetMap.Add(widgetZone, routeInfo);
		}

		public IEnumerable<WidgetRouteInfo> GetWidgets(string widgetZone)
		{
			if (string.IsNullOrEmpty(widgetZone) || !_widgetMap.ContainsKey(widgetZone))
				return null;

			return _widgetMap[widgetZone];
		}
	}
}
