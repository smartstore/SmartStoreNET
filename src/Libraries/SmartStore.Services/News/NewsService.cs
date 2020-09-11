using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Seo;

namespace SmartStore.Services.News
{
    public partial class NewsService : INewsService, IXmlSitemapPublisher
    {
        private readonly IRepository<NewsItem> _newsItemRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;

        public NewsService(IRepository<NewsItem> newsItemRepository,
            IRepository<StoreMapping> storeMappingRepository)
        {
            _newsItemRepository = newsItemRepository;
            _storeMappingRepository = storeMappingRepository;

            this.QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public virtual void DeleteNews(NewsItem newsItem)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            _newsItemRepository.Delete(newsItem);
        }

        public virtual NewsItem GetNewsById(int newsId)
        {
            if (newsId == 0)
                return null;

            return _newsItemRepository.GetById(newsId);
        }

        public virtual IQueryable<NewsItem> GetNewsByIds(int[] newsIds)
        {
            if (newsIds == null || newsIds.Length == 0)
                return null;

            var query =
                from x in _newsItemRepository.Table
                where newsIds.Contains(x.Id)
                select x;

            return query;
        }

        public virtual IPagedList<NewsItem> GetAllNews(int languageId, int storeId, int pageIndex, int pageSize, bool showHidden = false, DateTime? maxAge = null,
            string title = "", string intro = "", string full = "")
        {
            var query = _newsItemRepository.Table;

            if (languageId > 0)
            {
                query = query.Where(n => languageId == n.LanguageId);
            }

            if (maxAge.HasValue)
            {
                query = query.Where(n => n.CreatedOnUtc >= maxAge.Value);
            }

            if (title.HasValue())
                query = query.Where(b => b.Title.Contains(title));
            if (intro.HasValue())
                query = query.Where(b => b.Short.Contains(intro));
            if (full.HasValue())
                query = query.Where(b => b.Full.Contains(full));

            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(n => n.Published);
                query = query.Where(n => !n.StartDateUtc.HasValue || n.StartDateUtc <= utcNow);
                query = query.Where(n => !n.EndDateUtc.HasValue || n.EndDateUtc >= utcNow);
            }

            query = query.OrderByDescending(n => n.CreatedOnUtc);

            //Store mapping
            if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
            {
                query = from n in query
                        join sm in _storeMappingRepository.Table
                        on new { c1 = n.Id, c2 = "NewsItem" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into n_sm
                        from sm in n_sm.DefaultIfEmpty()
                        where !n.LimitedToStores || storeId == sm.StoreId
                        select n;

                //only distinct items (group by ID)
                query = from n in query
                        group n by n.Id into nGroup
                        orderby nGroup.Key
                        select nGroup.FirstOrDefault();

                query = query.OrderByDescending(n => n.CreatedOnUtc);
            }

            var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
            return news;
        }

        public virtual void InsertNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Insert(news);
        }

        public virtual void UpdateNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Update(news);
        }

        public virtual void UpdateCommentTotals(NewsItem newsItem)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            int approvedCommentCount = 0;
            int notApprovedCommentCount = 0;
            var newsComments = newsItem.NewsComments;
            foreach (var nc in newsComments)
            {
                if (nc.IsApproved)
                    approvedCommentCount++;
                else
                    notApprovedCommentCount++;
            }

            newsItem.ApprovedCommentCount = approvedCommentCount;
            newsItem.NotApprovedCommentCount = notApprovedCommentCount;
            UpdateNews(newsItem);
        }

        #region XML Sitemap

        public XmlSitemapProvider PublishXmlSitemap(XmlSitemapBuildContext context)
        {
            if (!context.LoadSetting<SeoSettings>().XmlSitemapIncludesNews || !context.LoadSetting<NewsSettings>().Enabled)
                return null;

            var query = GetAllNews(0, context.RequestStoreId, 0, int.MaxValue).SourceQuery;
            return new NewsXmlSitemapResult { Query = query };
        }

        class NewsXmlSitemapResult : XmlSitemapProvider
        {
            public IQueryable<NewsItem> Query { get; set; }

            public override int GetTotalCount()
            {
                return Query.Count();
            }

            public override IEnumerable<NamedEntity> Enlist()
            {
                var topics = Query.Select(x => new { x.Id, x.CreatedOnUtc, x.LanguageId }).ToList();
                foreach (var x in topics)
                {
                    yield return new NamedEntity { EntityName = "NewsItem", Id = x.Id, LastMod = x.CreatedOnUtc, LanguageId = x.LanguageId };
                }
            }

            public override int Order => 100;
        }

        #endregion
    }
}
