using System;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;

namespace SmartStore.Web.Framework
{
	public static class PluginExtensions
	{
		/// <summary>
		/// Determines whether a plugin is installed and activated for a particular store.
		/// </summary>
		public static bool IsPluginReady(this IPluginFinder pluginFinder, ISettingService settingService, string systemName, int storeId)
		{
			try
			{
				var pluginDescriptor = pluginFinder.GetPluginDescriptorBySystemName(systemName);

				if (pluginDescriptor != null && pluginDescriptor.Installed)
				{
					if (storeId == 0 || settingService.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(storeId, true))
						return true;
				}
			}
			catch (Exception exc)
			{
				exc.Dump();
			}

			return false;
		}
	}
}
