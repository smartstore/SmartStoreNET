using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
	public class NewsSettingsModel
    {
        [SmartResourceDisplayName("Admin.Configuration.Settings.News.Enabled")]
		public bool Enabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.AllowNotRegisteredUsersToLeaveComments")]
        public bool AllowNotRegisteredUsersToLeaveComments { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.NotifyAboutNewNewsComments")]
        public bool NotifyAboutNewNewsComments { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.ShowNewsOnMainPage")]
        public bool ShowNewsOnMainPage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.MainPageNewsCount")]
        public int MainPageNewsCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.NewsArchivePageSize")]
        public int NewsArchivePageSize { get; set; }

		[SmartResourceDisplayName("Admin.Configuration.Settings.News.MaxAgeInDays")]
		public int MaxAgeInDays { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.ShowHeaderRSSUrl")]
        public bool ShowHeaderRssUrl { get; set; }
    }
}