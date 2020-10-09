using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Blogs;

namespace SmartStore.Services.Blogs
{
    /// <summary>
    /// Blog service interface.
    /// </summary>
    public partial interface IBlogService
    {
        /// <summary>
        /// Inserts an blog post.
        /// </summary>
        /// <param name="blogPost">Blog post.</param>
        void InsertBlogPost(BlogPost blogPost);

        /// <summary>
        /// Updates the blog post.
        /// </summary>
        /// <param name="blogPost">Blog post.</param>
        void UpdateBlogPost(BlogPost blogPost);

        /// <summary>
        /// Deletes a blog post.
        /// </summary>
        /// <param name="blogPost">Blog post.</param>
        void DeleteBlogPost(BlogPost blogPost);

        /// <summary>
        /// Update blog post comment totals.
        /// </summary>
        /// <param name="blogPost">Blog post.</param>
        void UpdateCommentTotals(BlogPost blogPost);

        /// <summary>
        /// Gets a blog post.
        /// </summary>
        /// <param name="blogPostId">Blog post identifier.</param>
        /// <returns>Blog post.</returns>
        BlogPost GetBlogPostById(int blogPostId);

        /// <summary>
        /// Gets all blog posts.
        /// </summary>
		/// <param name="storeId">The store identifier; pass 0 to load all records.</param>
        /// <param name="dateFrom">Filter by created date; null if you want to get all records.</param>
        /// <param name="dateTo">Filter by created date; null if you want to get all records.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="showHidden">A value indicating whether to show hidden records.</param>
		/// <param name="maxAge">The maximum age of returned blog posts.</param>
        /// <returns>Blog posts.</returns>
		IPagedList<BlogPost> GetAllBlogPosts(
            int storeId,
            DateTime? dateFrom,
            DateTime? dateTo, 
            int pageIndex, 
            int pageSize, 
            bool showHidden = false,
            DateTime? maxAge = null,
            string title = "",
            string intro = "", 
            string body = "", 
            string tag = "");

        /// <summary>
        /// Gets all blog posts.
        /// </summary>
		/// <param name="storeId">The store identifier; pass 0 to load all records.</param>
        /// <param name="tag">Tag.</param>
        /// <param name="pageIndex">Page index.</param>
        /// <param name="pageSize">Page size.</param>
        /// <param name="showHidden">A value indicating whether to show hidden records.</param>
        /// <returns>Blog posts.</returns>
		IPagedList<BlogPost> GetAllBlogPostsByTag(
            int storeId,
            string tag,
            int pageIndex, 
            int pageSize, 
            bool showHidden = false, 
            DateTime? maxAge = null);

        /// <summary>
        /// Gets all blog post tags.
        /// </summary>
		/// <param name="storeId">The store identifier; pass 0 to load all records.</param>
        /// <param name="showHidden">A value indicating whether to show hidden records.</param>
        /// <returns>Blog post tags.</returns>
		IList<BlogPostTag> GetAllBlogPostTags(int storeId, bool showHidden = false);
    }
}
