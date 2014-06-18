using System;
using System.Linq;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;

namespace SmartStore.Plugin.Developer.DevTools
{
    /// <summary>
    /// Filter test plugin
    /// </summary>
	public class DevToolsPlugin : BasePlugin, IMiscPlugin
    {

        public DevToolsPlugin()
        {
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "FilterTestAdmin";
			routeValues = new RouteValueDictionary(new { area = "Developer.DevTools" });
        }

        public override void Install()
        {
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            base.Uninstall();
        }
    }
}
