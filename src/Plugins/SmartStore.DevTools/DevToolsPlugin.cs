using System;
using System.Linq;
using System.Web.Routing;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.Services.Common;
using SmartStore.Services.Configuration;

namespace SmartStore.DevTools
{
	public class DevToolsPlugin : BasePlugin, IConfigurable
    {
		private readonly ISettingService _settingService;

		public DevToolsPlugin(ISettingService settingService)
        {
			this._settingService = settingService;
			this.Logger = NullLogger.Instance;
        }

		public ILogger Logger { get; set; }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "DevTools";
			routeValues = new RouteValueDictionary() { { "area", "SmartStore.DevTools" } };
        }

        public override void Install()
        {
			_settingService.SaveSetting(new ProfilerSettings());
			base.Install();
			Logger.Info(string.Format("Plugin installed: SystemName: {0}, Version: {1}, Description: '{2}'", PluginDescriptor.SystemName, PluginDescriptor.Version, PluginDescriptor.FriendlyName));
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
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
