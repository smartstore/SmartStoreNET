using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Domain.Topics;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Topics
{
    public partial class TopicService : ITopicService
    {
		private readonly IRepository<Topic> _topicRepository;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly IStoreMappingService _storeMappingService;
		private readonly IEventPublisher _eventPublisher;

		public TopicService(
			IRepository<Topic> topicRepository,
			IRepository<StoreMapping> storeMappingRepository,
			IStoreMappingService storeMappingService,
			IEventPublisher eventPublisher)
        {
            _topicRepository = topicRepository;
			_storeMappingRepository = storeMappingRepository;
			_storeMappingService = storeMappingService;
			_eventPublisher = eventPublisher;

			this.QuerySettings = DbQuerySettings.Default;
		}

		public DbQuerySettings QuerySettings { get; set; }

        public virtual void DeleteTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Delete(topic);

			//event notification
			_eventPublisher.EntityDeleted(topic);
        }

        public virtual Topic GetTopicById(int topicId)
        {
            if (topicId == 0)
                return null;

            return _topicRepository.GetById(topicId);
        }

		public virtual Topic GetTopicBySystemName(string systemName, int storeId = 0)
        {
<<<<<<< HEAD
            if (String.IsNullOrEmpty(systemName))
                return null;
=======
			if (systemName.IsEmpty())
				return null;
>>>>>>> upstream/3.x

			var topic = _topicRepository.Table
				.Where(x => x.SystemName == systemName)
				.OrderBy(x => x.Id)
				.FirstOrDefaultCached("db.topic.bysysname-" + systemName);

			if (storeId > 0 && topic != null && !_storeMappingService.Authorize(topic))
			{
				topic = null;
			}

			return topic;
        }

		public virtual IList<Topic> GetAllTopics(int storeId = 0)
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

			//event notification
			_eventPublisher.EntityInserted(topic);
        }

        public virtual void UpdateTopic(Topic topic)
        {
            if (topic == null)
                throw new ArgumentNullException("topic");

            _topicRepository.Update(topic);

			//event notification
			_eventPublisher.EntityUpdated(topic);
        }
    }
}
