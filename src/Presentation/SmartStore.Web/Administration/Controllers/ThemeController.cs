using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Services.Configuration;
using SmartStore.Core.Themes;
using SmartStore.Services.Security;
using SmartStore.Admin.Models.Themes;
using SmartStore.Admin.Models.Settings;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Themes;
using SmartStore.Services.Themes;
using SmartStore.Web.Framework.Mvc;
using System.IO;
using System.Text;
using SmartStore.Core.Events;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class ThemeController : AdminControllerBase
	{
		#region Fields

        private readonly ISettingService _settingService;
        private readonly IThemeRegistry _themeRegistry;
        private readonly IThemeVariablesService _themeVarService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IEventPublisher _eventPublisher;
		private readonly IStoreService _storeService;
		private readonly IStoreContext _storeContext;

	    #endregion

		#region Constructors

        public ThemeController(
            ISettingService settingService, IThemeRegistry themeRegistry,
            IThemeVariablesService themeVarService,
            ICustomerActivityService customerActivityService, IPermissionService permissionService,
            ILocalizationService localizationService,
            IEventPublisher eventPublisher,
			IStoreService storeService,
			IStoreContext storeContext)
		{
            this._settingService = settingService;
            this._themeVarService = themeVarService;
            this._permissionService = permissionService;
            this._themeRegistry = themeRegistry;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
            this._eventPublisher = eventPublisher;
			this._storeService = storeService;
			this._storeContext = storeContext;
		}

		#endregion 

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List(int? storeId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

			int selectedStoreId = storeId ?? _storeContext.CurrentStore.Id;
			var themeSettings = _settingService.LoadSetting<ThemeSettings>(selectedStoreId);
            var model = themeSettings.ToModel();

            var commonListItems = new List<SelectListItem> 
            {
                new SelectListItem { Value = "0", Text = _localizationService.GetResource("Common.Auto") },
                new SelectListItem { Value = "1", Text = _localizationService.GetResource("Common.No") },
                new SelectListItem { Value = "2", Text = _localizationService.GetResource("Common.Yes") }
            };

            model.AvailableBundleOptimizationValues.AddRange(commonListItems);
            model.AvailableBundleOptimizationValues.FirstOrDefault(x => int.Parse(x.Value) == model.BundleOptimizationEnabled).Selected = true;

            // add theme configs
            model.DesktopThemes.AddRange(GetThemes(false, themeSettings));
            model.MobileThemes.AddRange(GetThemes(true, themeSettings));

			model.StoreId = selectedStoreId;
			model.AvailableStores = _storeService.GetAllStores().ToSelectListItems();

            return View(model);
        }

        private IList<ThemeManifestModel> GetThemes(bool mobile, ThemeSettings themeSettings)
        {
            var themes = from m in _themeRegistry.GetThemeManifests()
                                where m.MobileTheme == mobile
                                select PrepareThemeManifestModel(m, themeSettings);
            return themes.OrderByDescending(x => x.IsActive).ThenBy(x => x.Name).ToList();
        }

		protected virtual ThemeManifestModel PrepareThemeManifestModel(ThemeManifest manifest, ThemeSettings themeSettings)
        {
            var model = new ThemeManifestModel
                {
                    Name = manifest.ThemeName,
                    Title = manifest.ThemeTitle,
                    Description = manifest.PreviewText,
                    Author = manifest.Author,
                    Version = manifest.Version,
                    IsMobileTheme = manifest.MobileTheme,
                    SupportsRtl = manifest.SupportRtl,
                    PreviewImageUrl = manifest.PreviewImageUrl.HasValue() ? manifest.PreviewImageUrl : "{0}/{1}/preview.png".FormatInvariant(manifest.Location, manifest.ThemeName),
                    IsActive = manifest.MobileTheme ? themeSettings.DefaultMobileTheme == manifest.ThemeName : themeSettings.DefaultDesktopTheme == manifest.ThemeName
                };

            if (System.IO.File.Exists(System.IO.Path.Combine(manifest.Path, "Views\\Shared\\ConfigureTheme.cshtml")))
            {
                model.IsConfigurable = true;
            }
            
            return model;
        }

        [HttpPost]
        [ActionName("List")]
        public ActionResult ListPost(ThemeListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

			var themeSettings = _settingService.LoadSetting<ThemeSettings>(model.StoreId);

            bool showRestartNote = model.MobileDevicesSupported != themeSettings.MobileDevicesSupported;

            bool mobileThemeSwitched = false;
            bool themeSwitched = themeSettings.DefaultDesktopTheme.IsCaseInsensitiveEqual(model.DefaultDesktopTheme);
            if (!themeSwitched)
            {
                themeSwitched = themeSettings.DefaultMobileTheme.IsCaseInsensitiveEqual(model.DefaultMobileTheme);
                mobileThemeSwitched = themeSwitched;
            }

            if (themeSwitched)
            {
                _eventPublisher.Publish<ThemeSwitchedMessage>(new ThemeSwitchedMessage { 
                    IsMobile = mobileThemeSwitched,
                    OldTheme = mobileThemeSwitched ? themeSettings.DefaultMobileTheme : themeSettings.DefaultDesktopTheme,
                    NewTheme = mobileThemeSwitched ? model.DefaultMobileTheme : model.DefaultDesktopTheme
                });
            }

            themeSettings = model.ToEntity(themeSettings);
			_settingService.SaveSetting(themeSettings, model.StoreId);
            
            // activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Updated"));

            if (showRestartNote)
            {
                NotifyInfo(_localizationService.GetResource("Admin.Common.RestartAppRequest"));
            }

			return RedirectToAction("List", new { storeId = model.StoreId });
        }

        public ActionResult Configure(string theme, int storeId, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
				return RedirectToAction("List", new { storeId = storeId });
            }

            var model = new ConfigureThemeModel
            {
                ThemeName = theme,
				StoreId = storeId,
				AvailableStores = _storeService.GetAllStores().ToSelectListItems()
            };

			ViewData["ConfigureThemeUrl"] = Url.Action("Configure", new { theme = theme, selectedTab = selectedTab });
            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
		public ActionResult Configure(string theme, int storeId, Dictionary<string, object> values, bool continueEditing, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
				return RedirectToAction("List", new { storeId = storeId });
            }

            // save now
            _themeVarService.SaveThemeVariables(theme, storeId, values);

            // activity log
            _customerActivityService.InsertActivity("EditThemeVars", _localizationService.GetResource("ActivityLog.EditThemeVars"), theme);

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.ConfigureSuccess"));

			return continueEditing ?
				RedirectToAction("Configure", new { theme = theme, storeId = storeId, selectedTab = selectedTab }) :
				RedirectToAction("List", new { storeId = storeId });
        }

        public ActionResult Reset(string theme, int storeId, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
				return RedirectToAction("List", new { storeId = storeId });
            }

            _themeVarService.DeleteThemeVariables(theme, storeId);

            // activity log
            _customerActivityService.InsertActivity("ResetThemeVars", _localizationService.GetResource("ActivityLog.ResetThemeVars"), theme);

            NotifySuccess(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.ResetSuccess"));
            return RedirectToAction("Configure", new { theme = theme, storeId = storeId, selectedTab = selectedTab });
        }

        [HttpPost]
        public ActionResult ImportVariables(string theme, int storeId, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
				return RedirectToAction("List", new { storeId = storeId });
            }

            try
            {
                var file = Request.Files["importxmlfile"];
                if (file != null && file.ContentLength > 0)
                {
                    int importedCount = 0;
                    using (var sr = new StreamReader(file.InputStream, Encoding.UTF8))
                    {
                        string content = sr.ReadToEnd();
                        importedCount = _themeVarService.ImportVariables(theme, storeId, content);
                    }

                    // activity log
                    try
                    {
                        _customerActivityService.InsertActivity("ImportThemeVars", _localizationService.GetResource("ActivityLog.ResetThemeVars"), importedCount, theme);
                    }
                    catch { }

                    NotifySuccess(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.ImportSuccess").FormatInvariant(importedCount));
                }
                else
                {
					NotifyError(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.UploadFile"));
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return RedirectToAction("Configure", new { theme = theme, storeId = storeId });
        }

        [HttpPost]
        public ActionResult ExportVariables(string theme, int storeId, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
				return RedirectToAction("List", new { storeId = storeId });
            }

            try
            {
                var xml = _themeVarService.ExportVariables(theme, storeId);

                if (xml.IsEmpty())
                {
                    NotifyInfo(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.NoExportInfo"));
                }
                else
                {
                    string profileName = form["exportprofilename"];
                    string fileName = "themevars-{0}{1}-{2}.xml".FormatCurrent(theme, profileName.HasValue() ? "-" + profileName.ToValidFileName() : "", DateTime.Now.ToString("yyyyMMdd"));

                    // activity log
                    try
                    {
                        _customerActivityService.InsertActivity("ExportThemeVars", _localizationService.GetResource("ActivityLog.ExportThemeVars"), theme);
                    }
                    catch { }

                    return new XmlDownloadResult(xml, fileName);
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }
            
            return RedirectToAction("Configure", new { theme = theme, storeId = storeId });
        }

        #endregion
    }
}
