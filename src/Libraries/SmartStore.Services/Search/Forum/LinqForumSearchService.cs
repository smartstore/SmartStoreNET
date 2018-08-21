using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
using SmartStore.Services.Forums;

namespace SmartStore.Services.Search
{
    public partial class LinqForumSearchService : LinqSearchServiceBase, IForumSearchService
    {
        private readonly IRepository<ForumTopic> _forumTopicRepository;
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly IForumService _forumService;
        private readonly ICommonServices _services;

		public LinqForumSearchService(
            IRepository<ForumTopic> forumTopicRepository,
            IRepository<ForumPost> forumPostRepository,
            IForumService forumService,
            ICommonServices services)
		{
            _forumTopicRepository = forumTopicRepository;
            _forumPostRepository = forumPostRepository;
            _forumService = forumService;
			_services = services;

			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }

		#region Utilities

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
                else if (filter.FieldName == "lastposton")
                {
                    if (rangeFilter != null)
                    {
                        var lower = filter.Term as DateTime?;
                        var upper = rangeFilter.UpperTerm as DateTime?;

                        if (lower.HasValue)
                        {
                            if (rangeFilter.IncludesLower)
                                query = query.Where(x => x.ForumTopic.LastPostTime >= lower.Value);
                            else
                                query = query.Where(x => x.ForumTopic.LastPostTime > lower.Value);
                        }

                        if (upper.HasValue)
                        {
                            if (rangeFilter.IncludesLower)
                                query = query.Where(x => x.ForumTopic.LastPostTime <= upper.Value);
                            else
                                query = query.Where(x => x.ForumTopic.LastPostTime < upper.Value);
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
                if (sort.FieldName == "createdon")
                {
                    query = OrderBy(ref ordered, query, x => x.CreatedOnUtc, sort.Descending);
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

		#endregion

		public ForumSearchResult Search(ForumSearchQuery searchQuery, bool direct = false)
		{
            _services.EventPublisher.Publish(new ForumSearchingEvent(searchQuery));

			var totalHits = 0;
			Func<IList<ForumTopic>> hitsFactory = null;

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

					var ids = query.Select(x => x.Id).ToArray();
                    hitsFactory = () => _forumService.GetTopicsByIds(ids);
				}
			}

			var result = new ForumSearchResult(
				null,
				searchQuery,
				totalHits,
				hitsFactory,
				null);

            _services.EventPublisher.Publish(new ForumSearchedEvent(searchQuery, result));

			return result;
		}

		public IQueryable<ForumTopic> PrepareQuery(ForumSearchQuery searchQuery, IQueryable<ForumPost> baseQuery = null)
		{
			return GetForumTopicQuery(searchQuery, baseQuery);
		}
	}
}
