using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;

namespace SmartStore.Services.Cms
{
    /// <summary>
    /// Widget service
    /// </summary>
    public partial class WidgetService : IWidgetService
    {

        #region Fields

        private readonly IPluginFinder _pluginFinder;
        private readonly WidgetSettings _widgetSettings;
		private readonly ISettingService _settingService;	// codehint: sm-add

        #endregion
        
        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="pluginFinder">Plugin finder</param>
        /// <param name="widgetSettings">Widget settings</param>
		/// <param name="pluginService">Plugin service</param>
		public WidgetService(IPluginFinder pluginFinder, WidgetSettings widgetSettings, ISettingService settingService)
        {
            this._pluginFinder = pluginFinder;
            this._widgetSettings = widgetSettings;
			this._settingService = settingService;	// codehint: sm-add
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load active widgets
        /// </summary>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
		public virtual IList<IWidgetPlugin> LoadActiveWidgets(int storeId = 0)
        {
            return LoadAllWidgets(storeId)
                   .Where(x => _widgetSettings.ActiveWidgetSystemNames.Contains(x.PluginDescriptor.SystemName, StringComparer.InvariantCultureIgnoreCase))
                   .ToList();
        }

        /// <summary>
        /// Load active widgets
        /// </summary>
        /// <param name="widgetZone">Widget zone</param>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
		public virtual IList<IWidgetPlugin> LoadActiveWidgetsByWidgetZone(string widgetZone, int storeId = 0)
        {
            if (String.IsNullOrWhiteSpace(widgetZone))
                return new List<IWidgetPlugin>();

            return LoadActiveWidgets(storeId)
                   .Where(x => x.GetWidgetZones().Contains(widgetZone, StringComparer.InvariantCultureIgnoreCase))
                   .ToList();
        }

        /// <summary>
        /// Load widget by system name
        /// </summary>
         /// <param name="systemName">System name</param>
        /// <returns>Found widget</returns>
        public virtual IWidgetPlugin LoadWidgetBySystemName(string systemName)
        {
            var descriptor = _pluginFinder.GetPluginDescriptorBySystemName<IWidgetPlugin>(systemName);
            if (descriptor != null)
                return descriptor.Instance<IWidgetPlugin>();

            return null;
        }

        /// <summary>
        /// Load all widgets
        /// </summary>
		/// <param name="storeId">Load records allows only in specified store; pass 0 to load all records</param>
        /// <returns>Widgets</returns>
		public virtual IList<IWidgetPlugin> LoadAllWidgets(int storeId = 0)
        {
			return _pluginFinder
				.GetPlugins<IWidgetPlugin>()
				.Where(x => storeId == 0 || _settingService.GetSettingByKey<string>(x.PluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(storeId, true))
				.ToList();
        }

        #endregion
    }
}
