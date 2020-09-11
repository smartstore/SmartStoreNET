using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Localization;
using SmartStore.Core.Search;
using SmartStore.Core.Search.Facets;
using SmartStore.Services.Customers;
using SmartStore.Services.Forums;
using SmartStore.Services.Localization;
using SmartStore.Services.Search.Extensions;

namespace SmartStore.Services.Search
{
    public partial class LinqForumSearchService : SearchServiceBase, IForumSearchService
    {
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly IRepository<ForumTopic> _forumTopicRepository;
        private readonly IRepository<Forum> _forumRepository;
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly IForumService _forumService;
        private readonly ICommonServices _services;
        private readonly CustomerSettings _customerSettings;

        public LinqForumSearchService(
            IRepository<ForumPost> forumPostRepository,
            IRepository<ForumTopic> forumTopicRepository,
            IRepository<Forum> forumRepository,
            IRepository<ForumGroup> forumGroupRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IRepository<AclRecord> aclRepository,
            IForumService forumService,
            ICommonServices services,
            CustomerSettings customerSettings)
        {
            _forumPostRepository = forumPostRepository;
            _forumTopicRepository = forumTopicRepository;
            _forumRepository = forumRepository;
            _forumGroupRepository = forumGroupRepository;
            _storeMappingRepository = storeMappingRepository;
            _aclRepository = aclRepository;
            _forumService = forumService;
            _services = services;
            _customerSettings = customerSettings;

            T = NullLocalizer.Instance;
            QuerySettings = DbQuerySettings.Default;
        }

        public Localizer T { get; set; }
        public DbQuerySettings QuerySettings { get; set; }

        protected virtual IQueryable<ForumPost> GetPostQuery(ForumSearchQuery searchQuery, IQueryable<ForumPost> baseQuery)
        {
            // Post query.
            var ordered = false;
            var t = searchQuery.Term;
            var cnf = _customerSettings.CustomerNameFormat;
            var fields = searchQuery.Fields;
            var filters = new List<ISearchFilter>();
            var customer = _services.WorkContext.CurrentCustomer;
            var query = baseQuery ?? _forumPostRepository.TableUntracked.Expand(x => x.ForumTopic);

            // Apply search term.
            if (t.HasValue() && fields != null && fields.Length != 0 && fields.Any(x => x.HasValue()))
            {
                if (searchQuery.Mode == SearchMode.StartsWith)
                {
                    query = query.Where(x =>
                        (fields.Contains("subject") && x.ForumTopic.Subject.StartsWith(t)) ||
                        (fields.Contains("text") && x.Text.StartsWith(t)) ||
                        (fields.Contains("username") && (
                            cnf == CustomerNameFormat.ShowEmails ? x.Customer.Email.StartsWith(t) :
                            cnf == CustomerNameFormat.ShowUsernames ? x.Customer.Username.StartsWith(t) :
                            cnf == CustomerNameFormat.ShowFirstName ? x.Customer.FirstName.StartsWith(t) :
                            x.Customer.FullName.StartsWith(t))
                        ));
                }
                else
                {
                    query = query.Where(x =>
                        (fields.Contains("subject") && x.ForumTopic.Subject.Contains(t)) ||
                        (fields.Contains("text") && x.Text.Contains(t)) ||
                        (fields.Contains("username") && (
                            cnf == CustomerNameFormat.ShowEmails ? x.Customer.Email.Contains(t) :
                            cnf == CustomerNameFormat.ShowUsernames ? x.Customer.Username.Contains(t) :
                            cnf == CustomerNameFormat.ShowFirstName ? x.Customer.FirstName.Contains(t) :
                            x.Customer.FullName.Contains(t))
                        ));
                }
            }

            // Flatten filters.
            foreach (var filter in searchQuery.Filters)
            {
                var combinedFilter = filter as ICombinedSearchFilter;
                if (combinedFilter != null)
                {
                    // Find VisibleOnly combined filter and process it separately.
                    var cf = combinedFilter.Filters.OfType<IAttributeSearchFilter>().ToArray();
                    if (cf.Length == 2 && cf[0].FieldName == "published" && true == (bool)cf[0].Term && cf[1].FieldName == "customerid")
                    {
                        if (!customer.IsForumModerator())
                        {
                            query = query.Where(x => x.ForumTopic.Published && (x.Published || x.CustomerId == customer.Id));
                        }
                    }
                    else
                    {
                        FlattenFilters(combinedFilter.Filters, filters);
                    }
                }
                else
                {
                    filters.Add(filter);
                }
            }

            if (!QuerySettings.IgnoreAcl)
            {
                var roleIds = GetIdList(filters, "roleid");
                if (roleIds.Any())
                {
                    query =
                        from fp in query
                        join ft in _forumTopicRepository.TableUntracked on fp.TopicId equals ft.Id
                        join ff in _forumRepository.Table on ft.ForumId equals ff.Id
                        join fg in _forumGroupRepository.Table on ff.ForumGroupId equals fg.Id
                        join a in _aclRepository.Table on new { a1 = fg.Id, a2 = "ForumGroup" } equals new { a1 = a.EntityId, a2 = a.EntityName } into fg_acl
                        from a in fg_acl.DefaultIfEmpty()
                        where !fg.SubjectToAcl || roleIds.Contains(a.CustomerRoleId)
                        select fp;
                }
            }

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
                else if (filter.FieldName == "forumid")
                {
                    query = query.Where(x => x.ForumTopic.ForumId == (int)filter.Term);
                }
                else if (filter.FieldName == "customerid")
                {
                    query = query.Where(x => x.CustomerId == (int)filter.Term);
                }
                else if (filter.FieldName == "published")
                {
                    query = query.Where(x => x.Published == (bool)filter.Term);
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
                else if (filter.FieldName == "storeid")
                {
                    if (!QuerySettings.IgnoreMultiStore)
                    {
                        var storeId = (int)filter.Term;
                        if (storeId != 0)
                        {
                            query =
                                from p in query
                                join sm in _storeMappingRepository.TableUntracked on new { eid = p.ForumTopic.Forum.ForumGroupId, ename = "ForumGroup" }
                                equals new { eid = sm.EntityId, ename = sm.EntityName } into gsm
                                from sm in gsm.DefaultIfEmpty()
                                where !p.ForumTopic.Forum.ForumGroup.LimitedToStores || sm.StoreId == storeId
                                select p;
                        }
                    }
                }
            }

            query =
                from p in query
                group p by p.Id into grp
                orderby grp.Key
                select grp.FirstOrDefault();

            // Sorting.
            foreach (var sort in searchQuery.Sorting)
            {
                if (sort.FieldName == "subject")
                {
                    query = OrderBy(ref ordered, query, x => x.ForumTopic.Subject, sort.Descending);
                }
                else if (sort.FieldName == "username")
                {
                    switch (cnf)
                    {
                        case CustomerNameFormat.ShowEmails:
                            query = OrderBy(ref ordered, query, x => x.Customer.Email, sort.Descending);
                            break;
                        case CustomerNameFormat.ShowUsernames:
                            query = OrderBy(ref ordered, query, x => x.Customer.Username, sort.Descending);
                            break;
                        case CustomerNameFormat.ShowFirstName:
                            query = OrderBy(ref ordered, query, x => x.Customer.FirstName, sort.Descending);
                            break;
                        default:
                            query = OrderBy(ref ordered, query, x => x.Customer.FullName, sort.Descending);
                            break;
                    }
                }
                else if (sort.FieldName == "createdon")
                {
                    // We want to sort by ForumPost.CreatedOnUtc, not ForumTopic.CreatedOnUtc.
                    query = OrderBy(ref ordered, query, x => x.ForumTopic.LastPostTime, sort.Descending);
                }
                else if (sort.FieldName == "numposts")
                {
                    query = OrderBy(ref ordered, query, x => x.ForumTopic.NumPosts, sort.Descending);
                }
            }

            if (!ordered)
            {
                query = query
                    .OrderByDescending(x => x.ForumTopic.TopicTypeId)
                    .ThenByDescending(x => x.ForumTopic.LastPostTime)
                    .ThenByDescending(x => x.TopicId);
            }

            return query;
        }

        protected virtual IDictionary<string, FacetGroup> GetFacets(ForumSearchQuery searchQuery, int totalHits)
        {
            var result = new Dictionary<string, FacetGroup>();
            var storeId = searchQuery.StoreId ?? _services.StoreContext.CurrentStore.Id;
            var languageId = searchQuery.LanguageId ?? _services.WorkContext.WorkingLanguage.Id;

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
                    var take = maxChoices * 3;
                    var customers = customerQuery.Take(() => take).ToList();

                    foreach (var customer in customers)
                    {
                        var name = customer.FormatUserName(_customerSettings, T, true);
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
                    foreach (var value in descriptor.Values)
                    {
                        facets.Add(new Facet(value));
                    }
                }

                if (facets.Any(x => x.Published))
                {
                    //facets.Each(x => $"{key} {x.Value.ToString()}".Dump());

                    var group = new FacetGroup(
                        "Forum",
                        key,
                        descriptor.Label,
                        descriptor.IsMultiSelect,
                        false,
                        descriptor.DisplayOrder,
                        facets.OrderBy(descriptor))
                    {
                        IsScrollable = facets.Count > 14
                    };

                    result.Add(key, group);
                }
            }

            return result;
        }

        public ForumSearchResult Search(ForumSearchQuery searchQuery, bool direct = false)
        {
            _services.EventPublisher.Publish(new ForumSearchingEvent(searchQuery));

            var totalHits = 0;
            int[] hitsEntityIds = null;
            Func<IList<ForumPost>> hitsFactory = null;
            IDictionary<string, FacetGroup> facets = null;

            if (searchQuery.Take > 0)
            {
                var query = GetPostQuery(searchQuery, null);
                totalHits = query.Count();

                // Fix paging boundaries.
                if (searchQuery.Skip > 0 && searchQuery.Skip >= totalHits)
                {
                    searchQuery.Slice((totalHits / searchQuery.Take) * searchQuery.Take, searchQuery.Take);
                }

                if (searchQuery.ResultFlags.HasFlag(SearchResultFlags.WithHits))
                {
                    var skip = searchQuery.PageIndex * searchQuery.Take;
                    query = query
                        .Skip(() => skip)
                        .Take(() => searchQuery.Take);

                    hitsEntityIds = query.Select(x => x.Id).ToArray();
                    hitsFactory = () => _forumService.GetPostsByIds(hitsEntityIds);
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
                hitsEntityIds,
                hitsFactory,
                null,
                facets);

            _services.EventPublisher.Publish(new ForumSearchedEvent(searchQuery, result));

            return result;
        }

        public IQueryable<ForumPost> PrepareQuery(ForumSearchQuery searchQuery, IQueryable<ForumPost> baseQuery = null)
        {
            return GetPostQuery(searchQuery, baseQuery);
        }
    }
}
