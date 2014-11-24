using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.Web.Controllers
{
	// Keep this very simple: no dependencies at all!
	public class ErrorController : Controller
	{

		public ActionResult NotFound()
		{
			this.Response.StatusCode = 404;
			this.Response.TrySkipIisCustomErrors = true;

			return View();
		}

		public ActionResult Index()
		{
			this.Response.StatusCode = 500;
			this.Response.TrySkipIisCustomErrors = true;

			return View("Error");
		}

	}
}