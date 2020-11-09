using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using Autofac;
using Newtonsoft.Json;
using SmartStore.Admin.Models.Localization;
using SmartStore.Core;
using SmartStore.Core.Async;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Stores;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class LanguageController : AdminControllerBase
    {
        #region Fields

        private readonly ILanguageService _languageService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly AdminAreaSettings _adminAreaSettings;
        private readonly IPluginFinder _pluginFinder;
        private readonly ICountryService _countryService;
        private readonly ICommonServices _services;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAsyncState _asyncState;

        #endregion

        #region Constructors

        public LanguageController(
            ILanguageService languageService,
            IStoreMappingService storeMappingService,
            IGenericAttributeService genericAttributeService,
            AdminAreaSettings adminAreaSettings,
            IPluginFinder pluginFinder,
            ICountryService countryService,
            ICommonServices services,
            IDateTimeHelper dateTimeHelper,
            IAsyncState asyncState)
        {
            _languageService = languageService;
            _storeMappingService = storeMappingService;
            _genericAttributeService = genericAttributeService;
            _adminAreaSettings = adminAreaSettings;
            _pluginFinder = pluginFinder;
            _countryService = countryService;
            _services = services;
            _dateTimeHelper = dateTimeHelper;
            _asyncState = asyncState;
        }

        #endregion

        #region Utilities

        private void PrepareLanguageModel(LanguageModel model, Language language, bool excludeProperties)
        {
            var allCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .OrderBy(x => x.DisplayName)
                .ToList();

            var allCountryNames = _countryService.GetAllCountries(true)
                .ToDictionarySafe(x => x.TwoLetterIsoCode.EmptyNull().ToLower(), x => x.GetLocalized(y => y.Name, _services.WorkContext.WorkingLanguage, true, false));

            model.AvailableCultures = allCultures
                .Select(x => new SelectListItem { Text = "{0} [{1}]".FormatInvariant(x.DisplayName, x.IetfLanguageTag), Value = x.IetfLanguageTag })
                .ToList();

            model.AvailableTwoLetterLanguageCodes = new List<SelectListItem>();
            model.AvailableFlags = new List<SelectListItem>();

            foreach (var item in allCultures)
            {
                if (!model.AvailableTwoLetterLanguageCodes.Any(x => x.Value.IsCaseInsensitiveEqual(item.TwoLetterISOLanguageName)))
                {
                    // Display language name is not provided by net framework
                    var index = item.DisplayName.EmptyNull().IndexOf(" (");

                    if (index == -1)
                        index = item.DisplayName.EmptyNull().IndexOf(" [");

                    var displayName = "{0} [{1}]".FormatInvariant(
                        index == -1 ? item.DisplayName : item.DisplayName.Substring(0, index),
                        item.TwoLetterISOLanguageName);

                    if (item.TwoLetterISOLanguageName.Length == 2)
                    {
                        model.AvailableTwoLetterLanguageCodes.Add(new SelectListItem { Text = displayName, Value = item.TwoLetterISOLanguageName });
                    }
                }
            }

            foreach (var path in Directory.EnumerateFiles(_services.WebHelper.MapPath("~/Content/Images/flags/"), "*.png", SearchOption.TopDirectoryOnly))
            {
                var name = Path.GetFileNameWithoutExtension(path).EmptyNull().ToLower();
                string countryDescription = null;

                if (allCountryNames.ContainsKey(name))
                    countryDescription = "{0} [{1}]".FormatInvariant(allCountryNames[name], name);

                if (countryDescription.IsEmpty())
                    countryDescription = name;

                model.AvailableFlags.Add(new SelectListItem { Text = countryDescription, Value = Path.GetFileName(path) });
            }

            if (!excludeProperties)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(language);
            }

            model.AvailableFlags = model.AvailableFlags.OrderBy(x => x.Text).ToList();

            if (language != null)
            {
                var lastImportInfos = GetLastResourcesImportInfos();
                if (lastImportInfos.TryGetValue(language.Id, out LastResourcesImportInfo info))
                {
                    model.LastResourcesImportOn = info.ImportedOn;
                    model.LastResourcesImportOnString = model.LastResourcesImportOn.Value.RelativeFormat(false, "f");
                }
            }
        }

        private void PrepareAvailableLanguageModel(
            AvailableLanguageModel model,
            AvailableResourcesModel resources,
            Dictionary<int, LastResourcesImportInfo> lastImportInfos,
            Language language = null,
            LanguageDownloadState state = null)
        {
            // Source Id (aka SetId), not entity Id!
            model.Id = resources.Id;
            model.PreviousSetId = resources.PreviousSetId;
            model.IsInstalled = language != null;
            model.Name = GetCultureDisplayName(resources.Language.Culture) ?? resources.Language.Name;
            model.LanguageCulture = resources.Language.Culture;
            model.UniqueSeoCode = resources.Language.TwoLetterIsoCode;
            model.Rtl = resources.Language.Rtl;
            model.Version = resources.Version;
            model.Type = resources.Type;
            model.Published = resources.Published;
            model.DisplayOrder = resources.DisplayOrder;
            model.TranslatedCount = resources.TranslatedCount;
            model.TranslatedPercentage = resources.TranslatedPercentage;
            model.IsDownloadRunning = state != null && state.Id == resources.Id;
            model.UpdatedOn = _dateTimeHelper.ConvertToUserTime(resources.UpdatedOn, DateTimeKind.Utc);
            model.UpdatedOnString = resources.UpdatedOn.RelativeFormat(true, "f");
            model.FlagImageFileName = GetFlagFileName(resources.Language.Culture);

            if (language != null && lastImportInfos.TryGetValue(language.Id, out LastResourcesImportInfo info))
            {
                // Only show percent at last import if it's less than the current percentage.
                var percentAtLastImport = Math.Round(info.TranslatedPercentage, 2);
                if (percentAtLastImport < model.TranslatedPercentage)
                {
                    model.TranslatedPercentageAtLastImport = percentAtLastImport;
                }

                model.LastResourcesImportOn = info.ImportedOn;
                model.LastResourcesImportOnString = model.LastResourcesImportOn.Value.RelativeFormat(false, "f");
            }
        }

        private async Task<CheckAvailableResourcesResult> CheckAvailableResources(bool enforce = false)
        {
            var cacheKey = "admin:language:checkavailableresourcesresult";
            var currentVersion = SmartStoreVersion.CurrentFullVersion;
            CheckAvailableResourcesResult result = null;
            string jsonString = null;

            if (!enforce)
            {
                jsonString = Session[cacheKey] as string;
            }

            if (jsonString == null)
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = TimeSpan.FromMilliseconds(10000);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Smartstore " + currentVersion);
                        client.DefaultRequestHeaders.Add("Authorization-Key", Services.StoreContext.CurrentStore.Url.EmptyNull().TrimEnd('/'));

                        var url = CommonHelper.GetAppSetting<string>("sm:TranslateCheckUrl").FormatInvariant(currentVersion);
                        var response = await client.GetAsync(url);

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = await response.Content.ReadAsStringAsync();
                            Session[cacheKey] = jsonString;
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotifyError(T("Admin.Configuration.Languages.CheckAvailableLanguagesFailed"));
                    Logger.ErrorsAll(ex);
                }
            }

            if (jsonString.HasValue())
            {
                result = JsonConvert.DeserializeObject<CheckAvailableResourcesResult>(jsonString);
            }

            return result ?? new CheckAvailableResourcesResult();
        }

        private async Task<string> DownloadAvailableResources(string downloadUrl, string storeUrl)
        {
            Guard.NotEmpty(downloadUrl, nameof(downloadUrl));

            var tempFilePath = Path.Combine(FileSystemHelper.TempDirTenant(), Guid.NewGuid().ToString() + ".xml");
            var buffer = new byte[32768];
            var len = 0;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Text.Xml));
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Smartstore " + SmartStoreVersion.CurrentFullVersion);
                client.DefaultRequestHeaders.Add("Authorization-Key", storeUrl.EmptyNull().TrimEnd('/'));

                using (var sourceStream = await client.GetStreamAsync(downloadUrl))
                using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    while ((len = await sourceStream.ReadAsync(buffer, 0, 32768)) > 0)
                    {
                        fileStream.Write(buffer, 0, len);
                    }
                }
            }

            return tempFilePath;
        }

        private string GetCultureDisplayName(string culture)
        {
            if (culture.HasValue())
            {
                try
                {
                    return (new CultureInfo(culture)).DisplayName;
                }
                catch { }
            }

            return null;
        }

        private string GetFlagFileName(string culture)
        {
            culture = culture.EmptyNull().ToLower();

            if (culture.HasValue() && culture.SplitToPair(out string cultureLeft, out string cultureRight, "-"))
            {
                var fileName = cultureRight + ".png";

                if (System.IO.File.Exists(CommonHelper.MapPath("~/Content/images/flags/" + fileName)))
                {
                    return fileName;
                }
            }

            return null;
        }

        private Dictionary<int, LastResourcesImportInfo> GetLastResourcesImportInfos()
        {
            Dictionary<int, LastResourcesImportInfo> result = null;

            try
            {
                var attributes = _genericAttributeService.GetAttributes("LastResourcesImportInfo", "Language").ToList();
                result = attributes.ToDictionarySafe(x => x.EntityId, x => JsonConvert.DeserializeObject<LastResourcesImportInfo>(x.Value));
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return result ?? new Dictionary<int, LastResourcesImportInfo>();
        }

        #endregion

        #region Languages

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Language.Read)]
        public ActionResult List()
        {
            var lastImportInfos = GetLastResourcesImportInfos();
            var languages = _languageService.GetAllLanguages(true);
            var defaultLanguageId = _languageService.GetDefaultLanguageId();

            var model = languages.Select(x =>
            {
                var langModel = x.ToModel();
                langModel.Name = GetCultureDisplayName(x.LanguageCulture) ?? x.Name;

                if (lastImportInfos.TryGetValue(x.Id, out LastResourcesImportInfo info))
                {
                    langModel.LastResourcesImportOn = info.ImportedOn;
                    langModel.LastResourcesImportOnString = langModel.LastResourcesImportOn.Value.RelativeFormat(false, "f");
                }

                if (x.Id == defaultLanguageId)
                {
                    ViewBag.DefaultLanguageNote = T("Admin.Configuration.Languages.DefaultLanguage.Note", langModel.Name).Text;
                }

                return langModel;
            })
            .ToList();

            return View(model);
        }

        [Permission(Permissions.Configuration.Language.Read)]
        public async Task<ActionResult> AvailableLanguages(bool enforce = false)
        {
            var languages = _languageService.GetAllLanguages(true);
            var languageDic = languages.ToDictionarySafe(x => x.LanguageCulture, StringComparer.OrdinalIgnoreCase);

            var downloadState = _asyncState.Get<LanguageDownloadState>();
            var lastImportInfos = GetLastResourcesImportInfos();
            var checkResult = await CheckAvailableResources(enforce);

            var model = new AvailableLanguageListModel();
            model.Languages = new List<AvailableLanguageModel>();
            model.Version = checkResult.Version;
            model.ResourceCount = checkResult.ResourceCount;

            foreach (var resources in checkResult.Resources)
            {
                if (resources.Language.Culture.HasValue())
                {
                    languageDic.TryGetValue(resources.Language.Culture, out Language language);

                    var alModel = new AvailableLanguageModel();
                    PrepareAvailableLanguageModel(alModel, resources, lastImportInfos, language, downloadState);

                    model.Languages.Add(alModel);
                }
            }

            return PartialView(model);
        }

        [Permission(Permissions.Configuration.Language.Create)]
        public ActionResult Create()
        {
            var model = new LanguageModel();

            PrepareLanguageModel(model, null, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Language.Create)]
        public ActionResult Create(LanguageModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var language = model.ToEntity();
                _languageService.InsertLanguage(language);

                SaveStoreMappings(language, model.SelectedStoreIds);

                var plugins = _pluginFinder.GetPluginDescriptors(true);
                var filterLanguages = new List<Language>() { language };

                foreach (var plugin in plugins)
                {
                    _services.Localization.ImportPluginResourcesFromXml(plugin, null, false, filterLanguages);
                }

                NotifySuccess(T("Admin.Configuration.Languages.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = language.Id }) : RedirectToAction("List");
            }

            PrepareLanguageModel(model, null, true);

            return View(model);
        }

        [Permission(Permissions.Configuration.Language.Read)]
        public async Task<ActionResult> Edit(int id)
        {
            var language = _languageService.GetLanguageById(id);
            if (language == null)
                return RedirectToAction("List");

            // Set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

            var model = language.ToModel();
            PrepareLanguageModel(model, language, false);

            // Provide combobox with downloadable resources for this language.
            var lastImportInfos = GetLastResourcesImportInfos();
            var checkResult = await CheckAvailableResources();
            string cultureParentName = null;

            try
            {
                var ci = CultureInfo.GetCultureInfo(language.LanguageCulture);
                if (!ci.IsNeutralCulture && ci.Parent != null)
                {
                    cultureParentName = ci.Parent.Name;
                }
            }
            catch { }

            foreach (var resources in checkResult.Resources.Where(x => x.Published))
            {
                var srcCulture = resources.Language.Culture;
                if (srcCulture.HasValue())
                {
                    var downloadDisplayOrder = srcCulture.IsCaseInsensitiveEqual(language.LanguageCulture) ? 1 : 0;

                    if (downloadDisplayOrder == 0 && cultureParentName.IsCaseInsensitiveEqual(srcCulture))
                    {
                        downloadDisplayOrder = 2;
                    }

                    if (downloadDisplayOrder == 0 && resources.Language.TwoLetterIsoCode.IsCaseInsensitiveEqual(language.UniqueSeoCode))
                    {
                        downloadDisplayOrder = 3;
                    }

                    if (downloadDisplayOrder != 0)
                    {
                        var alModel = new AvailableLanguageModel();
                        PrepareAvailableLanguageModel(alModel, resources, lastImportInfos, language);
                        alModel.DisplayOrder = downloadDisplayOrder;

                        model.AvailableDownloadLanguages.Add(alModel);
                    }
                }
            }

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Language.Update)]
        public ActionResult Edit(LanguageModel model, bool continueEditing)
        {
            var language = _languageService.GetLanguageById(model.Id);
            if (language == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                // Ensure we have at least one published language.
                var allLanguages = _languageService.GetAllLanguages();
                if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id && !model.Published)
                {
                    NotifyError(T("Admin.Configuration.Languages.OnePublishedLanguageRequired"));
                    return RedirectToAction("Edit", new { id = language.Id });
                }

                language = model.ToEntity(language);
                _languageService.UpdateLanguage(language);

                SaveStoreMappings(language, model.SelectedStoreIds);

                NotifySuccess(T("Admin.Configuration.Languages.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = language.Id }) : RedirectToAction("List");
            }

            PrepareLanguageModel(model, language, true);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Language.Delete)]
        public ActionResult Delete(int id)
        {
            var language = _languageService.GetLanguageById(id);
            if (language == null)
                return RedirectToAction("List");

            // Ensure we have at least one published language
            var allLanguages = _languageService.GetAllLanguages();
            if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id)
            {
                NotifyError(T("Admin.Configuration.Languages.OnePublishedLanguageRequired"));
                return RedirectToAction("Edit", new { id = language.Id });
            }

            _languageService.DeleteLanguage(language);

            NotifySuccess(T("Admin.Configuration.Languages.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Resources

        [Permission(Permissions.Configuration.Language.Read)]
        public ActionResult Resources(int languageId)
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true)
                .Select(x => new SelectListItem
                {
                    Selected = (x.Id.Equals(languageId)),
                    Text = x.Name,
                    Value = x.Id.ToString()
                }).ToList();

            var language = _languageService.GetLanguageById(languageId);
            ViewBag.LanguageId = languageId;
            ViewBag.LanguageName = language.Name;

            var resourceQuery = _services.Localization.All(languageId);

            var gridModel = new GridModel<LanguageResourceModel>
            {
                Data = resourceQuery
                    .Take(() => _adminAreaSettings.GridPageSize)
                    .ToList()
                    .Select(x => new LanguageResourceModel
                    {
                        Id = x.Id,
                        LanguageId = languageId,
                        LanguageName = language.Name,
                        ResourceName = x.ResourceName,
                        ResourceValue = x.ResourceValue.EmptyNull(),
                    }),
                Total = resourceQuery.AsQueryable().Count()
            };

            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Language.EditResource)]
        public ActionResult Resources(int languageId, GridCommand command)
        {
            var model = new GridModel<LanguageResourceModel>();

            var language = _languageService.GetLanguageById(languageId);
            var resources = _services.Localization.All(languageId).ForCommand(command);

            model.Data = resources.PagedForCommand(command).ToList().Select(x =>
            {
                var resModel = new LanguageResourceModel
                {
                    Id = x.Id,
                    LanguageId = languageId,
                    LanguageName = language.Name,
                    ResourceName = x.ResourceName,
                    ResourceValue = x.ResourceValue.EmptyNull(),
                };

                return resModel;
            });

            model.Total = resources.AsQueryable().Count();

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Language.EditResource)]
        public ActionResult ResourceUpdate(LanguageResourceModel model, GridCommand command)
        {
            if (model.ResourceName != null)
                model.ResourceName = model.ResourceName.Trim();

            if (model.ResourceValue != null)
                model.ResourceValue = model.ResourceValue.Trim();

            if (!ModelState.IsValid)
            {
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var resource = _services.Localization.GetLocaleStringResourceById(model.Id);
            // if the resourceName changed, ensure it isn't being used by another resource
            if (!resource.ResourceName.Equals(model.ResourceName, StringComparison.InvariantCultureIgnoreCase))
            {
                var res = _services.Localization.GetLocaleStringResourceByName(model.ResourceName, model.LanguageId, false);
                if (res != null && res.Id != resource.Id)
                {
                    return Content(T("Admin.Configuration.Languages.Resources.NameAlreadyExists", res.ResourceName));
                }
            }

            resource.ResourceName = model.ResourceName;
            resource.ResourceValue = model.ResourceValue;
            resource.IsTouched = true;

            _services.Localization.UpdateLocaleStringResource(resource);

            return Resources(model.LanguageId, command);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Language.EditResource)]
        public ActionResult ResourceAdd(int id, LanguageResourceModel model, GridCommand command)
        {
            if (model.ResourceName != null)
                model.ResourceName = model.ResourceName.Trim();
            if (model.ResourceValue != null)
                model.ResourceValue = model.ResourceValue.Trim();

            if (!ModelState.IsValid)
            {
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var res = _services.Localization.GetLocaleStringResourceByName(model.ResourceName, model.LanguageId, false);
            if (res == null)
            {
                var resource = new LocaleStringResource { LanguageId = id };
                resource.ResourceName = model.ResourceName;
                resource.ResourceValue = model.ResourceValue;
                resource.IsTouched = true;

                _services.Localization.InsertLocaleStringResource(resource);
            }
            else
            {
                return Content(T("Admin.Configuration.Languages.Resources.NameAlreadyExists", model.ResourceName));
            }

            return Resources(id, command);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Configuration.Language.EditResource)]
        public ActionResult ResourceDelete(int id, int languageId, GridCommand command)
        {
            var resource = _services.Localization.GetLocaleStringResourceById(id);

            _services.Localization.DeleteLocaleStringResource(resource);

            return Resources(languageId, command);
        }

        #endregion

        #region Export / Import

        [Permission(Permissions.Configuration.Language.Read)]
        public ActionResult ExportXml(int id)
        {
            var language = _languageService.GetLanguageById(id);
            if (language == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                var xml = _services.Localization.ExportResourcesToXml(language);
                return new XmlDownloadResult(xml, "language-pack-{0}.xml".FormatInvariant(language.UniqueSeoCode));
            }
            catch (Exception ex)
            {
                NotifyError(ex);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Language.EditResource)]
        public async Task<ActionResult> ImportXml(int id, FormCollection form, ImportModeFlags mode, bool updateTouched, int? availableLanguageSetId)
        {
            var language = _languageService.GetLanguageById(id);
            if (language == null)
                return RedirectToAction("List");

            // Set page timeout to 5 minutes
            Server.ScriptTimeout = 300;

            string tempFilePath = null;

            try
            {
                var file = Request.Files["importxmlfile"];
                if (file != null && file.ContentLength > 0)
                {
                    _services.Localization.ImportResourcesFromXml(language, file.InputStream.AsString(), mode: mode, updateTouchedResources: updateTouched);

                    NotifySuccess(T("Admin.Configuration.Languages.Imported"));
                }
                else if ((availableLanguageSetId ?? 0) != 0)
                {
                    var checkResult = await CheckAvailableResources();
                    var availableResources = checkResult.Resources.First(x => x.Id == availableLanguageSetId.Value);

                    tempFilePath = await DownloadAvailableResources(availableResources.DownloadUrl, _services.StoreContext.CurrentStore.Url);

                    var xmlDoc = new XmlDocument();
                    xmlDoc.Load(tempFilePath);

                    _services.Localization.ImportResourcesFromXml(language, xmlDoc, null, false, mode, updateTouched);

                    var str = JsonConvert.SerializeObject(new LastResourcesImportInfo
                    {
                        TranslatedPercentage = availableResources.TranslatedPercentage,
                        ImportedOn = DateTime.UtcNow
                    });
                    _genericAttributeService.SaveAttribute(language, "LastResourcesImportInfo", str);

                    NotifySuccess(T("Admin.Configuration.Languages.Imported"));
                }
                else
                {
                    NotifyError(T("Admin.Configuration.Languages.UploadFileOrSelectLanguage"));
                }
            }
            catch (Exception exception)
            {
                NotifyError(exception);
                Logger.ErrorsAll(exception);
            }
            finally
            {
                FileSystemHelper.DeleteFile(tempFilePath);
            }

            return RedirectToAction("Edit", new { id = language.Id });
        }

        #endregion

        #region Download

        private void DownloadCore(ILifetimeScope scope, CancellationToken ct, LanguageDownloadContext context)
        {
            var asyncState = scope.Resolve<IAsyncState>();
            var services = scope.Resolve<ICommonServices>();
            var languageService = scope.Resolve<ILanguageService>();
            var genericAttributeService = scope.Resolve<IGenericAttributeService>();
            var logger = scope.Resolve<ILogger>();
            string tempFilePath = null;

            try
            {
                // 1. Download resources
                var state = asyncState.Get<LanguageDownloadState>() ?? new LanguageDownloadState
                {
                    Id = context.SetId,
                    ProgressMessage = T("Admin.Configuration.Languages.DownloadingResources")
                };
                asyncState.Set(state);

                var resources = context.AvailableResources.Resources.First(x => x.Id == context.SetId);
                tempFilePath = DownloadAvailableResources(resources.DownloadUrl, services.StoreContext.CurrentStore.Url).Result;

                state.ProgressMessage = T("Admin.Configuration.Languages.ImportResources");
                asyncState.Set(state);

                // 2. Create language entity (if required)
                var allLanguages = languageService.GetAllLanguages();
                var lastLanguage = allLanguages.OrderByDescending(x => x.DisplayOrder).FirstOrDefault();

                var language = languageService.GetLanguageByCulture(resources.Language.Culture);
                if (language == null)
                {
                    language = new Language();
                    language.LanguageCulture = resources.Language.Culture;
                    language.UniqueSeoCode = resources.Language.TwoLetterIsoCode;
                    language.Name = GetCultureDisplayName(resources.Language.Culture) ?? resources.Name;
                    language.FlagImageFileName = GetFlagFileName(resources.Language.Culture);
                    language.Rtl = resources.Language.Rtl;
                    language.Published = false;
                    language.DisplayOrder = lastLanguage != null ? lastLanguage.DisplayOrder + 1 : 0;

                    languageService.InsertLanguage(language);
                }

                // 3. Import resources
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(tempFilePath);

                services.Localization.ImportResourcesFromXml(language, xmlDoc);

                var str = JsonConvert.SerializeObject(new LastResourcesImportInfo
                {
                    TranslatedPercentage = resources.TranslatedPercentage,
                    ImportedOn = DateTime.UtcNow
                });
                genericAttributeService.SaveAttribute(language, "LastResourcesImportInfo", str);
            }
            catch (Exception ex)
            {
                logger.ErrorsAll(ex);
            }
            finally
            {
                if (asyncState.Exists<LanguageDownloadState>())
                {
                    asyncState.Remove<LanguageDownloadState>();
                }

                FileSystemHelper.DeleteFile(tempFilePath);
            }
        }

        [Permission(Permissions.Configuration.Language.EditResource)]
        public async Task<ActionResult> Download(int setId)
        {
            var ctx = new LanguageDownloadContext(setId)
            {
                AvailableResources = await CheckAvailableResources()
            };

            if (ctx.AvailableResources.Resources.Any())
            {
                var task = AsyncRunner.Run((c, ct, obj) => DownloadCore(c, ct, obj as LanguageDownloadContext), ctx);
            }

            return RedirectToAction("List");
        }

        [HttpPost]
        public JsonResult DownloadProgress()
        {
            try
            {
                var state = _asyncState.Get<LanguageDownloadState>();
                if (state != null)
                {
                    var progressInfo = new
                    {
                        id = state.Id,
                        percent = state.ProgressPercent,
                        message = state.ProgressMessage
                    };

                    return Json(new object[] { progressInfo });
                }
            }
            catch (Exception ex)
            {
                ex.Dump();
            }

            return Json(null);
        }

        #endregion
    }
}
