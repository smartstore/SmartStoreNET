using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data.Caching;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;

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
            var forum = GetForumById(forumId);
            if (forum == null)
            {
                return;
            }

            var queryLastValues = 
                from ft in _forumTopicRepository.TableUntracked
                join fp in _forumPostRepository.TableUntracked on ft.Id equals fp.TopicId
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

            forum.LastTopicId = lastValues?.LastTopicId ?? 0;
            forum.LastPostId = lastValues?.LastPostId ?? 0;
            forum.LastPostCustomerId = lastValues?.LastPostCustomerId ?? 0;
            forum.LastPostTime = lastValues?.LastPostTime;
            forum.NumTopics = _forumTopicRepository.Table.Where(x => x.ForumId == forumId).Count();
            forum.NumPosts = (
                from ft in _forumTopicRepository.Table
                join fp in _forumPostRepository.Table on ft.Id equals fp.TopicId
                where ft.ForumId == forumId
                select fp.Id).Count();

            UpdateForum(forum);
        }

        private void UpdateForumTopicStats(int forumTopicId)
        {
            var forumTopic = GetTopicById(forumTopicId);
            if (forumTopic == null)
            {
                return;
            }

            var queryLastValues = 
                from fp in _forumPostRepository.TableUntracked
                where fp.TopicId == forumTopicId
                orderby fp.CreatedOnUtc descending
                select new
                {
                    LastPostId = fp.Id,
                    LastPostCustomerId = fp.CustomerId,
                    LastPostTime = fp.CreatedOnUtc
                };
            var lastValues = queryLastValues.FirstOrDefault();

            forumTopic.LastPostId = lastValues?.LastPostId ?? 0;
            forumTopic.LastPostCustomerId = lastValues?.LastPostCustomerId ?? 0;
            forumTopic.LastPostTime = lastValues?.LastPostTime;
            forumTopic.NumPosts = _forumPostRepository.Table.Where(x => x.TopicId == forumTopicId).Count();

            UpdateTopic(forumTopic);
        }

        private void UpdateCustomerStats(int customerId)
        {
            if (customerId != 0)
            {
                var customer = _customerService.GetCustomerById(customerId);
                if (customer != null)
                {
                    var numPosts = _forumPostRepository.Table.Where(x => x.CustomerId == customerId).Count();

                    _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ForumPostCount, numPosts);
                }
            }
        }

        #region Group

        public virtual ForumGroup GetForumGroupById(int forumGroupId)
        {
            if (forumGroupId == 0)
            {
                return null;
            }

            return _forumGroupRepository.GetById(forumGroupId);
        }

        public virtual IList<ForumGroup> GetAllForumGroups(int storeId = 0)
        {
            var query = _forumGroupRepository.Table.Expand(x => x.Forums);

            if (!QuerySettings.IgnoreMultiStore && storeId > 0)
            {
                query =
                    from fg in query
                    join sm in _storeMappingRepository.Table on new { c1 = fg.Id, c2 = "ForumGroup" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into fg_sm
                    from sm in fg_sm.DefaultIfEmpty()
                    where !fg.LimitedToStores || storeId == sm.StoreId
                    select fg;

                query =
                    from fg in query
                    group fg by fg.Id into fgGroup
                    orderby fgGroup.Key
                    select fgGroup.FirstOrDefault();
            }

            query = query.OrderBy(x => x.DisplayOrder);

            return query.ToListCached();
        }

        public virtual void InsertForumGroup(ForumGroup forumGroup)
        {
            Guard.NotNull(forumGroup, nameof(forumGroup));

            _forumGroupRepository.Insert(forumGroup);
        }

        public virtual void UpdateForumGroup(ForumGroup forumGroup)
        {
            Guard.NotNull(forumGroup, nameof(forumGroup));

            _forumGroupRepository.Update(forumGroup);
        }

        public virtual void DeleteForumGroup(ForumGroup forumGroup)
        {
            if (forumGroup != null)
            {
                _forumGroupRepository.Delete(forumGroup);
            }            
        }

        #endregion

        #region Forum

        public virtual Forum GetForumById(int forumId)
        {
            if (forumId == 0)
            {
                return null;
            }

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
            Guard.NotNull(forum, nameof(forum));

            _forumRepository.Insert(forum);
        }

        public virtual void UpdateForum(Forum forum)
        {
            Guard.NotNull(forum, nameof(forum));

            _forumRepository.Update(forum);
        }

        public virtual void DeleteForum(Forum forum)
        {
            if (forum == null)
            {
                return;
            }

            // Delete forum subscriptions (topics).
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

            // Delete forum subscriptions (forum).
            var queryFs2 = from fs in _forumSubscriptionRepository.Table
                           where fs.ForumId == forum.Id
                           select fs;
            foreach (var fs2 in queryFs2.ToList())
            {
                _forumSubscriptionRepository.Delete(fs2);
            }

            // Delete forum.
            _forumRepository.Delete(forum);
        }

        #endregion

        #region Topic

        public virtual ForumTopic GetTopicById(int forumTopicId)
        {
            if (forumTopicId == 0)
            {
                return null;
            }

            var entity = _forumTopicRepository.Table.FirstOrDefault(x => x.Id == forumTopicId);
            return entity;
        }

        public virtual IList<ForumTopic> GetTopicsByIds(int[] topicIds)
        {
            if (topicIds == null || topicIds.Length == 0)
            {
                return new List<ForumTopic>();
            }

            var query = _forumTopicRepository.Table
                .Where(x => topicIds.Contains(x.Id));

            return query.OrderBySequence(topicIds).ToList();
        }

        public virtual IPagedList<ForumTopic> GetAllTopics(int forumId, int pageIndex, int pageSize)
        {
            var query = _forumTopicRepository.TableUntracked;
            if (forumId != 0)
            {
                query = query.Where(x => x.ForumId == forumId);
            }

            query = query
                .OrderByDescending(x => x.TopicTypeId)
                .ThenByDescending(x => x.LastPostTime)
                .ThenByDescending(x => x.Id);

            var topics = new PagedList<ForumTopic>(query, pageIndex, pageSize);
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
            Guard.NotNull(forumTopic, nameof(forumTopic));

            _forumTopicRepository.Insert(forumTopic);

            UpdateForumStats(forumTopic.ForumId);

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

                    if (subscription.Customer.Email.HasValue())
                    {
                        _services.MessageFactory.SendNewForumTopicMessage(subscription.Customer, forumTopic, languageId);	
                    }
                }
            }
        }

        public virtual void UpdateTopic(ForumTopic forumTopic)
        {
            Guard.NotNull(forumTopic, nameof(forumTopic));

            _forumTopicRepository.Update(forumTopic);
        }

        public virtual void DeleteTopic(ForumTopic forumTopic)
        {
            if (forumTopic == null)
            {
                return;
            }

            int customerId = forumTopic.CustomerId;
            int forumId = forumTopic.ForumId;

            _forumTopicRepository.Delete(forumTopic);

            // Delete forum subscriptions.
            var queryFs = from ft in _forumSubscriptionRepository.Table
                          where ft.TopicId == forumTopic.Id
                          select ft;
            var forumSubscriptions = queryFs.ToList();
            foreach (var fs in forumSubscriptions)
            {
                _forumSubscriptionRepository.Delete(fs);
            }

            UpdateForumStats(forumId);
            UpdateCustomerStats(customerId);
        }

        public virtual ForumTopic MoveTopic(int forumTopicId, int newForumId)
        {
            var forumTopic = GetTopicById(forumTopicId);
            if (forumTopic == null)
            {
                return forumTopic;
            }

            if (IsCustomerAllowedToMoveTopic(_services.WorkContext.CurrentCustomer, forumTopic))
            {
                var previousForumId = forumTopic.ForumId;
                var newForum = GetForumById(newForumId);
                if (newForum != null && previousForumId != newForumId)
                {
                    forumTopic.ForumId = newForum.Id;
                    UpdateTopic(forumTopic);

                    UpdateForumStats(previousForumId);
                    UpdateForumStats(newForumId);
                }
            }

            return forumTopic;
        }

        public virtual int CalculateTopicPageIndex(int forumTopicId, int pageSize, int postId)
        {
            var pageIndex = 0;
            var forumPosts = GetAllPosts(forumTopicId, 0, true, 0, int.MaxValue);

            for (var i = 0; i < forumPosts.TotalCount; i++)
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

        #endregion

        #region Post

        public virtual ForumPost GetPostById(int forumPostId)
        {
            if (forumPostId == 0)
            {
                return null;
            }

            var forumPost = _forumPostRepository.Table.FirstOrDefault(x => x.Id == forumPostId);
            return forumPost;
        }

        public virtual IList<ForumPost> GetPostsByIds(int[] postIds)
        {
            if (postIds == null || postIds.Length == 0)
            {
                return new List<ForumPost>();
            }

            var query = _forumPostRepository.TableUntracked.Expand(x => x.ForumTopic).Expand(x => x.Customer)
                .Where(x => postIds.Contains(x.Id));

            return query.OrderBySequence(postIds).ToList();
        }

        public virtual IPagedList<ForumPost> GetAllPosts(int forumTopicId, int customerId, bool ascSort, int pageIndex, int pageSize)
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
            Guard.NotNull(forumPost, nameof(forumPost));

            _forumPostRepository.Insert(forumPost);

            var forumTopic = GetTopicById(forumPost.TopicId);

            UpdateForumTopicStats(forumPost.TopicId);
            UpdateForumStats(forumTopic.ForumId);
            UpdateCustomerStats(forumPost.CustomerId);

            if (sendNotifications)
            {
                var forum = forumTopic.Forum;
                var languageId = _services.WorkContext.WorkingLanguage.Id;
                var subscriptions = GetAllSubscriptions(0, 0, forumTopic.Id, 0, int.MaxValue);
                var friendlyTopicPageIndex = CalculateTopicPageIndex(
                    forumPost.TopicId,
                    _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10,
                    forumPost.Id) + 1;

                foreach (var subscription in subscriptions)
                {
                    if (subscription.CustomerId == forumPost.CustomerId)
                    {
                        continue;
                    }

                    if (subscription.Customer.Email.HasValue())
                    {
                        _services.MessageFactory.SendNewForumPostMessage(subscription.Customer, forumPost, friendlyTopicPageIndex, languageId);
                    }
                }
            }
        }

        public virtual void UpdatePost(ForumPost forumPost)
        {
            Guard.NotNull(forumPost, nameof(forumPost));

            _forumPostRepository.Update(forumPost);
        }

        public virtual void DeletePost(ForumPost forumPost)
        {
            if (forumPost == null)
            {
                return;
            }

            var forumTopicId = forumPost.TopicId;
            var customerId = forumPost.CustomerId;
            var forumTopic = GetTopicById(forumTopicId);
            var forumId = forumTopic.ForumId;

            // Delete topic if it was the first post.
            var deleteTopic = false;
            var firstPost = forumTopic.GetFirstPost(this);
            if (firstPost != null && firstPost.Id == forumPost.Id)
            {
                deleteTopic = true;
            }

            // Delete forum post.
            _forumPostRepository.Delete(forumPost);

            // Delete topic.
            if (deleteTopic)
            {
                DeleteTopic(forumTopic);
            }

            if (!deleteTopic)
            {
                UpdateForumTopicStats(forumTopicId);
            }
            UpdateForumStats(forumId);
            UpdateCustomerStats(customerId);
        }

        #endregion

        #region Private message

        public virtual PrivateMessage GetPrivateMessageById(int privateMessageId)
        {
            if (privateMessageId == 0)
            {
                return null;
            }

            var privateMessage = _forumPrivateMessageRepository.Table.FirstOrDefault(x => x.Id == privateMessageId);
            return privateMessage;
        }

		public virtual IPagedList<PrivateMessage> GetAllPrivateMessages(
			int storeId, 
			int fromCustomerId,
            int toCustomerId, 
			bool? isRead, 
			bool? isDeletedByAuthor, 
			bool? isDeletedByRecipient,
			int pageIndex, 
			int pageSize)
        {
			var query = _forumPrivateMessageRepository.Table;

            if (storeId > 0)
            {
                query = query.Where(pm => storeId == pm.StoreId);
            }
            if (fromCustomerId > 0)
            {
                query = query.Where(pm => fromCustomerId == pm.FromCustomerId);
            }
            if (toCustomerId > 0)
            {
                query = query.Where(pm => toCustomerId == pm.ToCustomerId);
            }
            if (isRead.HasValue)
            {
                query = query.Where(pm => isRead.Value == pm.IsRead);
            }
            if (isDeletedByAuthor.HasValue)
            {
                query = query.Where(pm => isDeletedByAuthor.Value == pm.IsDeletedByAuthor);
            }
            if (isDeletedByRecipient.HasValue)
            {
                query = query.Where(pm => isDeletedByRecipient.Value == pm.IsDeletedByRecipient);
            }

			query = query.OrderByDescending(pm => pm.CreatedOnUtc);

			var privateMessages = new PagedList<PrivateMessage>(query, pageIndex, pageSize);
            return privateMessages;
        }

        public virtual void InsertPrivateMessage(PrivateMessage privateMessage)
        {
            Guard.NotNull(privateMessage, nameof(privateMessage));

            _forumPrivateMessageRepository.Insert(privateMessage);

            var customerTo = _customerService.GetCustomerById(privateMessage.ToCustomerId);
            if (customerTo == null)
            {
                throw new SmartException("Recipient could not be loaded");
            }

			_genericAttributeService.SaveAttribute(customerTo, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, false, privateMessage.StoreId);

            if (_forumSettings.NotifyAboutPrivateMessages)
            {
				_services.MessageFactory.SendPrivateMessageNotification(customerTo, privateMessage, _services.WorkContext.WorkingLanguage.Id);                
            }
        }

        public virtual void UpdatePrivateMessage(PrivateMessage privateMessage)
        {
            Guard.NotNull(privateMessage, nameof(privateMessage));

            if (privateMessage.IsDeletedByAuthor && privateMessage.IsDeletedByRecipient)
            {
                _forumPrivateMessageRepository.Delete(privateMessage);
            }
            else
            {
                _forumPrivateMessageRepository.Update(privateMessage);
            }
        }

        public virtual void DeletePrivateMessage(PrivateMessage privateMessage)
        {
            if (privateMessage != null)
            {
                _forumPrivateMessageRepository.Delete(privateMessage);
            }            
        }

        #endregion

        #region Subscription

        public virtual ForumSubscription GetSubscriptionById(int forumSubscriptionId)
        {
            if (forumSubscriptionId == 0)
            {
                return null;
            }

            var forumSubscription = _forumSubscriptionRepository.Table.FirstOrDefault(x => x.Id == forumSubscriptionId);
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
            Guard.NotNull(forumSubscription, nameof(forumSubscription));

            _forumSubscriptionRepository.Insert(forumSubscription);
        }

        public virtual void UpdateSubscription(ForumSubscription forumSubscription)
        {
            Guard.NotNull(forumSubscription, nameof(forumSubscription));

            _forumSubscriptionRepository.Update(forumSubscription);
        }

        public virtual void DeleteSubscription(ForumSubscription forumSubscription)
        {
            if (forumSubscription != null)
            {
                _forumSubscriptionRepository.Delete(forumSubscription);
            }
        }

        #endregion

        #region Customer

        public virtual bool IsCustomerAllowedToCreateTopic(Customer customer, Forum forum)
        {
            if (forum == null || customer == null)
            {
                return false;
            }

            if (customer.IsGuest() && !_forumSettings.AllowGuestsToCreateTopics)
            {
                return false;
            }

            if (customer.IsForumModerator())
            {
                return true;
            }

            return true;
        }

        public virtual bool IsCustomerAllowedToEditTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null || customer == null || customer.IsGuest())
            {
                return false;
            }

            if (customer.IsForumModerator())
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToEditPosts)
            {
                var ownTopic = customer.Id == topic.CustomerId;
                return ownTopic;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToMoveTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null || customer == null)
            {
                return false;
            }

            if (customer.IsGuest())
            {
                return false;
            }

            if (customer.IsForumModerator())
            {
                return true;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToDeleteTopic(Customer customer, ForumTopic topic)
        {
            if (topic == null || customer == null || customer.IsGuest())
            {
                return false;
            }

            if (customer.IsForumModerator())
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToDeletePosts)
            {
                var ownTopic = customer.Id == topic.CustomerId;
                return ownTopic;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToCreatePost(Customer customer, ForumTopic topic)
        {
            if (topic == null || customer == null)
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
            if (post == null || customer == null || customer.IsGuest())
            {
                return false;
            }

            if (customer.IsForumModerator())
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToEditPosts)
            {
                var ownPost = customer.Id == post.CustomerId;
                return ownPost;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToDeletePost(Customer customer, ForumPost post)
        {
            if (post == null || customer == null || customer.IsGuest())
            {
                return false;
            }

            if (customer.IsForumModerator())
            {
                return true;
            }

            if (_forumSettings.AllowCustomersToDeletePosts)
            {
                var ownPost = customer.Id == post.CustomerId;
                return ownPost;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToSetTopicPriority(Customer customer)
        {
            if (customer == null || customer.IsGuest())
            {
                return false;
            }

            if (customer.IsForumModerator())
            {
                return true;
            }            

            return false;
        }

        public virtual bool IsCustomerAllowedToSubscribe(Customer customer)
        {
            if (customer == null || customer.IsGuest())
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}
