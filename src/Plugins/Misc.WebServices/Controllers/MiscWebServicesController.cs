using System.Web.Mvc;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Misc.WebServices.Controllers
{

    public class MiscWebServicesController : PluginControllerBase
    {
        public ActionResult Configure()
        {
            return View("SmartStore.Plugin.Misc.WebServices.Views.MiscWebServices.Configure");
        }
    }
}
