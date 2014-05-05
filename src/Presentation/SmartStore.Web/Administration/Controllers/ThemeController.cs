using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Services.Configuration;
using SmartStore.Core.Themes;
using SmartStore.Services.Security;
using SmartStore.Admin.Models.Themes;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Services.Themes;
using SmartStore.Web.Framework.Mvc;
using System.IO;
using System.Text;
using SmartStore.Core.Events;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
using SmartStore.Core.Packaging;
using System.Threading.Tasks;
using System.Net;
using System.Web.Hosting;
using SmartStore.Services;
using SmartStore.Core.Localization;
using System.Diagnostics;
using SmartStore.Web.Framework.Themes;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
    public partial class ThemeController : AdminControllerBase
	{
		#region Fields

        private readonly ISettingService _settingService;
        private readonly IThemeRegistry _themeRegistry;
        private readonly IThemeVariablesService _themeVarService;
		private readonly IStoreService _storeService;
		private readonly IPackageManager _packageManager;
		private readonly ICommonServices _services;

	    #endregion

		#region Constructors

        public ThemeController(
            ISettingService settingService, 
			IThemeRegistry themeRegistry,
            IThemeVariablesService themeVarService,
			IStoreService storeService,
			IPackageManager packageManager,
			ICommonServices services)
		{
            this._settingService = settingService;
            this._themeVarService = themeVarService;
            this._themeRegistry = themeRegistry;
			this._storeService = storeService;
			this._packageManager = packageManager;
			this._services = services;

			this.T = NullLocalizer.Instance;
		}

		#endregion 

		public Localizer T { get; set; }

        #region Methods

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List(int? storeId)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

			int selectedStoreId = storeId ?? _services.StoreContext.CurrentStore.Id;
			var themeSettings = _settingService.LoadSetting<ThemeSettings>(selectedStoreId);
            var model = themeSettings.ToModel();

            var commonListItems = new List<SelectListItem> 
            {
                new SelectListItem { Value = "0", Text = T("Common.Auto") },
                new SelectListItem { Value = "1", Text = T("Common.No") },
                new SelectListItem { Value = "2", Text = T("Common.Yes") }
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
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
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
                _services.EventPublisher.Publish<ThemeSwitchedMessage>(new ThemeSwitchedMessage { 
                    IsMobile = mobileThemeSwitched,
                    OldTheme = mobileThemeSwitched ? themeSettings.DefaultMobileTheme : themeSettings.DefaultDesktopTheme,
                    NewTheme = mobileThemeSwitched ? model.DefaultMobileTheme : model.DefaultDesktopTheme
                });
            }

            themeSettings = model.ToEntity(themeSettings);
			_settingService.SaveSetting(themeSettings, model.StoreId);
            
            // activity log
			_services.CustomerActivity.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));

			NotifySuccess(T("Admin.Configuration.Updated"));

            if (showRestartNote)
            {
				NotifyInfo(T("Admin.Common.RestartAppRequest"));
            }

			return RedirectToAction("List", new { storeId = model.StoreId });
        }

        public ActionResult Configure(string theme, int storeId, string selectedTab)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
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
		public async Task<ActionResult> Configure(string theme, int storeId, Dictionary<string, object> values, bool continueEditing, string selectedTab)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
				return RedirectToAction("List", new { storeId = storeId });
            }		

			// get current for later restore on parse error
			var currentVars = _themeVarService.GetThemeVariables(theme, storeId);
			
            // save now
            _themeVarService.SaveThemeVariables(theme, storeId, values);

			// check for parsing error
			var manifest = _themeRegistry.GetThemeManifest(theme);
			string error = await ValidateLess(manifest, storeId);
			if (error.HasValue())
			{
				// restore previous vars
				try
				{
					_themeVarService.DeleteThemeVariables(theme, storeId);
				}
				finally
				{
					// we do it here to absolutely ensure that this gets called
					_themeVarService.SaveThemeVariables(theme, storeId, currentVars);
				}

				TempData["LessParsingError"] = error.Trim().TrimStart('\r', '\n', '/', '*').TrimEnd('*', '/', '\r', '\n');
				TempData["OverriddenThemeVars"] = values;
				NotifyError(T("Admin.Configuration.Themes.Notifications.ConfigureError"));
				return RedirectToAction("Configure", new { theme = theme, storeId = storeId, selectedTab = selectedTab });
			}

            // activity log
			_services.CustomerActivity.InsertActivity("EditThemeVars", T("ActivityLog.EditThemeVars"), theme);

			NotifySuccess(T("Admin.Configuration.Themes.Notifications.ConfigureSuccess"));

			return continueEditing ?
				RedirectToAction("Configure", new { theme = theme, storeId = storeId, selectedTab = selectedTab }) :
				RedirectToAction("List", new { storeId = storeId });
        }

		/// <summary>
		/// Validates the result LESS file by calling it's url.
		/// </summary>
		/// <param name="theme">Theme name</param>
		/// <param name="storeId">Stored Id</param>
		/// <returns>The error message when a parsing error occured, <c>null</c> otherwise</returns>
		private async Task<string> ValidateLess(ThemeManifest manifest, int storeId)
		{
			string error = string.Empty;
			var url = "{0}Themes/{1}/Content/theme.less?storeId={2}&theme={1}".FormatInvariant(
				_services.WebHelper.GetStoreLocation().EnsureEndsWith("/"), 
				manifest.ThemeName,
				storeId);

			HttpWebRequest request = WebRequest.CreateHttp(url);
			WebResponse response = null;

			try
			{
				response = await request.GetResponseAsync();
			}
			catch (WebException ex)
			{
				if (ex.Response is HttpWebResponse)
				{
					var webResponse = (HttpWebResponse)ex.Response;

					var statusCode = webResponse.StatusCode;

					if (statusCode == HttpStatusCode.InternalServerError)
					{
						// catch only 500, as this indicates a parsing error.
						var stream = webResponse.GetResponseStream();

						using (var streamReader = new StreamReader(stream))
						{
							// read the content (the error message has been put there)
							error = streamReader.ReadToEnd();
							streamReader.Close();
							stream.Close();
						}
					}
				}
			}
			finally
			{
				if (response != null)
					response.Close();
			}

			return error;
		}

        public ActionResult Reset(string theme, int storeId, string selectedTab)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

            if (!_themeRegistry.ThemeManifestExists(theme))
            {
				return RedirectToAction("List", new { storeId = storeId });
            }

            _themeVarService.DeleteThemeVariables(theme, storeId);

            // activity log
			_services.CustomerActivity.InsertActivity("ResetThemeVars", T("ActivityLog.ResetThemeVars"), theme);

			NotifySuccess(T("Admin.Configuration.Themes.Notifications.ResetSuccess"));
            return RedirectToAction("Configure", new { theme = theme, storeId = storeId, selectedTab = selectedTab });
        }

        [HttpPost]
        public ActionResult ImportVariables(string theme, int storeId, FormCollection form)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
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
						_services.CustomerActivity.InsertActivity("ImportThemeVars", T("ActivityLog.ResetThemeVars"), importedCount, theme);
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

            return RedirectToAction("Configure", new { theme = theme, storeId = storeId });
        }

        [HttpPost]
        public ActionResult ExportVariables(string theme, int storeId, FormCollection form)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
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
					NotifyInfo(T("Admin.Configuration.Themes.Notifications.NoExportInfo"));
                }
                else
                {
                    string profileName = form["exportprofilename"];
                    string fileName = "themevars-{0}{1}-{2}.xml".FormatCurrent(theme, profileName.HasValue() ? "-" + profileName.ToValidFileName() : "", DateTime.Now.ToString("yyyyMMdd"));

                    // activity log
                    try
                    {
						_services.CustomerActivity.InsertActivity("ExportThemeVars", T("ActivityLog.ExportThemeVars"), theme);
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
