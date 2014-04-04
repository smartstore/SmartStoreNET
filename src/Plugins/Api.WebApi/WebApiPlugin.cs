using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Common;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Plugin.Api.WebApi.Security;

namespace SmartStore.Plugin.Api.WebApi
{
	public class WebApiPlugin : BasePlugin, IMiscPlugin
	{
		private readonly IPermissionService _permissionService;
		private readonly ILocalizationService _localizationService;
		private readonly ISettingService _settingService;

		public WebApiPlugin(IPermissionService permissionService, ILocalizationService localizationService, ISettingService settingService)
		{
			_permissionService = permissionService;
			_localizationService = localizationService;
			_settingService = settingService;
		}


		public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "Configure";
			controllerName = "WebApi";
            routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.Plugin.Api.WebApi.Controllers" }, { "area", null } };
		}

		public override void Install()
		{
			_permissionService.InstallPermissions(new WebApiPermissionProvider());

			var apiSettings = new WebApiSettings()
			{
				LogUnauthorized = true,
				ValidMinutePeriod = WebApiGlobal.DefaultTimePeriodMinutes
			};

			_settingService.SaveSetting<WebApiSettings>(apiSettings);

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();

			WebApiCaching.Remove(WebApiControllingCacheData.Key);
			WebApiCaching.Remove(WebApiUserCacheData.Key);
		}

		public override void Uninstall()
		{
			WebApiCaching.Remove(WebApiControllingCacheData.Key);
			WebApiCaching.Remove(WebApiUserCacheData.Key);

			_settingService.DeleteSetting<WebApiSettings>();

			_permissionService.UninstallPermissions(new WebApiPermissionProvider());

			_localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Api.WebApi", false);

			base.Uninstall();
		}

	}
}
