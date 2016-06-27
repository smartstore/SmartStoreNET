using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Caching;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Events;

namespace SmartStore.Services.Topics
{
    /// <summary>
    /// Topic service
    /// </summary>
    public partial class TopicService : ITopicService
    {
		private const string TOPICS_ALL_KEY = "SmartStore.topics.all-{0}";
		private const string TOPICS_PATTERN_KEY = "SmartStore.topics.";

		#region Fields

		private readonly IRepository<Topic> _topicRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IEventPublisher _eventPublisher;
		private readonly IRequestCache _requestCache;

		#endregion

		#region Ctor

		public TopicService(
			IRepository<Topic> topicRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IEventPublisher eventPublisher,
			IRequestCache requestCache)
        {
            _topicRepository = topicRepository;
			_storeMappingRepository = storeMappingRepository;
            _eventPublisher = eventPublisher;
			_requestCache = requestCache;

			this.QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Deletes a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void DeleteTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Delete(topic);

			_requestCache.RemoveByPattern(TOPICS_PATTERN_KEY);

			//event notification
			_eventPublisher.EntityDeleted(topic);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="topicId">The topic identifier</param>
        /// <returns>Topic</returns>
        public virtual Topic GetTopicById(int topicId)
        {
            if (topicId == 0)
                return null;

            return _topicRepository.GetById(topicId);
        }

        /// <summary>
        /// Gets a topic
        /// </summary>
        /// <param name="systemName">The topic system name</param>
		/// <param name="storeId">Store identifier</param>
        /// <returns>Topic</returns>
		public virtual Topic GetTopicBySystemName(string systemName, int storeId)
        {
            if (String.IsNullOrEmpty(systemName))
                return null;

			var allTopics = GetAllTopics(storeId);

			var topic = allTopics
				.OrderBy(x => x.Id)
				.FirstOrDefault(x => x.SystemName.IsCaseInsensitiveEqual(systemName));

			return topic;
        }

        /// <summary>
        /// Gets all topics
        /// </summary>
		/// <param name="storeId">Store identifier; pass 0 to load all records</param>
        /// <returns>Topics</returns>
		public virtual IList<Topic> GetAllTopics(int storeId)
        {
			var result = _requestCache.Get(TOPICS_ALL_KEY.FormatInvariant(storeId), () =>
			{
				var query = _topicRepository.Table;

				//Store mapping
				if (storeId > 0 && !QuerySettings.IgnoreMultiStore)
				{
					query = from t in query
							join sm in _storeMappingRepository.Table
							on new { c1 = t.Id, c2 = "Topic" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into t_sm
							from sm in t_sm.DefaultIfEmpty()
							where !t.LimitedToStores || storeId == sm.StoreId
							select t;

					//only distinct items (group by ID)
					query = from t in query
							group t by t.Id into tGroup
							orderby tGroup.Key
							select tGroup.FirstOrDefault();
				}

				query = query.OrderBy(t => t.Priority).ThenBy(t => t.SystemName);

				return query.ToList();
			});

			return result;
        }

        /// <summary>
        /// Inserts a topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void InsertTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Insert(topic);

			_requestCache.RemoveByPattern(TOPICS_PATTERN_KEY);

			//event notification
			_eventPublisher.EntityInserted(topic);
        }

        /// <summary>
        /// Updates the topic
        /// </summary>
        /// <param name="topic">Topic</param>
        public virtual void UpdateTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Update(topic);

			_requestCache.RemoveByPattern(TOPICS_PATTERN_KEY);

			//event notification
			_eventPublisher.EntityUpdated(topic);
        }

        #endregion
    }
}
