using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Localization;

namespace SmartStore.Plugin.ExternalAuth.OpenId
{
    /// <summary>
    /// OpenId externalAuth processor
    /// </summary>
    public class OpenIdExternalAuthMethod : BasePlugin, IExternalAuthenticationMethod
    {

        private readonly ILocalizationService _localizationService;

        public OpenIdExternalAuthMethod(ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        #region Methods
        
        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            //configuration is not required
            actionName = null;
            controllerName = null;
            routeValues = null;
        }

        /// <summary>
        /// Gets a route for displaying plugin in public store
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPublicInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "ExternalAuthOpenId";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.ExternalAuth.OpenId.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //locales
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        public override void Uninstall()
        {
            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.ExternalAuth.OpenId", false);

            base.Uninstall();
        }

        #endregion
        
    }
}
