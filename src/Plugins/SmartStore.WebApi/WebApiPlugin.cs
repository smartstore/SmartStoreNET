using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.WebApi;
using SmartStore.Web.Framework.WebApi.Caching;
using SmartStore.WebApi.Security;

namespace SmartStore.WebApi
{
	public class WebApiPlugin : BasePlugin, IConfigurable
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
			routeValues = new RouteValueDictionary() { { "Namespaces", "SmartStore.WebApi.Controllers" }, { "area", WebApiGlobal.PluginSystemName } };
		}

		public override void Install()
		{
			_permissionService.InstallPermissions(new WebApiPermissionProvider());

			var apiSettings = new WebApiSettings
			{
				LogUnauthorized = true,
				ValidMinutePeriod = WebApiGlobal.DefaultTimePeriodMinutes
			};

			_settingService.SaveSetting<WebApiSettings>(apiSettings);

			_localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

			base.Install();

			WebApiCachingControllingData.Remove();
			WebApiCachingUserData.Remove();
		}

		public override void Uninstall()
		{
			WebApiCachingControllingData.Remove();
			WebApiCachingUserData.Remove();

			_settingService.DeleteSetting<WebApiSettings>();

			_permissionService.UninstallPermissions(new WebApiPermissionProvider());

			base.Uninstall();
		}

	}
}
