using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{
    public interface IWidgetSelector
    {
        IEnumerable<WidgetRouteInfo> GetWidgets(string widgetZone, object model);
    }
}
