using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
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
		private readonly IManufacturerService _manufacturerService;
		private readonly IProductTagService _productTagService;
		private readonly ICustomerService _customerService;

		public ExportController(
			ICommonServices services,
			IExportService exportService,
			PluginMediator pluginMediator,
			IPictureService pictureService,
			ICategoryService categoryService,
			IManufacturerService manufacturerService,
			IProductTagService productTagService,
			ICustomerService customerService)
		{
			_services = services;
			_exportService = exportService;
			_pluginMediator = pluginMediator;
			_pictureService = pictureService;
			_categoryService = categoryService;
			_manufacturerService = manufacturerService;
			_productTagService = productTagService;
			_customerService = customerService;
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

			model.AllString = T("Admin.Common.All");
			//model.UnspecifiedString = T("Common.Unspecified");
			model.StoreCount = allStores.Count;
			model.Offset = profile.Offset;
			model.Limit = profile.Limit;
			model.BatchSize = profile.BatchSize;
			model.PerStore = profile.PerStore;

			Action<ExportFilterModelBase> initModelBase = x =>
			{
				x.StoreId = filter.StoreId;
				x.CreatedFrom = filter.CreatedFrom;
				x.CreatedTo = filter.CreatedTo;

				x.AvailableStores = allStores
					.Select(y => new SelectListItem { Text = y.Name, Value = y.Id.ToString() })
					.ToList();
			};

			if (model.Providing.EntityType == ExportEntityType.Product)
			{
				var allCategories = _categoryService.GetAllCategories(showHidden: true);
				var mappedCategories = allCategories.ToDictionary(x => x.Id);
				var allManufacturers = _manufacturerService.GetAllManufacturers(true);
				var allProductTags = _productTagService.GetAllProductTags();

				model.ProductFiltering = new ExportProductFilterModel
				{
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
					FeaturedProducts = filter.FeaturedProducts,
					ProductType = filter.ProductType
				};

				model.ProductFiltering.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();

				model.ProductFiltering.AvailableCategories = allCategories
					.Select(x => new SelectListItem { Text = x.GetCategoryNameWithPrefix(_categoryService, mappedCategories), Value = x.Id.ToString() })
					.ToList();

				model.ProductFiltering.AvailableManufacturers = allManufacturers
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				model.ProductFiltering.AvailableProductTags = allProductTags
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				initModelBase(model.ProductFiltering);
			}
			else if (model.Providing.EntityType == ExportEntityType.Order)
			{
				var allCustomerRoles = _customerService.GetAllCustomerRoles(true);

				model.OrderFiltering = new ExportOrderFilterModel
				{
					OrderStatus = filter.OrderStatus,
					PaymentStatus = filter.PaymentStatus,
					ShippingStatus = filter.ShippingStatus,
					CustomerRoleIds = filter.CustomerRoleIds
				};

				model.OrderFiltering.AvailableOrderStates = OrderStatus.Pending.ToSelectList(false).ToList();
				model.OrderFiltering.AvailablePaymentStates = PaymentStatus.Pending.ToSelectList(false).ToList();
				model.OrderFiltering.AvailableShippingStates = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();

				model.OrderFiltering.AvailableCustomerRoles = allCustomerRoles
					.OrderBy(x => x.Name)
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				initModelBase(model.OrderFiltering);
			}
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

			ExportFilter filter = null;

			Action<ExportFilterModelBase> getFromModelBase = x =>
			{
				filter.StoreId = x.StoreId ?? 0;
				filter.CreatedFrom = x.CreatedFrom;
				filter.CreatedTo = x.CreatedTo;
			};

			if (model.ProductFiltering != null && provider.Value.EntityType == ExportEntityType.Product)
			{
				filter = new ExportFilter
				{
					PriceMinimum = model.ProductFiltering.PriceMinimum,
					PriceMaximum = model.ProductFiltering.PriceMaximum,
					AvailabilityMinimum = model.ProductFiltering.AvailabilityMinimum,
					AvailabilityMaximum = model.ProductFiltering.AvailabilityMaximum,
					IsPublished = model.ProductFiltering.IsPublished,
					CategoryIds = model.ProductFiltering.CategoryIds,
					WithoutCategories = model.ProductFiltering.WithoutCategories,
					ManufacturerIds = model.ProductFiltering.ManufacturerIds,
					WithoutManufacturers = model.ProductFiltering.WithoutManufacturers,
					ProductTagIds = model.ProductFiltering.ProductTagIds,
					FeaturedProducts = model.ProductFiltering.FeaturedProducts,
					ProductType = model.ProductFiltering.ProductType
				};

				getFromModelBase(model.ProductFiltering);
			}
			else if (model.OrderFiltering != null && provider.Value.EntityType == ExportEntityType.Order)
			{
				filter = new ExportFilter
				{
					OrderStatus = model.OrderFiltering.OrderStatus,
					PaymentStatus = model.OrderFiltering.PaymentStatus,
					ShippingStatus = model.OrderFiltering.ShippingStatus,
					CustomerRoleIds = model.OrderFiltering.CustomerRoleIds
				};

				getFromModelBase(model.OrderFiltering);
			}

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