using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Web.Framework.Controllers;
using System;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Controllers
{
    public partial class WidgetController : PublicControllerBase
    {
        private readonly IWidgetSelector _widgetSelector;

        public WidgetController(IWidgetSelector widgetSelector)
        {
            this._widgetSelector = widgetSelector;
        }

        [ChildActionOnly]
        public ActionResult WidgetsByZone(IEnumerable<WidgetRouteInfo> widgets)
        {
            return PartialView(widgets);
        }

    }
}
