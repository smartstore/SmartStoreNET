using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Admin.Models.Settings
{
    public class NewsSettingsModel : ISeoModel
    {
        public NewsSettingsModel()
        {
            Locales = new List<SeoModelLocal>();
        }

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

        public string MetaTitle { get; set; }

        public string MetaDescription { get; set; }

        public string MetaKeywords { get; set; }

        public IList<SeoModelLocal> Locales { get; set; }
    }
}