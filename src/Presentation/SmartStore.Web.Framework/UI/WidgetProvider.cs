using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Routing;
using SmartStore.Collections;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI
{
	public class WidgetProvider : IWidgetProvider
	{
		private Multimap<string, WidgetRouteInfo> _zoneWidgetsMap = new Multimap<string, WidgetRouteInfo>();
		private Multimap<Regex, WidgetRouteInfo> _zoneExpressionWidgetsMap = new Multimap<Regex, WidgetRouteInfo>();
		
		public void RegisterAction(string widgetZone, string actionName, string controllerName, RouteValueDictionary routeValues, int order = 0)
		{
			Guard.ArgumentNotEmpty(() => widgetZone);
			Guard.ArgumentNotEmpty(() => actionName);
			Guard.ArgumentNotEmpty(() => controllerName);

			if (_zoneWidgetsMap == null)
			{
				_zoneWidgetsMap = new Multimap<string, WidgetRouteInfo>();
			}

			var routeInfo = new WidgetRouteInfo 
			{ 
				ActionName = actionName,
				ControllerName = controllerName,
				RouteValues = routeValues ?? new RouteValueDictionary(),
				Order = order
			};

			_zoneWidgetsMap.Add(widgetZone, routeInfo);
		}

		public void RegisterAction(Regex widgetZoneExpression, string actionName, string controllerName, RouteValueDictionary routeValues, int order = 0)
		{
			Guard.ArgumentNotNull(() => widgetZoneExpression);
			Guard.ArgumentNotEmpty(() => actionName);
			Guard.ArgumentNotEmpty(() => controllerName);

			if (_zoneExpressionWidgetsMap == null)
			{
				_zoneExpressionWidgetsMap = new Multimap<Regex, WidgetRouteInfo>();
			}

			var routeInfo = new WidgetRouteInfo
			{
				ActionName = actionName,
				ControllerName = controllerName,
				RouteValues = routeValues ?? new RouteValueDictionary(),
				Order = order
			};

			_zoneExpressionWidgetsMap.Add(widgetZoneExpression, routeInfo);
		}

		public IEnumerable<WidgetRouteInfo> GetWidgets(string widgetZone)
		{
			if (widgetZone.IsEmpty())
				return null;

			var result = new List<WidgetRouteInfo>();

			if (_zoneWidgetsMap != null && _zoneWidgetsMap.ContainsKey(widgetZone))
			{
				result.AddRange(_zoneWidgetsMap[widgetZone]);
			}

			if (_zoneExpressionWidgetsMap != null)
			{
				foreach (var entry in _zoneExpressionWidgetsMap)
				{
					var rg = entry.Key;
					if (rg.IsMatch(widgetZone))
					{
						foreach (var routeInfo in entry.Value)
						{
							result.Add(new WidgetRouteInfo 
							{ 
								ActionName = routeInfo.ActionName,
 								ControllerName = routeInfo.ControllerName,
								Order = routeInfo.Order,
								RouteValues = new RouteValueDictionary(routeInfo.RouteValues)
							});
						}
					}
				}
			}

			if (result.Count == 0)
				return null;

			return result;
		}
	}
}
