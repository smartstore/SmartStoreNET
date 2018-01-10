using System;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;

namespace SmartStore.Services.News
{
    /// <summary>
    /// News service
    /// </summary>
    public partial class NewsService : INewsService
    {
        #region Fields

        private readonly IRepository<NewsItem> _newsItemRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly ICommonServices _services;

		private readonly NewsSettings _newsSettings;

        #endregion

        #region Ctor

        public NewsService(IRepository<NewsItem> newsItemRepository,
			IRepository<StoreMapping> storeMappingRepository, 
			ICommonServices services,
			NewsSettings newsSettings)
        {
            _newsItemRepository = newsItemRepository;
			_storeMappingRepository = storeMappingRepository;
			_services = services;
			_newsSettings = newsSettings;

			this.QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a news
        /// </summary>
        /// <param name="newsItem">News item</param>
        public virtual void DeleteNews(NewsItem newsItem)
        {
            if (newsItem == null)
                throw new ArgumentNullException("newsItem");

            _newsItemRepository.Delete(newsItem);
        }

        /// <summary>
        /// Gets a news
        /// </summary>
        /// <param name="newsId">The news identifier</param>
        /// <returns>News</returns>
        public virtual NewsItem GetNewsById(int newsId)
        {
            if (newsId == 0)
                return null;

            return _newsItemRepository.GetById(newsId);
        }

		/// <summary>
		/// Get news by identifiers
		/// </summary>
		/// <param name="newsIds">News identifiers</param>
		/// <returns>News query</returns>
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

        /// <summary>
        /// Gets all news
        /// </summary>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
		/// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <param name="maxAge">The maximum age of returned news</param>
        /// <returns>News items</returns>
		public virtual IPagedList<NewsItem> GetAllNews(int languageId, int storeId, int pageIndex, int pageSize, bool showHidden = false, DateTime? maxAge = null)
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
						group n by n.Id	into nGroup
						orderby nGroup.Key
						select nGroup.FirstOrDefault();

				query = query.OrderByDescending(n => n.CreatedOnUtc);
			}

            var news = new PagedList<NewsItem>(query, pageIndex, pageSize);
            return news;
        }

        /// <summary>
        /// Inserts a news item
        /// </summary>
        /// <param name="news">News item</param>
        public virtual void InsertNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Insert(news);
        }

        /// <summary>
        /// Updates the news item
        /// </summary>
        /// <param name="news">News item</param>
        public virtual void UpdateNews(NewsItem news)
        {
            if (news == null)
                throw new ArgumentNullException("news");

            _newsItemRepository.Update(news);
        }
        
        /// <summary>
        /// Update news item comment totals
        /// </summary>
        /// <param name="newsItem">News item</param>
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

        #endregion
    }
}
