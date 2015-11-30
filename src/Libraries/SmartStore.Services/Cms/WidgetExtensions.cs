using System;
using System.Linq;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Plugins;

namespace SmartStore.Services.Cms	
{
    public static class WidgetExtensions
    {
        public static bool IsWidgetActive(this Provider<IWidget> widget, WidgetSettings widgetSettings)
        {
			Guard.ArgumentNotNull(() => widget);

			return widget.ToLazy().IsWidgetActive(widgetSettings);
        }

		public static bool IsWidgetActive(this Lazy<IWidget, ProviderMetadata> widget, WidgetSettings widgetSettings)
		{
			Guard.ArgumentNotNull(() => widget);
			Guard.ArgumentNotNull(() => widgetSettings);

			if (widgetSettings.ActiveWidgetSystemNames == null)
			{
				return false;
			}

			return widgetSettings.ActiveWidgetSystemNames.Contains(widget.Metadata.SystemName, StringComparer.OrdinalIgnoreCase);
		}
    }
}