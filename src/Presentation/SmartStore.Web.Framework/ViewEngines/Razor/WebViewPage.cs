#region Using...

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Web.Mvc;
using System.Web.WebPages;
using SmartStore.Core;
using SmartStore.Core.Localization;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Themes;
using SmartStore.Services.Localization;
using SmartStore.Services.Themes;
using SmartStore.Web.Framework.Themes;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.UI;
using SmartStore.Collections;
using System.Linq;
using SmartStore.Core.Logging;
using SmartStore.Web.Framework.Controllers;

#endregion

namespace SmartStore.Web.Framework.ViewEngines.Razor
{
    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
    {

		private IText _text;
        private IWorkContext _workContext;

		private IList<NotifyEntry> _internalNotifications;
        private IThemeRegistry _themeRegistry;
        private IThemeContext _themeContext;
        private ThemeManifest _themeManifest;
        private ExpandoObject _themeVars;
        private bool? _isHomePage;

        protected int CurrentCategoryId
        {
            get
            {
                var routeValues = Url.RequestContext.RouteData.Values;
                int id = 0;
                if (routeValues["controller"].ToString().IsCaseInsensitiveEqual("catalog") && 
                    routeValues["action"].ToString().IsCaseInsensitiveEqual("category"))
                {
                    id = Convert.ToInt32(routeValues["categoryId"].ToString());
                }
                return id;
            }
        }

        protected int CurrentManufacturerId
        {
            get
            {
                var routeValues = Url.RequestContext.RouteData.Values;
                int id = 0;
                if (routeValues["controller"].ToString().IsCaseInsensitiveEqual("catalog") &&
                    routeValues["action"].ToString().IsCaseInsensitiveEqual("manufacturer"))
                {
                    id = Convert.ToInt32(routeValues["manufacturerId"].ToString());
                }
                return id;
            }
        }

        protected int CurrentProductId
        {
            get
            {
                var routeValues = Url.RequestContext.RouteData.Values;
                int id = 0;
                if (routeValues["controller"].ToString().IsCaseInsensitiveEqual("catalog") &&
                    routeValues["action"].ToString().IsCaseInsensitiveEqual("product"))
                {
                    id = Convert.ToInt32(routeValues["productId"].ToString());
                }
                return id;
            }
        }

        protected bool IsHomePage
        {
            get
            {
                if (!_isHomePage.HasValue)
                {
                    var routeData = Url.RequestContext.RouteData;
                    _isHomePage = routeData.GetRequiredString("controller").IsCaseInsensitiveEqual("Home") &&
                        routeData.GetRequiredString("action").IsCaseInsensitiveEqual("Index");
                }
                return _isHomePage.Value;
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
				
				if (TempData.ContainsKey(key))
				{
					entries = TempData[key] as IList<NotifyEntry>;
					if (entries != null)
					{
						result = result.Concat(entries);
					}
				}

				if (ViewData.ContainsKey(key))
				{
					entries = ViewData[key] as IList<NotifyEntry>;
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
            }
        }

        public HelperResult RenderWrappedSection(string name, object wrapperHtmlAttributes)
        {
            Action<TextWriter> action = delegate(TextWriter tw)
                                {
                                    var htmlAttributes = HtmlHelper.AnonymousObjectToHtmlAttributes(wrapperHtmlAttributes);
                                    var tagBuilder = new TagBuilder("div");
                                    tagBuilder.MergeAttributes(htmlAttributes);

                                    var section = RenderSection(name, false);
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
            return IsSectionDefined(sectionName) ? RenderSection(sectionName) : defaultContent(new object());
        }

        public override string Layout
        {
            get
            {
                var layout = base.Layout;

                if (!string.IsNullOrEmpty(layout))
                {
                    var filename = System.IO.Path.GetFileNameWithoutExtension(layout);
                    ViewEngineResult viewResult = System.Web.Mvc.ViewEngines.Engines.FindView(ViewContext.Controller.ControllerContext, filename, "");

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
                if (_themeManifest == null)
                {
                    EnsureThemeContextInitialized();
                    _themeManifest = _themeRegistry.GetThemeManifest(this.ThemeName);
                }
                return _themeManifest;
            }
        }

        /// <summary>
        /// Gets the current theme name. Override this in configuration views.
        /// </summary>
        protected virtual string ThemeName
        {
            get
            {
                EnsureThemeContextInitialized();
                return _themeContext.WorkingDesktopTheme;
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
            Guard.ArgumentNotEmpty(varName, "varName");

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