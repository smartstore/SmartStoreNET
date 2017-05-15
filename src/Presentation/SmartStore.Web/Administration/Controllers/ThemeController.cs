using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using SmartStore.Admin.Models.Themes;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Domain.Themes;
using SmartStore.Core.Localization;
using SmartStore.Core.Packaging;
using SmartStore.Core.Themes;
using SmartStore.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Services.Themes;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Theming;

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
		private readonly IThemeContext _themeContext;
		private readonly Lazy<IThemeFileResolver> _themeFileResolver;

	    #endregion

		#region Constructors

        public ThemeController(
            ISettingService settingService, 
			IThemeRegistry themeRegistry,
            IThemeVariablesService themeVarService,
			IStoreService storeService,
			IPackageManager packageManager,
			ICommonServices services,
			IThemeContext themeContext,
			Lazy<IThemeFileResolver> themeFileResolver)
		{
            this._settingService = settingService;
            this._themeVarService = themeVarService;
            this._themeRegistry = themeRegistry;
			this._storeService = storeService;
			this._packageManager = packageManager;
			this._services = services;
			this._themeContext = themeContext;
			this._themeFileResolver = themeFileResolver;
		}

		#endregion 

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
            model.Themes.AddRange(GetThemes(themeSettings));

			model.StoreId = selectedStoreId;
			model.AvailableStores = _storeService.GetAllStores().ToSelectListItems();

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
                    SupportsRtl = manifest.SupportRtl,
                    PreviewImageUrl = manifest.PreviewImageUrl.HasValue() ? manifest.PreviewImageUrl : "{0}/{1}/preview.png".FormatInvariant(manifest.Location, manifest.ThemeName),
                    IsActive = themeSettings.DefaultTheme == manifest.ThemeName,
					State = manifest.State
                };

			if (HostingEnvironment.VirtualPathProvider.FileExists("{0}/{1}/Views/Shared/ConfigureTheme.cshtml".FormatInvariant(manifest.Location, manifest.ThemeName)))
			{
				model.IsConfigurable = true;
			}
            
            return model;
        }

		[HttpPost, ActionName("List")]
        public ActionResult ListPost(ThemeListModel model)
        {
			if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
                return AccessDeniedView();

			var themeSettings = _settingService.LoadSetting<ThemeSettings>(model.StoreId);

            bool themeSwitched = themeSettings.DefaultTheme.IsCaseInsensitiveEqual(model.DefaultTheme);

            if (themeSwitched)
            {
                _services.EventPublisher.Publish<ThemeSwitchedEvent>(new ThemeSwitchedEvent { 
                    OldTheme = themeSettings.DefaultTheme,
                    NewTheme = model.DefaultTheme
                });
            }

            themeSettings = model.ToEntity(themeSettings);
			_settingService.SaveSetting(themeSettings, model.StoreId);
            
            // activity log
			_services.CustomerActivity.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));

			NotifySuccess(T("Admin.Configuration.Updated"));

			return RedirectToAction("List", new { storeId = model.StoreId });
        }

        public ActionResult Configure(string theme, int storeId)
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

			ViewData["ConfigureThemeUrl"] = Url.Action("Configure", new { theme = theme });
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
		public ActionResult Configure(string theme, int storeId, IDictionary<string, object> values, bool continueEditing)
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
			values = FixThemeVarValues(values);
			_themeVarService.SaveThemeVariables(theme, storeId, values);

			// check for parsing error
			var manifest = _themeRegistry.GetThemeManifest(theme);
			string error = ValidateSass(manifest, storeId);
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

				TempData["SassParsingError"] = error.Trim().TrimStart('\r', '\n', '/', '*').TrimEnd('*', '/', '\r', '\n');
				TempData["OverriddenThemeVars"] = values;
				NotifyError(T("Admin.Configuration.Themes.Notifications.ConfigureError"));
				return RedirectToAction("Configure", new { theme = theme, storeId = storeId });
			}

            // activity log
			_services.CustomerActivity.InsertActivity("EditThemeVars", T("ActivityLog.EditThemeVars"), theme);

			NotifySuccess(T("Admin.Configuration.Themes.Notifications.ConfigureSuccess"));

			return continueEditing ?
				RedirectToAction("Configure", new { theme = theme, storeId = storeId }) :
				RedirectToAction("List", new { storeId = storeId });
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

		/// <summary>
		/// Validates the result SASS file by calling it's url.
		/// </summary>
		/// <param name="theme">Theme name</param>
		/// <param name="storeId">Stored Id</param>
		/// <returns>The error message when a parsing error occured, <c>null</c> otherwise</returns>
		private string ValidateSass(ThemeManifest manifest, int storeId)
		{	
			string error = string.Empty;

			var virtualPath = "~/Themes/{0}/Content/theme.scss".FormatCurrent(manifest.ThemeName);
			var resolver = this._themeFileResolver.Value;
			var file = resolver.Resolve(virtualPath);
			if (file != null)
			{
				virtualPath = file.ResultVirtualPath;
			}

			var url = "{0}?storeId={1}&theme={2}&validate=1".FormatInvariant(
				WebHelper.GetAbsoluteUrl(virtualPath, this.Request),
				storeId,
				manifest.ThemeName);

			var request = WebHelper.CreateHttpRequestForSafeLocalCall(new Uri(url));
			WebResponse response = null;

			try
			{
				response = request.GetResponse();
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
			catch (Exception ex)
			{
				error = ex.Message;
			}
			finally
			{
				if (response != null)
					response.Close();
			}

			return error;
		}

		public ActionResult ReloadThemes(int? storeId)
		{
			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageThemes))
			{
				_themeRegistry.ReloadThemes();
			}
	
			return RedirectToAction("List", new { storeId = storeId });
		}

        public ActionResult Reset(string theme, int storeId)
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
            return RedirectToAction("Configure", new { theme = theme, storeId = storeId });
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
					importedCount = _themeVarService.ImportVariables(theme, storeId, file.InputStream.AsString());

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

		#region Preview

		public ActionResult Preview(string theme, int? storeId, string returnUrl)
		{
			// Initializes the preview mode

			if (!storeId.HasValue)
			{
				storeId = _services.StoreContext.CurrentStore.Id;
			}

			if (theme.IsEmpty())
			{
				theme = _settingService.LoadSetting<ThemeSettings>(storeId.Value).DefaultTheme;
			}

			if (!_themeRegistry.ThemeManifestExists(theme))
				return HttpNotFound();

			using (HttpContext.PreviewModeCookie())
			{
				_themeContext.SetPreviewTheme(theme);
				_services.StoreContext.SetPreviewStore(storeId);
			}

			if (returnUrl.IsEmpty() && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
			{
				returnUrl = Request.UrlReferrer.ToString();
			}

			TempData["PreviewModeReturnUrl"] = returnUrl;

			return RedirectToAction("Index", "Home", new { area = (string)null });
		}

		public ActionResult PreviewTool()
		{
			// Prepares data for the preview mode (flyout) tool

			var currentTheme = _themeContext.CurrentTheme;
			ViewBag.Themes = (from m in _themeRegistry.GetThemeManifests(false)
						 select new SelectListItem
						 {
							 Value = m.ThemeName,
							 Text = m.ThemeTitle,
							 Selected = m == currentTheme
						 }).ToList();

			var currentStore = _services.StoreContext.CurrentStore;
			ViewBag.Stores = (_storeService.GetAllStores().Select(x => new SelectListItem
						 {
							 Value = x.Id.ToString(),
							 Text = x.Name,
							 Selected = x.Id == currentStore.Id
						 })).ToList();

			var themeSettings = _settingService.LoadSetting<ThemeSettings>(currentStore.Id);
			ViewBag.DisableApply = themeSettings.DefaultTheme.IsCaseInsensitiveEqual(currentTheme.ThemeName);
			var cookie = Request.Cookies["sm:PreviewToolOpen"];
			ViewBag.ToolOpen = cookie != null ? cookie.Value.ToBool() : false;

			return PartialView();
		}

		[HttpPost,  ActionName("PreviewTool")]
		[FormValueRequired(FormValueRequirementRule.MatchAll, "theme", "storeId")]
		[FormValueAbsent(FormValueRequirement.StartsWith, "PreviewMode.")]
		public ActionResult PreviewToolPost(string theme, int storeId, string returnUrl)
		{
			// Refreshes the preview mode (after a select change)

			using (HttpContext.PreviewModeCookie())
			{
				_themeContext.SetPreviewTheme(theme);
				_services.StoreContext.SetPreviewStore(storeId);
			}

			return RedirectToReferrer(returnUrl);
		}

		[HttpPost, ActionName("PreviewTool"), FormValueRequired("PreviewMode.Exit")]
		public ActionResult ExitPreview()
		{
			// Exits the preview mode

			using (HttpContext.PreviewModeCookie())
			{
				_themeContext.SetPreviewTheme(null);
				_services.StoreContext.SetPreviewStore(null);
			}

			var returnUrl = (string)TempData["PreviewModeReturnUrl"];
			return RedirectToReferrer(returnUrl);
		}

		[HttpPost, ActionName("PreviewTool"), FormValueRequired("PreviewMode.Apply")]
		public ActionResult ApplyPreviewTheme(string theme, int storeId)
		{
			// Applies the current previewed theme and exits the preview mode

			var themeSettings = _settingService.LoadSetting<ThemeSettings>(storeId);
			var oldTheme = themeSettings.DefaultTheme;
			themeSettings.DefaultTheme = theme;
			var themeSwitched = !oldTheme.IsCaseInsensitiveEqual(theme);

			if (themeSwitched)
			{
				_services.EventPublisher.Publish<ThemeSwitchedEvent>(new ThemeSwitchedEvent
				{
					OldTheme = oldTheme,
					NewTheme = theme
				});
			}

			_settingService.SaveSetting(themeSettings, storeId);

			_services.CustomerActivity.InsertActivity("EditSettings", T("ActivityLog.EditSettings"));
			NotifySuccess(T("Admin.Configuration.Updated"));

			return ExitPreview();
		}

		#endregion

	}
}
