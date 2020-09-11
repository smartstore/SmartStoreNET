using System;
using System.Globalization;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Localization;

namespace SmartStore.Web.Framework.Localization
{
    /// <summary>
    /// Provides properties and methods for defining a localized route, and for getting information about the localized route.
    /// </summary>
    public class LocalizedRoute : Route, ICloneable<LocalizedRoute>
    {
        #region Fields

        private static bool? _seoFriendlyUrlsForLanguagesEnabled;
        private string _leftPart;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern and handler class.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public LocalizedRoute(string url, IRouteHandler routeHandler)
            : base(url, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern, handler class and default parameter values.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public LocalizedRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
            : base(url, defaults, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern, handler class, default parameter values and constraints.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="constraints">A regular expression that specifies valid values for a URL parameter.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public LocalizedRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
            : base(url, defaults, constraints, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern, handler class, default parameter values, 
        /// constraints,and custom values.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="constraints">A regular expression that specifies valid values for a URL parameter.</param>
        /// <param name="dataTokens">Custom values that are passed to the route handler, but which are not used to determine whether the route matches a specific URL pattern. The route handler might need these values to process the request.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public LocalizedRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataTokens, routeHandler)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns information about the requested route.
        /// </summary>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
        /// <returns>
        /// An object that contains the values from the route definition.
        /// </returns>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            if (DataSettings.DatabaseIsInstalled() && SeoFriendlyUrlsForLanguagesEnabled)
            {
                var helper = new LocalizedUrlHelper(httpContext.Request);
                if (helper.IsLocalizedUrl())
                {
                    httpContext.RememberAppRelativePath();

                    helper.StripSeoCode();
                    httpContext.RewritePath("~/" + helper.RelativePath, true);
                }
            }

            if (_leftPart == null)
            {
                var url = this.Url;
                int idx = url.IndexOf('{');
                _leftPart = "~/" + (idx >= 0 ? url.Substring(0, idx) : url).TrimEnd('/');
            }

            // Perf
            if (!httpContext.Request.AppRelativeCurrentExecutionFilePath.StartsWith(_leftPart, true, CultureInfo.InvariantCulture))
                return null;

            RouteData data = base.GetRouteData(httpContext);
            return data;
        }

        /// <summary>
        /// Returns information about the URL that is associated with the route.
        /// </summary>
        /// <param name="requestContext">An object that encapsulates information about the requested route.</param>
        /// <param name="values">An object that contains the parameters for a route.</param>
        /// <returns>
        /// An object that contains information about the URL that is associated with the route.
        /// </returns>
        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            var data = base.GetVirtualPath(requestContext, values);

            if (data != null && DataSettings.DatabaseIsInstalled() && SeoFriendlyUrlsForLanguagesEnabled)
            {
                var helper = new LocalizedUrlHelper(requestContext.HttpContext.Request, true);
                if (helper.IsLocalizedUrl(out string cultureCode))
                {
                    if (requestContext.RouteData.DataTokens.Get("SeoCodeReplacement") is string seoCodeReplacement)
                    {
                        // The LanguageSeoCode filter detected a localized URL, but the locale does not exist or is inactive.
                        // The routing system is therefore about to render the "NotFound" view. Here we ensure that generated links
                        // in NotFound page do not contain the invalid seo code anymore: Either we strip it off or we replace it
                        // with the default language's seo code (according to "LocalizationSettings.DefaultLanguageRedirectBehaviour" setting).
                        cultureCode = seoCodeReplacement;
                    }

                    if (cultureCode.HasValue())
                    {
                        data.VirtualPath = String.Concat(cultureCode, "/", data.VirtualPath).TrimEnd('/');
                    }
                }
            }

            return data;
        }

        public static void ClearSeoFriendlyUrlsCachedValue()
        {
            _seoFriendlyUrlsForLanguagesEnabled = null;
        }

        #endregion

        #region Properties

        public bool IsClone { get; private set; }

        protected internal static bool SeoFriendlyUrlsForLanguagesEnabled
        {
            get
            {
                if (_seoFriendlyUrlsForLanguagesEnabled == null && EngineContext.Current.IsFullyInitialized)
                {
                    try
                    {
                        var enabled = EngineContext.Current.Resolve<LocalizationSettings>().SeoFriendlyUrlsForLanguagesEnabled;
                        _seoFriendlyUrlsForLanguagesEnabled = enabled;
                    }
                    catch { }
                }

                // Assume is enabled on very first request to prevent IIS 404 with localized URLs
                return _seoFriendlyUrlsForLanguagesEnabled ?? true;
            }
        }

        #endregion

        #region Clone Members

        public LocalizedRoute Clone()
        {
            var clone = new LocalizedRoute(this.Url,
                new RouteValueDictionary(this.Defaults),
                new RouteValueDictionary(this.Constraints),
                new RouteValueDictionary(this.DataTokens),
                new MvcRouteHandler())
            {
                RouteExistingFiles = this.RouteExistingFiles,
                IsClone = true
            };
            return clone;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        #endregion
    }
}