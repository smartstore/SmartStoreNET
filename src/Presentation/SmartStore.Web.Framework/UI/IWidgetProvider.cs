using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI
{
    /// <summary>
    /// Allows request scoped registration of custom action routes, whose results get injected into widget zones.
    /// </summary>
    public interface IWidgetProvider
    {
        /// <summary>
        /// Registers an action route for a widget zone
        /// </summary>
        /// <param name="widgetZones">The names of the widget zones to inject the action result to</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        /// <param name="order">Sort order of action result within the specified widget zone</param>
        void RegisterAction(string[] widgetZones, string actionName, string controllerName, RouteValueDictionary routeValues, int order = 0);

        /// <summary>
        /// Registers an action route for multiple widget zones by pattern
        /// </summary>
        /// <param name="widgetZoneExpression">The zone pattern to inject the action result to</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        /// <param name="order">Sort order of action result within the specified widget zone</param>
        void RegisterAction(Regex widgetZoneExpression, string actionName, string controllerName, RouteValueDictionary routeValues, int order = 0);

        IEnumerable<WidgetRouteInfo> GetWidgets(string widgetZone);

        /// <summary>
        /// Reads all known widgetzones from the json file /App_Data/widgetzones.json
        /// </summary>
        dynamic GetAllKnownWidgetZones();
    }

    public static class IWidgetProviderExtensions
    {
        public static void RegisterAction(this IWidgetProvider provider, string[] widgetZones, string actionName, string controllerName, object routeValues, int order = 0)
        {
            provider.RegisterAction(widgetZones, actionName, controllerName, new RouteValueDictionary(routeValues), order);
        }

        public static void RegisterAction(this IWidgetProvider provider, string widgetZone, string actionName, string controllerName, object routeValues, int order = 0)
        {
            provider.RegisterAction(new[] { widgetZone }, actionName, controllerName, new RouteValueDictionary(routeValues), order);
        }

        public static void RegisterAction(this IWidgetProvider provider, string widgetZone, string actionName, string controllerName, RouteValueDictionary routeValues, int order = 0)
        {
            provider.RegisterAction(new[] { widgetZone }, actionName, controllerName, routeValues, order);
        }

        public static void RegisterAction(this IWidgetProvider provider, Regex widgetZoneExpression, string actionName, string controllerName, object routeValues, int order = 0)
        {
            provider.RegisterAction(widgetZoneExpression, actionName, controllerName, new RouteValueDictionary(routeValues), order);
        }
    }
}
