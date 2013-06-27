using System;
using System.Web.Mvc;
using SmartStore.Core.Fakes;
using SmartStore.Plugin.Feed.ElmarShopinfo.Models;
using SmartStore.Plugin.Feed.ElmarShopinfo.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Logging;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.Plugin.Feed.ElmarShopinfo.Controllers
{
    public class FeedElmarShopinfoController : PluginControllerBase
    {
		private readonly IElmarShopinfoCoreService _elmarService;
		private readonly ILogger _logger;
        private readonly ISettingService _settingService;

        public FeedElmarShopinfoController(
			IElmarShopinfoCoreService elmarService,
			ILogger logger,
			ISettingService settingService)
        {
			this._elmarService = elmarService;
			this._logger = logger;
            this._settingService = settingService;
        }

        public ActionResult Configure()
        {
            var model = new FeedElmarShopinfoModel();
			model.Copy(_elmarService.Settings, true);

			_elmarService.SetupModel(model, _elmarService.Helper.ScheduledTask);

			return View("SmartStore.Plugin.Feed.ElmarShopinfo.Views.FeedElmarShopinfo.Configure", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Configure(FeedElmarShopinfoModel model)
        {
			if (!ModelState.IsValid)
				return Configure();

			model.Copy(_elmarService.Settings, false);
			_settingService.SaveSetting(_elmarService.Settings);

			_elmarService.Helper.ScheduleTaskUpdate(model.TaskEnabled, model.GenerateStaticFileEachMinutes * 60);

			SuccessNotification(_elmarService.Helper.Resource("ConfigSaveNote"), true);

			_elmarService.SetupModel(model);

			return View("SmartStore.Plugin.Feed.ElmarShopinfo.Views.FeedElmarShopinfo.Configure", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("generate")]
        public ActionResult GenerateFeed(FeedElmarShopinfoModel model)
        {
			if (!ModelState.IsValid)
				return Configure();

            try 
			{
				_elmarService.CreateFeed(false);

				model.GenerateFeedResult = _elmarService.Helper.Resource("SuccessResult");
            }
            catch (Exception exc)
			{
				ErrorNotification(exc.Message, true);
                _logger.Error(exc.Message, exc);
            }

			_elmarService.SetupModel(model, _elmarService.Helper.ScheduledTask);

			return View("SmartStore.Plugin.Feed.ElmarShopinfo.Views.FeedElmarShopinfo.Configure", model);
        }
    }	// class
}
