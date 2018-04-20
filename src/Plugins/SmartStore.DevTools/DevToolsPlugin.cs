using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.Services.Cms;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;
using SmartStore.Core.Caching;

namespace SmartStore.DevTools
{
	[DisplayOrder(10)]
	[SystemName("Widgets.DevToolsDemo")]
	[FriendlyName("Dev-Tools Demo Widget")]
	public class DevToolsPlugin : BasePlugin, IConfigurable, IWidget
	{
		private readonly ISettingService _settingService;
		private readonly ICacheableRouteRegistrar _cacheableRouteRegistrar;

		public DevToolsPlugin(ISettingService settingService,
			ICacheableRouteRegistrar cacheAbleRouteRegistrar)
        {
			_settingService = settingService;
			_cacheableRouteRegistrar = cacheAbleRouteRegistrar;

			this.Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

		public IList<string> GetWidgetZones() => new List<string> { "home_page_top" };

		public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
		{
			actionName = "MyDemoWidget";
			controllerName = "DevTools";

			routeValues = new RouteValueDictionary
			{
				{ "Namespaces", "SmartStore.DevTools.Controllers" },
				{ "area", "SmartStore.DevTools" }
			};
		}

		public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "DevTools";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.DevTools" } };
        }

        public override void Install()
        {
			// Example for how to add a route to the output cache
			//_cacheableRouteRegistrar.RegisterCacheableRoute("SmartStore.DevTools/DevTools/PublicInfo");

			_settingService.SaveSetting(new ProfilerSettings());
			base.Install();

			Logger.Info(string.Format("Plugin installed: SystemName: {0}, Version: {1}, Description: '{2}'", PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
			// Example for how to remove a route from the output cache
			//_cacheableRouteRegistrar.RemoveCacheableRoute("SmartStore.DevTools/DevTools/PublicInfo");

			_settingService.DeleteSetting<ProfilerSettings>();
			base.Uninstall();
        }

		private static bool? _hasPendingMigrations;
		internal static bool HasPendingMigrations()
		{
			bool result = true;

			if (_hasPendingMigrations == null)
			{
				try
				{
					var migrator = new DbSeedingMigrator<SmartObjectContext>();
					result = migrator.GetPendingMigrations().Any();

					if (result == false)
					{
						// Don't check again
						_hasPendingMigrations = false;
					}
				}
				catch { }
			}
			else
			{
				result = _hasPendingMigrations.Value;
			}

			return result;
		}
	}
}
