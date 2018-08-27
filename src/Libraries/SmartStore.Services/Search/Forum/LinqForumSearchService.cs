using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Forums;
using SmartStore.Services.Localization;
using SmartStore.Services.Search.Extensions;

namespace SmartStore.Services.Search
{
    public partial class LinqForumSearchService : SearchServiceBase, IForumSearchService
    {
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IForumService _forumService;
        private readonly ICommonServices _services;

		public LinqForumSearchService(
            IRepository<ForumPost> forumPostRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IForumService forumService,
            ICommonServices services)
		{
            _forumPostRepository = forumPostRepository;
            _storeMappingRepository = storeMappingRepository;
            _forumService = forumService;
			_services = services;

            QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        protected virtual IQueryable<ForumTopic> GetForumTopicQuery(ForumSearchQuery searchQuery, IQueryable<ForumPost> baseQuery)
		{
            var ordered = false;
            var term = searchQuery.Term;
            var fields = searchQuery.Fields;
            var filters = new List<ISearchFilter>();
            var query = baseQuery ?? _forumPostRepository.Table.Expand(x => x.ForumTopic);

            //var searchType = ForumSearchType.All;   //TODO, obsolete?

            // Apply search term.
            if (term.HasValue() && fields != null && fields.Length != 0 && fields.Any(x => x.HasValue()))
            {
                if (searchQuery.Mode == SearchMode.StartsWith)
                {
                    query = query.Where(x =>
                        (fields.Contains("subject") && x.ForumTopic.Subject.StartsWith(term)) ||
                        (fields.Contains("text") && x.Text.StartsWith(term)));
                }
                else
                {
                    query = query.Where(x =>
                        (fields.Contains("subject") && x.ForumTopic.Subject.Contains(term)) ||
                        (fields.Contains("text") && x.Text.Contains(term)));
                }
            }

            // Filters.
            FlattenFilters(searchQuery.Filters, filters);

            foreach (IAttributeSearchFilter filter in filters)
            {
                var rangeFilter = filter as IRangeSearchFilter;

                if (filter.FieldName == "id")
                {
                    if (rangeFilter != null)
                    {
                        var lower = filter.Term as int?;
                        var upper = rangeFilter.UpperTerm as int?;

                        if (lower.HasValue)
                        {
                            if (rangeFilter.IncludesLower)
                                query = query.Where(x => x.Id >= lower.Value);
                            else
                                query = query.Where(x => x.Id > lower.Value);
                        }

                        if (upper.HasValue)
                        {
                            if (rangeFilter.IncludesUpper)
                                query = query.Where(x => x.Id <= upper.Value);
                            else
                                query = query.Where(x => x.Id < upper.Value);
                        }
                    }
                }
                else if (filter.FieldName == "forumId")
                {
                    query = query.Where(x => x.ForumTopic.ForumId == (int)filter.Term);
                }
                else if (filter.FieldName == "customerId")
                {
                    query = query.Where(x => x.ForumTopic.CustomerId == (int)filter.Term);
                }
                else if (filter.FieldName == "createdon")
                {
                    if (rangeFilter != null)
                    {
                        var lower = filter.Term as DateTime?;
                        var upper = rangeFilter.UpperTerm as DateTime?;

                        if (lower.HasValue)
                        {
                            if (rangeFilter.IncludesLower)
                                query = query.Where(x => x.CreatedOnUtc >= lower.Value);
                            else
                                query = query.Where(x => x.CreatedOnUtc > lower.Value);
                        }

                        if (upper.HasValue)
                        {
                            if (rangeFilter.IncludesLower)
                                query = query.Where(x => x.CreatedOnUtc <= upper.Value);
                            else
                                query = query.Where(x => x.CreatedOnUtc < upper.Value);
                        }
                    }
                }
            }

            var topicsQuery =
                from ft in query.Select(x => x.ForumTopic)
                group ft by ft.Id into grp
                orderby grp.Key
                select grp.FirstOrDefault();

            // Sorting of topics.
            foreach (var sort in searchQuery.Sorting)
            {
                if (sort.FieldName == "subject")
                {
                    topicsQuery = OrderBy(ref ordered, topicsQuery, x => x.Subject, sort.Descending);
                }
                else if (sort.FieldName == "createdon")
                {
                    topicsQuery = OrderBy(ref ordered, topicsQuery, x => x.CreatedOnUtc, sort.Descending);
                }
                else if (sort.FieldName == "posts")
                {
                    topicsQuery = OrderBy(ref ordered, topicsQuery, x => x.NumPosts, sort.Descending);
                }
                else if (sort.FieldName == "views")
                {
                    topicsQuery = OrderBy(ref ordered, topicsQuery, x => x.Views, sort.Descending);
                }
            }

            if (!ordered)
            {
                topicsQuery = topicsQuery
                    .OrderByDescending(x => x.TopicTypeId)
                    .ThenByDescending(x => x.LastPostTime)
                    .ThenByDescending(x => x.Id);
            }

            return topicsQuery;
        }

        protected virtual IDictionary<string, FacetGroup> GetFacets(ForumSearchQuery searchQuery, int totalHits)
        {
            var result = new Dictionary<string, FacetGroup>();
            var storeId = searchQuery.StoreId ?? _services.StoreContext.CurrentStore.Id;
            var languageId = searchQuery.LanguageId ?? _services.WorkContext.WorkingLanguage.Id;
            var userNamesEnabled = _services.Settings.LoadSetting<CustomerSettings>().UsernamesEnabled;

            foreach (var key in searchQuery.FacetDescriptors.Keys)
            {
                var descriptor = searchQuery.FacetDescriptors[key];
                var facets = new List<Facet>();
                var kind = FacetGroup.GetKindByKey("Forum", key);

                if (kind == FacetGroupKind.Forum)
                {
                    var enoughFacets = false;
                    var groups = _forumService.GetAllForumGroups(storeId);

                    foreach (var group in groups)
                    {
                        foreach (var forum in group.Forums)
                        {
                            facets.Add(new Facet(new FacetValue(forum.Id, IndexTypeCode.Int32)
                            {
                                IsSelected = descriptor.Values.Any(x => x.IsSelected && x.Value.Equals(forum.Id)),
                                Label = forum.GetLocalized(x => x.Name, languageId),
                                DisplayOrder = forum.DisplayOrder
                            }));

                            if (descriptor.MaxChoicesCount > 0 && facets.Count >= descriptor.MaxChoicesCount)
                            {
                                enoughFacets = true;
                                break;
                            }
                        }

                        if (enoughFacets)
                        {
                            break;
                        }
                    }
                }
                else if (kind == FacetGroupKind.Customer)
                {
                    // Get customers with most posts.
                    var customerQuery = FacetUtility.GetCustomersByNumberOfPosts(
                        _forumPostRepository,
                        _storeMappingRepository,
                        QuerySettings.IgnoreMultiStore ? 0 : storeId,
                        descriptor.MinHitCount);

                    // Limit the result. Do not allow to get all customers.
                    var maxChoices = descriptor.MaxChoicesCount > 0 ? descriptor.MaxChoicesCount : 20;
                    var customers = customerQuery.Take(maxChoices * 3).ToList();

                    foreach (var customer in customers)
                    {
                        var name = FacetUtility.GetPublicName(customer, userNamesEnabled);
                        if (name.HasValue())
                        {
                            facets.Add(new Facet(new FacetValue(customer.Id, IndexTypeCode.Int32)
                            {
                                IsSelected = descriptor.Values.Any(x => x.IsSelected && x.Value.Equals(customer.Id)),
                                Label = name,
                                DisplayOrder = 0
                            }));
                            if (facets.Count >= maxChoices)
                            {
                                break;
                            }
                        }
                    }
                }
                else if (kind == FacetGroupKind.Date)
                {
                    var hasActivePredefinedFacet = false;
                    var dates = FacetUtility.GetForumDates();

                    foreach (var date in dates)
                    {
                        var newFacet = new Facet(date);
                        newFacet.Value.IsSelected = descriptor.Values.Any(x => x.IsSelected && x.Value.Equals(date.Value));

                        if (newFacet.Value.IsSelected)
                        {
                            hasActivePredefinedFacet = true;
                        }

                        facets.Add(newFacet);
                    }

                    // Add facet for custom date range.
                    var dateDescriptorValue = descriptor.Values.FirstOrDefault();
                    var customDateValue = new FacetValue(
                        dateDescriptorValue != null && !hasActivePredefinedFacet ? dateDescriptorValue.Value : null,
                        dateDescriptorValue != null && !hasActivePredefinedFacet ? dateDescriptorValue.UpperValue : null,
                        IndexTypeCode.DateTime,
                        true,
                        true);

                    customDateValue.IsSelected = customDateValue.Value != null || customDateValue.UpperValue != null;

                    if (!(totalHits == 0 && !customDateValue.IsSelected))
                    {
                        facets.Insert(0, new Facet("custom", customDateValue));
                    }
                }

                if (facets.Any(x => x.Published))
                {
                    //facets.Each(x => $"{key} {x.Value.ToString()}".Dump());

                    result.Add(key, new FacetGroup(
                        "Forum",
                        key,
                        descriptor.Label,
                        descriptor.IsMultiSelect,
                        false,
                        descriptor.DisplayOrder,
                        facets.OrderBy(descriptor)));
                }
            }

            return result;
        }

        public ForumSearchResult Search(ForumSearchQuery searchQuery, bool direct = false)
		{
            _services.EventPublisher.Publish(new ForumSearchingEvent(searchQuery));

			var totalHits = 0;
			Func<IList<ForumTopic>> hitsFactory = null;
            IDictionary<string, FacetGroup> facets = null;

            if (searchQuery.Take > 0)
			{
				var query = GetForumTopicQuery(searchQuery, null);
				totalHits = query.Count();

				// Fix paging boundaries
				if (searchQuery.Skip > 0 && searchQuery.Skip >= totalHits)
				{
					searchQuery.Slice((totalHits / searchQuery.Take) * searchQuery.Take, searchQuery.Take);
				}

				if (searchQuery.ResultFlags.HasFlag(SearchResultFlags.WithHits))
				{
					query = query
						.Skip(searchQuery.PageIndex * searchQuery.Take)
						.Take(searchQuery.Take);

					var ids = query.Select(x => x.Id).Distinct().ToArray();
                    hitsFactory = () => _forumService.GetTopicsByIds(ids);
				}

                if (searchQuery.ResultFlags.HasFlag(SearchResultFlags.WithFacets) && searchQuery.FacetDescriptors.Any())
                {
                    facets = GetFacets(searchQuery, totalHits);
                }
            }

			var result = new ForumSearchResult(
				null,
				searchQuery,
				totalHits,
				hitsFactory,
				null,
                facets);

            _services.EventPublisher.Publish(new ForumSearchedEvent(searchQuery, result));

			return result;
		}

		public IQueryable<ForumTopic> PrepareQuery(ForumSearchQuery searchQuery, IQueryable<ForumPost> baseQuery = null)
		{
			return GetForumTopicQuery(searchQuery, baseQuery);
		}
	}
}
