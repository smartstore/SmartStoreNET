using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Misc.WebServices.Security;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;

namespace SmartStore.Plugin.Misc.WebServices
{
    public class WebServicePlugin : BasePlugin, IMiscPlugin
    {
        #region Ctor

        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public WebServicePlugin(IPermissionService permissionService, ILocalizationService localizationService)
        {
            this._permissionService = permissionService;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods

        public override void Install()
        {
            //install new permissions
            _permissionService.InstallPermissions(new WebServicePermissionProvider());
            
            //locales
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        public override void Uninstall()
        {
            //uninstall permissions
            _permissionService.UninstallPermissions(new WebServicePermissionProvider());

            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.ExternalAuth.OpenId", false);
            
            base.Uninstall();
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
            controllerName = "MiscWebServices";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Misc.WebServices.Controllers.Controllers" }, { "area", null } };
        }

        #endregion
    }
}
