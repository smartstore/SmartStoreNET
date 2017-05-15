using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Localization;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.DataExchange;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
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
        private readonly AdminAreaSettings _adminAreaSettings;
		private readonly IPluginFinder _pluginFinder;
		private readonly ICountryService _countryService;
		private readonly ICommonServices _services;

        #endregion

        #region Constructors

        public LanguageController(ILanguageService languageService,
			IStoreMappingService storeMappingService,
            AdminAreaSettings adminAreaSettings,
			IPluginFinder pluginFinder,
			ICountryService countryService,
			ICommonServices services)
        {
            this._languageService = languageService;
			this._storeMappingService = storeMappingService;
            this._adminAreaSettings = adminAreaSettings;
			this._pluginFinder = pluginFinder;
			this._countryService = countryService;
			this._services = services;
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

            var gridModel = new GridModel<LanguageModel>
            {
                Data = languages.Select(x => x.ToModel()),
                Total = languages.Count()
            };

            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
			var model = new GridModel<LanguageModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageLanguages))
			{
				var languages = _languageService.GetAllLanguages(true);

				model.Data = languages.Select(x => x.ToModel());
				model.Total = languages.Count();
			}
			else
			{
				model.Data = Enumerable.Empty<LanguageModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
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
    }
}
