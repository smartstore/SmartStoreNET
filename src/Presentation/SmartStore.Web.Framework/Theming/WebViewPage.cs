using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.WebPages;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Themes;
using SmartStore.Services;

namespace SmartStore.Web.Framework.Theming
{
    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
    {
		private WebViewPageHelper _helper;

        protected int CurrentCategoryId
        {
            get
            {
				return _helper.CurrentCategoryId;
            }
        }

        protected int CurrentManufacturerId
        {
            get
            {
				return _helper.CurrentManufacturerId;
			}
        }

        protected int CurrentProductId
        {
            get
            {
				return _helper.CurrentProductId;
			}
        }

        protected bool IsHomePage
        {
            get
            {
				return _helper.IsHomePage;
			}
        }

		protected bool IsMobileDevice
		{
			get
			{
				return _helper.IsMobileDevice;
			}
		}

        protected bool IsStoreClosed
        {
            get
            {
                return _helper.IsStoreClosed;
            }
        }

		public bool EnableHoneypotProtection
		{
			get { return _helper.EnableHoneypotProtection; }
		}

		protected bool HasMessages
		{
			get
			{
				return _helper.ResolveNotifications(null).Any();
			}
		}

		protected ICollection<LocalizedString> GetMessages(NotifyType type)
		{
			return _helper.ResolveNotifications(type).AsReadOnly();
		}

        /// <summary>
        /// Get a localized resource
        /// </summary>
        public Localizer T
        {
            get
            {
				return _helper.T;
            }
        }

		public ICommonServices CommonServices
		{
			get
			{
				return _helper.Services;
			}
		}

		public IWorkContext WorkContext
        {
            get
            {
                return _helper.Services.WorkContext;
            }
        }
        
        public override void InitHelpers()
        {
            base.InitHelpers();

			_helper = EngineContext.Current.Resolve<WebViewPageHelper>();
			_helper.Initialize(this.ViewContext);
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
		/// Gets the manifest of the current active theme
		/// </summary>
		protected ThemeManifest ThemeManifest
        {
            get
            {
				return _helper.ThemeContext?.CurrentTheme;
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
                return _helper.ThemeContext?.WorkingThemeName;
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
		/// Gets the runtime theme variables as specified in the theme's config file
		/// alongside the merged user-defined variables
		/// </summary>
		public dynamic ThemeVariables
		{
			get
			{
				return _helper.ThemeVariables;
			}
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
			var webHelper = _helper.Services?.WebHelper;
			if (webHelper == null)
			{
				return url;
			}

			url = url.NullEmpty() ?? webHelper.GetThisPageUrl(true);
			var url2 = webHelper.ModifyQueryString(url, query, null);

			if (removeQueryName.HasValue())
			{
				url2 = webHelper.RemoveQueryString(url2, removeQueryName);
			}

			return url2;
		}

		public string GenerateHelpUrl(HelpTopic topic)
		{
			var seoCode = WorkContext?.WorkingLanguage?.UniqueSeoCode;
			if (seoCode.IsEmpty())
			{
				return topic?.EnPath;
			}

			return SmartStoreVersion.GenerateHelpUrl(seoCode, topic);
		}

		public string GenerateHelpUrl(string path)
		{
			var seoCode = WorkContext?.WorkingLanguage?.UniqueSeoCode;
			if (seoCode.IsEmpty())
			{
				return path;
			}

			return SmartStoreVersion.GenerateHelpUrl(seoCode, path);
		}

		/// <summary>
		/// Tries to find a matching localization file for a given culture in the following order 
		/// (assuming <paramref name="culture"/> is 'de-DE', <paramref name="pattern"/> is 'lang-*.js' and <paramref name="fallbackCulture"/> is 'en-US'):
		/// <list type="number">
		///		<item>Exact match > lang-de-DE.js</item>
		///		<item>Neutral culture > lang-de.js</item>
		///		<item>Any region for language > lang-de-CH.js</item>
		///		<item>Exact match for fallback culture > lang-en-US.js</item>
		///		<item>Neutral fallback culture > lang-en.js</item>
		///		<item>Any region for fallback language > lang-en-GB.js</item>
		/// </list>
		/// </summary>
		/// <param name="culture">The ISO culture code to get a localization file for, e.g. 'de-DE'</param>
		/// <param name="virtualPath">The virtual path to search in</param>
		/// <param name="pattern">The pattern to match, e.g. 'lang-*.js'. The wildcard char MUST exist.</param>
		/// <param name="fallbackCulture">Optional.</param>
		/// <returns>Result</returns>
		public LocalizationFileResolveResult ResolveLocalizationFile(
			string culture,
			string virtualPath,
			string pattern,
			string fallbackCulture = "en")
		{
			return _helper.LocalizationFileResolver.Resolve(culture, virtualPath, pattern, true, fallbackCulture);
		}
	}

    public abstract class WebViewPage : WebViewPage<dynamic>
    {
    }
}