using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    public class NewsSettingsModel
    {
		public int ActiveStoreScopeConfiguration { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.Enabled")]
		public StoreDependingSetting<bool> Enabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.AllowNotRegisteredUsersToLeaveComments")]
        public StoreDependingSetting<bool> AllowNotRegisteredUsersToLeaveComments { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.NotifyAboutNewNewsComments")]
        public StoreDependingSetting<bool> NotifyAboutNewNewsComments { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.ShowNewsOnMainPage")]
        public StoreDependingSetting<bool> ShowNewsOnMainPage { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.MainPageNewsCount")]
        public StoreDependingSetting<int> MainPageNewsCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.NewsArchivePageSize")]
        public StoreDependingSetting<int> NewsArchivePageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.News.ShowHeaderRSSUrl")]
        public StoreDependingSetting<bool> ShowHeaderRssUrl { get; set; }
    }
}