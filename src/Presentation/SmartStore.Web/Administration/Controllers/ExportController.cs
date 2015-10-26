using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using Autofac;
using SmartStore.Admin.Extensions;
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
using SmartStore.Services.DataExchange.ExportTask;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
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
		private readonly ICategoryService _categoryService;
		private readonly IManufacturerService _manufacturerService;
		private readonly IProductTagService _productTagService;
		private readonly ICustomerService _customerService;
		private readonly ILanguageService _languageService;
		private readonly ICurrencyService _currencyService;
		private readonly IEmailAccountService _emailAccountService;
		private readonly IComponentContext _componentContext;
		private readonly IDateTimeHelper _dateTimeHelper;
		private readonly DataExchangeSettings _dataExchangeSettings;

		public ExportController(
			ICommonServices services,
			IExportService exportService,
			PluginMediator pluginMediator,
			ICategoryService categoryService,
			IManufacturerService manufacturerService,
			IProductTagService productTagService,
			ICustomerService customerService,
			ILanguageService languageService,
			ICurrencyService currencyService,
			IEmailAccountService emailAccountService,
			IComponentContext componentContext,
			IDateTimeHelper dateTimeHelper,
			DataExchangeSettings dataExchangeSettings)
		{
			_services = services;
			_exportService = exportService;
			_pluginMediator = pluginMediator;
			_categoryService = categoryService;
			_manufacturerService = manufacturerService;
			_productTagService = productTagService;
			_customerService = customerService;
			_languageService = languageService;
			_currencyService = currencyService;
			_emailAccountService = emailAccountService;
			_componentContext = componentContext;
			_dateTimeHelper = dateTimeHelper;
			_dataExchangeSettings = dataExchangeSettings;
		}

		#region Utilities

		private string GetThumbnailUrl(Provider<IExportProvider> provider)
		{
			string url = null;

			if (provider != null)
				url = _pluginMediator.GetIconUrl(provider.Metadata);

			if (url.IsEmpty())
				url = _pluginMediator.GetDefaultIconUrl(null);

			url = Url.Content(url);

			return url;
		}

		private void PrepareProfileModel(ExportProfileModel model, ExportProfile profile, Provider<IExportProvider> provider)
		{
			model.Id = profile.Id;
			model.Name = profile.Name;
			model.ProviderSystemName = profile.ProviderSystemName;
			model.FolderName = profile.FolderName;
			model.FileNamePattern = profile.FileNamePattern;
			model.Enabled = profile.Enabled;
			model.ScheduleTaskId = profile.SchedulingTaskId;
			model.ScheduleTaskName = profile.ScheduleTask.Name.NaIfEmpty();
			model.IsTaskRunning = profile.ScheduleTask.IsRunning;
			model.IsTaskEnabled = profile.ScheduleTask.Enabled;
			model.LogFileExists = System.IO.File.Exists(profile.GetExportLogFilePath());
			model.HasActiveProvider = (provider != null);

			model.Provider = new ExportProfileModel.ProviderModel();
			model.Provider.ThumbnailUrl = GetThumbnailUrl(provider);

			if (provider != null)
			{
				var descriptor = provider.Metadata.PluginDescriptor;

				if (descriptor != null)
				{
					model.Provider.Url = descriptor.Url;
					model.Provider.ConfigurationUrl = Url.Action("ConfigurePlugin", "Plugin", new { systemName = descriptor.SystemName, area = "Admin" });
					model.Provider.Author = descriptor.Author;
					model.Provider.Version = descriptor.Version.ToString();
				}

				model.Provider.FriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata);
				model.Provider.Description = _pluginMediator.GetLocalizedDescription(provider.Metadata);
				model.Provider.EntityType = provider.Value.EntityType;
				model.Provider.EntityTypeName = provider.Value.EntityType.GetLocalizedEnum(_services.Localization, _services.WorkContext);
				model.Provider.FileExtension = provider.Value.FileExtension;
			}
		}

		private void PrepareProfileModelForEdit(ExportProfileModel model, ExportProfile profile, Provider<IExportProvider> provider)
		{
			var filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
			var projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);

			var allStores = _services.StoreService.GetAllStores();
			var allLanguages = _languageService.GetAllLanguages(true);
			var allCurrencies = _currencyService.GetAllCurrencies(true);
			var allEmailAccounts = _emailAccountService.GetAllEmailAccounts();

			model.AllString = T("Admin.Common.All");
			model.UnspecifiedString = T("Common.Unspecified");
			model.StoreCount = allStores.Count;
			model.Offset = profile.Offset;
			model.Limit = profile.Limit;
			model.BatchSize = profile.BatchSize;
			model.PerStore = profile.PerStore;
			model.EmailAccountId = profile.EmailAccountId;
			model.CompletedEmailAddresses = profile.CompletedEmailAddresses;
			model.CreateZipArchive = profile.CreateZipArchive;
			model.Cleanup = profile.Cleanup;

			model.FileNamePatternExample = profile.ResolveFileNamePattern(_services.StoreContext.CurrentStore, 1, _dataExchangeSettings.MaxFileNameLength);

			model.AvailableEmailAccounts = allEmailAccounts
				.Select(x => new SelectListItem { Text = x.FriendlyName, Value = x.Id.ToString() })
				.ToList();

			model.SerializedCompletedEmailAddresses = string.Join(",", profile.CompletedEmailAddresses.SplitSafe(",").Select(x => x.EncodeJsString()));

			// projection
			model.Projection = new ExportProjectionModel
			{
				StoreId = projection.StoreId,
				LanguageId = projection.LanguageId,
				CurrencyId = projection.CurrencyId,
				CustomerId = projection.CustomerId,
				DescriptionMergingId = projection.DescriptionMergingId,
				DescriptionToPlainText = projection.DescriptionToPlainText,
				AppendDescriptionText = projection.AppendDescriptionText,
				RemoveCriticalCharacters = projection.RemoveCriticalCharacters,
				CriticalCharacters = projection.CriticalCharacters,
				PriceType = projection.PriceType,
				ConvertNetToGrossPrices = projection.ConvertNetToGrossPrices,
				Brand = projection.Brand,
				PictureSize = projection.PictureSize,
				ShippingTime = projection.ShippingTime,
				ShippingCosts = projection.ShippingCosts,
				FreeShippingThreshold = projection.FreeShippingThreshold,
				AttributeCombinationAsProduct = projection.AttributeCombinationAsProduct,
				AttributeCombinationValueMergingId = projection.AttributeCombinationValueMergingId,
				NoGroupedProducts = projection.NoGroupedProducts,
				OrderStatusChangeId = projection.OrderStatusChangeId
			};

			model.Projection.AvailableStores = allStores
				.Select(y => new SelectListItem { Text = y.Name, Value = y.Id.ToString() })
				.ToList();

			model.Projection.AvailableLanguages = allLanguages
				.Select(y => new SelectListItem { Text = y.Name, Value = y.Id.ToString() })
				.ToList();

			model.Projection.AvailableCurrencies = allCurrencies
				.Select(y => new SelectListItem { Text = y.Name, Value = y.Id.ToString() })
				.ToList();

			// filtering
			model.Filter = new ExportFilterModel
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
				ManufacturerId = filter.ManufacturerId,
				WithoutManufacturers = filter.WithoutManufacturers,
				ProductTagId = filter.ProductTagId,
				FeaturedProducts = filter.FeaturedProducts,
				ProductType = filter.ProductType,
				IdMinimum = filter.IdMinimum,
				IdMaximum = filter.IdMaximum,
				OrderStatusIds = filter.OrderStatusIds,
				PaymentStatusIds = filter.PaymentStatusIds,
				ShippingStatusIds = filter.ShippingStatusIds,
				CustomerRoleIds = filter.CustomerRoleIds
			};

			model.Filter.AvailableStores = allStores
				.Select(y => new SelectListItem { Text = y.Name, Value = y.Id.ToString() })
				.ToList();

			// deployment
			model.Deployments = profile.Deployments
				.Select(x =>
				{
					var deploymentModel = PrepareDeploymentModel(x, null, false);

					if (x.IsPublic)
					{
						try
						{
							var publicFolder = Path.Combine(HttpRuntime.AppDomainAppPath, ExportProfileTask.PublicFolder);
							var resultInfo = XmlHelper.Deserialize<ExportExecuteResult>(profile.ResultInfo);

							if (resultInfo != null && resultInfo.Files != null)
							{
								foreach (var fileInfo in resultInfo.Files)
								{
									if (System.IO.File.Exists(Path.Combine(publicFolder, fileInfo.FileName)) && !deploymentModel.PublicFiles.Any(y => y.FileName == fileInfo.FileName))
									{
										var store = allStores.FirstOrDefault(y => y.Id == fileInfo.StoreId) ?? _services.StoreContext.CurrentStore;

										deploymentModel.PublicFiles.Add(new ExportDeploymentModel.PublicFile
										{
											StoreId = store.Id,
											StoreName = store.Name,
											FileName = fileInfo.FileName,
											FileUrl = string.Concat(store.Url.EnsureEndsWith("/"), ExportProfileTask.PublicFolder.EnsureEndsWith("/"), fileInfo.FileName)
										});
									}
								}
							}
						}
						catch (Exception exc)
						{
							exc.Dump();
						}
					}

					return deploymentModel;
				})
				.ToList();


			if (provider != null)
			{
				model.Provider.Supporting = provider.Metadata.ExportSupport;

				if (model.Provider.EntityType == ExportEntityType.Product)
				{
					var allCategories = _categoryService.GetAllCategories(showHidden: true);
					var mappedCategories = allCategories.ToDictionary(x => x.Id);
					var allManufacturers = _manufacturerService.GetAllManufacturers(true);
					var allProductTags = _productTagService.GetAllProductTags();

					model.Projection.AvailableDescriptionMergings = ExportDescriptionMerging.Description.ToSelectList(false);
					model.Projection.AvailablePriceTypes = PriceDisplayType.LowestPrice.ToSelectList(false);
					model.Projection.AvailableAttributeCombinationValueMerging = ExportAttributeValueMerging.AppendAllValuesToName.ToSelectList(false);

					model.Projection.SerializedAppendDescriptionText = string.Join(",", projection.AppendDescriptionText.SplitSafe(",").Select(x => x.EncodeJsString()));
					model.Projection.SerializedCriticalCharacters = string.Join(",", projection.CriticalCharacters.SplitSafe(",").Select(x => x.EncodeJsString()));

					model.Filter.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();

					model.Filter.AvailableCategories = allCategories
						.Select(x => new SelectListItem { Text = x.GetCategoryNameWithPrefix(_categoryService, mappedCategories), Value = x.Id.ToString() })
						.ToList();

					model.Filter.AvailableManufacturers = allManufacturers
						.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
						.ToList();

					model.Filter.AvailableProductTags = allProductTags
						.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
						.ToList();

				}
				else if (model.Provider.EntityType == ExportEntityType.Order)
				{
					var allCustomerRoles = _customerService.GetAllCustomerRoles(true);

					model.Projection.AvailableOrderStatusChange = ExportOrderStatusChange.Processing.ToSelectList(false);

					model.Filter.AvailableOrderStates = OrderStatus.Pending.ToSelectList(false).ToList();
					model.Filter.AvailablePaymentStates = PaymentStatus.Pending.ToSelectList(false).ToList();
					model.Filter.AvailableShippingStates = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();

					model.Filter.AvailableCustomerRoles = allCustomerRoles
						.OrderBy(x => x.Name)
						.Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
						.ToList();
				}

				try
				{
					var configInfo = provider.Value.ConfigurationInfo;
					if (configInfo != null)
					{
						model.Provider.ConfigPartialViewName = configInfo.PartialViewName;
						model.Provider.ConfigDataType = configInfo.ModelType;
						model.Provider.ConfigData = XmlHelper.Deserialize(profile.ProviderConfigData, configInfo.ModelType);

						if (configInfo.Initialize != null)
						{
							try
							{
								configInfo.Initialize(model.Provider.ConfigData);
							}
							catch (Exception exc)
							{
								NotifyWarning(exc.ToAllMessages());
							}
						}
					}
				}
				catch (Exception exc)
				{
					NotifyError(exc);
				}
			}
		}

		private ExportDeploymentModel PrepareDeploymentModel(ExportDeployment deployment, Provider<IExportProvider> provider, bool forEdit)
		{
			var model = new ExportDeploymentModel
			{
				Id = deployment.Id,
				ProfileId = deployment.ProfileId,
				Name = deployment.Name,
				Enabled = deployment.Enabled,
				IsPublic = deployment.IsPublic,
				CreateZip = deployment.CreateZip,
				DeploymentType = deployment.DeploymentType,
				DeploymentTypeName = deployment.DeploymentType.GetLocalizedEnum(_services.Localization, _services.WorkContext),
				Username = deployment.Username,
				Password = deployment.Password,
				Url = deployment.Url,
				HttpTransmissionType = deployment.HttpTransmissionType,
				FileSystemPath = deployment.FileSystemPath,
				EmailAddresses = deployment.EmailAddresses,
				EmailSubject = deployment.EmailSubject,
				EmailAccountId = deployment.EmailAccountId,
				PassiveMode = deployment.PassiveMode,
				UseSsl = deployment.UseSsl,
				PublicFiles = new List<ExportDeploymentModel.PublicFile>()
			};

			if (forEdit)
			{
				var allEmailAccounts = _emailAccountService.GetAllEmailAccounts();

				model.AvailableDeploymentTypes = ExportDeploymentType.FileSystem.ToSelectList(false).ToList();
				model.AvailableHttpTransmissionTypes = ExportHttpTransmissionType.SimplePost.ToSelectList(false).ToList();

				model.SerializedEmailAddresses = string.Join(",", deployment.EmailAddresses.SplitSafe(",").Select(x => x.EncodeJsString()));

				model.AvailableEmailAccounts = allEmailAccounts
					.Select(x => new SelectListItem { Text = x.FriendlyName, Value = x.Id.ToString() })
					.ToList();

				if (provider != null)
				{
					model.ThumbnailUrl = GetThumbnailUrl(provider);
				}
			}

			return model;
		}

		private void ModelToEntity(ExportDeploymentModel model, ExportDeployment deployment)
		{
			deployment.ProfileId = model.ProfileId;
			deployment.Name = model.Name;
			deployment.Enabled = model.Enabled;
			deployment.DeploymentType = model.DeploymentType;
			deployment.IsPublic = model.IsPublic;
			deployment.CreateZip = model.CreateZip;
			deployment.Username = model.Username;
			deployment.Password = model.Password;
			deployment.Url = model.Url;
			deployment.HttpTransmissionType = model.HttpTransmissionType;
			deployment.FileSystemPath = model.FileSystemPath;
			deployment.EmailAddresses = model.EmailAddresses;
			deployment.EmailSubject = model.EmailSubject;
			deployment.EmailAccountId = model.EmailAccountId;
			deployment.PassiveMode = model.PassiveMode;
			deployment.UseSsl = model.UseSsl;
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

			var providers = _exportService.LoadAllExportProviders().ToList();
			var profiles = _exportService.GetExportProfiles().ToList();
			var model = new List<ExportProfileModel>();

			foreach (var profile in profiles)
			{
				var profileModel = new ExportProfileModel();

				PrepareProfileModel(profileModel, profile, providers.FirstOrDefault(x => x.Metadata.SystemName == profile.ProviderSystemName));

				profileModel.TaskModel = profile.ScheduleTask.ToScheduleTaskModel(_services.Localization, _dateTimeHelper, Url);

				model.Add(profileModel);
			}

			return View(model);
		}

		public ActionResult Create()
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return Content(T("Admin.AccessDenied.Description"));

			var count = 0;
			var allProviders = _exportService.LoadAllExportProviders(0, false);

			var model = new ExportProfileModel();
			model.UnspecifiedString = T("Common.Unspecified");

			model.Provider = new ExportProfileModel.ProviderModel();

			model.AvailableProviders = allProviders
				.Select(x =>
				{
					var item = new ExportProfileModel.ProviderSelectItem
					{
						Id = ++count,
						SystemName = x.Metadata.SystemName,
						FriendlyName = _pluginMediator.GetLocalizedFriendlyName(x.Metadata),
						ImageUrl = GetThumbnailUrl(x),
						Description = _pluginMediator.GetLocalizedDescription(x.Metadata)
					};
					return item;
				})
				.ToList();

			model.AvailableProfiles = _exportService.GetExportProfiles()
				.ToList()
				.Select(x =>
				{
					var item = new ExportProfileModel.ProviderSelectItem
					{
						Id = x.Id,
						SystemName = x.ProviderSystemName,
						FriendlyName = x.Name,
						ImageUrl = GetThumbnailUrl(allProviders.FirstOrDefault(y => y.Metadata.SystemName.IsCaseInsensitiveEqual(x.ProviderSystemName)))
					};
					return item;
				})
				.ToList();

			return PartialView(model);			
		}

		[HttpPost]
		public ActionResult Create(ExportProfileModel model)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			if (model.ProviderSystemName.HasValue())
			{
				var provider = _exportService.LoadProvider(model.ProviderSystemName);
				if (provider != null)
				{
					var profile = _exportService.InsertExportProfile(provider, model.CloneProfileId ?? 0);

					return RedirectToAction("Edit", new { id = profile.Id });
				}
			}

			NotifyError(T("Admin.Common.ProviderNotLoaded", model.ProviderSystemName.NaIfEmpty()));

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
			profile.FileNamePattern = model.FileNamePattern;
			profile.FolderName = model.FolderName;
			profile.Enabled = model.Enabled;
			profile.Offset = model.Offset;
			profile.Limit = model.Limit;
			profile.BatchSize = model.BatchSize;
			profile.PerStore = model.PerStore;
			profile.CompletedEmailAddresses = model.CompletedEmailAddresses;
			profile.EmailAccountId = model.EmailAccountId ?? 0;
			profile.CreateZipArchive = model.CreateZipArchive;
			profile.Cleanup = model.Cleanup;

			if (profile.Name.IsEmpty())
				profile.Name = provider.Metadata.FriendlyName;

			if (profile.Name.IsEmpty())
				profile.Name = provider.Metadata.SystemName;

			// projection
			if (model.Projection != null)
			{
				var projection = new ExportProjection
				{
					StoreId = model.Projection.StoreId,
					LanguageId = model.Projection.LanguageId,
					CurrencyId = model.Projection.CurrencyId,
					CustomerId = model.Projection.CustomerId,
					DescriptionMergingId = model.Projection.DescriptionMergingId,
					DescriptionToPlainText = model.Projection.DescriptionToPlainText,
					AppendDescriptionText = model.Projection.AppendDescriptionText,
					RemoveCriticalCharacters = model.Projection.RemoveCriticalCharacters,
					CriticalCharacters = model.Projection.CriticalCharacters,
					PriceType = model.Projection.PriceType,
					ConvertNetToGrossPrices = model.Projection.ConvertNetToGrossPrices,
					Brand = model.Projection.Brand,
					PictureSize = model.Projection.PictureSize,
					ShippingTime = model.Projection.ShippingTime,
					ShippingCosts = model.Projection.ShippingCosts,
					FreeShippingThreshold = model.Projection.FreeShippingThreshold,
					AttributeCombinationAsProduct = model.Projection.AttributeCombinationAsProduct,
					AttributeCombinationValueMergingId = model.Projection.AttributeCombinationValueMergingId,
					NoGroupedProducts = model.Projection.NoGroupedProducts,
					OrderStatusChangeId = model.Projection.OrderStatusChangeId
				};

				profile.Projection = XmlHelper.Serialize<ExportProjection>(projection);
			}

			// filtering
			if (model.Filter != null)
			{
				var filter = new ExportFilter
				{
					StoreId = model.Filter.StoreId ?? 0,
					CreatedFrom = model.Filter.CreatedFrom,
					CreatedTo = model.Filter.CreatedTo,
					PriceMinimum = model.Filter.PriceMinimum,
					PriceMaximum = model.Filter.PriceMaximum,
					AvailabilityMinimum = model.Filter.AvailabilityMinimum,
					AvailabilityMaximum = model.Filter.AvailabilityMaximum,
					IsPublished = model.Filter.IsPublished,
					CategoryIds = model.Filter.CategoryIds,
					WithoutCategories = model.Filter.WithoutCategories,
					ManufacturerId = model.Filter.ManufacturerId,
					WithoutManufacturers = model.Filter.WithoutManufacturers,
					ProductTagId = model.Filter.ProductTagId,
					FeaturedProducts = model.Filter.FeaturedProducts,
					ProductType = model.Filter.ProductType,
					IdMinimum = model.Filter.IdMinimum,
					IdMaximum = model.Filter.IdMaximum,
					OrderStatusIds = model.Filter.OrderStatusIds,
					PaymentStatusIds = model.Filter.PaymentStatusIds,
					ShippingStatusIds = model.Filter.ShippingStatusIds,
					CustomerRoleIds = model.Filter.CustomerRoleIds
				};

				profile.Filtering = XmlHelper.Serialize<ExportFilter>(filter);
			}

			// provider configuration
			profile.ProviderConfigData = null;
			try
			{
				var configInfo = provider.Value.ConfigurationInfo;
				if (configInfo != null && model.CustomProperties.ContainsKey("ProviderConfigData"))
				{
					profile.ProviderConfigData = XmlHelper.Serialize(model.CustomProperties["ProviderConfigData"], configInfo.ModelType);
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

			if (!profile.Enabled)
			{
				NotifyInfo(T("Admin.DataExchange.Export.EnableProfileForPreview"));

				return RedirectToAction("Edit", new { id = profile.Id });
			}

			var provider = _exportService.LoadProvider(profile.ProviderSystemName);

			var task = new ExportProfileTask();
			var totalRecords = task.GetRecordCount(profile, provider, _componentContext);

			var model = new ExportPreviewModel
			{
				Id = profile.Id,
				Name = profile.Name,
				ThumbnailUrl = GetThumbnailUrl(provider),
				GridPageSize = ExportProfileTask.PageSize,
				EntityType = provider.Value.EntityType,
				TotalRecords = totalRecords,
				LogFileExists = System.IO.File.Exists(profile.GetExportLogFilePath())
			};

			return View(model);
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult PreviewList(GridCommand command, int id, int totalRecords)
		{
			ExportProfile profile = null;
			Provider<IExportProvider> provider = null;

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageExports) &&
				(profile = _exportService.GetExportProfileById(id)) != null &&
				(provider = _exportService.LoadProvider(profile.ProviderSystemName)) != null)
			{
				var productModel = new List<ExportPreviewProductModel>();
				var orderModel = new List<ExportPreviewOrderModel>();
				var task = new ExportProfileTask();

				Action<dynamic> previewData = x =>
				{
					if (provider.Value.EntityType == ExportEntityType.Product)
					{
						var product = x._Entity as Product;
						var pm = new ExportPreviewProductModel
						{
							Id = x.Id,
							ProductTypeId = x.ProductTypeId,
							ProductTypeName = product.GetProductTypeLabel(_services.Localization),
							ProductTypeLabelHint = product.ProductTypeLabelHint,
							Name = x.Name,
							Sku = x.Sku,
							Price = x.Price,
							Published = x.Published,
							StockQuantity = x.StockQuantity,
							AdminComment = x.AdminComment
						};

						productModel.Add(pm);
					}
					else if (provider.Value.EntityType == ExportEntityType.Order)
					{
						var om = new ExportPreviewOrderModel
						{
							Id = x.Id,
							HasNewPaymentNotification = x.HasNewPaymentNotification,
							OrderNumber = x.OrderNumber,
							OrderStatus = x.OrderStatus,
							PaymentStatus = x.PaymentStatus,
							ShippingStatus = x.ShippingStatus,
							CustomerEmail = x.Customer.Email,
							StoreName = (x.Store == null ? "".NaIfEmpty() : x.Store.Name),
							CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc),
							OrderTotal = x.OrderTotal
						};

						orderModel.Add(om);
					}
				};

				task.Preview(profile, provider, _componentContext, command.Page - 1, totalRecords, previewData);

				var normalizedTotal = totalRecords;

				if (profile.Limit > 0 && normalizedTotal > profile.Limit)
					normalizedTotal = profile.Limit;

				if (provider.Value.EntityType == ExportEntityType.Product)
				{
					return new JsonResult
					{
						Data = new GridModel<ExportPreviewProductModel> { Data = productModel, Total = normalizedTotal }
					};
				}

				if (provider.Value.EntityType == ExportEntityType.Order)
				{
					return new JsonResult
					{
						Data = new GridModel<ExportPreviewOrderModel> { Data = orderModel, Total = normalizedTotal }
					};
				}
			}

			return new EmptyResult();
		}

		[HttpPost]
		public ActionResult Execute(int id, string selectedIds, bool exportAll)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			profile.CacheSelectedEntityIds(selectedIds);

			var returnUrl = Url.Action("List", "Export", new { area = "admin" });

			return RedirectToAction("RunJob", "ScheduleTask", new { area = "admin", id = profile.SchedulingTaskId, returnUrl = returnUrl });
		}

		[ChildActionOnly]
		public ActionResult InfoProfile(string systemName, string returnUrl)
		{
			var profiles = _exportService.GetExportProfilesBySystemName(systemName);

			var model = new ProfileInfoForProviderModel
			{
				ReturnUrl = returnUrl,
				SystemName = systemName
			};

			model.Profiles = profiles
				.OrderBy(x => x.Enabled)
				.Select(x =>
				{
					var profileModel = new ProfileInfoForProviderModel.ProfileModel
					{
						Id = x.Id,
						Name = x.Name,
						Enabled = x.Enabled,
						ScheduleTaskId = (x.Enabled ? x.SchedulingTaskId : (int?)null)
					};

					return profileModel;
				})
				.ToList();

			return PartialView(model);
		}

		public ActionResult DownloadLogFile(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var path = profile.GetExportLogFilePath();
			var stream = new FileStream(path, FileMode.Open);

			var result = new FileStreamResult(stream, MediaTypeNames.Text.Plain);
			result.FileDownloadName = profile.Name.ToValidFileName() + "-log.txt";

			return result;
		}

		public ActionResult ResolveFileNamePatternExample(int id, string pattern)
		{
			var profile = _exportService.GetExportProfileById(id);
			
			_services.DbContext.DetachEntity<ExportProfile>(profile);
			profile.FileNamePattern = pattern.EmptyNull();

			var provider = _exportService.LoadProvider(profile.ProviderSystemName);

			var resolvedPattern = profile.ResolveFileNamePattern(_services.StoreContext.CurrentStore, 1, _dataExchangeSettings.MaxFileNameLength);

			return this.Content(resolvedPattern);
		}

		#region Export deployment

		public ActionResult CreateDeployment(int id)
		{
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageExports))
				return AccessDeniedView();

			var profile = _exportService.GetExportProfileById(id);
			if (profile == null)
				return RedirectToAction("List");

			var provider = _exportService.LoadProvider(profile.ProviderSystemName);

			//var fileSystemName = ExportDeploymentType.FileSystem.GetLocalizedEnum(_services.Localization, _services.WorkContext);

			var model = PrepareDeploymentModel(new ExportDeployment
			{
				ProfileId = id,
				Enabled = true,
				DeploymentType = ExportDeploymentType.FileSystem,
				Name = profile.Name
			}, provider, true);

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

			var provider = _exportService.LoadProvider(deployment.Profile.ProviderSystemName);

			var model = PrepareDeploymentModel(deployment, provider, true);

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

			model = PrepareDeploymentModel(deployment, _exportService.LoadProvider(deployment.Profile.ProviderSystemName), true);

			return View(model);
		}

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