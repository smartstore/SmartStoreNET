
using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Blogs
{
    public class BlogSettings : ISettings
    {
		public BlogSettings()
		{
			Enabled = true;
			PostsPageSize = 10;
			AllowNotRegisteredUsersToLeaveComments = true;
			NumberOfTags = 15;
			MaxAgeInDays = 180;
		}
		
		/// <summary>
        /// Gets or sets a value indicating whether blog is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the page size for posts
        /// </summary>
        public int PostsPageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether not registered user can leave comments
        /// </summary>
        public bool AllowNotRegisteredUsersToLeaveComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify about new blog comments
        /// </summary>
        public bool NotifyAboutNewBlogComments { get; set; }

        /// <summary>
        /// Gets or sets a number of blog tags that appear in the tag cloud
        /// </summary>
        public int NumberOfTags { get; set; }

		/// <summary>
		/// The maximum age of blog items (in days) for RSS feed
		/// </summary>
		public int MaxAgeInDays { get; set; }

        /// <summary>
        /// Enable the blog RSS feed link in customers browser address bar
        /// </summary>
        public bool ShowHeaderRssUrl { get; set; }
    }
}