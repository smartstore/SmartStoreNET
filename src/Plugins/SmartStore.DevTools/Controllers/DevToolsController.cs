using System.Web.Mvc;
using SmartStore.DevTools.Models;
using SmartStore.Services;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.Theming;

namespace SmartStore.DevTools.Controllers
{

    public class DevToolsController : SmartController
    {
        private readonly ICommonServices _services;

        public DevToolsController(ICommonServices services)
        {
            _services = services;
        }

        [LoadSetting, ChildActionOnly]
        public ActionResult Configure(ProfilerSettings settings)
        {
            return View(settings);
        }

        [SaveSetting(false), HttpPost, ChildActionOnly, ActionName("Configure")]
        public ActionResult ConfigurePost(ProfilerSettings settings)
        {
			return RedirectToConfiguration("SmartStore.DevTools");
        }

        public ActionResult MiniProfiler()
        {
            return View();
        }

        public ActionResult MachineName()
        {
            ViewBag.EnvironmentIdentifier = _services.ApplicationEnvironment.EnvironmentIdentifier;

            return View();
        }
        
        public ActionResult WidgetZone(string widgetZone)
        {
			var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
			var settings = _services.Settings.LoadSetting<ProfilerSettings>(storeScope);

            if (settings.DisplayWidgetZones)
            { 
                ViewData["widgetZone"] = widgetZone;

                return View();
            }

            return new EmptyResult();
        }

		[AdminAuthorize, AdminThemed]
		public ActionResult BackendExtension()
		{
			var model = new BackendExtensionModel
			{
				Welcome = "Hello world!"
			};

			return View(model);
		}

        [AdminAuthorize]
        public ActionResult ProductEditTab(int productId, FormCollection form)
        {
            var model = new BackendExtensionModel
            {
                Welcome = "Hello world!"
            };

            var result = PartialView(model);
            result.ViewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "CustomProperties[DevTools]" };
            return result;
        }

		public ActionResult MyDemoWidget()
		{
			return Content("Hello world! This is a sample widget created for demonstration purposes by Dev-Tools plugin.");
		}
	}
}