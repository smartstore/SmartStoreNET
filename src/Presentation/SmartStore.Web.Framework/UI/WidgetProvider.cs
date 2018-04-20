using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Routing;
using SmartStore.Collections;
using SmartStore.Utilities;
using SmartStore.Core.Infrastructure;
using SmartStore.Core;
using SmartStore.Core.IO;
using System.Web;
using Newtonsoft.Json.Linq;

namespace SmartStore.Web.Framework.UI
{
	public class WidgetProvider : IWidgetProvider
	{
		private readonly IApplicationEnvironment _env;

		private Multimap<string, WidgetRouteInfo> _zoneWidgetsMap = new Multimap<string, WidgetRouteInfo>();
		private Multimap<Regex, WidgetRouteInfo> _zoneExpressionWidgetsMap = new Multimap<Regex, WidgetRouteInfo>();

		public WidgetProvider(IApplicationEnvironment env)
		{
			_env = env;
		}

		public void RegisterAction(string[] widgetZones, string actionName, string controllerName, RouteValueDictionary routeValues, int order = 0)
		{
			Guard.NotNull(widgetZones, nameof(widgetZones));
			Guard.NotEmpty(actionName, nameof(actionName));
			Guard.NotEmpty(controllerName, nameof(controllerName));

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

			foreach (var zone in widgetZones)
			{
				if (zone.HasValue())
					_zoneWidgetsMap.Add(zone, routeInfo);
			}	
		}

		public void RegisterAction(Regex widgetZoneExpression, string actionName, string controllerName, RouteValueDictionary routeValues, int order = 0)
		{
			Guard.NotNull(widgetZoneExpression, nameof(widgetZoneExpression));
			Guard.NotEmpty(actionName, nameof(actionName));
			Guard.NotEmpty(controllerName, nameof(controllerName));

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
		
		public dynamic GetAllKnownWidgetZones()
		{
			var fileName = "widgetzones.json";

			var cacheKey = HttpRuntime.Cache.BuildScopedKey(fileName);
			var rawJson = HttpRuntime.Cache.Get(cacheKey);

			if (rawJson == null)
			{
				if (_env.AppDataFolder.FileExists(fileName))
				{
					rawJson = _env.AppDataFolder.ReadFile(fileName);
					var virtualPath = _env.AppDataFolder.GetVirtualPath(fileName);
					var cacheDependency = _env.AppDataFolder.VirtualPathProvider.GetCacheDependency(virtualPath, DateTime.UtcNow);
					HttpRuntime.Cache.Insert(cacheKey, rawJson, cacheDependency);
				}
				else
				{
					HttpRuntime.Cache.Insert(cacheKey, "");
				}
			}

			if (rawJson is string json && json.HasValue())
			{
				try
				{
					return JObject.Parse(json);
				}
				catch
				{
					// Json is invalid. Don't parse again.
					HttpRuntime.Cache.Insert(cacheKey, "");
				}
			}

			return null;
		}
	}
}
