using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Cms
{
    /// <summary>
    /// Provides an interface for widget plugins
    /// </summary>
    public partial interface IWidgetPlugin : IWidget, IPlugin
    {
        /// <summary>
        /// Gets a route for plugin configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);
    }
}
