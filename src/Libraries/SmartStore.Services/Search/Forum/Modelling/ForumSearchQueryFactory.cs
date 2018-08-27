using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Search.Extensions;

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
        d   -   Date
	*/

    public class ForumSearchQueryFactory : SearchQueryFactoryBase, IForumSearchQueryFactory
    {
        protected readonly ICommonServices _services;
        protected readonly IForumSearchQueryAliasMapper _forumSearchQueryAliasMapper;
        protected readonly ForumSearchSettings _searchSettings;
        protected readonly ForumSettings _forumSettings;

        public ForumSearchQueryFactory(
            ICommonServices services,
            HttpContextBase httpContext,
            IForumSearchQueryAliasMapper forumSearchQueryAliasMapper,
            ForumSearchSettings searchSettings,
            ForumSettings forumSettings)
            : base(httpContext)
        {
            _services = services;
            _forumSearchQueryAliasMapper = forumSearchQueryAliasMapper;
            _searchSettings = searchSettings;
            _forumSettings = forumSettings;

            QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public ForumSearchQuery Current { get; private set; }

        protected override string[] Tokens => new string[] { "q", "i", "s", "o", "f", "c", "d" };

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

            // Store.
            if (!QuerySettings.IgnoreMultiStore)
            {
                query.HasStoreId(_services.StoreContext.CurrentStore.Id);
            }

            // Instant-Search never uses these filter parameters.
            if (!isInstantSearch)
            {
                ConvertPagingSorting(query, routeData, origin);
                ConvertForum(query, routeData, origin);
                ConvertCustomer(query, routeData, origin);
                ConvertDate(query, routeData, origin);
            }

            OnConverted(query, routeData, origin);

            Current = query;
            return query;
        }

        protected virtual void ConvertPagingSorting(ForumSearchQuery query, RouteData routeData, string origin)
        {
            var index = Math.Max(1, GetValueFor<int?>("i") ?? 1);
            var size = _forumSettings.SearchResultsPageSize;
            var orderBy = GetValueFor<ForumTopicSorting?>("o");

            query.Slice((index - 1) * size, size);
            query.SortBy(orderBy.Value);
            query.CustomData["CurrentSortOrder"] = orderBy.Value;
        }

        private void AddFacet(
            ForumSearchQuery query,
            FacetGroupKind kind,
            bool isMultiSelect,
            FacetSorting sorting,
            Action<FacetDescriptor> addValues)
        {
            string fieldName;
            var displayOrder = 0;

            switch (kind)
            {
                case FacetGroupKind.Forum:
                    fieldName = "forumid";
                    displayOrder = _searchSettings.ForumDisplayOrder;
                    break;
                case FacetGroupKind.Customer:
                    fieldName = "customerid";
                    displayOrder = _searchSettings.CustomerDisplayOrder;
                    break;
                case FacetGroupKind.Date:
                    fieldName = "createdon";
                    displayOrder = _searchSettings.DateDisplayOrder;
                    break;
                default:
                    throw new SmartException($"Unknown field name for facet group '{kind.ToString()}'");
            }

            var descriptor = new FacetDescriptor(fieldName);
            descriptor.Label = _services.Localization.GetResource(FacetUtility.GetLabelResourceKey(kind) ?? kind.ToString());
            descriptor.IsMultiSelect = isMultiSelect;
            descriptor.DisplayOrder = displayOrder;
            descriptor.OrderBy = sorting;
            descriptor.MinHitCount = _searchSettings.FilterMinHitCount;
            descriptor.MaxChoicesCount = _searchSettings.FilterMaxChoicesCount;

            addValues(descriptor);
            query.WithFacet(descriptor);
        }

        protected virtual void ConvertForum(ForumSearchQuery query, RouteData routeData, string origin)
        {
            if (origin == "Forum/Forum")
            {
                // We don't need forum facetting in forum pages.
                return;
            }

            List<int> ids;

            if (GetValueFor(query, "f", FacetGroupKind.Forum, out ids) && ids != null && ids.Any())
            {
                query.WithForumIds(ids.ToArray());
            }

            AddFacet(query, FacetGroupKind.Forum, true, FacetSorting.HitsDesc, descriptor =>
            {
                if (ids != null)
                {
                    foreach (var id in ids)
                    {
                        descriptor.AddValue(new FacetValue(id, IndexTypeCode.Int32)
                        {
                            IsSelected = true
                        });
                    }
                }
            });
        }

        protected virtual void ConvertCustomer(ForumSearchQuery query, RouteData routeData, string origin)
        {
            List<int> ids;

            if (GetValueFor(query, "c", FacetGroupKind.Customer, out ids) && ids != null && ids.Any())
            {
                query.WithCustomerIds(ids.ToArray());
            }

            AddFacet(query, FacetGroupKind.Customer, true, FacetSorting.HitsDesc, descriptor =>
            {
                if (ids != null)
                {
                    foreach (var id in ids)
                    {
                        descriptor.AddValue(new FacetValue(id, IndexTypeCode.Int32)
                        {
                            IsSelected = true
                        });
                    }
                }
            });
        }

        protected virtual void ConvertDate(ForumSearchQuery query, RouteData routeData, string origin)
        {
            string date;
            DateTime? fromUtc = null;
            DateTime? toUtc = null;

            if (GetValueFor(query, "d", FacetGroupKind.Date, out date) && TryParseRange(date, out fromUtc, out toUtc))
            {
                if (fromUtc.HasValue && toUtc.HasValue && fromUtc > toUtc)
                {
                    var tmp = fromUtc;
                    fromUtc = toUtc;
                    toUtc = tmp;
                }

                if (fromUtc.HasValue || toUtc.HasValue)
                {
                    query.CreatedBetween(fromUtc, toUtc);
                }
            }

            AddFacet(query, FacetGroupKind.Date, false, FacetSorting.DisplayOrder, descriptor =>
            {
                if (fromUtc.HasValue || toUtc.HasValue)
                {
                    descriptor.AddValue(new FacetValue(
                        fromUtc,
                        toUtc,
                        IndexTypeCode.DateTime,
                        fromUtc.HasValue,
                        toUtc.HasValue)
                    {
                        IsSelected = true
                    });
                }
            });
        }

        protected virtual void OnConverted(ForumSearchQuery query, RouteData routeData, string origin)
        {
        }

        protected virtual bool GetValueFor<T>(ForumSearchQuery query, string key, FacetGroupKind kind, out T value)
        {
            return GetValueFor(_forumSearchQueryAliasMapper.GetCommonFacetAliasByGroupKind(kind, query.LanguageId ?? 0) ?? key, out value);
        }
    }
}
