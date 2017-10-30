using System;
using System.Collections.Generic;
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
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Plugins;
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
        private readonly AdminAreaSettings _adminAreaSettings;
		private readonly IPluginFinder _pluginFinder;
        private readonly PluginMediator _pluginMediator;
        private readonly ICountryService _countryService;
		private readonly ICommonServices _services;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IAsyncState _asyncState;

        #endregion

        #region Constructors

        public LanguageController(
            ILanguageService languageService,
			IStoreMappingService storeMappingService,
            AdminAreaSettings adminAreaSettings,
			IPluginFinder pluginFinder,
            PluginMediator pluginMediator,
            ICountryService countryService,
			ICommonServices services,
            IDateTimeHelper dateTimeHelper,
            IAsyncState asyncState)
        {
            _languageService = languageService;
			_storeMappingService = storeMappingService;
            _adminAreaSettings = adminAreaSettings;
			_pluginFinder = pluginFinder;
            _pluginMediator = pluginMediator;
			_countryService = countryService;
			_services = services;
            _dateTimeHelper = dateTimeHelper;
            _asyncState = asyncState;
        }

		#endregion

		#region Utilities

		private void PrepareLanguageModel(LanguageModel model, Language language, bool excludeProperties)
		{
			var languageId = _services.WorkContext.WorkingLanguage.Id;

			var allCultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
				.OrderBy(x => x.DisplayName)
				.ToList();

			var allCountryNames = _countryService.GetAllCountries(true)
				.ToDictionarySafe(x => x.TwoLetterIsoCode.EmptyNull().ToLower(), x => x.GetLocalized(y => y.Name, languageId, true, false));

			model.AvailableCultures = allCultures
				.Select(x => new SelectListItem { Text = "{0} [{1}]".FormatInvariant(x.DisplayName, x.IetfLanguageTag), Value = x.IetfLanguageTag })
				.ToList();

			model.AvailableTwoLetterLanguageCodes = new List<SelectListItem>();
			model.AvailableFlags = new List<SelectListItem>();

			foreach (var item in allCultures)
			{
				if (!model.AvailableTwoLetterLanguageCodes.Any(x => x.Value.IsCaseInsensitiveEqual(item.TwoLetterISOLanguageName)))
				{
					// display language name is not provided by net framework
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

			model.AvailableFlags = model.AvailableFlags.OrderBy(x => x.Text).ToList();

			model.AvailableStores = _services.StoreService.GetAllStores()
				.Select(s => s.ToModel())
				.ToList();

			if (!excludeProperties)
			{
				model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(language);
			}
		}

        private List<CheckAvailableResourcesResult> CheckAvailableResources(bool enforce = false)
        {
            var cacheKey = "admin:language:checkavailablelanguagesresult";
            var currentVersion = SmartStoreVersion.CurrentFullVersion;
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
                        client.DefaultRequestHeaders.UserAgent.ParseAdd($"SmartStore.NET {currentVersion}");
                        client.DefaultRequestHeaders.Add("Authorization-Key", Services.StoreContext.CurrentStore.Url.EmptyNull().TrimEnd('/'));

                        var url = CommonHelper.GetAppSetting<string>("sm:TranslateCheckUrl").FormatInvariant(currentVersion);
                        var response = client.GetAsync(url).Result;

                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            jsonString = response.Content.ReadAsStringAsync().Result;
                            Session[cacheKey] = jsonString;
                        }
                    }
                }
                catch (Exception exception)
                {
                    NotifyError(T("Admin.Configuration.Languages.CheckAvailableLanguagesFailed"));
                    Logger.ErrorsAll(exception);
                }
            }

            if (jsonString.HasValue())
            {
                return JsonConvert.DeserializeObject<List<CheckAvailableResourcesResult>>(jsonString);
            }

            return null;
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

        private string GetFlagFileName(string isoCode)
        {
            isoCode = isoCode.EmptyNull().ToLower();

            if (isoCode.HasValue())
            {
                switch (isoCode)
                {
                    case "en":
                        isoCode = "us";
                        break;
                }

                var fileName = isoCode + ".png";

                if (System.IO.File.Exists(CommonHelper.MapPath("~/Content/images/flags/" + fileName)))
                {
                    return fileName;
                }
            }

            return null;
        }

        #endregion

        #region Languages

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var languages = _languageService.GetAllLanguages(true);
            var model = languages.Select(x => x.ToModel()).ToList();

            return View(model);
        }

        public ActionResult AvailableLanguages(bool enforce = false)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return Content(T("Admin.AccessDenied.Description"));

            var languages = _languageService.GetAllLanguages(true);
            var languageDic = languages.ToDictionarySafe(x => x.LanguageCulture, StringComparer.OrdinalIgnoreCase);

            var allPlugins = _pluginFinder.GetPluginDescriptors(true);
            var allPluginsDic = allPlugins.ToDictionarySafe(x => x.SystemName, StringComparer.OrdinalIgnoreCase);

            var downloadState = _asyncState.Get<LanguageDownloadState>();

            var model = new List<AvailableLanguageModel>();
            var checkAvailableResourcesResult = CheckAvailableResources(enforce);

            foreach (var checkResult in checkAvailableResourcesResult)
            {
                var culture = checkResult.Language.Culture;
                if (culture.IsEmpty())
                    continue;

                Language language = null;
                PluginDescriptor pluginDescriptor = null;

                languageDic.TryGetValue(culture, out language);

                var availableLanguage = new AvailableLanguageModel();
                availableLanguage.Id = checkResult.Id;
                availableLanguage.IsInstalled = language != null;
                availableLanguage.Name = GetCultureDisplayName(culture) ?? checkResult.Language.Name;
                availableLanguage.LanguageCulture = culture;
                availableLanguage.UniqueSeoCode = checkResult.Language.TwoLetterIsoCode;
                availableLanguage.Rtl = checkResult.Language.Rtl;
                availableLanguage.Type = checkResult.Type;
                availableLanguage.NumberOfResources = checkResult.Aggregation.NumberOfResources;
                availableLanguage.NumberOfTranslatedResources = checkResult.Aggregation.NumberOfTouched;
                availableLanguage.TranslatedPercentage = checkResult.Aggregation.TouchedPercentage;
                availableLanguage.IsDownloadRunning = downloadState != null && downloadState.Id == checkResult.Id;
                availableLanguage.UpdatedOn = _dateTimeHelper.ConvertToUserTime(checkResult.UpdatedOn, DateTimeKind.Utc);
                availableLanguage.UpdatedOnString = availableLanguage.UpdatedOn.RelativeFormat(false, "f");
                availableLanguage.FlagImageFileName = GetFlagFileName(checkResult.Language.TwoLetterIsoCode);

                // Installed plugin infos
                foreach (var systemName in checkResult.PluginSystemNames)
                {
                    if (allPluginsDic.TryGetValue(systemName, out pluginDescriptor))
                    {
                        availableLanguage.Plugins.Add(new AvailableLanguageModel.PluginModel
                        {
                            SystemName = systemName,
                            FriendlyName = pluginDescriptor.GetLocalizedValue(_services.Localization, "FriendlyName"),
                            IconUrl = _pluginMediator.GetIconUrl(pluginDescriptor)
                        });
                    }
                }

                model.Add(availableLanguage);
            }

            return PartialView(model);
        }

        public ActionResult Create()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var model = new LanguageModel();
            
			PrepareLanguageModel(model, null, false);
            
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(LanguageModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var language = model.ToEntity();
                _languageService.InsertLanguage(language);

				//Stores
				_storeMappingService.SaveStoreMappings<Language>(language, model.SelectedStoreIds);

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

            //If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var language = _languageService.GetLanguageById(id);
            if (language == null)
                return RedirectToAction("List");

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

            var model = language.ToModel();

			PrepareLanguageModel(model, language, false);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(LanguageModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var language = _languageService.GetLanguageById(model.Id);
            if (language == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                //ensure we have at least one published language
                var allLanguages = _languageService.GetAllLanguages();
                if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id &&
                    !model.Published)
                {
					NotifyError("At least one published language is required.");
                    return RedirectToAction("Edit", new { id = language.Id });
                }

                //update
                language = model.ToEntity(language);
                _languageService.UpdateLanguage(language);

				//Stores
				_storeMappingService.SaveStoreMappings<Language>(language, model.SelectedStoreIds);

                //notification
                NotifySuccess(T("Admin.Configuration.Languages.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = language.Id }) : RedirectToAction("List");
            }

			PrepareLanguageModel(model, language, true);

			//If we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var language = _languageService.GetLanguageById(id);
            if (language == null)
                return RedirectToAction("List");

            //ensure we have at least one published language
            var allLanguages = _languageService.GetAllLanguages();
            if (allLanguages.Count == 1 && allLanguages[0].Id == language.Id)
            {
				NotifyError("At least one published language is required.");
                return RedirectToAction("Edit", new { id = language.Id });
            }

            //delete
            _languageService.DeleteLanguage(language);

            //notification
            NotifySuccess(T("Admin.Configuration.Languages.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Resources

        public ActionResult Resources(int languageId)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

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

            var resources = _services.Localization.All(languageId);

            var gridModel = new GridModel<LanguageResourceModel>
            {
                Data = resources
					.Take(_adminAreaSettings.GridPageSize)
					.ToList()
                    .Select(x => new LanguageResourceModel
                    {
						Id = x.Id,
						LanguageId = languageId,
                        LanguageName = language.Name,
                        ResourceName = x.ResourceName,
                        ResourceValue = x.ResourceValue.EmptyNull(),
                    }),
                Total = resources.AsQueryable().Count()
            };
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Resources(int languageId, GridCommand command)
        {
			var model = new GridModel<LanguageResourceModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
			{
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
			}
			else
			{
				model.Data = Enumerable.Empty<LanguageResourceModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ResourceUpdate(LanguageResourceModel model, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
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
			}

            return Resources(model.LanguageId, command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ResourceAdd(int id, LanguageResourceModel model, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
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
			}

            return Resources(id, command);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult ResourceDelete(int id, int languageId, GridCommand command)
        {
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
			{
				var resource = _services.Localization.GetLocaleStringResourceById(id);

				_services.Localization.DeleteLocaleStringResource(resource);
			}

            return Resources(languageId, command);
        }

        #endregion

        #region Export / Import

        public ActionResult ExportXml(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var language = _languageService.GetLanguageById(id);
            if (language == null)
                return RedirectToAction("List");

            try
            {
                var xml = _services.Localization.ExportResourcesToXml(language);
                return new XmlDownloadResult(xml, "language-pack-{0}.xml".FormatInvariant(language.UniqueSeoCode));
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("List");
            }
        }

        [HttpPost]
        public ActionResult ImportXml(int id, FormCollection form, ImportModeFlags mode, bool updateTouched)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var language = _languageService.GetLanguageById(id);
            if (language == null)
                return RedirectToAction("List");

            //set page timeout to 5 minutes
            this.Server.ScriptTimeout = 300;

            try
            {
                var file = Request.Files["importxmlfile"];
                if (file != null && file.ContentLength > 0)
                {
					_services.Localization.ImportResourcesFromXml(language, file.InputStream.AsString(), mode: mode, updateTouchedResources: updateTouched);
                }
                else
                {
					NotifyError(T("Admin.Common.UploadFile"));
                    return RedirectToAction("Edit", new { id = language.Id });
                }

                NotifySuccess(T("Admin.Configuration.Languages.Imported"));
                return RedirectToAction("Edit", new { id = language.Id });
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("Edit", new { id = language.Id });
            }

        }

        #endregion

        #region Download

        private void DownloadCore(ILifetimeScope scope, CancellationToken ct, LanguageDownloadContext context)
        {
            var asyncState = scope.Resolve<IAsyncState>();
            var services = scope.Resolve<ICommonServices>();
            var languageService = scope.Resolve<ILanguageService>();
            var logger = scope.Resolve<ILogger>();
            var tempFilePath = Path.Combine(FileSystemHelper.TempDirTenant(), Guid.NewGuid().ToString() + ".xml");

            try
            {
                var currentVersion = SmartStoreVersion.CurrentFullVersion;
                var availableResources = context.CheckAvailableResources.First(x => x.Id == context.SetId);

                // 1. Download resources
                var state = asyncState.Get<LanguageDownloadState>() ?? new LanguageDownloadState
                {
                    Id = context.SetId,
                    ProgressMessage = T("Admin.Configuration.Languages.DownloadingResources")
                };
                asyncState.Set(state);

                var url = CommonHelper.GetAppSetting<string>("sm:TranslateDownloadUrl").FormatInvariant(context.SetId);
                var buffer = new byte[32768];
                var len = 0;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Text.Xml));
                    client.DefaultRequestHeaders.UserAgent.ParseAdd($"SmartStore.NET {currentVersion}");
                    client.DefaultRequestHeaders.Add("Authorization-Key", services.StoreContext.CurrentStore.Url.EmptyNull().TrimEnd('/'));

                    using (var sourceStream = client.GetStreamAsync(url).Result)
                    using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                    {
                        while ((len = sourceStream.Read(buffer, 0, 32768)) > 0)
                        {
                            fileStream.Write(buffer, 0, len);
                        }
                    }
                }

                state.ProgressMessage = T("Admin.Configuration.Languages.ImportResources");
                asyncState.Set(state);

                // 2. Create language entity (if required)
                var allLanguages = languageService.GetAllLanguages();
                var lastLanguage = allLanguages.OrderByDescending(x => x.DisplayOrder).FirstOrDefault();

                var language = languageService.GetLanguageByCulture(availableResources.Language.Culture);
                if (language == null)
                {
                    language = new Language();
                    language.LanguageCulture = availableResources.Language.Culture;
                    language.UniqueSeoCode = availableResources.Language.TwoLetterIsoCode;
                    language.Name = GetCultureDisplayName(availableResources.Language.Culture) ?? availableResources.Name;
                    language.FlagImageFileName = GetFlagFileName(availableResources.Language.TwoLetterIsoCode);
                    language.Rtl = availableResources.Language.Rtl;
                    language.Published = false;
                    language.DisplayOrder = lastLanguage != null ? lastLanguage.DisplayOrder + 1 : 0;

                    languageService.InsertLanguage(language);
                }

                // 3. Import resources
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(tempFilePath);

                services.Localization.ImportResourcesFromXml(language, xmlDoc);
            }
            catch (Exception exception)
            {
                logger.ErrorsAll(exception);
            }
            finally
            {
                if (asyncState.Exists<LanguageDownloadState>())
                {
                    asyncState.Remove<LanguageDownloadState>();
                }

                FileSystemHelper.Delete(tempFilePath);
            }
        }

        public ActionResult Download(int setId)
        {
            if (_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
            {
                var ctx = new LanguageDownloadContext(setId);
                ctx.CheckAvailableResources = CheckAvailableResources();

                AsyncRunner.Run(
                    (container, ct, obj) => DownloadCore(container, ct, obj as LanguageDownloadContext),
                    ctx, 
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
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
            catch (Exception exception)
            {
                exception.Dump();
            }

            return Json(null);
        }

        #endregion Download
    }
}
