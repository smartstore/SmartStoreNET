using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;

namespace SmartStore.Services.Topics
{
    public partial class TopicService : ITopicService
    {
		private readonly IRepository<Topic> _topicRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IEventPublisher _eventPublisher;

		public TopicService(
			IRepository<Topic> topicRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IEventPublisher eventPublisher)
        {
            _topicRepository = topicRepository;
			_storeMappingRepository = storeMappingRepository;
            _eventPublisher = eventPublisher;

			this.QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

        public virtual void DeleteTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Delete(topic);
        }

        public virtual Topic GetTopicById(int topicId)
        {
            if (topicId == 0)
                return null;

            return _topicRepository.GetById(topicId);
        }

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

		public virtual IList<Topic> GetAllTopics(int storeId)
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

			return query.ToListCached("db.topic.all-" + storeId);
		}

        public virtual void InsertTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Insert(topic);
        }

        public virtual void UpdateTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Update(topic);
        }
    }
}
