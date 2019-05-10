using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;

namespace SmartStore.Services.Forums
{
    /// <summary>
    /// Forum service interface
    /// </summary>
    public partial interface IForumService
    {
        #region Group

        /// <summary>
        /// Gets a forum group
        /// </summary>
        /// <param name="groupId">The forum group identifier</param>
        /// <returns>Forum group</returns>
        ForumGroup GetForumGroupById(int groupId);

        /// <summary>
        /// Gets all forum groups
        /// </summary>
        /// <param name="storeId">Store identifier</param>
        /// <param name="showHidden">Whether to load hidden records</param>
        /// <returns>Forum groups</returns>
		IList<ForumGroup> GetAllForumGroups(int storeId = 0, bool showHidden = false);

        /// <summary>
        /// Deletes a forum group
        /// </summary>
        /// <param name="group">Forum group</param>
        void DeleteForumGroup(ForumGroup group);

        /// <summary>
        /// Inserts a forum group
        /// </summary>
        /// <param name="group">Forum group</param>
        void InsertForumGroup(ForumGroup group);

        /// <summary>
        /// Updates the forum group
        /// </summary>
        /// <param name="group">Forum group</param>
        void UpdateForumGroup(ForumGroup group);

        #endregion

        #region Forum

        /// <summary>
        /// Gets a forum
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <returns>Forum</returns>
        Forum GetForumById(int forumId);

        /// <summary>
        /// Deletes a forum
        /// </summary>
        /// <param name="forum">Forum</param>
        void DeleteForum(Forum forum);

        /// <summary>
        /// Inserts a forum
        /// </summary>
        /// <param name="forum">Forum</param>
        void InsertForum(Forum forum);

        /// <summary>
        /// Updates the forum
        /// </summary>
        /// <param name="forum">Forum</param>
        void UpdateForum(Forum forum);

        #endregion

        #region Topic

        /// <summary>
        /// Gets a forum topic
        /// </summary>
        /// <param name="topicId">The forum topic identifier</param>
        /// <returns>Forum Topic</returns>
        ForumTopic GetTopicById(int topicId);

        /// <summary>
        /// Gets forum topics by identifiers
        /// </summary>
        /// <param name="topicIds">Array of topic identifiers</param>
        /// <returns>List of topics</returns>
        IList<ForumTopic> GetTopicsByIds(int[] topicIds);

        /// <summary>
        /// Gets all forum topics
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">Whether to load hidden records</param>
        /// <returns>Forum Topics</returns>
        IPagedList<ForumTopic> GetAllTopics(int forumId, int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Gets active forum topics
        /// </summary>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="topicCount">Count of forum topics to return</param>
        /// <param name="showHidden">Whether to load hidden records</param>
        /// <returns>Forum Topics</returns>
        IList<ForumTopic> GetActiveTopics(int forumId, int topicCount, bool showHidden = false);

        /// <summary>
        /// Deletes a forum topic
        /// </summary>
        /// <param name="topic">Forum topic</param>
        void DeleteTopic(ForumTopic topic);

        /// <summary>
        /// Inserts a forum topic
        /// </summary>
        /// <param name="topic">Forum topic</param>
        /// <param name="sendNotifications">A value indicating whether to send notifications to subscribed customers</param>
        void InsertTopic(ForumTopic topic, bool sendNotifications);

        /// <summary>
        /// Updates the forum topic
        /// </summary>
        /// <param name="topic">Forum topic</param>
        /// <param name="updateStatistics">A value indicating whether to update counter.</param>
        void UpdateTopic(ForumTopic topic, bool updateStatistics);

        /// <summary>
        /// Moves the forum topic
        /// </summary>
        /// <param name="topicId">The forum topic identifier</param>
        /// <param name="newForumId">New forum identifier</param>
        /// <returns>Moved forum topic</returns>
        ForumTopic MoveTopic(int topicId, int newForumId);

        /// <summary>
        /// Calculates topic page index by post identifier
        /// </summary>
        /// <param name="topicId">Topic identifier</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="postId">Post identifier</param>
        /// <returns>Page index</returns>
        int CalculateTopicPageIndex(int topicId, int pageSize, int postId);

        #endregion

        #region Post

        /// <summary>
        /// Gets a forum post
        /// </summary>
        /// <param name="postId">The forum post identifier</param>
        /// <returns>Forum Post</returns>
        ForumPost GetPostById(int postId);

        /// <summary>
        /// Gets forum posts by identifiers.
        /// </summary>
        /// <param name="postIds">Forum post identfiers.</param>
        /// <returns>Forum posts.</returns>
        IList<ForumPost> GetPostsByIds(int[] postIds);

        /// <summary>
        /// Gets all forum posts
        /// </summary>
        /// <param name="topicId">The forum topic identifier</param>
        /// <param name="customerId">The customer identifier</param>
        /// <param name="ascSort">Sort order</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">Whether to load hidden records</param>
        /// <returns>Forum Posts</returns>
        IPagedList<ForumPost> GetAllPosts(int topicId, int customerId, bool ascSort, int pageIndex, int pageSize, bool showHidden = false);

        /// <summary>
        /// Deletes a forum post
        /// </summary>
        /// <param name="post">Forum post</param>
        void DeletePost(ForumPost post);

        /// <summary>
        /// Inserts a forum post
        /// </summary>
        /// <param name="post">The forum post</param>
        /// <param name="sendNotifications">A value indicating whether to send notifications to subscribed customers</param>
        void InsertPost(ForumPost post, bool sendNotifications);

        /// <summary>
        /// Updates the forum post
        /// </summary>
        /// <param name="post">Forum post</param>
        /// <param name="updateStatistics">A value indicating whether to update counter.</param>
        void UpdatePost(ForumPost post, bool updateStatistics);

        #endregion

        #region Private message

        /// <summary>
        /// Gets a private message
        /// </summary>
        /// <param name="messageId">The private message identifier</param>
        /// <returns>Private message</returns>
        PrivateMessage GetPrivateMessageById(int messageId);

        /// <summary>
        /// Gets private messages
        /// </summary>
		/// <param name="storeId">The store identifier; pass 0 to load all messages</param>
        /// <param name="fromCustomerId">The customer identifier who sent the message</param>
        /// <param name="toCustomerId">The customer identifier who should receive the message</param>
        /// <param name="isRead">A value indicating whether loaded messages are read. false - to load not read messages only, 1 to load read messages only, null to load all messages</param>
        /// <param name="isDeletedByAuthor">A value indicating whether loaded messages are deleted by author. false - messages are not deleted by author, null to load all messages</param>
        /// <param name="isDeletedByRecipient">A value indicating whether loaded messages are deleted by recipient. false - messages are not deleted by recipient, null to load all messages</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Private messages</returns>
		IPagedList<PrivateMessage> GetAllPrivateMessages(
            int storeId,
            int fromCustomerId,
            int toCustomerId,
            bool? isRead,
            bool? isDeletedByAuthor,
            bool? isDeletedByRecipient,
            int pageIndex,
            int pageSize);

        /// <summary>
        /// Deletes a private message
        /// </summary>
        /// <param name="message">Private message</param>
        void DeletePrivateMessage(PrivateMessage message);

        /// <summary>
        /// Inserts a private message
        /// </summary>
        /// <param name="message">Private message</param>
        void InsertPrivateMessage(PrivateMessage message);

        /// <summary>
        /// Updates the private message
        /// </summary>
        /// <param name="message">Private message</param>
        void UpdatePrivateMessage(PrivateMessage message);

        #endregion

        #region Subscription

        /// <summary>
        /// Gets a forum subscription
        /// </summary>
        /// <param name="forumSubscriptionId">The forum subscription identifier</param>
        /// <returns>Forum subscription</returns>
        ForumSubscription GetSubscriptionById(int forumSubscriptionId);

        /// <summary>
        /// Gets forum subscriptions
        /// </summary>
        /// <param name="customerId">The customer identifier</param>
        /// <param name="forumId">The forum identifier</param>
        /// <param name="topicId">The topic identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Forum subscriptions</returns>
        IPagedList<ForumSubscription> GetAllSubscriptions(int customerId, int forumId,
            int topicId, int pageIndex, int pageSize);

        /// <summary>
        /// Deletes a forum subscription
        /// </summary>
        /// <param name="forumSubscription">Forum subscription</param>
        void DeleteSubscription(ForumSubscription forumSubscription);

        /// <summary>
        /// Inserts a forum subscription
        /// </summary>
        /// <param name="forumSubscription">Forum subscription</param>
        void InsertSubscription(ForumSubscription forumSubscription);

        /// <summary>
        /// Updates the forum subscription
        /// </summary>
        /// <param name="forumSubscription">Forum subscription</param>
        void UpdateSubscription(ForumSubscription forumSubscription);

        #endregion

        #region Customer

        /// <summary>
        /// Check whether customer is allowed to create new topics
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="forum">Forum</param>
        /// <returns>True if allowed, otherwise false</returns>
        bool IsCustomerAllowedToCreateTopic(Customer customer, Forum forum);

        /// <summary>
        /// Check whether customer is allowed to edit topic
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        bool IsCustomerAllowedToEditTopic(Customer customer, ForumTopic topic);

        /// <summary>
        /// Check whether customer is allowed to move topic
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        bool IsCustomerAllowedToMoveTopic(Customer customer, ForumTopic topic);

        /// <summary>
        /// Check whether customer is allowed to delete topic
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        bool IsCustomerAllowedToDeleteTopic(Customer customer, ForumTopic topic);

        /// <summary>
        /// Check whether customer is allowed to create new post
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="topic">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        bool IsCustomerAllowedToCreatePost(Customer customer, ForumTopic topic);

        /// <summary>
        /// Check whether customer is allowed to edit post
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="post">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        bool IsCustomerAllowedToEditPost(Customer customer, ForumPost post);

        /// <summary>
        /// Check whether customer is allowed to delete post
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <param name="post">Topic</param>
        /// <returns>True if allowed, otherwise false</returns>
        bool IsCustomerAllowedToDeletePost(Customer customer, ForumPost post);

        /// <summary>
        /// Check whether customer is allowed to watch topics
        /// </summary>
        /// <param name="customer">Customer</param>
        /// <returns>True if allowed, otherwise false</returns>
        bool IsCustomerAllowedToSubscribe(Customer customer);

        #endregion
    }
}
