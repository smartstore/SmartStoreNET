using System.Web.Mvc;

namespace SmartStore.Web.Controllers
{
    public partial class KeepAliveController : Controller
    {
        public ActionResult Index()
        {
            return Content("I am alive!");
        }
    }
}
