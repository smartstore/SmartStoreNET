using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Developer.DevTools.Controllers
{
	public class DevToolsAdminController : PluginControllerBase
    {

        public ActionResult Index()
        {
			return View();
        }

		public ActionResult Configure()
		{
			return View();
		}

	}
}