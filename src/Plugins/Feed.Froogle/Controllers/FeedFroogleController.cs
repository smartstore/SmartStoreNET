using System;
using System.Web.Mvc;
using SmartStore.Plugin.Feed.Froogle.Models;
using SmartStore.Plugin.Feed.Froogle.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Feed.Froogle.Controllers
{
	public class FeedFroogleController : PluginControllerBase
	{
		private readonly IGoogleFeedService _googleService;
		private readonly ISettingService _settingService;
		private readonly IPermissionService _permissionService;

		public FeedFroogleController(
			IGoogleFeedService googleService,
			ISettingService settingService,
			IPermissionService permissionService)
		{
			_googleService = googleService;
			_settingService = settingService;
			_permissionService = permissionService;
		}

		private ActionResult RedirectToConfig()
		{
			return RedirectToAction("ConfigureMiscPlugin", "Plugin", new { systemName = _googleService.Helper.SystemName, area = "Admin" });
		}

		public ActionResult Configure()
		{
			var model = new FeedFroogleModel();
			model.Copy(_googleService.Settings, true);

			if (TempData["GenerateFeedRunning"] != null)
				model.IsRunning = (bool)TempData["GenerateFeedRunning"];

			_googleService.SetupModel(model);

			return View( model);
		}

		[HttpPost]
		[FormValueRequired("save")]
		public ActionResult Configure(FeedFroogleModel model)
		{
			if (!ModelState.IsValid)
				return Configure();

			model.Copy(_googleService.Settings, false);
			_settingService.SaveSetting(_googleService.Settings);

			_googleService.Helper.UpdateScheduleTask(model.TaskEnabled, model.GenerateStaticFileEachMinutes * 60);

			NotifySuccess(_googleService.Helper.GetResource("ConfigSaveNote"), true);

			_googleService.SetupModel(model);

			return View(model);
		}

		public ActionResult GenerateFeed()
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageScheduleTasks))
				return AccessDeniedView();

			if (_googleService.Helper.RunScheduleTask())
				TempData["GenerateFeedRunning"] = true;

			return RedirectToConfig();
		}

		[HttpPost]
		public ActionResult GenerateFeedProgress()
		{
			string message = _googleService.Helper.GetProgressInfo(true);
			return Json(new { message = message	}, JsonRequestBehavior.DenyGet);
		}

		public ActionResult DeleteFiles()
		{
			_googleService.Helper.DeleteFeedFiles();

			return RedirectToConfig();
		}

		[HttpPost]
		public ActionResult GoogleProductEdit(int pk, string name, string value)
		{
			_googleService.UpdateInsert(pk, name, value);

			return this.Content("");
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult GoogleProductList(GridCommand command, string searchProductName, string touched)
		{
			return new JsonResult
			{
				Data = _googleService.GetGridModel(command, searchProductName, touched)
			};
		}
	}
}
