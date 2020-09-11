using System.Web.Mvc;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Controllers
{
    public partial class WidgetController : PublicControllerBase
    {
        [ChildActionOnly]
        public ActionResult WidgetsByZone(WidgetZoneModel zoneModel)
        {
            return PartialView(zoneModel);
        }

        [ChildActionOnly]
        public ActionResult TabWidgets(object model, string viewDataKey)
        {
            var widgets = this.ControllerContext.ParentActionViewContext.ViewData[viewDataKey];
            return PartialView(widgets);
        }
    }
}
