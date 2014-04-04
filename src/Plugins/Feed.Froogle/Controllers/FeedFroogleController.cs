using System;
using System.Web.Mvc;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Plugin.Feed.Froogle.Models;
using SmartStore.Plugin.Feed.Froogle.Services;
using SmartStore.Services.Configuration;
using SmartStore.Core.Logging;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.Plugin.Feed.Froogle.Controllers
{

    public class FeedFroogleController : PluginControllerBase
    {
        private readonly IGoogleService _googleService;
        private readonly ISettingService _settingService;

        public FeedFroogleController(
			IGoogleService googleService, 
			ISettingService settingService)
		{
			this._googleService = googleService;
			this._settingService = settingService;
		}
        
        public ActionResult Configure()
        {
			// codehint: sm-edit
            var model = new FeedFroogleModel();
			model.Copy(_googleService.Settings, true);

			_googleService.SetupModel(model, _googleService.Helper.ScheduledTask);

            return View("SmartStore.Plugin.Feed.Froogle.Views.FeedFroogle.Configure", model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public ActionResult Configure(FeedFroogleModel model)
        {
            if (!ModelState.IsValid)
				return Configure();

			model.Copy(_googleService.Settings, false);
			_settingService.SaveSetting(_googleService.Settings);

			_googleService.Helper.ScheduleTaskUpdate(model.TaskEnabled, model.GenerateStaticFileEachMinutes * 60);

			NotifySuccess(_googleService.Helper.Resource("ConfigSaveNote"), true);

			_googleService.SetupModel(model);

			return View("SmartStore.Plugin.Feed.Froogle.Views.FeedFroogle.Configure", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("generate")]
        public ActionResult GenerateFeed(FeedFroogleModel model)
        {
			if (!ModelState.IsValid)
				return Configure();

            try
			{
				_googleService.CreateFeed();

				model.GenerateFeedResult = _googleService.Helper.Resource("SuccessResult");
            }
            catch (Exception exc)
			{
				NotifyError(exc.Message, true);
            }

			_googleService.SetupModel(model, _googleService.Helper.ScheduledTask);

			return View("SmartStore.Plugin.Feed.Froogle.Views.FeedFroogle.Configure", model);
        }

		[HttpPost]
		public ActionResult GoogleProductEdit(int pk, string name, string value) {
			_googleService.UpdateInsert(pk, name, value);

			return this.Content("");
		}

        [HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult GoogleProductList(GridCommand command, string searchProductName)
        {
            return new JsonResult {
                Data = _googleService.GetGridModel(command, searchProductName)
            };
        }
    }
}
