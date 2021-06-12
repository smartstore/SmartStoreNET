using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Web.Mvc;
using SmartStore.Admin.Models.DataExchange;
using SmartStore.Admin.Models.Tasks;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Domain;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Domain.Shipping;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Tasks;
using SmartStore.Core.IO;
using SmartStore.Core.Plugins;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
using SmartStore.Services.DataExchange.Export;
using SmartStore.Services.DataExchange.Export.Deployment;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Tasks;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Plugins;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class ExportController : AdminControllerBase
    {
        #region Fields

        private readonly IExportProfileService _exportService;
        private readonly PluginMediator _pluginMediator;
        private readonly ICategoryService _categoryService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IProductTagService _productTagService;
        private readonly ICustomerService _customerService;
        private readonly ILanguageService _languageService;
        private readonly ICurrencyService _currencyService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ICountryService _countryService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly AdminModelHelper _adminModelHelper;
        private readonly ITaskScheduler _taskScheduler;
        private readonly IDataExporter _dataExporter;
        private readonly DataExchangeSettings _dataExchangeSettings;
        private readonly Lazy<CustomerSettings> _customerSettings;

        #endregion

        #region Constructor

        public ExportController(
            IExportProfileService exportService,
            PluginMediator pluginMediator,
            ICategoryService categoryService,
            IManufacturerService manufacturerService,
            IProductTagService productTagService,
            ICustomerService customerService,
            ILanguageService languageService,
            ICurrencyService currencyService,
            IEmailAccountService emailAccountService,
            IScheduleTaskService scheduleTaskService,
            ICountryService countryService,
            IDateTimeHelper dateTimeHelper,
            AdminModelHelper adminModelHelper,
            ITaskScheduler taskScheduler,
            IDataExporter dataExporter,
            DataExchangeSettings dataExchangeSettings,
            Lazy<CustomerSettings> customerSettings)
        {
            _exportService = exportService;
            _pluginMediator = pluginMediator;
            _categoryService = categoryService;
            _manufacturerService = manufacturerService;
            _productTagService = productTagService;
            _customerService = customerService;
            _languageService = languageService;
            _currencyService = currencyService;
            _emailAccountService = emailAccountService;
            _scheduleTaskService = scheduleTaskService;
            _countryService = countryService;
            _dateTimeHelper = dateTimeHelper;
            _adminModelHelper = adminModelHelper;
            _taskScheduler = taskScheduler;
            _dataExporter = dataExporter;
            _dataExchangeSettings = dataExchangeSettings;
            _customerSettings = customerSettings;
        }

        #endregion

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

        private void ModelToEntity(ExportDeploymentModel model, ExportDeployment deployment)
        {
            MiniMapper.Map(model, deployment);

            deployment.EmailAddresses = string.Join(",", model.EmailAddresses ?? new string[0]);
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

            return continueEditing ?
                RedirectToAction("EditDeployment", new { id = deploymentId }) :
                RedirectToAction("Edit", new { id = profileId });
        }

        private void AddFileInfo(
            List<ExportFileDetailsModel.FileInfo> list,
            string path,
            DataExportResult.ExportFileInfo fileInfo = null,
            string publicFolderUrl = null,
            Store store = null)
        {
            if (System.IO.File.Exists(path) && !list.Any(x => x.FilePath == path))
            {
                var fi = new ExportFileDetailsModel.FileInfo
                {
                    FilePath = path,
                    FileName = Path.GetFileName(path),
                    FileExtension = Path.GetExtension(path)
                };

                fi.DisplayOrder = fi.FileExtension.IsCaseInsensitiveEqual(".zip") ? 0 : 1;

                if (fileInfo != null)
                {
                    fi.RelatedType = fileInfo.RelatedType;

                    if (fileInfo.Label.HasValue())
                    {
                        fi.Label = fileInfo.Label;
                    }
                    else
                    {
                        fi.Label = T("Admin.Common.Data");

                        if (fileInfo.RelatedType.HasValue)
                        {
                            fi.Label = string.Concat(fi.Label, " ", fileInfo.RelatedType.Value.GetLocalizedEnum(Services.Localization, Services.WorkContext));
                        }
                    }
                }

                if (store != null)
                {
                    fi.StoreId = store.Id;
                    fi.StoreName = store.Name;
                }

                if (publicFolderUrl.HasValue())
                {
                    fi.FileUrl = publicFolderUrl + fi.FileName;
                }

                list.Add(fi);
            }
        }

        private ExportFileDetailsModel CreateFileDetailsModel(ExportProfile profile, Provider<IExportProvider> provider, ExportDeployment deployment)
        {
            var model = new ExportFileDetailsModel
            {
                Id = (deployment == null ? profile.Id : deployment.Id),
                IsForDeployment = (deployment != null),
                ExportFiles = new List<ExportFileDetailsModel.FileInfo>(),
                PublicFiles = new List<ExportFileDetailsModel.FileInfo>()
            };

            try
            {
                // add export files
                var zipPath = profile.GetExportZipPath();
                var resultInfo = XmlHelper.Deserialize<DataExportResult>(profile.ResultInfo);

                if (deployment == null)
                {
                    AddFileInfo(model.ExportFiles, zipPath);

                    if (resultInfo.Files != null)
                    {
                        var exportFolder = profile.GetExportFolder(true);

                        resultInfo.Files.Each(x => AddFileInfo(model.ExportFiles, Path.Combine(exportFolder, x.FileName), x));
                    }
                }
                else if (deployment.DeploymentType == ExportDeploymentType.FileSystem)
                {
                    if (resultInfo.Files != null)
                    {
                        var deploymentFolder = deployment.GetDeploymentFolder();

                        resultInfo.Files.Each(x => AddFileInfo(model.ExportFiles, Path.Combine(deploymentFolder, x.FileName), x));
                    }
                }

                // Add public files.
                var publicDeployment = deployment == null
                    ? profile.Deployments.FirstOrDefault(x => x.DeploymentType == ExportDeploymentType.PublicFolder)
                    : (deployment.DeploymentType == ExportDeploymentType.PublicFolder ? deployment : null);

                if (publicDeployment != null)
                {
                    var currentStore = Services.StoreContext.CurrentStore;
                    var deploymentFolder = publicDeployment.GetDeploymentFolder();

                    // note public folder not cleaned up during export, so only display files that has been created during last export.
                    // otherwise the merchant might publish URLs of old export files.
                    if (profile.CreateZipArchive)
                    {
                        AddFileInfo(
                            model.PublicFiles,
                            Path.Combine(deploymentFolder, Path.GetFileName(zipPath)),
                            null,
                            publicDeployment.GetPublicFolderUrl(Services, currentStore));
                    }
                    else if (resultInfo.Files != null)
                    {
                        var allStores = Services.StoreService.GetAllStores();

                        foreach (var file in resultInfo.Files)
                        {
                            var store = file.StoreId == 0 ? null : allStores.FirstOrDefault(x => x.Id == file.StoreId);

                            AddFileInfo(
                                model.PublicFiles,
                                Path.Combine(deploymentFolder, file.FileName),
                                file,
                                publicDeployment.GetPublicFolderUrl(Services, store ?? currentStore),
                                store);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return model;
        }

        private ExportDeploymentModel CreateDeploymentModel(ExportProfile profile, ExportDeployment deployment, Provider<IExportProvider> provider, bool forEdit)
        {
            var model = new ExportDeploymentModel();

            MiniMapper.Map(deployment, model);

            model.EmailAddresses = deployment.EmailAddresses.SplitSafe(",");
            model.DeploymentTypeName = deployment.DeploymentType.GetLocalizedEnum(Services.Localization, Services.WorkContext);
            model.PublicFolderUrl = deployment.GetPublicFolderUrl(Services);

            if (forEdit)
            {
                var allEmailAccounts = _emailAccountService.GetAllEmailAccounts();

                model.CreateZip = profile.CreateZipArchive;
                model.AvailableDeploymentTypes = ExportDeploymentType.FileSystem.ToSelectList(false).ToList();
                model.AvailableHttpTransmissionTypes = ExportHttpTransmissionType.SimplePost.ToSelectList(false).ToList();
                model.AvailableEmailAddresses = new MultiSelectList(model.EmailAddresses);

                model.AvailableEmailAccounts = allEmailAccounts
                    .Select(x => new SelectListItem { Text = x.FriendlyName, Value = x.Id.ToString() })
                    .ToList();

                if (provider != null)
                {
                    model.ThumbnailUrl = GetThumbnailUrl(provider);
                }
            }
            else
            {
                var fileDetailsModel = CreateFileDetailsModel(profile, null, deployment);

                model.FileCount = fileDetailsModel.FileCount;
            }

            return model;
        }

        private void PrepareProfileModel(
            ExportProfileModel model,
            ExportProfile profile,
            Provider<IExportProvider> provider,
            ScheduleTaskHistory lastHistoryEntry)
        {
            MiniMapper.Map(profile, model);

            model.ScheduleTaskId = profile.SchedulingTaskId;
            model.ScheduleTaskName = profile.ScheduleTask.Name.NaIfEmpty();
            model.IsTaskRunning = lastHistoryEntry?.IsRunning ?? false;
            model.IsTaskEnabled = profile.ScheduleTask.Enabled;
            model.LogFileExists = System.IO.File.Exists(profile.GetExportLogPath());
            model.HasActiveProvider = provider != null;
            model.FileNamePatternDescriptions = T("Admin.DataExchange.Export.FileNamePatternDescriptions").Text.SplitSafe(";");

            model.Provider = new ExportProfileModel.ProviderModel();
            model.Provider.ThumbnailUrl = GetThumbnailUrl(provider);

            var descriptor = provider.Metadata.PluginDescriptor;

            if (descriptor != null)
            {
                model.Provider.Url = descriptor.Url;
                model.Provider.Author = descriptor.Author;
                model.Provider.Version = descriptor.Version.ToString();
            }

            model.Provider.FriendlyName = _pluginMediator.GetLocalizedFriendlyName(provider.Metadata);
            model.Provider.Description = _pluginMediator.GetLocalizedDescription(provider.Metadata);
            model.Provider.EntityType = provider.Value.EntityType;
            model.Provider.EntityTypeName = provider.Value.EntityType.GetLocalizedEnum(Services.Localization, Services.WorkContext);
            model.Provider.FileExtension = provider.Value.FileExtension;
        }

        private void PrepareProfileModelForEdit(ExportProfileModel model, ExportProfile profile, Provider<IExportProvider> provider)
        {
            var filter = XmlHelper.Deserialize<ExportFilter>(profile.Filtering);
            var projection = XmlHelper.Deserialize<ExportProjection>(profile.Projection);

            var language = Services.WorkContext.WorkingLanguage;
            var store = Services.StoreContext.CurrentStore;

            var allStores = Services.StoreService.GetAllStores();
            var allLanguages = _languageService.GetAllLanguages(true);
            var allCurrencies = _currencyService.GetAllCurrencies(true);
            var allEmailAccounts = _emailAccountService.GetAllEmailAccounts();

            model.AllString = T("Admin.Common.All");
            model.UnspecifiedString = T("Common.Unspecified");
            model.StoreCount = allStores.Count;
            model.Offset = profile.Offset;
            model.Limit = (profile.Limit == 0 ? (int?)null : profile.Limit);
            model.BatchSize = (profile.BatchSize == 0 ? (int?)null : profile.BatchSize);
            model.PerStore = profile.PerStore;
            model.EmailAccountId = profile.EmailAccountId;
            model.CompletedEmailAddresses = profile.CompletedEmailAddresses.SplitSafe(",");
            model.CreateZipArchive = profile.CreateZipArchive;
            model.Cleanup = profile.Cleanup;
            model.PrimaryStoreCurrencyCode = Services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            model.FileNamePatternExample = profile.ResolveFileNamePattern(store, 1, _dataExchangeSettings.MaxFileNameLength);

            model.AvailableEmailAccounts = allEmailAccounts
                .Select(x => new SelectListItem { Text = x.FriendlyName, Value = x.Id.ToString() })
                .ToList();

            model.AvailableCompletedEmailAddresses = new MultiSelectList(profile.CompletedEmailAddresses.SplitSafe(","));

            // Projection.
            model.Projection = new ExportProjectionModel();

            MiniMapper.Map(projection, model.Projection);

            model.Projection.NumberOfPictures = projection.NumberOfMediaFiles;
            model.Projection.AppendDescriptionText = projection.AppendDescriptionText.SplitSafe(",");
            model.Projection.CriticalCharacters = projection.CriticalCharacters.SplitSafe(",");

            if (profile.Projection.IsEmpty())
            {
                model.Projection.DescriptionMergingId = (int)ExportDescriptionMerging.Description;
            }

            model.Projection.AvailableStores = allStores
                .Select(y => new SelectListItem { Text = y.Name, Value = y.Id.ToString() })
                .ToList();

            model.Projection.AvailableLanguages = allLanguages
                .Select(y => new SelectListItem { Text = y.Name, Value = y.Id.ToString() })
                .ToList();

            model.Projection.AvailableCurrencies = allCurrencies
                .Select(y => new SelectListItem { Text = y.GetLocalized(z => z.Name), Value = y.Id.ToString() })
                .ToList();

            // Filtering.
            model.Filter = new ExportFilterModel();

            MiniMapper.Map(filter, model.Filter);

            model.Filter.AvailableLanguages = new List<SelectListItem>();
            model.Filter.AvailableLanguages.Add(new SelectListItem { Text = T("Common.Unspecified"), Value = "" });

            foreach (var lang in _languageService.GetAllLanguages())
            {
                model.Filter.AvailableLanguages.Add(new SelectListItem { Text = lang.Name, Value = lang.Id.ToString() });
            }

            // Deployment.
            model.Deployments = profile.Deployments
                .Select(x =>
                {
                    var deploymentModel = CreateDeploymentModel(profile, x, null, false);

                    if (x.ResultInfo.HasValue())
                    {
                        var resultInfo = XmlHelper.Deserialize<DataDeploymentResult>(x.ResultInfo);

                        deploymentModel.LastResult = new ExportDeploymentModel.LastResultInfo
                        {
                            Execution = _dateTimeHelper.ConvertToUserTime(resultInfo.LastExecutionUtc, DateTimeKind.Utc),
                            ExecutionPretty = resultInfo.LastExecutionUtc.RelativeFormat(true, "f"),
                            Error = resultInfo.LastError
                        };
                    }
                    return deploymentModel;
                })
                .ToList();


            if (provider != null)
            {
                model.Provider.Feature = provider.Metadata.ExportFeatures;

                if (model.Provider.EntityType == ExportEntityType.Product)
                {
                    var allManufacturers = _manufacturerService.GetAllManufacturers(true);
                    var allProductTags = _productTagService.GetAllProductTags();

                    model.Projection.AvailableAttributeCombinationValueMerging = ExportAttributeValueMerging.AppendAllValuesToName.ToSelectList(false);

                    model.Projection.AvailableDescriptionMergings = ExportDescriptionMerging.Description.ToSelectList(false);

                    model.Projection.AvailablePriceTypes = PriceDisplayType.LowestPrice
                        .ToSelectList(false)
                        .Where(x => x.Value != ((int)PriceDisplayType.Hide).ToString())
                        .ToList();

                    model.Projection.AvailableAppendDescriptionTexts = new MultiSelectList(projection.AppendDescriptionText.SplitSafe(","));
                    model.Projection.AvailableCriticalCharacters = new MultiSelectList(projection.CriticalCharacters.SplitSafe(","));

                    model.Filter.AvailableProductTypes = ProductType.SimpleProduct.ToSelectList(false).ToList();

                    if (model.Filter.CategoryIds?.Any() ?? false)
                    {
                        var tree = _categoryService.GetCategoryTree(includeHidden: true);

                        model.Filter.SelectedCategories = model.Filter.CategoryIds
                            .Where(x => x != 0)
                            .Select(x =>
                            {
                                var node = tree.SelectNodeById(x);
                                var item = new SelectListItem { Value = x.ToString(), Text = node == null ? x.ToString() : _categoryService.GetCategoryPath(node) };
                                return item;
                            })
                            .ToList();
                    }
                    else
                    {
                        model.Filter.SelectedCategories = new List<SelectListItem>();
                    }

                    model.Filter.AvailableManufacturers = allManufacturers
                        .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                        .ToList();

                    model.Filter.AvailableProductTags = allProductTags
                        .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                        .ToList();

                }
                else if (model.Provider.EntityType == ExportEntityType.Customer)
                {
                    var allCountries = _countryService.GetAllCountries(true);

                    model.Filter.AvailableCountries = allCountries
                        .Select(x => new SelectListItem { Text = x.GetLocalized(y => y.Name, language, true, false), Value = x.Id.ToString() })
                        .ToList();
                }
                else if (model.Provider.EntityType == ExportEntityType.Order)
                {
                    model.Projection.AvailableOrderStatusChange = ExportOrderStatusChange.Processing.ToSelectList(false);

                    model.Filter.AvailableOrderStates = OrderStatus.Pending.ToSelectList(false).ToList();
                    model.Filter.AvailablePaymentStates = PaymentStatus.Pending.ToSelectList(false).ToList();
                    model.Filter.AvailableShippingStates = ShippingStatus.NotYetShipped.ToSelectList(false).ToList();
                }
                else if (model.Provider.EntityType == ExportEntityType.ShoppingCartItem)
                {
                    model.Filter.AvailableShoppingCartTypes = ShoppingCartType.ShoppingCart.ToSelectList(false).ToList();
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
                catch (Exception ex)
                {
                    NotifyError(ex);
                }
            }
        }

        #endregion

        #region List

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Export.Read)]
        public ActionResult List()
        {
            var providers = _exportService.LoadAllExportProviders(0, false).ToList();
            var profiles = _exportService.GetExportProfiles().ToList();
            var lastHistoryEntries = _scheduleTaskService.GetHistoryEntries(0, int.MaxValue, 0, true, true).ToDictionarySafe(x => x.ScheduleTaskId);
            var model = new List<ExportProfileModel>();

            foreach (var profile in profiles)
            {
                var provider = providers.FirstOrDefault(x => x.Metadata.SystemName == profile.ProviderSystemName);
                if (provider != null)
                {
                    var profileModel = new ExportProfileModel();
                    var fileDetailsModel = CreateFileDetailsModel(profile, provider, null);

                    lastHistoryEntries.TryGetValue(profile.SchedulingTaskId, out var lastHistoryEntry);
                    PrepareProfileModel(profileModel, profile, provider, lastHistoryEntry);

                    profileModel.FileCount = fileDetailsModel.FileCount;
                    profileModel.TaskModel = _adminModelHelper.CreateScheduleTaskModel(profile.ScheduleTask, lastHistoryEntry) ?? new ScheduleTaskModel();

                    model.Add(profileModel);
                }
            }

            return View(model);
        }

        [Permission(Permissions.Configuration.Export.Read)]
        public ActionResult ProfileListDetails(int profileId)
        {
            var profile = _exportService.GetExportProfileById(profileId);
            if (profile != null)
            {
                var provider = _exportService.LoadProvider(profile.ProviderSystemName);
                if (provider != null && !provider.Metadata.IsHidden)
                {
                    var model = CreateFileDetailsModel(profile, provider, null);
                    return Json(this.RenderPartialViewToString("~/Administration/Views/Export/ProfileFileCount.cshtml", model.FileCount), JsonRequestBehavior.AllowGet);
                }
            }

            return new EmptyResult();
        }

        [Permission(Permissions.Configuration.Export.Read)]
        public ActionResult ProfileFileDetails(int profileId, int deploymentId)
        {
            if (profileId != 0)
            {
                var profile = _exportService.GetExportProfileById(profileId);
                if (profile != null)
                {
                    var provider = _exportService.LoadProvider(profile.ProviderSystemName);
                    if (provider != null && !provider.Metadata.IsHidden)
                    {
                        var model = CreateFileDetailsModel(profile, provider, null);
                        return PartialView(model);
                    }
                }
            }
            else if (deploymentId != 0)
            {
                var deployment = _exportService.GetExportDeploymentById(deploymentId);
                if (deployment != null)
                {
                    var model = CreateFileDetailsModel(deployment.Profile, null, deployment);
                    return PartialView(model);
                }
            }

            return new EmptyResult();
        }

        #endregion

        #region Create

        [Permission(Permissions.Configuration.Export.Create)]
        public ActionResult Create()
        {
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
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Export.Create)]
        public ActionResult Create(ExportProfileModel model)
        {
            if (model.ProviderSystemName.HasValue())
            {
                var provider = _exportService.LoadProvider(model.ProviderSystemName);
                if (provider != null)
                {
                    var profile = _exportService.InsertExportProfile(provider, false, null, model.CloneProfileId ?? 0);

                    return RedirectToAction("Edit", new { id = profile.Id });
                }
            }

            NotifyError(T("Admin.Common.ProviderNotLoaded", model.ProviderSystemName.NaIfEmpty()));

            return RedirectToAction("List");
        }

        #endregion

        #region Edit

        [Permission(Permissions.Configuration.Export.Read)]
        public ActionResult Edit(int id)
        {
            var profile = _exportService.GetExportProfileById(id);
            if (profile == null)
                return RedirectToAction("List");

            var provider = _exportService.LoadProvider(profile.ProviderSystemName);
            if (provider == null || provider.Metadata.IsHidden)
                return RedirectToAction("List");

            var model = new ExportProfileModel();

            PrepareProfileModel(model, profile, provider, _scheduleTaskService.GetLastHistoryEntryByTaskId(profile.SchedulingTaskId));
            PrepareProfileModelForEdit(model, profile, provider);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Export.Update)]
        public ActionResult Edit(ExportProfileModel model, bool continueEditing)
        {
            var profile = _exportService.GetExportProfileById(model.Id);
            if (profile == null)
                return RedirectToAction("List");

            var provider = _exportService.LoadProvider(profile.ProviderSystemName);
            if (provider == null || provider.Metadata.IsHidden)
                return RedirectToAction("List");

            if (!ModelState.IsValid)
            {
                PrepareProfileModel(model, profile, provider, _scheduleTaskService.GetLastHistoryEntryByTaskId(profile.SchedulingTaskId));
                PrepareProfileModelForEdit(model, profile, provider);
                return View(model);
            }

            profile.Name = model.Name;
            profile.FileNamePattern = model.FileNamePattern;
            profile.FolderName = model.FolderName;
            profile.Enabled = model.Enabled;
            profile.ExportRelatedData = model.ExportRelatedData;
            profile.Offset = model.Offset;
            profile.Limit = model.Limit ?? 0;
            profile.BatchSize = model.BatchSize ?? 0;
            profile.PerStore = model.PerStore;
            profile.CompletedEmailAddresses = string.Join(",", model.CompletedEmailAddresses ?? new string[0]);
            profile.EmailAccountId = model.EmailAccountId ?? 0;
            profile.CreateZipArchive = model.CreateZipArchive;
            profile.Cleanup = model.Cleanup;
            
            if (profile.Name.IsEmpty())
                profile.Name = provider.Metadata.FriendlyName;

            if (profile.Name.IsEmpty())
                profile.Name = provider.Metadata.SystemName;

            // Projection.
            if (model.Projection != null)
            {
                var projection = new ExportProjection();

                MiniMapper.Map(model.Projection, projection);

                projection.NumberOfMediaFiles = model.Projection.NumberOfPictures;
                projection.AppendDescriptionText = string.Join(",", model.Projection.AppendDescriptionText ?? new string[0]);
                projection.RemoveCriticalCharacters = model.Projection.RemoveCriticalCharacters;
                projection.CriticalCharacters = string.Join(",", model.Projection.CriticalCharacters ?? new string[0]);

                profile.Projection = XmlHelper.Serialize(projection);
            }

            // Filtering.
            if (model.Filter != null)
            {
                var filter = new ExportFilter();

                MiniMapper.Map(model.Filter, filter);

                filter.StoreId = model.Filter.StoreId ?? 0;
                filter.CategoryIds = model.Filter.CategoryIds?.Where(x => x != 0)?.ToArray() ?? new int[0];

                profile.Filtering = XmlHelper.Serialize(filter);
            }

            // Provider configuration.
            profile.ProviderConfigData = null;
            try
            {
                var configInfo = provider.Value.ConfigurationInfo;
                if (configInfo != null && model.CustomProperties.ContainsKey("ProviderConfigData"))
                {
                    profile.ProviderConfigData = XmlHelper.Serialize(model.CustomProperties["ProviderConfigData"], configInfo.ModelType);
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            _exportService.UpdateExportProfile(profile);

            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

            return (continueEditing ? RedirectToAction("Edit", new { id = profile.Id }) : RedirectToAction("List"));
        }



        [Permission(Permissions.Configuration.Export.Read)]
        public ActionResult ResolveFileNamePatternExample(int id, string pattern)
        {
            var profile = _exportService.GetExportProfileById(id);

            Services.DbContext.DetachEntity(profile);
            profile.FileNamePattern = pattern.EmptyNull();

            var resolvedPattern = profile.ResolveFileNamePattern(Services.StoreContext.CurrentStore, 1, _dataExchangeSettings.MaxFileNameLength);

            return Content(resolvedPattern);
        }

        #endregion

        #region Delete / Execute

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Export.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var profile = _exportService.GetExportProfileById(id);
            if (profile == null)
                return RedirectToAction("List");

            var provider = _exportService.LoadProvider(profile.ProviderSystemName);
            if (provider == null || provider.Metadata.IsHidden)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Export.Execute)]
        public ActionResult Execute(int id, string selectedIds)
        {
            // Permissions checked internally by DataExporter.
            var profile = _exportService.GetExportProfileById(id);
            if (profile == null)
            {
                return RedirectToAction("List");
            }

            var provider = _exportService.LoadProvider(profile.ProviderSystemName);
            if (provider == null || provider.Metadata.IsHidden)
            {
                return RedirectToAction("List");
            }

            var taskParams = new Dictionary<string, string>
            {
                { TaskExecutor.CurrentCustomerIdParamName, Services.WorkContext.CurrentCustomer.Id.ToString() },
                { TaskExecutor.CurrentStoreIdParamName, Services.StoreContext.CurrentStore.Id.ToString() }
            };

            if (selectedIds.HasValue())
            {
                taskParams.Add("SelectedIds", selectedIds);
            }

            _taskScheduler.RunSingleTask(profile.SchedulingTaskId, taskParams);

            NotifyInfo(T("Admin.System.ScheduleTasks.RunNow.Progress.DataExportTask"));

            return RedirectToReferrer(null, () => RedirectToAction("List"));
        }

        #endregion

        #region Preview

        [Permission(Permissions.Configuration.Export.Read)]
        public ActionResult Preview(int id)
        {
            var profile = _exportService.GetExportProfileById(id);
            if (profile == null)
            {
                return RedirectToAction("List");
            }

            var provider = _exportService.LoadProvider(profile.ProviderSystemName);
            if (provider == null || provider.Metadata.IsHidden)
            {
                return RedirectToAction("List");
            }

            if (!profile.Enabled)
            {
                NotifyInfo(T("Admin.DataExchange.Export.EnableProfileForPreview"));

                return RedirectToAction("Edit", new { id = profile.Id });
            }

            var request = new DataExportRequest(profile, provider);

            var model = new ExportPreviewModel
            {
                Id = profile.Id,
                Name = profile.Name,
                ThumbnailUrl = GetThumbnailUrl(provider),
                GridPageSize = DataExporter.PageSize,
                EntityType = provider.Value.EntityType,
                LogFileExists = System.IO.File.Exists(profile.GetExportLogPath()),
                UsernamesEnabled = _customerSettings.Value.CustomerLoginType != CustomerLoginType.Email
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Export.Read)]
        public ActionResult PreviewList(GridCommand command, int id)
        {
            ExportProfile profile = null;
            Provider<IExportProvider> provider = null;

            if ((profile = _exportService.GetExportProfileById(id)) == null ||
                (provider = _exportService.LoadProvider(profile.ProviderSystemName)) == null ||
                provider.Metadata.IsHidden)
            {
                return new JsonResult { Data = Enumerable.Empty<ExportPreviewProductModel>() };
            }

            object gridData = null;
            var pageIndex = command.Page - 1;
            var request = new DataExportRequest(profile, provider);
            var result = _dataExporter.Preview(request, pageIndex);

            var normalizedTotal = profile.Limit > 0 && result.TotalRecords > profile.Limit
                ? profile.Limit
                : result.TotalRecords;

            if (provider.Value.EntityType == ExportEntityType.Product)
            {
                var models = new List<ExportPreviewProductModel>();

                foreach (var item in result.Data)
                {
                    var product = item.Entity as Product;
                    var model = new ExportPreviewProductModel();
                    model.Id = product.Id;
                    model.ProductTypeId = product.ProductTypeId;
                    model.ProductTypeName = product.GetProductTypeLabel(Services.Localization);
                    model.ProductTypeLabelHint = product.ProductTypeLabelHint;
                    model.Name = item.Name;
                    model.Sku = item.Sku;
                    model.Price = item.Price;
                    model.Published = product.Published;
                    model.StockQuantity = product.StockQuantity;
                    model.AdminComment = item.AdminComment;
                    models.Add(model);
                }

                gridData = new GridModel<ExportPreviewProductModel> { Data = models, Total = normalizedTotal };
            }
            else if (provider.Value.EntityType == ExportEntityType.Order)
            {
                var models = new List<ExportPreviewOrderModel>();

                foreach (var item in result.Data)
                {
                    var model = new ExportPreviewOrderModel();
                    model.Id = item.Id;
                    model.HasNewPaymentNotification = item.HasNewPaymentNotification;
                    model.OrderNumber = item.OrderNumber;
                    model.OrderStatus = item.OrderStatus;
                    model.PaymentStatus = item.PaymentStatus;
                    model.ShippingStatus = item.ShippingStatus;
                    model.CustomerId = item.CustomerId;
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(item.CreatedOnUtc, DateTimeKind.Utc);
                    model.OrderTotal = item.OrderTotal;
                    model.StoreName = (string)item.Store.Name;
                    models.Add(model);
                }

                gridData = new GridModel<ExportPreviewOrderModel> { Data = models, Total = normalizedTotal };
            }
            else if (provider.Value.EntityType == ExportEntityType.Category)
            {
                var models = new List<ExportPreviewCategoryModel>();

                foreach (var item in result.Data)
                {
                    var category = item.Entity as Category;
                    var model = new ExportPreviewCategoryModel();
                    model.Id = category.Id;
                    model.Breadcrumb = ((ICategoryNode)category).GetCategoryPath(_categoryService, aliasPattern: "({0})");
                    model.FullName = item.FullName;
                    model.Alias = item.Alias;
                    model.Published = category.Published;
                    model.DisplayOrder = category.DisplayOrder;
                    model.LimitedToStores = category.LimitedToStores;
                    models.Add(model);
                }
                gridData = new GridModel<ExportPreviewCategoryModel> { Data = models, Total = normalizedTotal };
            }
            else if (provider.Value.EntityType == ExportEntityType.Manufacturer)
            {
                var models = new List<ExportPreviewManufacturerModel>();

                foreach (var item in result.Data)
                {
                    var model = new ExportPreviewManufacturerModel();
                    model.Id = item.Id;
                    model.Name = item.Name;
                    model.Published = item.Published;
                    model.DisplayOrder = item.DisplayOrder;
                    model.LimitedToStores = item.LimitedToStores;
                    models.Add(model);
                }

                gridData = new GridModel<ExportPreviewManufacturerModel> { Data = models, Total = normalizedTotal };
            }
            else if (provider.Value.EntityType == ExportEntityType.Customer)
            {
                var models = new List<ExportPreviewCustomerModel>();

                foreach (var item in result.Data)
                {
                    var customer = item.Entity as Customer;
                    var customerRoles = item.CustomerRoles as List<dynamic>;
                    var customerRolesString = string.Join(", ", customerRoles.Select(x => x.Name));

                    var model = new ExportPreviewCustomerModel();
                    model.Id = customer.Id;
                    model.Active = customer.Active;
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(customer.CreatedOnUtc, DateTimeKind.Utc);
                    model.CustomerRoleNames = customerRolesString;
                    model.Email = customer.Email;
                    model.FullName = item._FullName;
                    model.LastActivityDate = _dateTimeHelper.ConvertToUserTime(customer.LastActivityDateUtc, DateTimeKind.Utc);
                    model.Username = customer.Username;
                    models.Add(model);
                }

                gridData = new GridModel<ExportPreviewCustomerModel> { Data = models, Total = normalizedTotal };
            }
            else if (provider.Value.EntityType == ExportEntityType.NewsLetterSubscription)
            {
                var models = new List<ExportPreviewNewsLetterSubscriptionModel>();

                foreach (var item in result.Data)
                {
                    var subscription = item.Entity as NewsLetterSubscription;
                    var model = new ExportPreviewNewsLetterSubscriptionModel();
                    model.Id = subscription.Id;
                    model.Active = subscription.Active;
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(subscription.CreatedOnUtc, DateTimeKind.Utc);
                    model.Email = subscription.Email;
                    model.StoreName = (string)item.Store.Name;
                    models.Add(model);
                }

                gridData = new GridModel<ExportPreviewNewsLetterSubscriptionModel> { Data = models, Total = normalizedTotal };
            }
            else if (provider.Value.EntityType == ExportEntityType.ShoppingCartItem)
            {
                var guest = T("Admin.Customers.Guest").Text;
                var cartTypeName = ShoppingCartType.ShoppingCart.GetLocalizedEnum(Services.Localization, Services.WorkContext);
                var wishlistTypeName = ShoppingCartType.Wishlist.GetLocalizedEnum(Services.Localization, Services.WorkContext);
                var models = new List<ExportPreviewShoppingCartItemModel>();

                foreach (var item in result.Data)
                {
                    var cartItem = item.Entity as ShoppingCartItem;
                    var model = new ExportPreviewShoppingCartItemModel();
                    model.Id = cartItem.Id;
                    model.ShoppingCartTypeId = cartItem.ShoppingCartTypeId;
                    model.ShoppingCartTypeName = cartItem.ShoppingCartType == ShoppingCartType.Wishlist ? wishlistTypeName : cartTypeName;
                    model.CustomerId = cartItem.CustomerId;
                    model.CustomerEmail = cartItem.Customer.IsGuest() ? guest : cartItem.Customer.Email;
                    model.ProductTypeId = cartItem.Product.ProductTypeId;
                    model.ProductTypeName = cartItem.Product.GetProductTypeLabel(Services.Localization);
                    model.ProductTypeLabelHint = cartItem.Product.ProductTypeLabelHint;
                    model.Name = cartItem.Product.Name;
                    model.Sku = cartItem.Product.Sku;
                    model.Price = cartItem.Product.Price;
                    model.Published = cartItem.Product.Published;
                    model.StockQuantity = cartItem.Product.StockQuantity;
                    model.AdminComment = cartItem.Product.AdminComment;
                    model.CreatedOn = _dateTimeHelper.ConvertToUserTime(cartItem.CreatedOnUtc, DateTimeKind.Utc);
                    model.StoreName = (string)item.Store.Name;
                    models.Add(model);
                }

                gridData = new GridModel<ExportPreviewShoppingCartItemModel> { Data = models, Total = normalizedTotal };
            }

            return new JsonResult { Data = gridData ?? Enumerable.Empty<ExportPreviewProductModel>() };
        }

        #endregion

        #region Download

        [Permission(Permissions.Configuration.Export.Read)]
        public ActionResult DownloadLogFile(int id)
        {
            var profile = _exportService.GetExportProfileById(id);
            if (profile != null)
            {
                var path = profile.GetExportLogPath();
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        var stream = new FileStream(path, FileMode.Open);
                        var result = new FileStreamResult(stream, MediaTypeNames.Text.Plain);

                        return result;
                    }
                    catch (IOException)
                    {
                        NotifyWarning(T("Admin.Common.FileInUse"));
                    }
                }
            }

            return RedirectToAction("List");
        }

        [HttpGet]
        public ActionResult DownloadExportFile(int id, string name, bool? isDeployment)
        {
            if (PathHelper.HasInvalidFileNameChars(name))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid file name.");
            }

            string message = null;
            string path = null;

            if (Services.Permissions.Authorize(Permissions.Configuration.Export.Read))
            {
                if (isDeployment ?? false)
                {
                    var deployment = _exportService.GetExportDeploymentById(id);
                    if (deployment != null)
                    {
                        var deploymentFolder = deployment.GetDeploymentFolder();
                        if (deploymentFolder.HasValue())
                        {
                            path = Path.Combine(deploymentFolder, name);
                        }
                    }
                }
                else
                {
                    var profile = _exportService.GetExportProfileById(id);
                    if (profile != null)
                    {
                        path = Path.Combine(profile.GetExportFolder(true), name);
                        if (!System.IO.File.Exists(path))
                        {
                            path = Path.Combine(profile.GetExportFolder(false), name);
                        }
                    }
                }

                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        var stream = new FileStream(path, FileMode.Open);

                        var result = new FileStreamResult(stream, MimeTypes.MapNameToMimeType(path));
                        result.FileDownloadName = Path.GetFileName(path);

                        return result;
                    }
                    catch (IOException)
                    {
                        message = T("Admin.Common.FileInUse");
                    }
                }
            }
            else
            {
                message = T("Admin.AccessDenied.Description");
            }

            if (message.IsEmpty())
            {
                message = T("Admin.Common.ResourceNotFound");
            }

            return File(Encoding.UTF8.GetBytes(message), MediaTypeNames.Text.Plain, "DownloadExportFile.txt");
        }

        #endregion

        #region Deployment

        [Permission(Permissions.Configuration.Export.Update)]
        public ActionResult CreateDeployment(int id)
        {
            var profile = _exportService.GetExportProfileById(id);
            if (profile == null)
                return RedirectToAction("List");

            var provider = _exportService.LoadProvider(profile.ProviderSystemName);
            if (provider.Metadata.IsHidden)
                return RedirectToAction("List");

            var model = CreateDeploymentModel(profile, new ExportDeployment
            {
                ProfileId = id,
                Enabled = true,
                DeploymentType = ExportDeploymentType.FileSystem,
                Name = profile.Name
            }, provider, true);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Export.Update)]
        public ActionResult CreateDeployment(ExportDeploymentModel model, bool continueEditing, ExportDeploymentType deploymentType)
        {
            var profile = _exportService.GetExportProfileById(model.ProfileId);
            if (profile == null)
                return RedirectToAction("List");

            var provider = _exportService.LoadProvider(profile.ProviderSystemName);
            if (provider == null || provider.Metadata.IsHidden)
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

        [Permission(Permissions.Configuration.Export.Update)]
        public ActionResult EditDeployment(int id)
        {
            var deployment = _exportService.GetExportDeploymentById(id);
            if (deployment == null)
                return RedirectToAction("List");

            var provider = _exportService.LoadProvider(deployment.Profile.ProviderSystemName);
            if (provider == null || provider.Metadata.IsHidden)
                return RedirectToAction("List");

            var model = CreateDeploymentModel(deployment.Profile, deployment, provider, true);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Export.Update)]
        public ActionResult EditDeployment(ExportDeploymentModel model, bool continueEditing)
        {
            var deployment = _exportService.GetExportDeploymentById(model.Id);
            if (deployment == null)
                return RedirectToAction("List");

            var provider = _exportService.LoadProvider(deployment.Profile.ProviderSystemName);
            if (provider == null || provider.Metadata.IsHidden)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                ModelToEntity(model, deployment);

                _exportService.UpdateExportProfile(deployment.Profile);

                NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

                return SmartRedirect(continueEditing, deployment.ProfileId, deployment.Id);
            }

            model = CreateDeploymentModel(deployment.Profile, deployment, _exportService.LoadProvider(deployment.Profile.ProviderSystemName), true);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Export.Update)]
        public ActionResult DeleteDeployment(int id)
        {
            var deployment = _exportService.GetExportDeploymentById(id);
            if (deployment == null)
                return RedirectToAction("List");

            var provider = _exportService.LoadProvider(deployment.Profile.ProviderSystemName);
            if (provider == null || provider.Metadata.IsHidden)
                return RedirectToAction("List");

            int profileId = deployment.ProfileId;

            _exportService.DeleteExportDeployment(deployment);

            NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));

            return SmartRedirect(false, profileId, 0);
        }

        #endregion

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
    }
}