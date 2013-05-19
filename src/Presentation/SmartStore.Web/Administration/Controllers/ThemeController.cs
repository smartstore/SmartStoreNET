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
using SmartStore.Services.Logging;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework.Themes;
using SmartStore.Services.Themes;
using SmartStore.Web.Framework.Mvc;
using System.IO;
using System.Text;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class ThemeController : AdminControllerBase
	{
		#region Fields

        private ThemeSettings _themeSettings;
        private readonly ISettingService _settingService;
        private readonly IThemeRegistry _themeRegistry;
        private readonly IThemeVariablesService _themeVarService;
        private readonly IPermissionService _permissionService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;

	    #endregion

		#region Constructors

        public ThemeController(
            ISettingService settingService, ThemeSettings themeSettings, IThemeRegistry themeRegistry,
            IThemeVariablesService themeVarService,
            ICustomerActivityService customerActivityService, IPermissionService permissionService,
            ILocalizationService localizationService)
		{
            this._settingService = settingService;
            this._themeSettings = themeSettings;
            this._themeVarService = themeVarService;
            this._permissionService = permissionService;
            this._themeRegistry = themeRegistry;
            this._customerActivityService = customerActivityService;
            this._localizationService = localizationService;
		}

		#endregion 

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            var model = _themeSettings.ToModel();

            var commonListItems = new List<SelectListItem> 
            {
                new SelectListItem { Value = "0", Text = _localizationService.GetResource("Common.Auto") },
                new SelectListItem { Value = "1", Text = _localizationService.GetResource("Common.No") },
                new SelectListItem { Value = "2", Text = _localizationService.GetResource("Common.Yes") }
            };

            model.AvailableBundleOptimizationValues.AddRange(commonListItems);
            model.AvailableBundleOptimizationValues.FirstOrDefault(x => int.Parse(x.Value) == model.BundleOptimizationEnabled).Selected = true;

            model.AvailableCssCacheValues.AddRange(commonListItems);
            model.AvailableCssCacheValues.FirstOrDefault(x => int.Parse(x.Value) == model.CssCacheEnabled).Selected = true;

            model.AvailableCssMinifyValues.AddRange(commonListItems);
            model.AvailableCssMinifyValues.FirstOrDefault(x => int.Parse(x.Value) == model.CssMinifyEnabled).Selected = true;

            // add theme configs
            model.DesktopThemes.AddRange(GetThemes(false));
            model.MobileThemes.AddRange(GetThemes(true));

            return View(model);
        }

        private IList<ThemeManifestModel> GetThemes(bool mobile)
        {
            var themes = from m in _themeRegistry.GetThemeManifests()
                                where m.MobileTheme == mobile
                                select PrepareThemeManifestModel(m);
            return themes.OrderByDescending(x => x.IsActive).ThenBy(x => x.Name).ToList();
        }

        protected virtual ThemeManifestModel PrepareThemeManifestModel(ThemeManifest manifest)
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
                    PreviewImageUrl = manifest.PreviewImageUrl,
                    IsActive = manifest.MobileTheme ? _themeSettings.DefaultMobileTheme == manifest.ThemeName : _themeSettings.DefaultDesktopTheme == manifest.ThemeName
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

            bool showRestartNote = model.BundleOptimizationEnabled != _themeSettings.BundleOptimizationEnabled
                                    || model.CssCacheEnabled != _themeSettings.CssCacheEnabled
                                    || model.CssMinifyEnabled != _themeSettings.CssMinifyEnabled
                                    || model.MobileDevicesSupported != _themeSettings.MobileDevicesSupported;

            _themeSettings = model.ToEntity(_themeSettings);
            _settingService.SaveSetting(_themeSettings);

            //activity log
            _customerActivityService.InsertActivity("EditSettings", _localizationService.GetResource("ActivityLog.EditSettings"));

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Updated"));

            if (showRestartNote)
            {
                InfoNotification(_localizationService.GetResource("Admin.Common.RestartAppRequest"));
            }

            return RedirectToAction("List");
        }

        public ActionResult Configure(string theme, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List");
            }

            var model = new ConfigureThemeModel
            {
                ThemeName = theme
            };

            ViewData["SelectedTab"] = selectedTab;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormNameAttribute("save-continue", "continueEditing")]
        public ActionResult Configure(string theme, Dictionary<string, object> values, bool continueEditing, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List");
            }

            // save now
            _themeVarService.SaveThemeVariables(theme, values);

            // activity log
            _customerActivityService.InsertActivity("EditThemeVars", _localizationService.GetResource("ActivityLog.EditThemeVars"), theme);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.ConfigureSuccess"));
            return continueEditing ? RedirectToAction("Configure", new { theme = theme, selectedTab = selectedTab }) : RedirectToAction("List");
        }

        public ActionResult Reset(string theme, string selectedTab)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List");
            }

            _themeVarService.DeleteThemeVariables(theme);

            // activity log
            _customerActivityService.InsertActivity("ResetThemeVars", _localizationService.GetResource("ActivityLog.ResetThemeVars"), theme);

            SuccessNotification(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.ResetSuccess"));
            return RedirectToAction("Configure", new { theme = theme, selectedTab = selectedTab });
        }

        [HttpPost]
        public ActionResult ImportVariables(string theme, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List");
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
                        importedCount = _themeVarService.ImportVariables(theme, content);
                    }

                    // activity log
                    _customerActivityService.InsertActivity("ImportThemeVars", _localizationService.GetResource("ActivityLog.ResetThemeVars"), importedCount, theme);

                    SuccessNotification(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.ImportSuccess").FormatInvariant(importedCount));
                }
                else
                {
                    ErrorNotification(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.UploadFile"));
                }
            }
            catch (Exception ex)
            {
                ErrorNotification(ex);
            }

            return RedirectToAction("Configure", new { theme = theme });
        }

        [HttpPost]
        public ActionResult ExportVariables(string theme, FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List");
            }

            try
            {
                var xml = _themeVarService.ExportVariables(theme);

                if (xml.IsEmpty())
                {
                    InfoNotification(_localizationService.GetResource("Admin.Configuration.Themes.Notifications.NoExportInfo"));
                }
                else
                {
                    string profileName = form["exportprofilename"];
                    string fileName = "themevars-{0}{1}-{2}.xml".FormatCurrent(theme, profileName.HasValue() ? "-" + profileName.ToValidFileName() : "", DateTime.Now.ToString("yyyyMMdd"));

                    // activity log
                    _customerActivityService.InsertActivity("ExportThemeVars", _localizationService.GetResource("ActivityLog.ExportThemeVars"), theme);

                    return new XmlDownloadResult(xml, fileName);
                }
            }
            catch (Exception ex)
            {
                ErrorNotification(ex);
            }
            
            return RedirectToAction("Configure", new { theme = theme });
        }

        #endregion
    }
}
