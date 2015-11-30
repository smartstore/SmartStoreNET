using SmartStore.Core.Domain.Customers;

namespace SmartStore.Core.Domain.Blogs
{
    /// <summary>
    /// Represents a blog comment
    /// </summary>
    public partial class BlogComment : CustomerContent
    {
        /// <summary>
        /// Gets or sets the comment text
        /// </summary>
        public string CommentText { get; set; }

        /// <summary>
        /// Gets or sets the blog post identifier
        /// </summary>
        public int BlogPostId { get; set; }

        /// <summary>
        /// Gets or sets the blog post
        /// </summary>
        public virtual BlogPost BlogPost { get; set; }
    }
}