using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Mvc;

namespace SmartStore.Admin.Models.Settings
{
    public class BlogSettingsModel : ModelBase
    {
		public int ActiveStoreScopeConfiguration { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.Enabled")]
        public StoreDependingSetting<bool> Enabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.PostsPageSize")]
        public StoreDependingSetting<int> PostsPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.AllowNotRegisteredUsersToLeaveComments")]
        public StoreDependingSetting<bool> AllowNotRegisteredUsersToLeaveComments { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.NotifyAboutNewBlogComments")]
        public StoreDependingSetting<bool> NotifyAboutNewBlogComments { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.NumberOfTags")]
        public StoreDependingSetting<int> NumberOfTags { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.ShowHeaderRSSUrl")]
        public StoreDependingSetting<bool> ShowHeaderRssUrl { get; set; }
    }
}