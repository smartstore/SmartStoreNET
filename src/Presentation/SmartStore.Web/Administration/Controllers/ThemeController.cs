using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Hosting;
using System.Web.Mvc;
using SmartStore.Admin.Models.Themes;
using SmartStore.Collections;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Security;
using SmartStore.Core.Themes;
using SmartStore.Services.Themes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Theming.Assets;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public partial class ThemeController : AdminControllerBase
    {
        private readonly IThemeRegistry _themeRegistry;
        private readonly IThemeVariablesService _themeVarService;
        private readonly IThemeContext _themeContext;
        private readonly IAssetCache _assetCache;

        public ThemeController(
            IThemeRegistry themeRegistry,
            IThemeVariablesService themeVarService,
            IThemeContext themeContext,
            IAssetCache assetCache)
        {
            _themeVarService = themeVarService;
            _themeRegistry = themeRegistry;
            _themeContext = themeContext;
            _assetCache = assetCache;
        }

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Configuration.Theme.Read)]
        public ActionResult List(int? storeId)
        {
            var selectedStoreId = storeId ?? Services.StoreContext.CurrentStore.Id;
            var themeSettings = Services.Settings.LoadSetting<ThemeSettings>(selectedStoreId);
            var model = themeSettings.ToModel();

            var bundlingOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = "{0} ({1})".FormatCurrent(T("Common.Auto"), T("Common.Recommended")) },
                new SelectListItem { Value = "1", Text = T("Common.No") },
                new SelectListItem { Value = "2", Text = T("Common.Yes") }
            };
            model.AvailableBundleOptimizationValues.AddRange(bundlingOptions);
            model.AvailableBundleOptimizationValues.FirstOrDefault(x => int.Parse(x.Value) == model.BundleOptimizationEnabled).Selected = true;

            var assetCachingOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "0", Text = T("Common.Auto") },
                new SelectListItem { Value = "1", Text = T("Common.No") },
                new SelectListItem { Value = "2", Text = "{0} ({1})".FormatCurrent(T("Common.Yes"), T("Common.Recommended")) }
            };
            model.AvailableAssetCachingValues.AddRange(assetCachingOptions);
            model.AvailableAssetCachingValues.FirstOrDefault(x => int.Parse(x.Value) == model.AssetCachingEnabled).Selected = true;

            // Add theme configs.
            model.Themes.AddRange(GetThemes(themeSettings));

            model.StoreId = selectedStoreId;
            model.AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems();

            return View(model);
        }

        private IList<ThemeManifestModel> GetThemes(ThemeSettings themeSettings, bool includeHidden = true)
        {
            var themes = from m in _themeRegistry.GetThemeManifests(includeHidden)
                         select PrepareThemeManifestModel(m, themeSettings);

            var sortedThemes = themes.ToArray().SortTopological(StringComparer.OrdinalIgnoreCase).Cast<ThemeManifestModel>();

            return sortedThemes.OrderByDescending(x => x.IsActive).ToList();
        }

        protected virtual ThemeManifestModel PrepareThemeManifestModel(ThemeManifest manifest, ThemeSettings themeSettings)
        {
            var model = new ThemeManifestModel
            {
                Name = manifest.ThemeName,
                BaseTheme = manifest.BaseThemeName,
                Title = manifest.ThemeTitle,
                Description = manifest.PreviewText,
                Author = manifest.Author,
                Url = manifest.Url,
                Version = manifest.Version,
                PreviewImageUrl = manifest.PreviewImageUrl.HasValue() ? manifest.PreviewImageUrl : "{0}{1}/preview.png".FormatInvariant(manifest.Location.EnsureEndsWith("/"), manifest.ThemeName),
                IsActive = themeSettings.DefaultTheme == manifest.ThemeName,
                State = manifest.State,
            };

            model.IsConfigurable = HostingEnvironment.VirtualPathProvider.FileExists("{0}{1}/Views/Shared/ConfigureTheme.cshtml".FormatInvariant(manifest.Location.EnsureEndsWith("/"), manifest.ThemeName));

            return model;
        }

        [HttpPost, ActionName("List")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Theme.Update)]
        public ActionResult ListPost(ThemeListModel model, FormCollection form)
        {
            var themeSettings = Services.Settings.LoadSetting<ThemeSettings>(model.StoreId);

            var themeSwitched = themeSettings.DefaultTheme.IsCaseInsensitiveEqual(model.DefaultTheme);
            if (themeSwitched)
            {
                Services.EventPublisher.Publish(new ThemeSwitchedEvent
                {
                    OldTheme = themeSettings.DefaultTheme,
                    NewTheme = model.DefaultTheme
                });
            }

            var bundlingOnNow = themeSettings.BundleOptimizationEnabled == 2 || (themeSettings.BundleOptimizationEnabled == 0 && !HttpContext.IsDebuggingEnabled);
            var bundlingOnFuture = model.BundleOptimizationEnabled == 2 || (model.BundleOptimizationEnabled == 0 && !HttpContext.IsDebuggingEnabled);
            if (bundlingOnNow != bundlingOnFuture)
            {
                // Clear asset cache, otherwise we get problems with postprocessing, minification etc.
                _assetCache.Clear();
            }

            themeSettings = model.ToEntity(themeSettings);
            Services.Settings.SaveSetting(themeSettings, model.StoreId);

            Services.EventPublisher.Publish(new ModelBoundEvent(model, themeSettings, form));

            NotifySuccess(T("Admin.Configuration.Updated"));

            return RedirectToAction("List", new { storeId = model.StoreId });
        }

        [Permission(Permissions.Configuration.Theme.Read)]
        public ActionResult Configure(string theme, int storeId)
        {
            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List", new { storeId });
            }

            var model = new ConfigureThemeModel
            {
                ThemeName = theme,
                StoreId = storeId,
                AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems()
            };

            ViewData["ConfigureThemeUrl"] = Url.Action("Configure", new { theme });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Theme.Update)]
        public ActionResult Configure(string theme, int storeId, IDictionary<string, object> values, bool continueEditing)
        {
            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List", new { storeId });
            }

            try
            {
                values = FixThemeVarValues(values);
                _themeVarService.SaveThemeVariables(theme, storeId, values);

                Services.CustomerActivity.InsertActivity("EditThemeVars", T("ActivityLog.EditThemeVars"), theme);

                NotifySuccess(T("Admin.Configuration.Themes.Notifications.ConfigureSuccess"));

                return continueEditing
                    ? RedirectToAction("Configure", new { theme, storeId })
                    : RedirectToAction("List", new { storeId });
            }
            catch (ThemeValidationException ex)
            {
                TempData["SassParsingError"] = ex.Message.Trim().TrimStart('\r', '\n', '/', '*').TrimEnd('*', '/', '\r', '\n');
                TempData["OverriddenThemeVars"] = ex.AttemptedVars;
                NotifyError(T("Admin.Configuration.Themes.Notifications.ConfigureError"));
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            // Fail.
            return RedirectToAction("Configure", new { theme, storeId });
        }

        private IDictionary<string, object> FixThemeVarValues(IDictionary<string, object> values)
        {
            var fixedDict = new Dictionary<string, object>();

            foreach (var kvp in values)
            {
                var value = kvp.Value;

                var strValue = string.Empty;

                var arrValue = value as string[];
                if (arrValue != null)
                {
                    strValue = strValue = arrValue.Length > 0 ? arrValue[0] : value.ToString();
                }
                else
                {
                    strValue = value.ToString();
                }

                fixedDict[kvp.Key] = strValue;
            }

            return fixedDict;
        }

        [Permission(Permissions.Configuration.Theme.Update)]
        public ActionResult ReloadThemes(int? storeId)
        {
            _themeRegistry.ReloadThemes();

            return RedirectToAction("List", new { storeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Theme.Update)]
        public ActionResult Reset(string theme, int storeId)
        {
            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List", new { storeId });
            }

            _themeVarService.DeleteThemeVariables(theme, storeId);

            Services.CustomerActivity.InsertActivity("ResetThemeVars", T("ActivityLog.ResetThemeVars"), theme);

            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    Message = T("Admin.Configuration.Themes.Notifications.ResetSuccess").Text
                }
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Configuration.Theme.Update)]
        public ActionResult ImportVariables(string theme, int storeId, FormCollection form)
        {
            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List", new { storeId });
            }

            try
            {
                var file = Request.Files["importxmlfile"];
                if (file != null && file.ContentLength > 0)
                {
                    int importedCount = 0;
                    importedCount = _themeVarService.ImportVariables(theme, storeId, file.InputStream.AsString());

                    try
                    {
                        Services.CustomerActivity.InsertActivity("ImportThemeVars", T("ActivityLog.ResetThemeVars"), importedCount, theme);
                    }
                    catch { }

                    NotifySuccess(T("Admin.Configuration.Themes.Notifications.ImportSuccess", importedCount));
                }
                else
                {
                    NotifyError(T("Admin.Configuration.Themes.Notifications.UploadFile"));
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return RedirectToAction("Configure", new { theme, storeId });
        }

        [HttpPost]
        [Permission(Permissions.Configuration.Theme.Read)]
        public ActionResult ExportVariables(string theme, int storeId, FormCollection form)
        {
            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return RedirectToAction("List", new { storeId });
            }

            try
            {
                var xml = _themeVarService.ExportVariables(theme, storeId);

                if (xml.IsEmpty())
                {
                    NotifyInfo(T("Admin.Configuration.Themes.Notifications.NoExportInfo"));
                }
                else
                {
                    string profileName = form["exportprofilename"];
                    string fileName = "themevars-{0}{1}-{2}.xml".FormatCurrent(theme, profileName.HasValue() ? "-" + profileName.ToValidFileName() : "", DateTime.Now.ToString("yyyyMMdd"));

                    try
                    {
                        Services.CustomerActivity.InsertActivity("ExportThemeVars", T("ActivityLog.ExportThemeVars"), theme);
                    }
                    catch { }

                    return new XmlDownloadResult(xml, fileName);
                }
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return RedirectToAction("Configure", new { theme, storeId });
        }

        [Permission(Permissions.Configuration.Theme.Update)]
        public ActionResult ClearAssetCache()
        {
            try
            {
                _assetCache.Clear();
                NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
            }
            catch (Exception ex)
            {
                NotifyError(ex);
            }

            return RedirectToReferrer();
        }

        #endregion

        #region Preview

        [Permission(Permissions.Configuration.Theme.Read)]
        public ActionResult Preview(string theme, int? storeId, string returnUrl)
        {
            // Initializes the preview mode.
            if (!storeId.HasValue)
            {
                storeId = Services.StoreContext.CurrentStore.Id;
            }

            if (theme.IsEmpty())
            {
                theme = Services.Settings.LoadSetting<ThemeSettings>(storeId.Value).DefaultTheme;
            }

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
                return HttpNotFound();
            }

            using (HttpContext.PreviewModeCookie())
            {
                _themeContext.SetPreviewTheme(theme);
                Services.StoreContext.SetPreviewStore(storeId);
            }

            if (returnUrl.IsEmpty() && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                returnUrl = Request.UrlReferrer.ToString();
            }

            TempData["PreviewModeReturnUrl"] = returnUrl;

            return RedirectToAction("Index", "Home", new { area = (string)null });
        }

        [Permission(Permissions.Configuration.Theme.Read)]
        public ActionResult PreviewTool()
        {
            // Prepares data for the preview mode (flyout) tool.
            var currentTheme = _themeContext.CurrentTheme;
            ViewBag.Themes = (from m in _themeRegistry.GetThemeManifests(false)
                              select new SelectListItem
                              {
                                  Value = m.ThemeName,
                                  Text = m.ThemeTitle,
                                  Selected = m == currentTheme
                              }).ToList();

            var currentStore = Services.StoreContext.CurrentStore;
            ViewBag.Stores = Services.StoreService.GetAllStores().Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name,
                Selected = x.Id == currentStore.Id
            }).ToList();

            var themeSettings = Services.Settings.LoadSetting<ThemeSettings>(currentStore.Id);
            ViewBag.DisableApply = themeSettings.DefaultTheme.IsCaseInsensitiveEqual(currentTheme.ThemeName);
            var cookie = Request.Cookies["sm:PreviewToolOpen"];
            ViewBag.ToolOpen = cookie != null ? cookie.Value.ToBool() : false;

            return PartialView();
        }

        [HttpPost, ActionName("PreviewTool")]
        [FormValueRequired(FormValueRequirementRule.MatchAll, "theme", "storeId")]
        [FormValueAbsent(FormValueRequirement.StartsWith, "PreviewMode.")]
        [Permission(Permissions.Configuration.Theme.Update)]
        public ActionResult PreviewToolPost(string theme, int storeId, string returnUrl)
        {
            // Refreshes the preview mode (after a select change).
            using (HttpContext.PreviewModeCookie())
            {
                _themeContext.SetPreviewTheme(theme);
                Services.StoreContext.SetPreviewStore(storeId);
            }

            return RedirectToReferrer(returnUrl);
        }

        [HttpPost, ActionName("PreviewTool"), FormValueRequired("PreviewMode.Exit")]
        [Permission(Permissions.Configuration.Theme.Read)]
        public ActionResult ExitPreview()
        {
            // Exits the preview mode.
            using (HttpContext.PreviewModeCookie())
            {
                _themeContext.SetPreviewTheme(null);
                Services.StoreContext.SetPreviewStore(null);
            }

            var returnUrl = (string)TempData["PreviewModeReturnUrl"];
            return RedirectToReferrer(returnUrl);
        }

        [HttpPost, ActionName("PreviewTool"), FormValueRequired("PreviewMode.Apply")]
        [Permission(Permissions.Configuration.Theme.Update)]
        public ActionResult ApplyPreviewTheme(string theme, int storeId)
        {
            // Applies the current previewed theme and exits the preview mode.
            var themeSettings = Services.Settings.LoadSetting<ThemeSettings>(storeId);
            var oldTheme = themeSettings.DefaultTheme;
            themeSettings.DefaultTheme = theme;
            var themeSwitched = !oldTheme.IsCaseInsensitiveEqual(theme);

            if (themeSwitched)
            {
                Services.EventPublisher.Publish(new ThemeSwitchedEvent
                {
                    OldTheme = oldTheme,
                    NewTheme = theme
                });
            }

            Services.Settings.SaveSetting(themeSettings, storeId);

            NotifySuccess(T("Admin.Configuration.Updated"));

            return ExitPreview();
        }

        #endregion
    }
}
