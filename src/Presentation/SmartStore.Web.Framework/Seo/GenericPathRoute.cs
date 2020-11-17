using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using SmartStore.Collections;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Seo;
using SmartStore.Web.Framework.Localization;

namespace SmartStore.Web.Framework.Seo
{
    /// <summary>
    /// Provides properties and methods for defining a SEO friendly route, and for getting information about the route.
    /// </summary>
    public class GenericPathRoute : LocalizedRoute
    {
        const string SlugKey = "generic_se_name";

        // Key = Prefix, Value = EntityType
        private static readonly Multimap<string, string> _urlPrefixes = new Multimap<string, string>(StringComparer.OrdinalIgnoreCase);

        private static readonly Dictionary<string, GenericPath> _paths = new Dictionary<string, GenericPath>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern and handler class.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GenericPathRoute(string url, IRouteHandler routeHandler)
            : base(url, routeHandler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the System.Web.Routing.Route class, using the specified URL pattern, handler class and default parameter values.
        /// </summary>
        /// <param name="url">The URL pattern for the route.</param>
        /// <param name="defaults">The values to use if the URL does not contain all the parameters.</param>
        /// <param name="routeHandler">The object that processes requests for the route.</param>
        public GenericPathRoute(string url, RouteValueDictionary defaults, IRouteHandler routeHandler)
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
        public GenericPathRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, IRouteHandler routeHandler)
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
        public GenericPathRoute(string url, RouteValueDictionary defaults, RouteValueDictionary constraints, RouteValueDictionary dataTokens, IRouteHandler routeHandler)
            : base(url, defaults, constraints, dataTokens, routeHandler)
        {
        }

        public static void RegisterPaths(params GenericPath[] paths)
        {
            foreach (var path in paths)
            {
                if (path.EntityName.IsEmpty())
                {
                    throw new ArgumentException($"'{nameof(path)}.{nameof(path.EntityName)}' is required.", nameof(paths));
                }

                if (path.IdParamName.IsEmpty())
                {
                    throw new ArgumentException($"'{nameof(path)}.{nameof(path.IdParamName)}' is required.", nameof(paths));
                }

                if (path.Route == null)
                {
                    throw new ArgumentException($"'{nameof(path)}.{nameof(path.Route)}' is required.", nameof(paths));
                }

                _paths[path.EntityName] = path;
            }
        }

        public static IEnumerable<GenericPath> Paths { get; } = _paths.Values.OrderBy(x => x.Order);

        public static void RegisterUrlPrefix(string prefix, params string[] entityNames)
        {
            Guard.NotEmpty(prefix, nameof(prefix));

            _urlPrefixes.AddRange(prefix, entityNames);
        }

        public static string GetUrlPrefixFor(string entityName)
        {
            Guard.NotEmpty(entityName, nameof(entityName));

            if (_urlPrefixes.Count == 0)
                return null;

            return _urlPrefixes.FirstOrDefault(x => x.Value.Contains(entityName, StringComparer.OrdinalIgnoreCase)).Key;
        }

        /// <summary>
        /// Returns information about the requested route.
        /// </summary>
        /// <param name="httpContext">An object that encapsulates information about the HTTP request.</param>
        /// <returns>
        /// An object that contains the values from the route definition.
        /// </returns>
        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            RouteData data = base.GetRouteData(httpContext);

            if (data != null && DataSettings.DatabaseIsInstalled())
            {
                var slug = NormalizeSlug(data.Values);

                if (TryResolveUrlPrefix(slug, out var urlPrefix, out var actualSlug, out var entityNames))
                {
                    slug = actualSlug;
                }

                var urlRecordService = EngineContext.Current.Resolve<IUrlRecordService>();
                var urlRecord = urlRecordService.GetBySlug(slug);
                if (urlRecord == null)
                {
                    // no URL record found
                    return NotFound(data);
                }

                if (!urlRecord.IsActive)
                {
                    // URL record is not active. let's find the latest one
                    var activeSlug = urlRecordService.GetActiveSlug(urlRecord.EntityId, urlRecord.EntityName, urlRecord.LanguageId);
                    if (activeSlug.HasValue())
                    {
                        // The active one is found
                        var webHelper = EngineContext.Current.Resolve<IWebHelper>();
                        var response = httpContext.Response;
                        response.Status = "301 Moved Permanently";
                        if (urlPrefix.HasValue())
                        {
                            activeSlug = urlPrefix + "/" + activeSlug;
                        }
                        response.RedirectLocation = string.Format("{0}{1}", webHelper.GetStoreLocation(), activeSlug);
                        response.End();
                        return null;
                    }
                    else
                    {
                        // no active slug found
                        return NotFound(data);
                    }
                }

                // Verify prefix matches any assigned entity name
                if (entityNames != null && !entityNames.Contains(urlRecord.EntityName, StringComparer.OrdinalIgnoreCase))
                {
                    // does NOT match
                    return NotFound(data);
                }

                // process URL
                data.DataTokens["UrlRecord"] = urlRecord;
                data.Values["SeName"] = slug;

                //string controller, action, paramName;

                if (!_paths.TryGetValue(urlRecord.EntityName, out var path))
                {
                    throw new SmartException(string.Format("Unsupported EntityName for UrlRecord: {0}", urlRecord.EntityName));
                }

                var route = path.Route;

                data.Values["controller"] = route.Defaults["controller"];
                data.Values["action"] = route.Defaults["action"];
                data.Values[path.IdParamName ?? "id"] = urlRecord.EntityId;
            }

            return data;
        }

        private string NormalizeSlug(RouteValueDictionary routeValues)
        {
            var slug = routeValues[SlugKey] as string;
            var lastChar = slug[slug.Length - 1];
            if (lastChar == '/' || lastChar == '\\')
            {
                slug = slug.TrimEnd('/', '\\');
                routeValues[SlugKey] = slug;
            }

            return slug;
        }

        private RouteData NotFound(RouteData data)
        {
            data.Values["controller"] = "Error";
            data.Values["action"] = "NotFound";

            return data;
        }

        private bool TryResolveUrlPrefix(string slug, out string urlPrefix, out string actualSlug, out ICollection<string> entityNames)
        {
            urlPrefix = null;
            actualSlug = null;
            entityNames = null;

            if (_urlPrefixes.Count > 0)
            {
                var firstSepIndex = slug.IndexOf('/');
                if (firstSepIndex > 0)
                {
                    var prefix = slug.Substring(0, firstSepIndex);
                    if (_urlPrefixes.ContainsKey(prefix))
                    {
                        urlPrefix = prefix;
                        entityNames = _urlPrefixes[prefix];
                        actualSlug = slug.Substring(prefix.Length + 1);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}