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

        [AdminAuthorize, ChildActionOnly]
        public ActionResult Configure()
        {
            // load settings for a chosen store scope
            var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
            var settings = _services.Settings.LoadSetting<ProfilerSettings>(storeScope);

            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            storeDependingSettingHelper.GetOverrideKeys(settings, settings, storeScope, _services.Settings);

            return View(settings);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly]
        public ActionResult Configure(ProfilerSettings model, FormCollection form)
        {
            if (!ModelState.IsValid)
                return Configure();

            ModelState.Clear();

            // load settings for a chosen store scope
            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);

            storeDependingSettingHelper.UpdateSettings(model /*settings*/, form, storeScope, _services.Settings);

            return Configure();
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
	}
}