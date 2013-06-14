using System.Web.Mvc;
using SmartStore.Core.Domain.Forums;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
    public class ForumSettingsModel
    {
		public int ActiveStoreScopeConfiguration { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ForumsEnabled")]
        public StoreDependingSetting<bool> ForumsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.RelativeDateTimeFormattingEnabled")]
        public StoreDependingSetting<bool> RelativeDateTimeFormattingEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ShowCustomersPostCount")]
        public StoreDependingSetting<bool> ShowCustomersPostCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowGuestsToCreatePosts")]
        public StoreDependingSetting<bool> AllowGuestsToCreatePosts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowGuestsToCreateTopics")]
        public StoreDependingSetting<bool> AllowGuestsToCreateTopics { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowCustomersToEditPosts")]
        public StoreDependingSetting<bool> AllowCustomersToEditPosts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowCustomersToDeletePosts")]
        public StoreDependingSetting<bool> AllowCustomersToDeletePosts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowCustomersToManageSubscriptions")]
        public StoreDependingSetting<bool> AllowCustomersToManageSubscriptions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.TopicsPageSize")]
        public StoreDependingSetting<int> TopicsPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.PostsPageSize")]
        public StoreDependingSetting<int> PostsPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ForumEditor")]
        public StoreDependingSetting<EditorType> ForumEditor { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.SignaturesEnabled")]
        public StoreDependingSetting<bool> SignaturesEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowPrivateMessages")]
        public StoreDependingSetting<bool> AllowPrivateMessages { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ShowAlertForPM")]
        public StoreDependingSetting<bool> ShowAlertForPM { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.NotifyAboutPrivateMessages")]
        public StoreDependingSetting<bool> NotifyAboutPrivateMessages { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ActiveDiscussionsFeedEnabled")]
        public StoreDependingSetting<bool> ActiveDiscussionsFeedEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ActiveDiscussionsFeedCount")]
        public StoreDependingSetting<int> ActiveDiscussionsFeedCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ForumFeedsEnabled")]
        public StoreDependingSetting<bool> ForumFeedsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ForumFeedCount")]
        public StoreDependingSetting<int> ForumFeedCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.SearchResultsPageSize")]
        public StoreDependingSetting<int> SearchResultsPageSize { get; set; }

        public SelectList ForumEditorValues { get; set; }
    }
}