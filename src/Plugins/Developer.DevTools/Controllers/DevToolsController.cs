using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Plugin.Developer.DevTools.Controllers
{
	public class DevToolsController : PluginControllerBase
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

		[HttpPost]
		[ChildActionOnly]
		public ActionResult Configure(ProfilerSettings model, FormCollection form)
		{
			if (!ModelState.IsValid)
				return Configure();

			// load settings for a chosen store scope
			var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
			var storeScope = this.GetActiveStoreScopeConfiguration(_storeService, _workContext);

			storeDependingSettingHelper.UpdateSettings(model /*settings*/, form, storeScope, _settingService);
			_settingService.ClearCache();

			return Configure();
		}

		[AllowAnonymous]
		public ActionResult MiniProfiler()
		{
			return View();
		}

	}
}