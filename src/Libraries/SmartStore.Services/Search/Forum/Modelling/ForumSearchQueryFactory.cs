using System.Collections.Generic;
using System.Web;
using System.Web.Routing;
using SmartStore.Core.Search;

namespace SmartStore.Services.Search.Modelling
{
    /*
		TOKENS:
		===============================
		q	-	Search term
		i	-	Page index
		s	-	Page size
		o	-	Order by
        f   -   Forum
        c   -   Customer
        p   -   Last post
	*/

    public class ForumSearchQueryFactory : SearchQueryFactoryBase, IForumSearchQueryFactory
    {
        protected readonly ICommonServices _services;
        protected readonly ForumSearchSettings _searchSettings;

        public ForumSearchQueryFactory(
            ICommonServices services,
            HttpContextBase httpContext,
            ForumSearchSettings searchSettings)
            : base(httpContext)
        {
            _services = services;
            _searchSettings = searchSettings;
        }

        public ForumSearchQuery Current { get; private set; }

        public ForumSearchQuery CreateFromQuery()
        {
            var ctx = _httpContext;

            if (ctx.Request == null)
                return null;

            var routeData = ctx.Request.RequestContext.RouteData;
            var area = routeData.GetAreaName();
            var controller = routeData.GetRequiredString("controller");
            var action = routeData.GetRequiredString("action");
            var origin = "{0}{1}/{2}".FormatInvariant(area == null ? "" : area + "/", controller, action);
            var fields = new List<string> { "text" };
            var term = GetValueFor<string>("q");
            var isInstantSearch = origin.IsCaseInsensitiveEqual("Search/InstantSearch");

            fields.AddRange(_searchSettings.SearchFields);

            var query = new ForumSearchQuery(fields.ToArray(), term, _searchSettings.SearchMode)
                .OriginatesFrom(origin)
                .WithLanguage(_services.WorkContext.WorkingLanguage)
                .WithCurrency(_services.WorkContext.WorkingCurrency);

            // Instant-Search never uses these filter parameters.
            if (!isInstantSearch)
            {
            }

            OnConverted(query, routeData, origin);

            Current = query;
            return query;
        }

        protected virtual void OnConverted(ForumSearchQuery query, RouteData routeData, string origin)
        {
        }
    }
}
