﻿using System.Web.Mvc;
using SmartStore.Core.Domain.Forums;
using SmartStore.Web.Framework;

namespace SmartStore.Admin.Models.Settings
{
	public class ForumSettingsModel
    {
        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ForumsEnabled")]
        public bool ForumsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.RelativeDateTimeFormattingEnabled")]
        public bool RelativeDateTimeFormattingEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ShowCustomersPostCount")]
        public bool ShowCustomersPostCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowGuestsToCreatePosts")]
        public bool AllowGuestsToCreatePosts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowGuestsToCreateTopics")]
        public bool AllowGuestsToCreateTopics { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowCustomersToEditPosts")]
        public bool AllowCustomersToEditPosts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowCustomersToDeletePosts")]
        public bool AllowCustomersToDeletePosts { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowCustomersToManageSubscriptions")]
        public bool AllowCustomersToManageSubscriptions { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.TopicsPageSize")]
        public int TopicsPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.PostsPageSize")]
        public int PostsPageSize { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ForumEditor")]
        public EditorType ForumEditor { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.SignaturesEnabled")]
        public bool SignaturesEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.AllowPrivateMessages")]
        public bool AllowPrivateMessages { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ShowAlertForPM")]
        public bool ShowAlertForPM { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.NotifyAboutPrivateMessages")]
        public bool NotifyAboutPrivateMessages { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ActiveDiscussionsFeedEnabled")]
        public bool ActiveDiscussionsFeedEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ActiveDiscussionsFeedCount")]
        public int ActiveDiscussionsFeedCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ForumFeedsEnabled")]
        public bool ForumFeedsEnabled { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.ForumFeedCount")]
        public int ForumFeedCount { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Settings.Forums.SearchResultsPageSize")]
        public int SearchResultsPageSize { get; set; }

        public SelectList ForumEditorValues { get; set; }
    }
}