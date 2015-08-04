using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.DataExchange;
using SmartStore.Services.Media;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class ExportController : AdminControllerBase
	{
		private readonly ICommonServices _services;
		private readonly IExportService _exportService;
		private readonly PluginMediator _pluginMediator;
		private readonly IPictureService _pictureService;
		private readonly ICategoryService _categoryService;

		public ExportController(
			ICommonServices services,
			IExportService exportService,
			PluginMediator pluginMediator,
			IPictureService pictureService,
			ICategoryService categoryService)
		{
			_services = services;
			_exportService = exportService;
			_pluginMediator = pluginMediator;
			_pictureService = pictureService;
			_categoryService = categoryService;
		}

		private void PrepareExportProfileModel(ExportProfileModel model, ExportProfile profile, Provider<IExportProvider> provider)
		{
			model.Providing = new ExportProfileModel.Provider
			{
				SystemName = profile.ProviderSystemName
			};

			model.Id = profile.Id;
			model.Name = profile.Name;
			model.Enabled = profile.Enabled;
			model.SchedulingHours = profile.ScheduleTask.Seconds / 3600;

			if (provider != null)
			{
				model.Providing.ThumbnailUrl = _pluginMediator.GetIconUrl(provider.Metadata);

				if (model.Providing.ThumbnailUrl.IsEmpty())
					model.Providing.ThumbnailUrl = _pictureService.GetDefaultPictureUrl(48);
				else
					model.Providing.ThumbnailUrl = Url.Content(model.Providing.ThumbnailUrl);

				model.Providing.Url = provider.Metadata.PluginDescriptor.Url;
				model.Providing.ConfigurationUrl = Url.Action("ConfigurePlugin", new { systemName = provider.Metadata.SystemName });
				model.Providing.FriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata);
				model.Providing.Author = provider.Metadata.PluginDescriptor.Author;
				model.Providing.Version = provider.Metadata.PluginDescriptor.Version.ToString();
				model.Providing.Description = _pluginMediator.GetLocalizedDescription(provider.Metadata);

				model.Providing.EntityType = provider.Value.EntityType;
				model.Providing.EntityTypeName = T("Enums.SmartStore.Core.Domain.ExportEntityType." + provider.Value.EntityType.ToString());
				model.Providing.FileType = provider.Value.FileType.ToUpper();
			}
		}

		private void PrepareExportProfileModelForEdit(ExportProfileModel model, ExportProfile profile, Provider<IExportProvider> provider)
		{
			var filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);

			var allStores = _services.StoreService.GetAllStores();
			var allCategories = _categoryService.GetAllCategories(showHidden: true);
			var mappedCategories = allCategories.ToDictionary(x => x.Id);

			var yes = T("Admin.Common.Yes").Text;
			var no = T("Admin.Common.No").Text;

			model.StoreCount = allStores.Count;
			model.AllString = T("Admin.Common.All");
			model.UnspecifiedString = T("Common.Unspecified");
			model.Offset = profile.Offset;
			model.Limit = profile.Limit;
			model.BatchSize = profile.BatchSize;
			model.PerStore = profile.PerStore;

			model.Filtering = new ExportProfileModel.Filter
			{
				StoreId = filter.StoreId,
				CreatedFrom = filter.CreatedFrom,
				CreatedTo = filter.CreatedTo,
				PriceMinimum = filter.PriceMinimum,
				PriceMaximum = filter.PriceMaximum,
				AvailabilityMinimum = filter.AvailabilityMinimum,
				AvailabilityMaximum = filter.AvailabilityMaximum,
				IsPublished = filter.IsPublished,
				CategoryIds = filter.CategoryIds,
				WithoutCategories = filter.WithoutCategories,
				ManufacturerIds = filter.ManufacturerIds,
				WithoutManufacturers = filter.WithoutManufacturers,
				ProductTagIds = filter.ProductTagIds,
				IncludeFeaturedProducts = filter.IncludeFeaturedProducts,
				OnlyFeaturedProducts = filter.OnlyFeaturedProducts,
				ProductType = filter.ProductType,
				OrderStatus = filter.OrderStatus,
				PaymentStatus = filter.PaymentStatus,
				ShippingStatus = filter.ShippingStatus,
				CustomerRoleIds = filter.CustomerRoleIds
			};

			model.Filtering.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();

			model.Filtering.AvailableStores = allStores
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
				.ToList();

			model.Filtering.AvailableCategories = allCategories
				.Select(x => new SelectListItem { Text = x.GetCategoryNameWithPrefix(_categoryService, mappedCategories), Value = x.Id.ToString() })
				.ToList();
		}

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

		public ActionResult List()
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			return View();
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command)
		{
			var model = new GridModel<ExportProfileModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
			{
				var providers = _exportService.LoadAllExportProviders().ToList();
				var query = _exportService.GetExportProfiles();
				var profiles = query.ToList();

				model.Total = profiles.Count;

				model.Data = profiles.Select(x =>
				{
					var profileModel = new ExportProfileModel();
					PrepareExportProfileModel(profileModel, x, providers.FirstOrDefault(y => y.Metadata.SystemName == x.ProviderSystemName));

					return profileModel;
				});
			}

			return new JsonResult {	Data = model };
		}

		public ActionResult Create()
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
			{
				var model = new ExportProfileModel();
				model.Providing = new ExportProfileModel.Provider();

				model.Providing.AvailableExportProviders = _exportService.LoadAllExportProviders()
					.Select(x =>
					{
						var item = new SelectListItem
						{
							Text = x.Metadata.FriendlyName,
							Value = x.Metadata.SystemName
						};

						return item;
					}).ToList();

				return PartialView(model);
			}

			return Content(T("Admin.AccessDenied.Description"));
		}

		[HttpPost]
		public ActionResult Create(ExportProfileModel model)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			if (model.Providing.SystemName.HasValue())
			{
				var provider = _exportService.LoadProvider(model.Providing.SystemName);
				if (provider != null)
				{
					var profile = _exportService.InsertExportProfile(provider);

					return RedirectToAction("Edit", new { id = profile.Id });
				}
			}

			NotifyError(T("Admin.Configuration.Export.ProviderSystemName.Validate", model.Providing.SystemName.NaIfEmpty()));

			return RedirectToAction("List");
		}

		public ActionResult Edit(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var provider = _exportService.LoadProvider(profile.ProviderSystemName);

			var model = new ExportProfileModel();

			PrepareExportProfileModel(model, profile, provider);
			PrepareExportProfileModelForEdit(model, profile, provider);

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		[FormValueRequired("save", "save-continue")]
		public ActionResult Edit(ExportProfileModel model, bool continueEditing)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(model.Id);
			if (profile == null)
				return RedirectToAction("List");

			var provider = _exportService.LoadProvider(profile.ProviderSystemName);

			if (!ModelState.IsValid)
			{
				PrepareExportProfileModel(model, profile, provider);
				PrepareExportProfileModelForEdit(model, profile, provider);
				return View(model);
			}

			var filter = new ExportFilter
			{
				StoreId = model.Filtering.StoreId ?? 0,
				CreatedFrom = model.Filtering.CreatedFrom,
				CreatedTo = model.Filtering.CreatedTo,
				PriceMinimum = model.Filtering.PriceMinimum,
				PriceMaximum = model.Filtering.PriceMaximum,
				AvailabilityMinimum = model.Filtering.AvailabilityMinimum,
				AvailabilityMaximum = model.Filtering.AvailabilityMaximum,
				IsPublished = model.Filtering.IsPublished,
				CategoryIds = model.Filtering.CategoryIds,
				WithoutCategories = model.Filtering.WithoutCategories,
				ManufacturerIds = model.Filtering.ManufacturerIds,
				WithoutManufacturers = model.Filtering.WithoutManufacturers,
				ProductTagIds = model.Filtering.ProductTagIds,
				IncludeFeaturedProducts = model.Filtering.IncludeFeaturedProducts,
				OnlyFeaturedProducts = model.Filtering.OnlyFeaturedProducts,
				ProductType = model.Filtering.ProductType,
				OrderStatus = model.Filtering.OrderStatus,
				PaymentStatus = model.Filtering.PaymentStatus,
				ShippingStatus = model.Filtering.ShippingStatus,
				CustomerRoleIds = model.Filtering.CustomerRoleIds
			};

			profile.Name = model.Name;
			profile.Enabled = model.Enabled;
			profile.ScheduleTask.Seconds = model.SchedulingHours * 3600;
			profile.Offset = model.Offset;
			profile.Limit = model.Limit;
			profile.BatchSize = model.BatchSize;
			profile.PerStore = model.PerStore;

			if (profile.Name.IsEmpty())
				profile.Name = provider.Metadata.FriendlyName;

			if (profile.Name.IsEmpty())
				profile.Name = provider.Metadata.SystemName;

			profile.Filtering = XmlHelper.Serialize<ExportFilter>(filter);

			_exportService.UpdateExportProfile(profile);

			NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

			return (continueEditing ? RedirectToAction("Edit", new { id = profile.Id }) : RedirectToAction("List"));
		}

		[HttpPost, ActionName("Delete")]
		public ActionResult DeleteConfirmed(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			try
			{
				_exportService.DeleteExportProfile(profile);

				NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

				return RedirectToAction("List");
			}
			catch (Exception exc)
			{
				NotifyError(exc);
			}

			return RedirectToAction("Edit", new { id = profile.Id });
		}

		public ActionResult Execute(int id)
		{
			return null;
		}
	}
}