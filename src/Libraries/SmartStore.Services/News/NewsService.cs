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

        public NewsService(
            IRepository<NewsItem> newsItemRepository,
            IRepository<StoreMapping> storeMappingRepository)
        {
            _newsItemRepository = newsItemRepository;
            _storeMappingRepository = storeMappingRepository;
        }

        public DbQuerySettings QuerySettings { get; set; } = DbQuerySettings.Default;

        public virtual void InsertNews(NewsItem news)
        {
            Guard.NotNull(news, nameof(news));

            _newsItemRepository.Insert(news);
        }

        public virtual void UpdateNews(NewsItem news)
        {
            Guard.NotNull(news, nameof(news));

            _newsItemRepository.Update(news);
        }

        public virtual void DeleteNews(NewsItem news)
        {
            if (news != null)
            {
                _newsItemRepository.Delete(news);
            }
        }

        public virtual void UpdateCommentTotals(NewsItem news)
        {
            Guard.NotNull(news, nameof(news));

            var approvedCommentCount = 0;
            var notApprovedCommentCount = 0;
            var newsComments = news.NewsComments;

            foreach (var nc in newsComments)
            {
                if (nc.IsApproved)
                {
                    approvedCommentCount++;
                }
                else
                {
                    notApprovedCommentCount++;
                }
            }

            news.ApprovedCommentCount = approvedCommentCount;
            news.NotApprovedCommentCount = notApprovedCommentCount;
            UpdateNews(news);
        }

        public virtual NewsItem GetNewsById(int newsId)
        {
            if (newsId == 0)
            {
                return null;
            }

            return _newsItemRepository.GetById(newsId);
        }

        public virtual IQueryable<NewsItem> GetNewsByIds(int[] newsIds)
        {
            if (newsIds == null || newsIds.Length == 0)
            {
                return null;
            }

            var query =
                from x in _newsItemRepository.Table
                where newsIds.Contains(x.Id)
                select x;

            return query;
        }

        public virtual IPagedList<NewsItem> GetAllNews(
            int storeId,
            int pageIndex,
            int pageSize,
            int languageId = 0,
            bool showHidden = false,
            DateTime? maxAge = null,
            string title = "", 
            string intro = "",
            string full = "",
            bool untracked = true)
        {
            var query = untracked ? _newsItemRepository.TableUntracked : _newsItemRepository.Table;

            if (maxAge.HasValue)
            {
                query = query.Where(n => n.CreatedOnUtc >= maxAge.Value);
            }
            if (title.HasValue())
            {
                query = query.Where(n => n.Title.Contains(title));
            }
            if (intro.HasValue())
            {
                query = query.Where(n => n.Short.Contains(intro));
            }
            if (full.HasValue())
            {
                query = query.Where(n => n.Full.Contains(full));
            }

            if (languageId != 0)
            {
                query = query.Where(n => !n.LanguageId.HasValue || n.LanguageId == languageId);
            }

            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(n => n.Published);
                query = query.Where(n => !n.StartDateUtc.HasValue || n.StartDateUtc <= utcNow);
                query = query.Where(n => !n.EndDateUtc.HasValue || n.EndDateUtc >= utcNow);
            }

            query = query.OrderByDescending(n => n.CreatedOnUtc);

            if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
            {
                query = from n in query
                        join sm in _storeMappingRepository.Table
                        on new { c1 = n.Id, c2 = "NewsItem" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into n_sm
                        from sm in n_sm.DefaultIfEmpty()
                        where !n.LimitedToStores || storeId == sm.StoreId
                        select n;

                // Only distinct items (group by ID).
                query = from n in query
                        group n by n.Id into nGroup
                        orderby nGroup.Key
                        select nGroup.FirstOrDefault();

                query = query.OrderByDescending(n => n.CreatedOnUtc);
            }

            var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
            return news;
        }

        #region XML Sitemap

        public XmlSitemapProvider PublishXmlSitemap(XmlSitemapBuildContext context)
        {
            if (!context.LoadSetting<SeoSettings>().XmlSitemapIncludesNews || !context.LoadSetting<NewsSettings>().Enabled)
            {
                return null;
            }

            var query = GetAllNews(context.RequestStoreId, 0, int.MaxValue).SourceQuery;
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
                var newsItems = Query.Select(x => new { x.Id, x.CreatedOnUtc, x.LanguageId }).ToList();

                foreach (var x in newsItems)
                {
                    yield return new NamedEntity { EntityName = "NewsItem", Id = x.Id, LastMod = x.CreatedOnUtc, LanguageId = x.LanguageId };
                }
            }

            public override int Order => 100;
        }

        #endregion
    }
}
