using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.DevTools.Models;
using SmartStore.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.DevTools.Controllers
{

	public class DevToolsController : SmartController
    {
		private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
		private readonly IStoreService _storeService;
		private readonly ISettingService _settingService;

		public DevToolsController(
			IWorkContext workContext,
			IStoreContext storeContext,
			IStoreService storeService,
			ISettingService settingService)
		{
			_workContext = workContext;
			_storeContext = storeContext;
			_storeService = storeService;
			_settingService = settingService;
		}

		[AdminAuthorize]
		[ChildActionOnly]
		public ActionResult Configure()
		{
			// load settings for a chosen store scope
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
			var settings = _settingService.LoadSetting<ProfilerSettings>(storeScope);

			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			storeDependingSettingHelper.GetOverrideKeys(settings, settings, storeScope, _settingService);

			return View(settings);
		}

		[AdminAuthorize]
		[HttpPost]
		[ChildActionOnly]
		public ActionResult Configure(ProfilerSettings model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return Configure();

			ModelState.Clear();

			// load settings for a chosen store scope
			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

			storeDependingSettingHelper.UpdateSettings(model /*settings*/, form, storeScope, _settingService);
			_settingService.ClearCache();

			return Configure();
		}

		public ActionResult MiniProfiler()
		{
			return View();
		}

        public ActionResult WidgetZone(string widgetZone)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);
            var settings = _settingService.LoadSetting<ProfilerSettings>(storeScope);

            if (settings.DisplayWidgetZones)
            { 
                ViewData["widgetZone"] = widgetZone;

                return View();
            }

            return new EmptyResult();
        }

		[AdminAuthorize]
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