using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Seo;
using SmartStore.Utilities;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Blogs
{
    public partial class BlogService : IBlogService, IXmlSitemapPublisher
    {
        private readonly IRepository<BlogPost> _blogPostRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly SeoSettings _seoSettings;

        public BlogService(IRepository<BlogPost> blogPostRepository,
            IRepository<StoreMapping> storeMappingRepository,
            SeoSettings seoSettings)
        {
            _blogPostRepository = blogPostRepository;
            _storeMappingRepository = storeMappingRepository;
            _seoSettings = seoSettings;

            this.QuerySettings = DbQuerySettings.Default;
        }

        public DbQuerySettings QuerySettings { get; set; }

        public virtual void DeleteBlogPost(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            _blogPostRepository.Delete(blogPost);
        }

        public virtual BlogPost GetBlogPostById(int blogPostId)
        {
            if (blogPostId == 0)
                return null;

            return _blogPostRepository.GetById(blogPostId);
        }

        public virtual IPagedList<BlogPost> GetAllBlogPosts(int storeId, int languageId,
            DateTime? dateFrom, DateTime? dateTo, int pageIndex, int pageSize, bool showHidden = false, DateTime? maxAge = null,
            string title = "", string intro = "", string body = "", string tag = "")
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


            if (title.HasValue())
                query = query.Where(b => b.Title.Contains(title));
            if (intro.HasValue())
                query = query.Where(b => b.Intro.Contains(intro));
            if (body.HasValue())
                query = query.Where(b => b.Body.Contains(body));
            if (tag.HasValue())
                query = query.Where(b => b.Tags.Contains(tag));

            if (!showHidden)
            {
                var utcNow = DateTime.UtcNow;
                query = query.Where(b => !b.StartDateUtc.HasValue || b.StartDateUtc <= utcNow);
                query = query.Where(b => !b.EndDateUtc.HasValue || b.EndDateUtc >= utcNow);
                query = query.Where(b => b.IsPublished);
            }

            if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
            {
                // Store mapping
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

        public virtual IPagedList<BlogPost> GetAllBlogPostsByTag(
            int storeId,
            int languageId,
            string tag,
            int pageIndex,
            int pageSize,
            bool showHidden = false,
            DateTime? maxAge = null)
        {
            tag = tag.Trim();

            //we laod all records and only then filter them by tag
            var blogPostsAll = GetAllBlogPosts(storeId, languageId, null, null, 0, int.MaxValue, showHidden, maxAge);
            var taggedBlogPosts = new List<BlogPost>();

            foreach (var blogPost in blogPostsAll)
            {
                var tags = blogPost.ParseTags().Select(x => SeoHelper.GetSeName(x,
                    _seoSettings.ConvertNonWesternChars,
                    _seoSettings.AllowUnicodeCharsInUrls,
                    true,
                    _seoSettings.SeoNameCharConversion));

                if (tags.FirstOrDefault(t => t.Equals(tag, StringComparison.InvariantCultureIgnoreCase)).HasValue())
                    taggedBlogPosts.Add(blogPost);
            }

            //server-side paging
            var result = new PagedList<BlogPost>(taggedBlogPosts, pageIndex, pageSize);
            return result;
        }

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

        public virtual void InsertBlogPost(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            _blogPostRepository.Insert(blogPost);
        }

        public virtual void UpdateBlogPost(BlogPost blogPost)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            _blogPostRepository.Update(blogPost);
        }

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

        #region XML Sitemap

        public XmlSitemapProvider PublishXmlSitemap(XmlSitemapBuildContext context)
        {
            if (!context.LoadSetting<SeoSettings>().XmlSitemapIncludesBlog || !context.LoadSetting<BlogSettings>().Enabled)
                return null;

            var query = GetAllBlogPosts(context.RequestStoreId, 0, null, null, 0, int.MaxValue).SourceQuery;

            return new BlogPostXmlSitemapResult { Query = query };
        }

        class BlogPostXmlSitemapResult : XmlSitemapProvider
        {
            public IQueryable<BlogPost> Query { get; set; }

            public override int GetTotalCount()
            {
                return Query.Count();
            }

            public override IEnumerable<NamedEntity> Enlist()
            {
                var topics = Query.Select(x => new { x.Id, x.CreatedOnUtc, x.LanguageId }).ToList();
                foreach (var x in topics)
                {
                    yield return new NamedEntity { EntityName = "BlogPost", Id = x.Id, LastMod = x.CreatedOnUtc, LanguageId = x.LanguageId };
                }
            }

            public override int Order => 300;
        }

        #endregion
    }
}
