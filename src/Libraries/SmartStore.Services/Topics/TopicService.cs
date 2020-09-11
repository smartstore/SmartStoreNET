using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Topics;
using SmartStore.Data.Caching;
using SmartStore.Services.Stores;
using SmartStore.Services.Seo;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Services.Topics
{
    public partial class TopicService : ITopicService, IXmlSitemapPublisher
    {
        private readonly ICommonServices _services;
        private readonly IRepository<Topic> _topicRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IRepository<AclRecord> _aclRepository;

        public TopicService(
            ICommonServices services,
            IRepository<Topic> topicRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IStoreMappingService storeMappingService,
            IRepository<AclRecord> aclRepository,
            SeoSettings seoSettings)
        {
            _services = services;
            _topicRepository = topicRepository;
            _storeMappingRepository = storeMappingRepository;
            _storeMappingService = storeMappingService;
            _aclRepository = aclRepository;

            this.QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public virtual void DeleteTopic(Topic topic)
        {
            Guard.NotNull(topic, nameof(topic));

            _topicRepository.Delete(topic);
        }

        public virtual Topic GetTopicById(int topicId)
        {
            if (topicId == 0)
                return null;

            return _topicRepository.GetByIdCached(topicId, "db.topic.id-" + topicId);
        }

        public virtual Topic GetTopicBySystemName(string systemName, int storeId = 0, bool checkPermission = true)
        {
            if (systemName.IsEmpty())
                return null;

            var query = BuildTopicsQuery(systemName, storeId, !checkPermission);
            var rolesIdent = checkPermission
                ? "0"
                : _services.WorkContext.CurrentCustomer.GetRolesIdent();

            var result = query.FirstOrDefaultCached("db.topic.bysysname-{0}-{1}-{2}".FormatInvariant(systemName, storeId, rolesIdent));

            return result;
        }

        public virtual IPagedList<Topic> GetAllTopics(int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = BuildTopicsQuery(null, storeId, showHidden);
            return new PagedList<Topic>(query, pageIndex, pageSize);
        }

        protected virtual IQueryable<Topic> BuildTopicsQuery(string systemName, int storeId, bool showHidden = false)
        {
            var entityName = nameof(Topic);
            var joinApplied = false;

            var query = _topicRepository.Table.Where(x => showHidden || x.IsPublished);

            if (systemName.HasValue())
            {
                query = query.Where(x => x.SystemName == systemName);
            }

            // Store mapping
            if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
            {
                query = from t in query
                        join m in _storeMappingRepository.Table
                        on new { c1 = t.Id, c2 = "Topic" } equals new { c1 = m.EntityId, c2 = m.EntityName } into tm
                        from m in tm.DefaultIfEmpty()
                        where !t.LimitedToStores || storeId == m.StoreId
                        select t;

                joinApplied = true;
            }

            // ACL (access control list)
            if (!showHidden && !QuerySettings.IgnoreAcl)
            {
                var allowedCustomerRolesIds = _services.WorkContext.CurrentCustomer.GetRoleIds();

                query = from c in query
                        join a in _aclRepository.Table
                        on new { c1 = c.Id, c2 = entityName } equals new { c1 = a.EntityId, c2 = a.EntityName } into ca
                        from a in ca.DefaultIfEmpty()
                        where !c.SubjectToAcl || allowedCustomerRolesIds.Contains(a.CustomerRoleId)
                        select c;

                joinApplied = true;
            }

            if (joinApplied)
            {
                // Only distinct topics (group by ID)
                query = from t in query
                        group t by t.Id into tGroup
                        orderby tGroup.Key
                        select tGroup.FirstOrDefault();
            }

            query = query.OrderBy(t => t.Priority).ThenBy(t => t.SystemName);

            return query;
        }

        public virtual void InsertTopic(Topic topic)
        {
            Guard.NotNull(topic, nameof(topic));

            _topicRepository.Insert(topic);
        }

        public virtual void UpdateTopic(Topic topic)
        {
            Guard.NotNull(topic, nameof(topic));

            _topicRepository.Update(topic);
        }

        #region XML Sitemap

        public XmlSitemapProvider PublishXmlSitemap(XmlSitemapBuildContext context)
        {
            if (!context.LoadSetting<SeoSettings>().XmlSitemapIncludesTopics)
                return null;

            var query = GetAllTopics(context.RequestStoreId).AlterQuery(q =>
            {
                return q.Where(t => t.IncludeInSitemap && !t.RenderAsWidget);
            }).SourceQuery;

            return new TopicXmlSitemapResult { Query = query };
        }

        class TopicXmlSitemapResult : XmlSitemapProvider
        {
            public IQueryable<Topic> Query { get; set; }

            public override int GetTotalCount()
            {
                return Query.Count();
            }

            public override IEnumerable<NamedEntity> Enlist()
            {
                var topics = Query.Select(x => new { x.Id }).ToList();
                foreach (var x in topics)
                {
                    yield return new NamedEntity { EntityName = "Topic", Id = x.Id, LastMod = DateTime.UtcNow };
                }
            }

            public override int Order => 200;
        }

        #endregion
    }
}
