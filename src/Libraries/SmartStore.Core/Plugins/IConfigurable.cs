using System.Web.Routing;

namespace SmartStore.Core.Plugins
{
    /// <summary>
    /// Marks a concrete provider or plugin implementation as configurable through the backend
    /// </summary>
    public interface IConfigurable
    {
        /// <summary>
        /// Gets a route for provider or plugin configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues);
    }
}
