using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Themes;
using SmartStore.Services.Common;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Theming
{
    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
    {
		private IText _text;
        private IWorkContext _workContext;

		private IList<NotifyEntry> _internalNotifications;
        private IThemeRegistry _themeRegistry;
        private IThemeContext _themeContext;
		private IMobileDeviceHelper _mobileDeviceHelper;
		private IWebHelper _webHelper;
		private ExpandoObject _themeVars;
        private bool? _isHomePage;
		private bool? _isMobileDevice;
		private int? _currentCategoryId;
		private int? _currentManufacturerId;
		private int? _currentProductId;

        protected int CurrentCategoryId
        {
            get
            {
				if (!_currentCategoryId.HasValue)
				{
					int id = 0;
					var routeValues = this.Url.RequestContext.RouteData.Values;
					if (routeValues["controller"].ToString().IsCaseInsensitiveEqual("catalog") 
						&& routeValues["action"].ToString().IsCaseInsensitiveEqual("category") 
						&& routeValues.ContainsKey("categoryId"))
					{
						id = Convert.ToInt32(routeValues["categoryId"].ToString());
					}
					_currentCategoryId = id;
				}

				return _currentCategoryId.Value;
            }
        }

        protected int CurrentManufacturerId
        {
            get
            {
				if (!_currentManufacturerId.HasValue)
				{
					var routeValues = this.Url.RequestContext.RouteData.Values;
					int id = 0;
					if (routeValues["controller"].ToString().IsCaseInsensitiveEqual("catalog") 
						&& routeValues["action"].ToString().IsCaseInsensitiveEqual("manufacturer")
						&& routeValues.ContainsKey("manufacturerId"))
					{
						id = Convert.ToInt32(routeValues["manufacturerId"].ToString());
					}
					_currentManufacturerId = id;
				}

				return _currentManufacturerId.Value;
            }
        }

        protected int CurrentProductId
        {
            get
            {
				if (!_currentProductId.HasValue)
				{
					var routeValues = this.Url.RequestContext.RouteData.Values;
					int id = 0;
					if (routeValues["controller"].ToString().IsCaseInsensitiveEqual("product") 
						&& routeValues["action"].ToString().IsCaseInsensitiveEqual("productdetails")
						&& routeValues.ContainsKey("productId"))
					{
						id = Convert.ToInt32(routeValues["productId"].ToString());
					}
					_currentProductId = id;
				}

				return _currentProductId.Value;
            }
        }

        protected bool IsHomePage
        {
            get
            {
                if (!_isHomePage.HasValue)
                {
                    var routeData = this.Url.RequestContext.RouteData;
                    _isHomePage = routeData.GetRequiredString("controller").IsCaseInsensitiveEqual("Home") &&
                        routeData.GetRequiredString("action").IsCaseInsensitiveEqual("Index");
                }

                return _isHomePage.Value;
            }
        }

		protected bool IsMobileDevice
		{
			get
			{
				if (!_isMobileDevice.HasValue)
				{
					_isMobileDevice = _mobileDeviceHelper.IsMobileDevice();
				}

				return _isMobileDevice.Value;
			}
		}

		protected bool HasMessages
		{
			get
			{
				return ResolveNotifications(null).Any();
			}
		}

		protected ICollection<LocalizedString> GetMessages(NotifyType type)
		{
			return ResolveNotifications(type).AsReadOnly();
		}

		private IEnumerable<LocalizedString> ResolveNotifications(NotifyType? type)
		{						
			IEnumerable<NotifyEntry> result = Enumerable.Empty<NotifyEntry>();

			if (_internalNotifications == null)
			{
				string key = NotifyAttribute.NotificationsKey;
				IList<NotifyEntry> entries;
				
				if (this.TempData.ContainsKey(key))
				{
					entries = this.TempData[key] as IList<NotifyEntry>;
					if (entries != null)
					{
						result = result.Concat(entries);
					}
				}

				if (this.ViewData.ContainsKey(key))
				{
					entries = this.ViewData[key] as IList<NotifyEntry>;
					if (entries != null)
					{
						result = result.Concat(entries);
					}
				}

				_internalNotifications = new List<NotifyEntry>(result);
			}

			if (type == null)
			{
				return _internalNotifications.Select(x => x.Message);
			}

			return _internalNotifications.Where(x => x.Type == type.Value).Select(x => x.Message);
		}

        /// <summary>
        /// Get a localized resource
        /// </summary>
        public Localizer T
        {
            get
            {
				return _text.Get;
            }
        }

        public IWorkContext WorkContext
        {
            get
            {
                return _workContext;
            }
        }
        
        public override void InitHelpers()
        {
            base.InitHelpers();

            if (DataSettings.DatabaseIsInstalled())
            {
				_text = EngineContext.Current.Resolve<IText>();
                _workContext = EngineContext.Current.Resolve<IWorkContext>();
				_mobileDeviceHelper = EngineContext.Current.Resolve<IMobileDeviceHelper>();
				_webHelper = EngineContext.Current.Resolve<IWebHelper>();
			}
        }

        public HelperResult RenderWrappedSection(string name, object wrapperHtmlAttributes)
        {
            Action<TextWriter> action = delegate(TextWriter tw)
                                {
                                    var htmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(wrapperHtmlAttributes);
                                    var tagBuilder = new TagBuilder("div");
                                    tagBuilder.MergeAttributes(htmlAttributes);

                                    var section = this.RenderSection(name, false);
                                    if (section != null)
                                    {
                                        tw.Write(tagBuilder.ToString(TagRenderMode.StartTag));
                                        section.WriteTo(tw);
                                        tw.Write(tagBuilder.ToString(TagRenderMode.EndTag));
                                    }
                                };
            return new HelperResult(action);
        }

        public HelperResult RenderSection(string sectionName, Func<object, HelperResult> defaultContent)
        {
            return this.IsSectionDefined(sectionName) ? this.RenderSection(sectionName) : defaultContent(new object());
        }

        public override string Layout
        {
            get
            {
                var layout = base.Layout;

                if (!string.IsNullOrEmpty(layout))
                {
                    var filename = System.IO.Path.GetFileNameWithoutExtension(layout);
                    ViewEngineResult viewResult = System.Web.Mvc.ViewEngines.Engines.FindView(this.ViewContext.Controller.ControllerContext, filename, "");

                    if (viewResult.View != null && viewResult.View is RazorView)
                    {
                        layout = (viewResult.View as RazorView).ViewPath;
                    }
                }

                return layout;
            }
            set
            {
                base.Layout = value;
            }
        }

        /// <summary>
        /// Return a value indicating whether the working language and theme support RTL (right-to-left)
        /// </summary>
        /// <returns></returns>
        public bool ShouldUseRtlTheme()
        {
            var supportRtl = _workContext.WorkingLanguage.Rtl;
            if (supportRtl)
            {
                //ensure that the active theme also supports it
                supportRtl = this.ThemeManifest.SupportRtl;
            }
            return supportRtl;
        }

        /// <summary>
        /// Gets the manifest of the current active theme
        /// </summary>
        protected ThemeManifest ThemeManifest
        {
            get
            {
				EnsureThemeContextInitialized();
				return _themeContext.CurrentTheme;
            }
        }

        /// <summary>
        /// Gets the current theme name. Override this in configuration views.
        /// </summary>
		[Obsolete("The theme name gets resolved automatically now. No need to override anymore.")]
        protected virtual string ThemeName
        {
            get
            {
                EnsureThemeContextInitialized();
                return _themeContext.WorkingThemeName;
            }
        }

        /// <summary>
        /// Gets the runtime theme variables as specified in the theme's config file
        /// alongside the merged user-defined variables
        /// </summary>
        public dynamic ThemeVariables
        {
            get
            {
                if (_themeVars == null)
                {
					var storeContext = EngineContext.Current.Resolve<IStoreContext>();
                    var repo = new ThemeVarsRepository();
                    _themeVars = repo.GetRawVariables(this.ThemeManifest.ThemeName, storeContext.CurrentStore.Id);
                }

                return _themeVars;
            }
        }

        public string GetThemeVariable(string varname, string defaultValue = "")
        {
            return GetThemeVariable<string>(varname, defaultValue);
        }

        /// <summary>
        /// Gets a runtime theme variable value
        /// </summary>
        /// <param name="varName">The name of the variable</param>
        /// <param name="defaultValue">The default value to return if the variable does not exist</param>
        /// <returns>The theme variable value</returns>
        public T GetThemeVariable<T>(string varName, T defaultValue = default(T))
        {
            Guard.NotEmpty(varName, "varName");

            var vars = this.ThemeVariables as IDictionary<string, object>;
            if (vars != null && vars.ContainsKey(varName))
            {
                string value = vars[varName] as string;
                if (!value.HasValue())
                {
                    return defaultValue;
                }
                return (T)value.Convert(typeof(T));
            }

            return defaultValue;
        }

		/// <summary>
		/// Modifies a URL (appends/updates a query string part and optionally removes another query string).
		/// </summary>
		/// <param name="url">The URL to modifiy. If <c>null</c>, the current page's URL is resolved.</param>
		/// <param name="query">The new query string part.</param>
		/// <param name="removeQueryName">A query string name to remove.</param>
		/// <returns>The modified URL.</returns>
		public string ModifyUrl(string url, string query, string removeQueryName = null)
		{
			url = url.NullEmpty() ?? _webHelper.GetThisPageUrl(true);
			var url2 =  _webHelper.ModifyQueryString(url, query, null);

			if (removeQueryName.HasValue())
			{
				url2 = _webHelper.RemoveQueryString(url2, removeQueryName);
			}

			return url2;
		}

		public string GenerateHelpUrl(string path)
		{
			return SmartStoreVersion.GenerateHelpUrl(WorkContext.WorkingLanguage.UniqueSeoCode, path);
		}

        private void EnsureThemeContextInitialized()
        {
            if (_themeRegistry == null)
                _themeRegistry = EngineContext.Current.Resolve<IThemeRegistry>();
            if (_themeContext == null)
                _themeContext = EngineContext.Current.Resolve<IThemeContext>();
        }

    }

    public abstract class WebViewPage : WebViewPage<dynamic>
    {
    }
}