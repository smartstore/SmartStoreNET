using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Developer.DevTools.Controllers
{
	public class MyCheckoutController : PluginControllerBase
    {

        public ActionResult MyBillingAddress()
        {
			return View();
        }

		public ActionResult MiniProfiler()
		{
			return View();
		}

	}
}