using SmartStore.Core.Configuration;

namespace SmartStore.Core.Domain.Forums
{
    public class ForumSettings : ISettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether forums are enabled
        /// </summary>
        public bool ForumsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether relative date and time formatting is enabled (e.g. 2 hours ago, a month ago)
        /// </summary>
        public bool RelativeDateTimeFormattingEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to edit posts that they created
        /// </summary>
        public bool AllowCustomersToEditPosts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to manage their subscriptions
        /// </summary>
        public bool AllowCustomersToManageSubscriptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether guests are allowed to create posts
        /// </summary>
        public bool AllowGuestsToCreatePosts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether guests are allowed to create topics
        /// </summary>
        public bool AllowGuestsToCreateTopics { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to delete posts that they created
        /// </summary>
        public bool AllowCustomersToDeletePosts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customer can vote on posts
        /// </summary>
        public bool AllowCustomersToVoteOnPosts { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether guests are allowed to vote on posts
        /// </summary>
        public bool AllowGuestsToVoteOnPosts { get; set; }

        /// <summary>
        /// Gets or sets maximum length of topic subject
        /// </summary>
        public int TopicSubjectMaxLength { get; set; } = 450;

        /// <summary>
        /// Gets or sets the maximum length for stripped forum topic names
        /// </summary>
        public int StrippedTopicMaxLength { get; set; } = 45;

        /// <summary>
        /// Gets or sets maximum length of post
        /// </summary>
        public int PostMaxLength { get; set; } = 4000;

        /// <summary>
        /// Gets or sets the page size for topics in forums
        /// </summary>
        public int TopicsPageSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets the page size for posts in topics
        /// </summary>
        public int PostsPageSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets the page size for search result
        /// </summary>
        public int SearchResultsPageSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets a value indicating whether sorting is enabled.
        /// </summary>
        public bool AllowSorting { get; set; } = true;

        /// <summary>
        /// Gets or sets the page size for latest customer posts
        /// </summary>
        public int LatestCustomerPostsPageSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets a value indicating whether to show customers forum post count
        /// </summary>
        public bool ShowCustomersPostCount { get; set; } = true;

        /// <summary>
        /// Gets or sets a forum editor type
        /// </summary>
        public EditorType ForumEditor { get; set; } = EditorType.BBCodeEditor;

        /// <summary>
        /// Gets or sets a value indicating whether customers are allowed to specify a signature
        /// </summary>
        public bool SignaturesEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether private messages are allowed
        /// </summary>
        public bool AllowPrivateMessages { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether an alert should be shown for new private messages
        /// </summary>
        public bool ShowAlertForPM { get; set; }

        /// <summary>
        /// Gets or sets the page size for private messages
        /// </summary>
        public int PrivateMessagesPageSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets the page size for (My Account) Forum Subscriptions
        /// </summary>
        public int ForumSubscriptionsPageSize { get; set; } = 20;

        /// <summary>
        /// Gets or sets a value indicating whether a customer should be notified about new private messages
        /// </summary>
        public bool NotifyAboutPrivateMessages { get; set; }

        /// <summary>
        /// Gets or sets maximum length of pm subject
        /// </summary>
        public int PMSubjectMaxLength { get; set; } = 450;

        /// <summary>
        /// Gets or sets maximum length of pm message
        /// </summary>
        public int PMTextMaxLength { get; set; } = 4000;

        /// <summary>
        /// Gets or sets the number of items to display for Active Discussions on forums home page
        /// </summary>
        public int HomePageActiveDiscussionsTopicCount { get; set; } = 5;

        /// <summary>
        /// Gets or sets the number of items to display for Active Discussions page
        /// </summary>
        public int ActiveDiscussionsPageTopicCount { get; set; } = 50;

        /// <summary>
        /// Gets or sets the number of items to display for Active Discussions RSS Feed
        /// </summary>
        public int ActiveDiscussionsFeedCount { get; set; } = 25;

        /// <summary>
        /// Gets or sets the whether the Active Discussions RSS Feed is enabled
        /// </summary>
        public bool ActiveDiscussionsFeedEnabled { get; set; }

        /// <summary>
        /// Gets or sets the whether Forums have an RSS Feed enabled
        /// </summary>
        public bool ForumFeedsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the number of items to display for Forum RSS Feed
        /// </summary>
        public int ForumFeedCount { get; set; } = 20;

        /// <summary>
        /// Gets or sets the meta title for forum index page
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the meta description for forum index page
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords for forum index page
        /// </summary>
        public string MetaKeywords { get; set; }
    }
}
