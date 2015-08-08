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
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.UI;
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
		private readonly ILanguageService _languageService;
		private readonly ICurrencyService _currencyService;
		private readonly IEmailAccountService _emailAccountService;

		public ExportController(
			ICommonServices services,
			IExportService exportService,
			PluginMediator pluginMediator,
			IPictureService pictureService,
			ICategoryService categoryService,
			IManufacturerService manufacturerService,
			IProductTagService productTagService,
			ICustomerService customerService,
			ILanguageService languageService,
			ICurrencyService currencyService,
			IEmailAccountService emailAccountService)
		{
			_services = services;
			_exportService = exportService;
			_pluginMediator = pluginMediator;
			_pictureService = pictureService;
			_categoryService = categoryService;
			_manufacturerService = manufacturerService;
			_productTagService = productTagService;
			_customerService = customerService;
			_languageService = languageService;
			_currencyService = currencyService;
			_emailAccountService = emailAccountService;
		}

		#region Utilities

		private string GetThumbnailUrl(Provider<IExportProvider> provider)
		{
			var url = _pluginMediator.GetIconUrl(provider.Metadata);

			if (url.IsEmpty())
				url = _pictureService.GetDefaultPictureUrl(48);
			else
				url = Url.Content(url);

			return url;
		}

		private void PrepareProfileModel(ExportProfileModel model, ExportProfile profile, Provider<IExportProvider> provider)
		{
			model.Id = profile.Id;
			model.Name = profile.Name;
			model.Enabled = profile.Enabled;
			model.SchedulingHours = profile.ScheduleTask.Seconds / 3600;

			model.Provider = new ExportProfileModel.ProviderModel
			{
				SystemName = profile.ProviderSystemName
			};

			if (provider != null)
			{
				model.Provider.ThumbnailUrl = GetThumbnailUrl(provider);

				model.Provider.Url = provider.Metadata.PluginDescriptor.Url;
				model.Provider.ConfigurationUrl = Url.Action("ConfigurePlugin", new { systemName = provider.Metadata.SystemName });
				model.Provider.FriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata);
				model.Provider.Author = provider.Metadata.PluginDescriptor.Author;
				model.Provider.Version = provider.Metadata.PluginDescriptor.Version.ToString();
				model.Provider.Description = _pluginMediator.GetLocalizedDescription(provider.Metadata);

				model.Provider.EntityType = provider.Value.EntityType;
				model.Provider.EntityTypeName = provider.Value.EntityType.GetLocalizedEnum(_services.Localization, _services.WorkContext);
				model.Provider.FileType = provider.Value.FileType.ToUpper();
			}
		}

		private void PrepareProfileModelForEdit(ExportProfileModel model, ExportProfile profile, Provider<IExportProvider> provider)
		{
			var filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
			var projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);

			var allStores = _services.StoreService.GetAllStores();
			var allLanguages = _languageService.GetAllLanguages(true);
			var allCurrencies = _currencyService.GetAllCurrencies(true);

			model.AllString = T("Admin.Common.All");
			model.UnspecifiedString = T("Common.Unspecified");
			model.StoreCount = allStores.Count;
			model.Offset = profile.Offset;
			model.Limit = profile.Limit;
			model.BatchSize = profile.BatchSize;
			model.PerStore = profile.PerStore;
			model.CreateZipArchive = profile.CreateZipArchive;
			model.CompletedEmailAddresses = profile.CompletedEmailAddresses;

			if (provider != null)
			{
				try
				{
					string partialName;
					Type dataType;
					if (provider.Value.RequiresConfiguration(out partialName, out dataType))
					{
						model.Provider.ConfigPartialViewName = partialName;
						model.Provider.ConfigDataType = dataType;
						model.Provider.ConfigData = XmlHelper.Deserialize(profile.ProviderConfigData, dataType);
					}
				}
				catch (Exception exc)
				{
					NotifyError(exc);
				}
			}

			// projection
			model.Projection = new ExportProjectionModel
			{
				LanguageId = projection.LanguageId,
				CurrencyId = projection.CurrencyId
			};

			model.Projection.AvailableLanguages = allLanguages
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
				.ToList();

			model.Projection.AvailableCurrencies = allCurrencies
				.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
				.ToList();

			// filtering
			Action<ExportFilterModelBase> initModelBase = x =>
			{
				x.StoreId = filter.StoreId;
				x.CreatedFrom = filter.CreatedFrom;
				x.CreatedTo = filter.CreatedTo;

				x.AvailableStores = allStores
					.Select(y => new SelectListItem { Text = y.Name, Value = y.Id.ToString() })
					.ToList();
			};

			if (model.Provider.EntityType == ExportEntityType.Product)
			{
				var allCategories = _categoryService.GetAllCategories(showHidden: true);
				var mappedCategories = allCategories.ToDictionary(x => x.Id);
				var allManufacturers = _manufacturerService.GetAllManufacturers(true);
				var allProductTags = _productTagService.GetAllProductTags();

				model.ProductFilter = new ExportProductFilterModel
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

				model.ProductFilter.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();

				model.ProductFilter.AvailableCategories = allCategories
					.Select(x => new SelectListItem { Text = x.GetCategoryNameWithPrefix(_categoryService, mappedCategories), Value = x.Id.ToString() })
					.ToList();

				model.ProductFilter.AvailableManufacturers = allManufacturers
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				model.ProductFilter.AvailableProductTags = allProductTags
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				initModelBase(model.ProductFilter);
			}
			else if (model.Provider.EntityType == ExportEntityType.Order)
			{
				var allCustomerRoles = _customerService.GetAllCustomerRoles(true);

				model.OrderFilter = new ExportOrderFilterModel
				{
					OrderStatus = filter.OrderStatus,
					PaymentStatus = filter.PaymentStatus,
					ShippingStatus = filter.ShippingStatus,
					CustomerRoleIds = filter.CustomerRoleIds
				};

				model.OrderFilter.AvailableOrderStates = OrderStatus.Pending.ToSelectList(false).ToList();
				model.OrderFilter.AvailablePaymentStates = PaymentStatus.Pending.ToSelectList(false).ToList();
				model.OrderFilter.AvailableShippingStates = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();

				model.OrderFilter.AvailableCustomerRoles = allCustomerRoles
					.OrderBy(x => x.Name)
					.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
					.ToList();

				initModelBase(model.OrderFilter);
			}
		}

		private ExportDeploymentModel PrepareDeploymentModel(ExportDeployment deployment)
		{
			var allEmailAccounts = _emailAccountService.GetAllEmailAccounts();

			var model = new ExportDeploymentModel
			{
				Id = deployment.Id,
				ProfileId = deployment.ProfileId,
				Name = deployment.Name,
				Enabled = deployment.Enabled,
				IsPublic = deployment.IsPublic,
				DeploymentType = deployment.DeploymentType,
				DeploymentTypeName = deployment.DeploymentType.GetLocalizedEnum(_services.Localization, _services.WorkContext),
				Username = deployment.Username,
				Password = deployment.Password,
				Url = deployment.Url,
				FileSystemPath = deployment.FileSystemPath,
				EmailAddresses = deployment.EmailAddresses,
				EmailSubject = deployment.EmailSubject,
				EmailAccountId = deployment.EmailAccountId
			};

			model.AvailableDeploymentTypes = ExportDeploymentType.FileSystem.ToSelectList(false).ToList();

			model.AvailableEmailAccounts = allEmailAccounts
				.Select(x => new SelectListItem { Text = x.FriendlyName, Value = x.Id.ToString() })
				.ToList();

			return model;
		}

		private void ModelToEntity(ExportDeploymentModel model, ExportDeployment deployment)
		{
			deployment.ProfileId = model.ProfileId;
			deployment.Name = model.Name;
			deployment.Enabled = model.Enabled;
			deployment.DeploymentType = model.DeploymentType;
			deployment.IsPublic = model.IsPublic;
			deployment.Username = model.Username;
			deployment.Password = model.Password;
			deployment.Url = model.Url;
			deployment.FileSystemPath = model.FileSystemPath;
			deployment.EmailAddresses = model.EmailAddresses;
			deployment.EmailSubject = model.EmailSubject;
			deployment.EmailAccountId = model.EmailAccountId;			
		}

		private ActionResult SmartRedirect(bool continueEditing, int profileId, int deploymentId)
		{
			if (!continueEditing)
			{
				TempData["SelectedTab.export-profile-edit"] = new SelectedTabInfo
				{
					TabId = "export-profile-edit-6",
					Path = Url.Action("Edit", new { id = profileId })
				};
			}

			return (continueEditing ?
				RedirectToAction("EditDeployment", new { id = deploymentId }) :
				RedirectToAction("Edit", new { id = profileId }));
		}

		#endregion

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
					PrepareProfileModel(profileModel, x, providers.FirstOrDefault(y => y.Metadata.SystemName == x.ProviderSystemName));

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
				model.Provider = new ExportProfileModel.ProviderModel();

				model.Provider.AvailableExportProviders = _exportService.LoadAllExportProviders()
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

			if (model.Provider.SystemName.HasValue())
			{
				var provider = _exportService.LoadProvider(model.Provider.SystemName);
				if (provider != null)
				{
					var profile = _exportService.InsertExportProfile(provider);

					return RedirectToAction("Edit", new { id = profile.Id });
				}
			}

			NotifyError(T("Admin.Configuration.Export.ProviderSystemName.Validate", model.Provider.SystemName.NaIfEmpty()));

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

			PrepareProfileModel(model, profile, provider);
			PrepareProfileModelForEdit(model, profile, provider);

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
				PrepareProfileModel(model, profile, provider);
				PrepareProfileModelForEdit(model, profile, provider);
				return View(model);
			}

			profile.Name = model.Name;
			profile.Enabled = model.Enabled;
			profile.ScheduleTask.Seconds = model.SchedulingHours * 3600;
			profile.Offset = model.Offset;
			profile.Limit = model.Limit;
			profile.BatchSize = model.BatchSize;
			profile.PerStore = model.PerStore;
			profile.CreateZipArchive = model.CreateZipArchive;
			profile.CompletedEmailAddresses = model.CompletedEmailAddresses;

			if (profile.Name.IsEmpty())
				profile.Name = provider.Metadata.FriendlyName;

			if (profile.Name.IsEmpty())
				profile.Name = provider.Metadata.SystemName;

			// projection
			var projection = new ExportProjection
			{
				LanguageId = model.Projection.LanguageId,
				CurrencyId = model.Projection.CurrencyId
			};

			profile.Projection = XmlHelper.Serialize<ExportProjection>(projection);

			// filtering
			ExportFilter filter = null;
			Action<ExportFilterModelBase> getFromModelBase = x =>
			{
				filter.StoreId = x.StoreId ?? 0;
				filter.CreatedFrom = x.CreatedFrom;
				filter.CreatedTo = x.CreatedTo;
			};

			if (model.ProductFilter != null && provider.Value.EntityType == ExportEntityType.Product)
			{
				filter = new ExportFilter
				{
					PriceMinimum = model.ProductFilter.PriceMinimum,
					PriceMaximum = model.ProductFilter.PriceMaximum,
					AvailabilityMinimum = model.ProductFilter.AvailabilityMinimum,
					AvailabilityMaximum = model.ProductFilter.AvailabilityMaximum,
					IsPublished = model.ProductFilter.IsPublished,
					CategoryIds = model.ProductFilter.CategoryIds,
					WithoutCategories = model.ProductFilter.WithoutCategories,
					ManufacturerIds = model.ProductFilter.ManufacturerIds,
					WithoutManufacturers = model.ProductFilter.WithoutManufacturers,
					ProductTagIds = model.ProductFilter.ProductTagIds,
					FeaturedProducts = model.ProductFilter.FeaturedProducts,
					ProductType = model.ProductFilter.ProductType
				};

				getFromModelBase(model.ProductFilter);
			}
			else if (model.OrderFilter != null && provider.Value.EntityType == ExportEntityType.Order)
			{
				filter = new ExportFilter
				{
					OrderStatus = model.OrderFilter.OrderStatus,
					PaymentStatus = model.OrderFilter.PaymentStatus,
					ShippingStatus = model.OrderFilter.ShippingStatus,
					CustomerRoleIds = model.OrderFilter.CustomerRoleIds
				};

				getFromModelBase(model.OrderFilter);
			}

			profile.Filtering = XmlHelper.Serialize<ExportFilter>(filter);

			// provider configuration
			profile.ProviderConfigData = null;
			try
			{
				string partialName;
				Type dataType;

				if (provider.Value.RequiresConfiguration(out partialName, out dataType) && model.CustomProperties.ContainsKey("ProviderConfigData"))
				{
					profile.ProviderConfigData = XmlHelper.Serialize(model.CustomProperties["ProviderConfigData"], dataType);
				}
			}
			catch (Exception exc)
			{
				NotifyError(exc);
			}

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

		public ActionResult Preview(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var provider = _exportService.LoadProvider(profile.ProviderSystemName);

			var model = new ExportPreviewModel
			{
				Id = profile.Id,
				Name = profile.Name,
				ThumbnailUrl = GetThumbnailUrl(provider)
			};

			return View(model);
		}

		public ActionResult Execute(int id)
		{
			return null;
		}

		#region Export deployment

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult DeploymentList(GridCommand command, int id)
		{
			var model = new GridModel<ExportDeploymentModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
			{
				var profile = _exportService.GetExportProfileById(id);
				if (profile != null)
				{
					model.Total = profile.Deployments.Count;

					model.Data = profile.Deployments.Select(x => 
					{
						var deploymentModel = new ExportDeploymentModel
						{
							Id = x.Id,
							ProfileId = profile.Id,
							Name = x.Name,
							Enabled = x.Enabled,
							DeploymentType = x.DeploymentType,
							DeploymentTypeName = x.DeploymentType.GetLocalizedEnum(_services.Localization, _services.WorkContext)
						};
						return deploymentModel;
					}).ToList();
				}
			}

			return new JsonResult { Data = model };
		}

		public ActionResult CreateDeployment(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var fileSystemName = ExportDeploymentType.FileSystem.GetLocalizedEnum(_services.Localization, _services.WorkContext);

			var model = PrepareDeploymentModel(new ExportDeployment
			{
				ProfileId = id,
				Enabled = true,
				DeploymentType = ExportDeploymentType.FileSystem,
				Name = profile.Name
			});

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		[FormValueRequired("save", "save-continue")]
		public ActionResult CreateDeployment(ExportDeploymentModel model, bool continueEditing, ExportDeploymentType deploymentType)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(model.ProfileId);
			if (profile == null)
				return RedirectToAction("List");

			if (ModelState.IsValid)
			{
				var deployment = new ExportDeployment();

				ModelToEntity(model, deployment);

				profile.Deployments.Add(deployment);

				_exportService.UpdateExportProfile(profile);

				return SmartRedirect(continueEditing, profile.Id, deployment.Id);
			}

			return CreateDeployment(profile.Id);
		}

		public ActionResult EditDeployment(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var deployment = _exportService.GetExportDeploymentById(id);
			if (deployment == null)
				return RedirectToAction("List");

			var model = PrepareDeploymentModel(deployment);

			return View(model);
		}

		[HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		[FormValueRequired("save", "save-continue")]
		public ActionResult EditDeployment(ExportDeploymentModel model, bool continueEditing)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var deployment = _exportService.GetExportDeploymentById(model.Id);
			if (deployment == null)
				return RedirectToAction("List");

			if (ModelState.IsValid)
			{
				ModelToEntity(model, deployment);

				_exportService.UpdateExportProfile(deployment.Profile);

				NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

				return SmartRedirect(continueEditing, deployment.ProfileId, deployment.Id);
			}

			model = PrepareDeploymentModel(deployment);

			return View(model);
		}

		[HttpPost]
		public ActionResult DeleteDeployment(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var deployment = _exportService.GetExportDeploymentById(id);
			if (deployment == null)
				return RedirectToAction("List");

			int profileId = deployment.ProfileId;

			_exportService.DeleteExportDeployment(deployment);

			NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

			return SmartRedirect(false, profileId, 0);
		}

		#endregion
	}
}