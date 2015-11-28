using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.FacebookAuth.Core;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Localization;

namespace SmartStore.FacebookAuth
{
    /// <summary>
    /// Facebook externalAuth processor
    /// </summary>
    public class FacebookExternalAuthMethod : BasePlugin, IExternalAuthenticationMethod, IConfigurable
    {
        #region Fields

        private readonly FacebookExternalAuthSettings _facebookExternalAuthSettings;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public FacebookExternalAuthMethod(FacebookExternalAuthSettings facebookExternalAuthSettings, ILocalizationService localizationService)
        {
            this._facebookExternalAuthSettings = facebookExternalAuthSettings;
            _localizationService = localizationService;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
			actionName = "Configure";
			controllerName = "ExternalAuthFacebook";
			routeValues = new RouteValueDictionary(new { Namespaces = "SmartStore.FacebookAuth.Controllers", area = Provider.SystemName });
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
            controllerName = "ExternalAuthFacebook";
			routeValues = new RouteValueDictionary(new { Namespaces = "SmartStore.FacebookAuth.Controllers", area = Provider.SystemName });
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

            base.Uninstall();
        }

        #endregion
        
    }
}
