using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.GoogleMerchantCenter.Models;
using SmartStore.GoogleMerchantCenter.Providers;
using SmartStore.GoogleMerchantCenter.Services;
using SmartStore.Services.DataExchange;
using SmartStore.Web.Framework.Controllers;
using Telerik.Web.Mvc;

namespace SmartStore.GoogleMerchantCenter.Controllers
{
	[AdminAuthorize]
	public class FeedGoogleMerchantCenterController : PluginControllerBase
	{
		private readonly IGoogleFeedService _googleFeedService;
		private readonly AdminAreaSettings _adminAreaSettings;
		private readonly IExportService _exportService;

		public FeedGoogleMerchantCenterController(
			IGoogleFeedService googleFeedService,
			AdminAreaSettings adminAreaSettings,
			IExportService exportService)
		{
			_googleFeedService = googleFeedService;
			_adminAreaSettings = adminAreaSettings;
			_exportService = exportService;
		}

		public ActionResult ProductEditTab(int productId)
		{
			var culture = CultureInfo.InvariantCulture;
			var model = new GoogleProductModel { ProductId = productId };
			var entity = _googleFeedService.GetGoogleProductRecord(productId);

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
			else
			{
				model.Exporting = true;
			}

			ViewBag.DefaultCategory = "";
			ViewBag.DefaultColor = "";
			ViewBag.DefaultSize = "";
			ViewBag.DefaultMaterial = "";
			ViewBag.DefaultPattern = "";
			ViewBag.DefaultGender = T("Common.Auto");
			ViewBag.DefaultAgeGroup = T("Common.Auto");		

			// we do not have export profile context here, so we simply use the first profile
			var profile = _exportService.GetExportProfilesBySystemName(ProductExportXmlProvider.SystemName).FirstOrDefault();
			if (profile != null)
			{
				var config = XmlHelper.Deserialize(profile.ProviderConfigData, typeof(ProfileConfigurationModel)) as ProfileConfigurationModel;
				if (config != null)
				{
					ViewBag.DefaultCategory = config.DefaultGoogleCategory;
					ViewBag.DefaultColor = config.Color;
					ViewBag.DefaultSize = config.Size;
					ViewBag.DefaultMaterial = config.Material;
					ViewBag.DefaultPattern = config.Pattern;

					if (config.Gender.HasValue() && config.Gender != ProductExportXmlProvider.Unspecified)
					{
						ViewBag.DefaultGender = T("Plugins.Feed.Froogle.Gender" + culture.TextInfo.ToTitleCase(config.Gender));
					}

					if (config.AgeGroup.HasValue() && config.AgeGroup != ProductExportXmlProvider.Unspecified)
					{
						ViewBag.DefaultAgeGroup = T("Plugins.Feed.Froogle.AgeGroup" + culture.TextInfo.ToTitleCase(config.AgeGroup));
					}
				}
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
			var categories = _googleFeedService.GetTaxonomyList();
			return Json(categories, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Configure()
		{
			var model = new FeedGoogleMerchantCenterModel();

			model.AvailableGoogleCategories = _googleFeedService.GetTaxonomyList();
			model.GridPageSize = _adminAreaSettings.GridPageSize;

			return View(model);
		}

		[HttpPost]
		public ActionResult GoogleProductEdit(int pk, string name, string value)
		{
			_googleFeedService.Upsert(pk, name, value);

			return this.Content("");
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult GoogleProductList(GridCommand command, string searchProductName, string touched)
		{
			return new JsonResult
			{
				Data = _googleFeedService.GetGridModel(command, searchProductName, touched)
			};
		}
	}
}
