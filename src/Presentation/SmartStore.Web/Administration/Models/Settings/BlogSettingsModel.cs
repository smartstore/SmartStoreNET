using System.Collections.Generic;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Seo;

namespace SmartStore.Admin.Models.Settings
{
    public class BlogSettingsModel : ModelBase, ISeoModel
    {
        public BlogSettingsModel()
        {
            Locales = new List<SeoModelLocal>();
        }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.Enabled")]
        public bool Enabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.PostsPageSize")]
        public int PostsPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.AllowNotRegisteredUsersToLeaveComments")]
        public bool AllowNotRegisteredUsersToLeaveComments { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.NotifyAboutNewBlogComments")]
        public bool NotifyAboutNewBlogComments { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.NumberOfTags")]
        public int NumberOfTags { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.MaxAgeInDays")]
        public int MaxAgeInDays { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Blog.ShowHeaderRSSUrl")]
        public bool ShowHeaderRssUrl { get; set; }

        public string MetaTitle { get; set; }

        public string MetaDescription { get; set; }

        public string MetaKeywords { get; set; }

        public IList<SeoModelLocal> Locales { get; set; }
    }
}