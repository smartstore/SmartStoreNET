using System;
using SmartStore.Core.Domain.Cms;

namespace SmartStore.Services.Cms	
{
    public static class WidgetExtensions
    {
        public static bool IsWidgetActive(this IWidgetPlugin widget, WidgetSettings widgetSettings)
        {
			Guard.ArgumentNotNull(() => widget);
			Guard.ArgumentNotNull(() => widgetSettings);

			if (widgetSettings.ActiveWidgetSystemNames == null)
			{
				return false;
			}

			foreach (string systemName in widgetSettings.ActiveWidgetSystemNames)
			{
				if (widget.PluginDescriptor.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase))
					return true;
			}

            return false;
        }
    }
}