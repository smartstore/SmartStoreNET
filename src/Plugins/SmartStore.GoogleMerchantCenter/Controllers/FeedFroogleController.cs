using System.Collections.Generic;
using System.Globalization;
using System.Web.Mvc;
using SmartStore.Core.Localization;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.GoogleMerchantCenter.Controllers
{
	[AdminAuthorize]
	public class FeedFroogleController : PluginControllerBase
	{
		private readonly FroogleSettings _settings;
		private readonly IGoogleFeedService _googleService;
		private readonly ISettingService _settingService;
		private readonly IPermissionService _permissionService;

		public FeedFroogleController(
			FroogleSettings settings,
			IGoogleFeedService googleService,
			ISettingService settingService,
			IPermissionService permissionService)
		{
			_settings = settings;
			_googleService = googleService;
			_settingService = settingService;
			_permissionService = permissionService;
		}

		private ActionResult RedirectToConfig()
		{
			return RedirectToAction("ConfigurePlugin", "Plugin", new { systemName = _googleService.Helper.SystemName, area = "Admin" });
		}

		public ActionResult ProductEditTab(int productId)
		{
			var model = new GoogleProductModel { ProductId = productId };
			var entity = _googleService.GetGoogleProductRecord(productId);

			if (entity != null)
			{
				model.Taxonomy = entity.Taxonomy;
				model.Gender = entity.Gender;
				model.AgeGroup = entity.AgeGroup;
				model.Color = entity.Color;
				model.Size = entity.Size;
				model.Material = entity.Material;
				model.Pattern = entity.Pattern;
				model.Exporting = entity.Export;
			}
			
			ViewBag.DefaultCategory = _settings.DefaultGoogleCategory;
			ViewBag.DefaultGender = T("Common.Auto");
			ViewBag.DefaultAgeGroup = T("Common.Auto");
			ViewBag.DefaultColor = _settings.Color;
			ViewBag.DefaultSize = _settings.Size;
			ViewBag.DefaultMaterial = _settings.Material;
			ViewBag.DefaultPattern = _settings.Pattern;

			var ci = CultureInfo.InvariantCulture;

			if (_settings.Gender.HasValue() && _settings.Gender != PluginHelper.NotSpecified)
			{
				ViewBag.DefaultGender = T("Plugins.Feed.Froogle.Gender" + ci.TextInfo.ToTitleCase(_settings.Gender));
			}

			if (_settings.AgeGroup.HasValue() && _settings.AgeGroup != PluginHelper.NotSpecified)
			{
				ViewBag.DefaultAgeGroup = T("Plugins.Feed.Froogle.AgeGroup" + ci.TextInfo.ToTitleCase(_settings.AgeGroup));
			}
			

			ViewBag.AvailableGenders = new List<SelectListItem>
			{ 
				new SelectListItem { Value = "male", Text = T("Plugins.Feed.Froogle.GenderMale") },
				new SelectListItem { Value = "female", Text = T("Plugins.Feed.Froogle.GenderFemale") },
				new SelectListItem { Value = "unisex", Text = T("Plugins.Feed.Froogle.GenderUnisex") }
			};

			ViewBag.AvailableAgeGroups = new List<SelectListItem>
			{ 
				new SelectListItem { Value = "adult", Text = T("Plugins.Feed.Froogle.AgeGroupAdult") },
				new SelectListItem { Value = "kids", Text = T("Plugins.Feed.Froogle.AgeGroupKids") },
			};

			var result = PartialView(model);
			result.ViewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "CustomProperties[GMC]" };
			return result;
		}

		public ActionResult GoogleCategories()
		{
			var categories = _googleService.GetTaxonomyList();
			return Json(categories, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Configure()
		{
			var model = new FeedFroogleModel();
			model.Copy(_googleService.Settings, true);

			if (TempData["GenerateFeedRunning"] != null)
				model.IsRunning = (bool)TempData["GenerateFeedRunning"];

			_googleService.SetupModel(model);

			return View(model);
		}

		[HttpPost]
		[FormValueRequired("save")]
		public ActionResult Configure(FeedFroogleModel model)
		{
			if (!ModelState.IsValid)
				return Configure();

			model.Copy(_googleService.Settings, false);
			_settingService.SaveSetting(_googleService.Settings);
			
			_googleService.Helper.UpdateScheduleTask(model.TaskEnabled);

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
