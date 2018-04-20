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
        private readonly FacebookExternalAuthSettings _facebookExternalAuthSettings;
        private readonly ILocalizationService _localizationService;

        public FacebookExternalAuthMethod(FacebookExternalAuthSettings facebookExternalAuthSettings, ILocalizationService localizationService)
        {
            _facebookExternalAuthSettings = facebookExternalAuthSettings;
            _localizationService = localizationService;
        }

		public static string SystemName => "SmartStore.FacebookAuth";

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
			routeValues = new RouteValueDictionary(new { Namespaces = "SmartStore.FacebookAuth.Controllers", area = SystemName });
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
			routeValues = new RouteValueDictionary(new { Namespaces = "SmartStore.FacebookAuth.Controllers", area = SystemName });
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _localizationService.ImportPluginResourcesFromXml(PluginDescriptor);

            base.Install();
        }

        public override void Uninstall()
        {
            _localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

            base.Uninstall();
        }        
    }
}
