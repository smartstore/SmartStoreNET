using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Domain.Topics;

namespace SmartStore.Services.Topics
{
    /// <summary>
    /// Topic service interface
    /// </summary>
    public partial interface ITopicService
    {
        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        void DeleteTopic(Topic topic);

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="topicId">The topic identifier</param>
        /// <returns>Topic</returns>
        Topic GetTopicById(int topicId);

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="systemName">The topic system name</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="checkPermission">Whether to check for permission (ACL). If true and check fails, null is returned.</param>
        /// <returns>Topic</returns>
        Topic GetTopicBySystemName(string systemName, int storeId = 0, bool checkPermission = true);

        /// <summary>
        /// Gets all topics
        /// </summary>
        /// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <param name="showHidden">Whether to load hidden records</param>
        /// <returns>Topics</returns>
        IPagedList<Topic> GetAllTopics(int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        void InsertTopic(Topic topic);

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="topic">Topic</param>
        void UpdateTopic(Topic topic);
    }
}
