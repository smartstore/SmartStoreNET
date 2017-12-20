using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Events;
using SmartStore.Data.Caching;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Messages;

namespace SmartStore.Services.Forums
{
    public partial class ForumService : IForumService
    {
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<Forum> _forumRepository;
        private readonly IRepository<ForumTopic> _forumTopicRepository;
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly IRepository<PrivateMessage> _forumPrivateMessageRepository;
        private readonly IRepository<ForumSubscription> _forumSubscriptionRepository;
        private readonly ForumSettings _forumSettings;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
		private readonly IRepository<StoreMapping> _storeMappingRepository;
		private readonly ICommonServices _services;

        public ForumService(
            IRepository<ForumGroup> forumGroupRepository,
            IRepository<Forum> forumRepository,
            IRepository<ForumTopic> forumTopicRepository,
            IRepository<ForumPost> forumPostRepository,
            IRepository<PrivateMessage> forumPrivateMessageRepository,
            IRepository<ForumSubscription> forumSubscriptionRepository,
            ForumSettings forumSettings,
            IRepository<Customer> customerRepository,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
			IRepository<StoreMapping> storeMappingRepository,
			ICommonServices services)
        {
            _forumGroupRepository = forumGroupRepository;
            _forumRepository = forumRepository;
            _forumTopicRepository = forumTopicRepository;
            _forumPostRepository = forumPostRepository;
            _forumPrivateMessageRepository = forumPrivateMessageRepository;
            _forumSubscriptionRepository = forumSubscriptionRepository;
            _forumSettings = forumSettings;
            _customerRepository = customerRepository;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
			_storeMappingRepository = storeMappingRepository;
			_services = services;
        }

		public DbQuerySettings QuerySettings { get; set; }

        private void UpdateForumStats(int forumId)
        {
            if (forumId == 0)
            {
                return;
            }
            var forum = GetForumById(forumId);
            if (forum == null)
            {
                return;
            }

            //number of topics
            var queryNumTopics = from ft in _forumTopicRepository.Table
                                 where ft.ForumId == forumId
                                 select ft.Id;
            int numTopics = queryNumTopics.Count();

            //number of posts
            var queryNumPosts = from ft in _forumTopicRepository.Table
                                join fp in _forumPostRepository.Table on ft.Id equals fp.TopicId
                                where ft.ForumId == forumId
                                select fp.Id;
            int numPosts = queryNumPosts.Count();

            //last values
            int lastTopicId = 0;
            int lastPostId = 0;
            int lastPostCustomerId = 0;
            DateTime? lastPostTime = null;
            var queryLastValues = from ft in _forumTopicRepository.Table
                                  join fp in _forumPostRepository.Table on ft.Id equals fp.TopicId
                                  where ft.ForumId == forumId
                                  orderby fp.CreatedOnUtc descending, ft.CreatedOnUtc descending
                                  select new
                                  {
                                      LastTopicId = ft.Id,
                                      LastPostId = fp.Id,
                                      LastPostCustomerId = fp.CustomerId,
                                      LastPostTime = fp.CreatedOnUtc
                                  };
            var lastValues = queryLastValues.FirstOrDefault();
            if (lastValues != null)
            {
                lastTopicId = lastValues.LastTopicId;
                lastPostId = lastValues.LastPostId;
                lastPostCustomerId = lastValues.LastPostCustomerId;
                lastPostTime = lastValues.LastPostTime;
            }

            //update forum
            forum.NumTopics = numTopics;
            forum.NumPosts = numPosts;
            forum.LastTopicId = lastTopicId;
            forum.LastPostId = lastPostId;
            forum.LastPostCustomerId = lastPostCustomerId;
            forum.LastPostTime = lastPostTime;
            UpdateForum(forum);
        }

        private void UpdateForumTopicStats(int forumTopicId)
        {
            if (forumTopicId == 0)
            {
                return;
            }
            var forumTopic = GetTopicById(forumTopicId);
            if (forumTopic == null)
            {
                return;
            }

            //number of posts
            var queryNumPosts = from fp in _forumPostRepository.Table
                                where fp.TopicId == forumTopicId
                                select fp.Id;
            int numPosts = queryNumPosts.Count();

            //last values
            int lastPostId = 0;
            int lastPostCustomerId = 0;
            DateTime? lastPostTime = null;
            var queryLastValues = from fp in _forumPostRepository.Table
                                  where fp.TopicId == forumTopicId
                                  orderby fp.CreatedOnUtc descending
                                  select new
                                  {
                                      LastPostId = fp.Id,
                                      LastPostCustomerId = fp.CustomerId,
                                      LastPostTime = fp.CreatedOnUtc
                                  };
            var lastValues = queryLastValues.FirstOrDefault();
            if (lastValues != null)
            {
                lastPostId = lastValues.LastPostId;
                lastPostCustomerId = lastValues.LastPostCustomerId;
                lastPostTime = lastValues.LastPostTime;
            }

            //update topic
            forumTopic.NumPosts = numPosts;
            forumTopic.LastPostId = lastPostId;
            forumTopic.LastPostCustomerId = lastPostCustomerId;
            forumTopic.LastPostTime = lastPostTime;
            UpdateTopic(forumTopic);
        }

        private void UpdateCustomerStats(int customerId)
        {
            if (customerId == 0)
            {
                return;
            }

            var customer = _customerService.GetCustomerById(customerId);

            if (customer == null)
            {
                return;
            }

            var query = from fp in _forumPostRepository.Table
                        where fp.CustomerId == customerId
                        select fp.Id;
            int numPosts = query.Count();

            _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ForumPostCount, numPosts);
        }

        private bool IsForumModerator(Customer customer)
        {
            if (customer.IsForumModerator())
            {
                return true;
            }
            return false;
        }

        public virtual void DeleteForumGroup(ForumGroup forumGroup)
        {
            if (forumGroup == null)
            {
                throw new ArgumentNullException("forumGroup");
            }

            _forumGroupRepository.Delete(forumGroup);
        }

        public virtual ForumGroup GetForumGroupById(int forumGroupId)
        {
            if (forumGroupId == 0)
                return null;

            return _forumGroupRepository.GetById(forumGroupId);
        }

		public virtual IList<ForumGroup> GetAllForumGroups(bool showHidden = false)
        {
			var query = _forumGroupRepository.Table;

			if (!showHidden && !QuerySettings.IgnoreMultiStore)
			{
				var currentStoreId = _services.StoreContext.CurrentStore.Id;

				query =
					from fg in query
					join sm in _storeMappingRepository.Table on new { c1 = fg.Id, c2 = "ForumGroup" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into fg_sm
					from sm in fg_sm.DefaultIfEmpty()
					where !fg.LimitedToStores || currentStoreId == sm.StoreId
					select fg;

				query =
					from fg in query
					group fg by fg.Id into fgGroup
					orderby fgGroup.Key
					select fgGroup.FirstOrDefault();
			}

			query = query.OrderBy(m => m.DisplayOrder);

			return query.ToListCached();
		}

        public virtual void InsertForumGroup(ForumGroup forumGroup)
        {
            if (forumGroup == null)
            {
                throw new ArgumentNullException("forumGroup");
            }

            _forumGroupRepository.Insert(forumGroup);
        }

        public virtual void UpdateForumGroup(ForumGroup forumGroup)
        {
            if (forumGroup == null)
            {
                throw new ArgumentNullException("forumGroup");
            }

            _forumGroupRepository.Update(forumGroup);
        }

        public virtual void DeleteForum(Forum forum)
        {            
            if (forum == null)
            {
                throw new ArgumentNullException("forum");
            }

            //delete forum subscriptions (topics)
            var queryTopicIds = from ft in _forumTopicRepository.Table
                           where ft.ForumId == forum.Id
                           select ft.Id;
            var queryFs1 = from fs in _forumSubscriptionRepository.Table
                           where queryTopicIds.Contains(fs.TopicId)
                           select fs;
            foreach (var fs in queryFs1.ToList())
            {
                _forumSubscriptionRepository.Delete(fs);
            }

            //delete forum subscriptions (forum)
            var queryFs2 = from fs in _forumSubscriptionRepository.Table
                           where fs.ForumId == forum.Id
                           select fs;
            foreach (var fs2 in queryFs2.ToList())
            {
                _forumSubscriptionRepository.Delete(fs2);
            }

            //delete forum
            _forumRepository.Delete(forum);
        }

        public virtual Forum GetForumById(int forumId)
        {
            if (forumId == 0)
                return null;

            return _forumRepository.GetById(forumId);
        }

        public virtual IList<Forum> GetAllForumsByGroupId(int forumGroupId)
        {
			var query = from f in _forumRepository.Table
						orderby f.DisplayOrder
						where f.ForumGroupId == forumGroupId
						select f;

			var forums = query.ToListCached();
			return forums;
		}

        public virtual void InsertForum(Forum forum)
        {
            if (forum == null)
            {
                throw new ArgumentNullException("forum");
            }

            _forumRepository.Insert(forum);
        }

        public virtual void UpdateForum(Forum forum)
        {
            if (forum == null)
            {
                throw new ArgumentNullException("forum");
            }

			_forumRepository.Update(forum);
        }

        public virtual void DeleteTopic(ForumTopic forumTopic)
        {            
            if (forumTopic == null)
            {                
                throw new ArgumentNullException("forumTopic");
            }                

            int customerId = forumTopic.CustomerId;
            int forumId = forumTopic.ForumId;

            //delete topic
            _forumTopicRepository.Delete(forumTopic);

            //delete forum subscriptions
            var queryFs = from ft in _forumSubscriptionRepository.Table
                          where ft.TopicId == forumTopic.Id
                          select ft;
            var forumSubscriptions = queryFs.ToList();
            foreach (var fs in forumSubscriptions)
            {
                _forumSubscriptionRepository.Delete(fs);
            }

            //update stats
            UpdateForumStats(forumId);
            UpdateCustomerStats(customerId);
        }

        public virtual ForumTopic GetTopicById(int forumTopicId)
        {
            return GetTopicById(forumTopicId, false);
        }

        public virtual ForumTopic GetTopicById(int forumTopicId, bool increaseViews)
        {
            if (forumTopicId == 0)
            {
                return null;
            }

            var query = from ft in _forumTopicRepository.Table
                        where ft.Id == forumTopicId
                        select ft;
            var forumTopic = query.SingleOrDefault();
            if (forumTopic == null)
            {
                return null;
            }

            if (increaseViews)
            {
                forumTopic.Views = ++forumTopic.Views;
                UpdateTopic(forumTopic);
            }

            return forumTopic;
        }

        public virtual IPagedList<ForumTopic> GetAllTopics(int forumId, int customerId, string keywords, ForumSearchType searchType, int limitDays, int pageIndex, int pageSize)
        {
            DateTime? limitDate = null;

            if (limitDays > 0)
            {
                limitDate = DateTime.UtcNow.AddDays(-limitDays);
            }

            var query1 = 
				from ft in _forumTopicRepository.Table
				join fp in _forumPostRepository.Table on ft.Id equals fp.TopicId
				where
					(forumId == 0 || ft.ForumId == forumId) &&
					(customerId == 0 || ft.CustomerId == customerId) &&
					(!limitDate.HasValue || limitDate.Value <= ft.LastPostTime) &&
					(
						((searchType == ForumSearchType.All || searchType == ForumSearchType.TopicTitlesOnly) && ft.Subject.Contains(keywords)) ||
						((searchType == ForumSearchType.All || searchType == ForumSearchType.PostTextOnly) && fp.Text.Contains(keywords))
					)							
				select ft.Id;

            var query2 = 
				from ft in _forumTopicRepository.Table
				where query1.Contains(ft.Id)
				orderby ft.TopicTypeId descending, ft.LastPostTime descending, ft.Id descending
				select ft;

            var topics = new PagedList<ForumTopic>(query2, pageIndex, pageSize);
            return topics;
        }

        public virtual IList<ForumTopic> GetActiveTopics(int forumId, int count)
        {
            var query =
				from ft in _forumTopicRepository.Table
				where (forumId == 0 || ft.ForumId == forumId) && (ft.LastPostTime.HasValue)
				select ft;

			if (!QuerySettings.IgnoreMultiStore)
			{
				var currentStoreId = _services.StoreContext.CurrentStore.Id;

				query =
					from ft in query
					join ff in _forumRepository.Table on ft.ForumId equals ff.Id
					join fg in _forumGroupRepository.Table on ff.ForumGroupId equals fg.Id
					join sm in _storeMappingRepository.Table on new { c1 = fg.Id, c2 = "ForumGroup" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into fg_sm
					from sm in fg_sm.DefaultIfEmpty()
					where !fg.LimitedToStores || currentStoreId == sm.StoreId
					select ft;

				query =
					from ft in query
					group ft by ft.Id into ftGroup
					orderby ftGroup.Key
					select ftGroup.FirstOrDefault();
			}

			query = query.OrderByDescending(x => x.LastPostTime);

            var forumTopics = query.Take(count).ToList();
            return forumTopics;
        }

        public virtual void InsertTopic(ForumTopic forumTopic, bool sendNotifications)
        {
            if (forumTopic == null)
            {
                throw new ArgumentNullException("forumTopic");
            }

            _forumTopicRepository.Insert(forumTopic);

            //update stats
            UpdateForumStats(forumTopic.ForumId);

            //send notifications
            if (sendNotifications)
            {
                var forum = forumTopic.Forum;
                var subscriptions = GetAllSubscriptions(0, forum.Id, 0, 0, int.MaxValue);
                var languageId = _services.WorkContext.WorkingLanguage.Id;

                foreach (var subscription in subscriptions)
                {
                    if (subscription.CustomerId == forumTopic.CustomerId)
                    {
                        continue;
                    }

                    if (!String.IsNullOrEmpty(subscription.Customer.Email))
                    {
                        _services.MessageFactory.SendNewForumTopicMessage(subscription.Customer, forumTopic, languageId);	
                    }
                }
            }
        }

        public virtual void UpdateTopic(ForumTopic forumTopic)
        {
            if (forumTopic == null)
            {
                throw new ArgumentNullException("forumTopic");
            }

            _forumTopicRepository.Update(forumTopic);
        }

        public virtual ForumTopic MoveTopic(int forumTopicId, int newForumId)
        {
            var forumTopic = GetTopicById(forumTopicId);
            if (forumTopic == null)
                return forumTopic;

            if (this.IsCustomerAllowedToMoveTopic(_services.WorkContext.CurrentCustomer, forumTopic))
            {
                int previousForumId = forumTopic.ForumId;
                var newForum = GetForumById(newForumId);

                if (newForum != null)
                {
                    if (previousForumId != newForumId)
                    {
                        forumTopic.ForumId = newForum.Id;
                        UpdateTopic(forumTopic);

                        //update forum stats
                        UpdateForumStats(previousForumId);
                        UpdateForumStats(newForumId);
                    }
                }
            }
            return forumTopic;
        }

        public virtual void DeletePost(ForumPost forumPost)
        {            
            if (forumPost == null)
            {
                throw new ArgumentNullException("forumPost");
            }

            int forumTopicId = forumPost.TopicId;
            int customerId = forumPost.CustomerId;
            var forumTopic = this.GetTopicById(forumTopicId);
            int forumId = forumTopic.ForumId;

            //delete topic if it was the first post
            bool deleteTopic = false;
            var firstPost = forumTopic.GetFirstPost(this);
            if (firstPost != null && firstPost.Id == forumPost.Id)
            {
                deleteTopic = true;
            }

            //delete forum post
            _forumPostRepository.Delete(forumPost);

            //delete topic
            if (deleteTopic)
            {
                DeleteTopic(forumTopic);
            }

            //update stats
            if (!deleteTopic)
            {
                UpdateForumTopicStats(forumTopicId);
            }
            UpdateForumStats(forumId);
            UpdateCustomerStats(customerId);

        }

        public virtual ForumPost GetPostById(int forumPostId)
        {
            if (forumPostId == 0)
            {
                return null;
            }

            var query = from fp in _forumPostRepository.Table
                        where fp.Id == forumPostId
                        select fp;
            var forumPost = query.SingleOrDefault();

            return forumPost;
        }

        public virtual IPagedList<ForumPost> GetAllPosts(int forumTopicId, int customerId, string keywords, int pageIndex, int pageSize)
        {
            return GetAllPosts(forumTopicId, customerId, keywords, true, pageIndex, pageSize);
        }

        public virtual IPagedList<ForumPost> GetAllPosts(int forumTopicId, int customerId, string keywords, bool ascSort, int pageIndex, int pageSize)
        {
            var query = _forumPostRepository.Table;
            if (forumTopicId > 0)
            {
                query = query.Where(fp => forumTopicId == fp.TopicId);
            }
            if (customerId > 0)
            {
                query = query.Where(fp => customerId == fp.CustomerId);
            }
            if (!String.IsNullOrEmpty(keywords))
            {
                query = query.Where(fp => fp.Text.Contains(keywords));
            }
            if (ascSort)
            {
                query = query.OrderBy(fp => fp.CreatedOnUtc).ThenBy(fp => fp.Id);
            }
            else
            {
                query = query.OrderByDescending(fp => fp.CreatedOnUtc).ThenBy(fp => fp.Id);
            }

            var forumPosts = new PagedList<ForumPost>(query, pageIndex, pageSize);

            return forumPosts;
        }

        public virtual void InsertPost(ForumPost forumPost, bool sendNotifications)
        {
            if (forumPost == null)
            {
                throw new ArgumentNullException("forumPost");
            }

            _forumPostRepository.Insert(forumPost);

            //update stats
            int customerId = forumPost.CustomerId;
            var forumTopic = this.GetTopicById(forumPost.TopicId);
            int forumId = forumTopic.ForumId;
            UpdateForumTopicStats(forumPost.TopicId);
            UpdateForumStats(forumId);
            UpdateCustomerStats(customerId);

            //notifications
            if (sendNotifications)
            {
                var forum = forumTopic.Forum;
                var subscriptions = GetAllSubscriptions(0, 0, forumTopic.Id, 0, int.MaxValue);

                var languageId = _services.WorkContext.WorkingLanguage.Id;

                int friendlyTopicPageIndex = CalculateTopicPageIndex(forumPost.TopicId, _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10, forumPost.Id) + 1;

                foreach (ForumSubscription subscription in subscriptions)
                {
                    if (subscription.CustomerId == forumPost.CustomerId)
                    {
                        continue;
                    }

                    if (!String.IsNullOrEmpty(subscription.Customer.Email))
                    {
                        _services.MessageFactory.SendNewForumPostMessage(subscription.Customer, forumPost, friendlyTopicPageIndex, languageId);
                    }
                }
            }
        }

        public virtual void UpdatePost(ForumPost forumPost)
        {
            //validation
            if (forumPost == null)
            {
                throw new ArgumentNullException("forumPost");
            }

            _forumPostRepository.Update(forumPost);
        }

        public virtual void DeletePrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException("privateMessage");
            }

            _forumPrivateMessageRepository.Delete(privateMessage);
        }

        public virtual PrivateMessage GetPrivateMessageById(int privateMessageId)
        {
            if (privateMessageId == 0)
            {
                return null;
            }

            var query = from pm in _forumPrivateMessageRepository.Table
                        where pm.Id == privateMessageId
                        select pm;
            var privateMessage = query.SingleOrDefault();

            return privateMessage;
        }

		public virtual IPagedList<PrivateMessage> GetAllPrivateMessages(
			int storeId, 
			int fromCustomerId,
            int toCustomerId, 
			bool? isRead, 
			bool? isDeletedByAuthor, 
			bool? isDeletedByRecipient,
            string keywords, 
			int pageIndex, 
			int pageSize)
        {
			var query = _forumPrivateMessageRepository.Table;

			if (storeId > 0)
				query = query.Where(pm => storeId == pm.StoreId);
			if (fromCustomerId > 0)
				query = query.Where(pm => fromCustomerId == pm.FromCustomerId);
			if (toCustomerId > 0)
				query = query.Where(pm => toCustomerId == pm.ToCustomerId);
			if (isRead.HasValue)
				query = query.Where(pm => isRead.Value == pm.IsRead);
			if (isDeletedByAuthor.HasValue)
				query = query.Where(pm => isDeletedByAuthor.Value == pm.IsDeletedByAuthor);
			if (isDeletedByRecipient.HasValue)
				query = query.Where(pm => isDeletedByRecipient.Value == pm.IsDeletedByRecipient);

			if (!String.IsNullOrEmpty(keywords))
			{
				query = query.Where(pm => pm.Subject.Contains(keywords));
				query = query.Where(pm => pm.Text.Contains(keywords));
			}

			query = query.OrderByDescending(pm => pm.CreatedOnUtc);

			var privateMessages = new PagedList<PrivateMessage>(query, pageIndex, pageSize);

            return privateMessages;
        }

        public virtual void InsertPrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
            {
                throw new ArgumentNullException("privateMessage");
            }

            _forumPrivateMessageRepository.Insert(privateMessage);

            var customerTo = _customerService.GetCustomerById(privateMessage.ToCustomerId);
            if (customerTo == null)
            {
                throw new SmartException("Recipient could not be loaded");
            }

			//UI notification
			_genericAttributeService.SaveAttribute(customerTo, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, false, privateMessage.StoreId);

            //Email notification
            if (_forumSettings.NotifyAboutPrivateMessages)
            {
				_services.MessageFactory.SendPrivateMessageNotification(customerTo, privateMessage, _services.WorkContext.WorkingLanguage.Id);                
            }
        }

        public virtual void UpdatePrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage == null)
                throw new ArgumentNullException("privateMessage");

            if (privateMessage.IsDeletedByAuthor && privateMessage.IsDeletedByRecipient)
            {
                _forumPrivateMessageRepository.Delete(privateMessage);
            }
            else
            {
                _forumPrivateMessageRepository.Update(privateMessage);
            }
        }

        public virtual void DeleteSubscription(ForumSubscription forumSubscription)
        {
            if (forumSubscription == null)
            {
                throw new ArgumentNullException("forumSubscription");
            }

            _forumSubscriptionRepository.Delete(forumSubscription);
        }

        public virtual ForumSubscription GetSubscriptionById(int forumSubscriptionId)
        {
            if (forumSubscriptionId == 0)
            {
                return null;
            }

            var query = from fs in _forumSubscriptionRepository.Table
                        where fs.Id == forumSubscriptionId
                        select fs;
            var forumSubscription = query.SingleOrDefault();

            return forumSubscription;
        }

        public virtual IPagedList<ForumSubscription> GetAllSubscriptions(int customerId, int forumId, int topicId, int pageIndex, int pageSize)
        {
            var fsQuery = 
				from fs in _forumSubscriptionRepository.Table
				join c in _customerRepository.Table on fs.CustomerId equals c.Id
				where
					(customerId == 0 || fs.CustomerId == customerId) &&
					(forumId == 0 || fs.ForumId == forumId) &&
					(topicId == 0 || fs.TopicId == topicId) &&
					(c.Active && !c.Deleted)
				select fs.SubscriptionGuid;

            var query = 
				from fs in _forumSubscriptionRepository.Table
				where fsQuery.Contains(fs.SubscriptionGuid)
				orderby fs.CreatedOnUtc descending, fs.SubscriptionGuid descending
				select fs;

            var forumSubscriptions = new PagedList<ForumSubscription>(query, pageIndex, pageSize);
            return forumSubscriptions;
        }

        public virtual void InsertSubscription(ForumSubscription forumSubscription)
        {
            if (forumSubscription == null)
            {
                throw new ArgumentNullException("forumSubscription");
            }

            _forumSubscriptionRepository.Insert(forumSubscription);
        }

        public virtual void UpdateSubscription(ForumSubscription forumSubscription)
        {
            if (forumSubscription == null)
            {
                throw new ArgumentNullException("forumSubscription");
            }
            
            _forumSubscriptionRepository.Update(forumSubscription);
        }

        public virtual bool IsCustomerAllowedToCreateTopic(Customer customer, Forum forum)
        {
            if (forum == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest() && !_forumSettings.AllowGuestsToCreateTopics)
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            return true;
        }

        public virtual bool IsCustomerAllowedToEditTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToEditPosts)
            {
                bool ownTopic = customer.Id == topic.CustomerId;
                return ownTopic;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToMoveTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToDeleteTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToDeletePosts)
            {
                bool ownTopic = customer.Id == topic.CustomerId;
                return ownTopic;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToCreatePost(Customer customer, ForumTopic topic)
        {
            if (topic == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest() && !_forumSettings.AllowGuestsToCreatePosts)
            {
                return false;
            }

            return true;
        }

        public virtual bool IsCustomerAllowedToEditPost(Customer customer, ForumPost post)
        {
            if (post == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToEditPosts)
            {
                bool ownPost = customer.Id == post.CustomerId;
                return ownPost;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToDeletePost(Customer customer, ForumPost post)
        {
            if (post == null)
            {
                return false;
            }

            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToDeletePosts)
            {
                bool ownPost = customer.Id == post.CustomerId;
                return ownPost;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToSetTopicPriority(Customer customer)
        {
            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (IsForumModerator(customer))
            {
                return true;
            }            

            return false;
        }

        public virtual bool IsCustomerAllowedToSubscribe(Customer customer)
        {
            if (customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            return true;
        }

        public virtual int CalculateTopicPageIndex(int forumTopicId, int pageSize, int postId)
        {
            int pageIndex = 0;
            var forumPosts = GetAllPosts(forumTopicId, 0, string.Empty, true, 0, int.MaxValue);

            for (int i = 0; i < forumPosts.TotalCount; i++)
            {
                if (forumPosts[i].Id == postId)
                {
                    if (pageSize > 0)
                    {
                        pageIndex = i / pageSize;
                    }
                }
            }

            return pageIndex;
        }
    }
}
