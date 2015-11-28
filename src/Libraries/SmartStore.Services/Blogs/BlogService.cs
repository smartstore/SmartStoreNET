using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;
using SmartStore.Utilities;

namespace SmartStore.Services.Blogs
{
    /// <summary>
    /// Blog service
    /// </summary>
    public partial class BlogService : IBlogService
    {
        #region Fields

        private readonly IRepository<BlogPost> _blogPostRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly ICommonServices _services;
		private readonly ILanguageService _languageService;

		private readonly BlogSettings _blogSettings;

        #endregion

        #region Ctor

        public BlogService(IRepository<BlogPost> blogPostRepository,
			IRepository<StoreMapping> storeMappingRepository,
			ICommonServices services,
			ILanguageService languageService,
			BlogSettings blogSettings)
        {
            _blogPostRepository = blogPostRepository;
			_storeMappingRepository = storeMappingRepository;
			_services = services;
			_languageService = languageService;
			_blogSettings = blogSettings;

			this.QuerySettings = DbQuerySettings.Default;
        }

		public DbQuerySettings QuerySettings { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        public virtual void DeleteBlogPost(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            _blogPostRepository.Delete(blogPost);

            //event notification
            _services.EventPublisher.EntityDeleted(blogPost);
        }

        /// <summary>
        /// Gets a blog post
        /// </summary>
        /// <param name="blogPostId">Blog post identifier</param>
        /// <returns>Blog post</returns>
        public virtual BlogPost GetBlogPostById(int blogPostId)
        {
            if (blogPostId == 0)
                return null;

                return _blogPostRepository.GetById(blogPostId);
        }

        /// <summary>
        /// Gets all blog posts
        /// </summary>
		/// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier; 0 if you want to get all records</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
		/// <param name="maxAge">The maximum age of returned blog posts</param>
        /// <returns>Blog posts</returns>
		public virtual IPagedList<BlogPost> GetAllBlogPosts(int storeId, int languageId,
			DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize, bool showHidden = false, DateTime? maxAge = null)
        {
            var query = _blogPostRepository.Table;

            if (dateFrom.HasValue)
                query = query.Where(b => dateFrom.Value <= b.CreatedOnUtc);

            if (dateTo.HasValue)
                query = query.Where(b => dateTo.Value >= b.CreatedOnUtc);

            if (languageId > 0)
                query = query.Where(b => languageId == b.LanguageId);

			if (maxAge.HasValue)
				query = query.Where(b => b.CreatedOnUtc >= maxAge.Value);

            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(b => !b.StartDateUtc.HasValue || b.StartDateUtc <= utcNow);
                query = query.Where(b => !b.EndDateUtc.HasValue || b.EndDateUtc >= utcNow);
            }

			if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
			{
				//Store mapping
				query = from bp in query
						join sm in _storeMappingRepository.Table
						on new { c1 = bp.Id, c2 = "BlogPost" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into bp_sm
						from sm in bp_sm.DefaultIfEmpty()
						where !bp.LimitedToStores || storeId == sm.StoreId
						select bp;

				//only distinct blog posts (group by ID)
				query = from bp in query
						group bp by bp.Id into bpGroup
						orderby bpGroup.Key
						select bpGroup.FirstOrDefault();
			}

            query = query.OrderByDescending(b => b.CreatedOnUtc);
            
            var blogPosts = new PagedList<BlogPost>(query, pageIndex, pageSize);
            return blogPosts;
        }

        /// <summary>
        /// Gets all blog posts
        /// </summary>
		/// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all news</param>
        /// <param name="tag">Tag</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Blog posts</returns>
		public virtual IPagedList<BlogPost> GetAllBlogPostsByTag(int storeId, int languageId, string tag,
            int pageIndex, int pageSize, bool showHidden = false)
        {
            tag = tag.Trim();

            //we laod all records and only then filter them by tag
			var blogPostsAll = GetAllBlogPosts(storeId, languageId, null, null, 0, int.MaxValue, showHidden);
            var taggedBlogPosts = new List<BlogPost>();
            foreach (var blogPost in blogPostsAll)
            {
                var tags = blogPost.ParseTags();
                if (!String.IsNullOrEmpty(tags.FirstOrDefault(t => t.Equals(tag, StringComparison.InvariantCultureIgnoreCase))))
                    taggedBlogPosts.Add(blogPost);
            }

            //server-side paging
            var result = new PagedList<BlogPost>(taggedBlogPosts, pageIndex, pageSize);
            return result;
        }

        /// <summary>
        /// Gets all blog post tags
        /// </summary>
		/// <param name="storeId">The store identifier; pass 0 to load all records</param>
        /// <param name="languageId">Language identifier. 0 if you want to get all news</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Blog post tags</returns>
		public virtual IList<BlogPostTag> GetAllBlogPostTags(int storeId, int languageId, bool showHidden = false)
        {
            var blogPostTags = new List<BlogPostTag>();

			var blogPosts = GetAllBlogPosts(storeId, languageId, null, null, 0, int.MaxValue, showHidden);
            foreach (var blogPost in blogPosts)
            {
                var tags = blogPost.ParseTags();
                foreach (string tag in tags)
                {
                    var foundBlogPostTag = blogPostTags.Find(bpt => bpt.Name.Equals(tag, StringComparison.InvariantCultureIgnoreCase));
                    if (foundBlogPostTag == null)
                    {
                        foundBlogPostTag = new BlogPostTag()
                        {
                            Name = tag,
                            BlogPostCount = 1
                        };
                        blogPostTags.Add(foundBlogPostTag);
                    }
                    else
                        foundBlogPostTag.BlogPostCount++;
                }
            }

            return blogPostTags;
        }

        /// <summary>
        /// Inserts an blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        public virtual void InsertBlogPost(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            _blogPostRepository.Insert(blogPost);

            //event notification
            _services.EventPublisher.EntityInserted(blogPost);
        }

        /// <summary>
        /// Updates the blog post
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        public virtual void UpdateBlogPost(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            _blogPostRepository.Update(blogPost);

            //event notification
            _services.EventPublisher.EntityUpdated(blogPost);
        }
        
        /// <summary>
        /// Update blog post comment totals
        /// </summary>
        /// <param name="blogPost">Blog post</param>
        public virtual void UpdateCommentTotals(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            int approvedCommentCount = 0;
            int notApprovedCommentCount = 0;
            var blogComments = blogPost.BlogComments;
            foreach (var bc in blogComments)
            {
                if (bc.IsApproved)
                    approvedCommentCount++;
                else
                    notApprovedCommentCount++;
            }

            blogPost.ApprovedCommentCount = approvedCommentCount;
            blogPost.NotApprovedCommentCount = notApprovedCommentCount;
            UpdateBlogPost(blogPost);
        }

		/// <summary>
		/// Creates a RSS feed with blog posts
		/// </summary>
		/// <param name="urlHelper">UrlHelper to generate URLs</param>
		/// <param name="languageId">Language identifier</param>
		/// <returns>SmartSyndicationFeed object</returns>
		public virtual SmartSyndicationFeed CreateRssFeed(UrlHelper urlHelper, int languageId)
		{
			if (urlHelper == null)
				throw new ArgumentNullException("urlHelper");

			DateTime? maxAge = null;
			var protocol = _services.WebHelper.IsCurrentConnectionSecured() ? "https" : "http";
			var selfLink = urlHelper.RouteUrl("BlogRSS", new { languageId = languageId }, protocol);
			var blogLink = urlHelper.RouteUrl("Blog", null, protocol);

			var title = "{0} - Blog".FormatInvariant(_services.StoreContext.CurrentStore.Name);

			if (_blogSettings.MaxAgeInDays > 0)
				maxAge = DateTime.UtcNow.Subtract(new TimeSpan(_blogSettings.MaxAgeInDays, 0, 0, 0));

			var language = _languageService.GetLanguageById(languageId);
			var feed = new SmartSyndicationFeed(new Uri(blogLink), title);

			feed.AddNamespaces(false);
			feed.Init(selfLink, language);

			if (!_blogSettings.Enabled)
				return feed;

			var items = new List<SyndicationItem>();
			var blogPosts = GetAllBlogPosts(_services.StoreContext.CurrentStore.Id, languageId, null, null, 0, int.MaxValue, false, maxAge);

			foreach (var blogPost in blogPosts)
			{
				var blogPostUrl = urlHelper.RouteUrl("BlogPost", new { SeName = blogPost.GetSeName(blogPost.LanguageId, ensureTwoPublishedLanguages: false) }, "http");

				var item = feed.CreateItem(blogPost.Title, blogPost.Body, blogPostUrl, blogPost.CreatedOnUtc);

				items.Add(item);
			}

			feed.Items = items;

			return feed;
		}

        #endregion
    }
}
