using System.Web.Mvc;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Infrastructure;

namespace SmartStore.Web.Controllers
{
    public class ErrorController : SmartController
    {
        [MapLegacyRoutes]
        public ActionResult NotFound()
        {
            return HttpNotFound();
        }

        public ActionResult Index()
        {
            this.Response.StatusCode = 500;
            this.Response.TrySkipIisCustomErrors = true;

            return View("Error");
        }

        //public ActionResult DoThrow()
        //{
        //	throw Error.Application("This error was thrown on purpose for testing reasons.");
        //}

    }
}