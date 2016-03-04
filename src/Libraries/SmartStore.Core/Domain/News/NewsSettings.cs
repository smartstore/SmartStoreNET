using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.News
{
    public class NewsSettings : ISettings
    {
		public NewsSettings()
		{
			Enabled = true;
			AllowNotRegisteredUsersToLeaveComments = true;
			ShowNewsOnMainPage = true;
			MainPageNewsCount = 3;
			NewsArchivePageSize = 10;
			MaxAgeInDays = 180;
		}
		
		/// <summary>
        /// Gets or sets a value indicating whether news are enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether not registered user can leave comments
        /// </summary>
        public bool AllowNotRegisteredUsersToLeaveComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify about new news comments
        /// </summary>
        public bool NotifyAboutNewNewsComments { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show news on the main page
        /// </summary>
        public bool ShowNewsOnMainPage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating news count displayed on the main page
        /// </summary>
        public int MainPageNewsCount { get; set; }

        /// <summary>
        /// Gets or sets the page size for news archive
        /// </summary>
        public int NewsArchivePageSize { get; set; }

		/// <summary>
		/// The maximum age of news (in days) for RSS feed
		/// </summary>
		public int MaxAgeInDays { get; set; }

        /// <summary>
        /// Enable the news RSS feed link in customers browser address bar
        /// </summary>
        public bool ShowHeaderRssUrl { get; set; }
    }
}