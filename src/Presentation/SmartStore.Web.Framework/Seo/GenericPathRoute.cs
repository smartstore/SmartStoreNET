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
		// Key = Prefix, Value = EntityType
		private readonly Multimap<string, string> _urlPrefixes = 
			new Multimap<string, string>(StringComparer.OrdinalIgnoreCase, x => new HashSet<string>(x, StringComparer.OrdinalIgnoreCase));
		
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

		public void RegisterUrlPrefix(string prefix, params string[] entityNames)
		{
			Guard.NotEmpty(prefix, nameof(prefix));

			_urlPrefixes.AddRange(prefix, entityNames);
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
                var slug = data.Values["generic_se_name"] as string;

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
                        response.RedirectLocation = string.Format("{0}{1}", webHelper.GetStoreLocation(false), activeSlug);
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
				if (entityNames != null && !entityNames.Contains(urlRecord.EntityName))
				{
					// does NOT match
					return NotFound(data);
				}

				// process URL
				data.DataTokens["UrlRecord"] = urlRecord;
				data.Values["SeName"] = slug;

				string controller, action, paramName;

                switch (urlRecord.EntityName.ToLowerInvariant())
                {
                    case "product":
                        {
							controller = "Product";
							action = "ProductDetails";
							paramName = "productid";
                        }
                        break;
                    case "category":
                        {
							controller = "Catalog";
							action = "Category";
							paramName = "categoryid";
                        }
                        break;
                    case "manufacturer":
                        {
							controller = "Catalog";
							action = "Manufacturer";
							paramName = "manufacturerid";
                        }
                        break;
					case "topic":
						{
							controller = "Topic";
							action = "TopicDetails";
							paramName = "topicId";
						}
						break;
					case "newsitem":
                        {
							controller = "News";
							action = "NewsItem";
							paramName = "newsItemId";
                        }
                        break;
                    case "blogpost":
                        {
							controller = "Blog";
							action = "BlogPost";
							paramName = "blogPostId";
                        }
                        break;
                    default:
                        {
                            throw new SmartException(string.Format("Unsupported EntityName for UrlRecord: {0}", urlRecord.EntityName));
                        }
                }

				data.Values["controller"] = controller;
				data.Values["action"] = action;
				data.Values[paramName] = urlRecord.EntityId;
			}

            return data;
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