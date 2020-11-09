using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Stores;
using SmartStore.Data.Caching;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Seo;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Localization;
using System.Web.Mvc;
using System;
using SmartStore.Data.Utilities;

namespace SmartStore.Services.Forums
{
    public partial class ForumService : IForumService, IXmlSitemapPublisher
    {
        private readonly IRepository<ForumGroup> _forumGroupRepository;
        private readonly IRepository<Forum> _forumRepository;
        private readonly IRepository<ForumTopic> _forumTopicRepository;
        private readonly IRepository<ForumPost> _forumPostRepository;
        private readonly IRepository<PrivateMessage> _forumPrivateMessageRepository;
        private readonly IRepository<ForumSubscription> _forumSubscriptionRepository;
        private readonly IRepository<StoreMapping> _storeMappingRepository;
        private readonly IRepository<AclRecord> _aclRepository;
        private readonly ForumSettings _forumSettings;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ICustomerService _customerService;
        private readonly Lazy<UrlHelper> _urlHelper;
        private readonly Lazy<IUrlRecordService> _urlRecordService;
        private readonly ICommonServices _services;

        public ForumService(
            IRepository<ForumGroup> forumGroupRepository,
            IRepository<Forum> forumRepository,
            IRepository<ForumTopic> forumTopicRepository,
            IRepository<ForumPost> forumPostRepository,
            IRepository<PrivateMessage> forumPrivateMessageRepository,
            IRepository<ForumSubscription> forumSubscriptionRepository,
            IRepository<StoreMapping> storeMappingRepository,
            IRepository<AclRecord> aclRepository,
            ForumSettings forumSettings,
            IRepository<Customer> customerRepository,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            Lazy<UrlHelper> urlHelper,
            Lazy<IUrlRecordService> urlRecordService,
            ICommonServices services)
        {
            _forumGroupRepository = forumGroupRepository;
            _forumRepository = forumRepository;
            _forumTopicRepository = forumTopicRepository;
            _forumPostRepository = forumPostRepository;
            _forumPrivateMessageRepository = forumPrivateMessageRepository;
            _forumSubscriptionRepository = forumSubscriptionRepository;
            _storeMappingRepository = storeMappingRepository;
            _aclRepository = aclRepository;
            _forumSettings = forumSettings;
            _customerRepository = customerRepository;
            _genericAttributeService = genericAttributeService;
            _customerService = customerService;
            _urlHelper = urlHelper;
            _urlRecordService = urlRecordService;
            _services = services;
        }

        public DbQuerySettings QuerySettings { get; set; }

        protected virtual void UpdateForumStatistics(Forum forum)
        {
            if (forum == null)
            {
                return;
            }

            var queryLastValues =
                from ft in _forumTopicRepository.TableUntracked
                join fp in _forumPostRepository.TableUntracked on ft.Id equals fp.TopicId
                where ft.ForumId == forum.Id && ft.Published && fp.Published
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

            forum.NumTopics = _forumTopicRepository
                .Table.Where(x => x.ForumId == forum.Id && x.Published)
                .Count();

            forum.NumPosts = (
                from ft in _forumTopicRepository.Table
                join fp in _forumPostRepository.Table on ft.Id equals fp.TopicId
                where ft.ForumId == forum.Id && ft.Published && fp.Published
                select fp.Id).Count();

            UpdateForum(forum);
        }

        protected virtual void UpdateTopicStatistics(ForumTopic topic)
        {
            if (topic == null)
            {
                return;
            }

            var queryLastValues =
                from fp in _forumPostRepository.TableUntracked
                where fp.TopicId == topic.Id && fp.Published
                orderby fp.CreatedOnUtc descending
                select new
                {
                    LastPostId = fp.Id,
                    LastPostCustomerId = fp.CustomerId,
                    LastPostTime = fp.CreatedOnUtc
                };
            var lastValues = queryLastValues.FirstOrDefault();

            topic.LastPostId = lastValues?.LastPostId ?? 0;
            topic.LastPostCustomerId = lastValues?.LastPostCustomerId ?? 0;
            topic.LastPostTime = lastValues?.LastPostTime;

            topic.NumPosts = topic.Published
                ? _forumPostRepository.Table.Where(x => x.TopicId == topic.Id && x.Published).Count()
                : 0;

            UpdateTopic(topic, false);
        }

        protected virtual void UpdateCustomerStatistics(Customer customer)
        {
            if (customer != null)
            {
                var numPosts = _forumPostRepository.Table
                    .Where(x => x.CustomerId == customer.Id && x.ForumTopic.Published && x.Published)
                    .Count();

                _genericAttributeService.SaveAttribute(customer, SystemCustomerAttributeNames.ForumPostCount, numPosts);
            }
        }

        #region Group

        public virtual ForumGroup GetForumGroupById(int groupId)
        {
            if (groupId == 0)
            {
                return null;
            }

            return _forumGroupRepository.GetById(groupId);
        }

        public virtual IList<ForumGroup> GetAllForumGroups(int storeId = 0, bool showHidden = false)
        {
            var joinApplied = false;
            var query = _forumGroupRepository.Table.Expand(x => x.Forums);

            if (!QuerySettings.IgnoreMultiStore && storeId > 0)
            {
                query =
                    from fg in query
                    join sm in _storeMappingRepository.Table on new { c1 = fg.Id, c2 = "ForumGroup" } equals new { c1 = sm.EntityId, c2 = sm.EntityName } into fg_sm
                    from sm in fg_sm.DefaultIfEmpty()
                    where !fg.LimitedToStores || storeId == sm.StoreId
                    select fg;

                joinApplied = true;
            }

            if (!showHidden && !QuerySettings.IgnoreAcl)
            {
                var allowedCustomerRolesIds = _services.WorkContext.CurrentCustomer.GetRoleIds();

                query =
                    from fg in query
                    join a in _aclRepository.Table on new { a1 = fg.Id, a2 = "ForumGroup" } equals new { a1 = a.EntityId, a2 = a.EntityName } into fg_acl
                    from a in fg_acl.DefaultIfEmpty()
                    where !fg.SubjectToAcl || allowedCustomerRolesIds.Contains(a.CustomerRoleId)
                    select fg;

                joinApplied = true;
            }

            if (joinApplied)
            {
                query =
                    from fg in query
                    group fg by fg.Id into fgGroup
                    orderby fgGroup.Key
                    select fgGroup.FirstOrDefault();
            }

            query = query.OrderBy(x => x.DisplayOrder);

            return query.ToListCached();
        }

        public virtual void InsertForumGroup(ForumGroup group)
        {
            Guard.NotNull(group, nameof(group));

            _forumGroupRepository.Insert(group);
        }

        public virtual void UpdateForumGroup(ForumGroup group)
        {
            Guard.NotNull(group, nameof(group));

            _forumGroupRepository.Update(group);
        }

        public virtual void DeleteForumGroup(ForumGroup group)
        {
            if (group != null)
            {
                _forumGroupRepository.Delete(group);
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

            var entity = _forumRepository.Table
                .Expand(x => x.ForumGroup)
                .FirstOrDefault(x => x.Id == forumId);

            return entity;
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
            var queryTopicIds =
                from ft in _forumTopicRepository.Table
                where ft.ForumId == forum.Id
                select ft.Id;

            var queryFs1 =
                from fs in _forumSubscriptionRepository.Table
                where queryTopicIds.Contains(fs.TopicId)
                select fs;

            foreach (var fs in queryFs1.ToList())
            {
                _forumSubscriptionRepository.Delete(fs);
            }

            // Delete forum subscriptions (forum).
            var queryFs2 =
                from fs in _forumSubscriptionRepository.Table
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

        public virtual ForumTopic GetTopicById(int topicId)
        {
            if (topicId == 0)
            {
                return null;
            }

            var entity = _forumTopicRepository.Table
                .Expand(x => x.Forum)
                .Expand(x => x.Forum.ForumGroup)
                .FirstOrDefault(x => x.Id == topicId);

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

        public virtual IPagedList<ForumTopic> GetAllTopics(int forumId, int pageIndex, int pageSize, bool showHidden = false)
        {
            var customer = _services.WorkContext.CurrentCustomer;
            var query = _forumTopicRepository.TableUntracked;

            if (forumId != 0)
            {
                query = query.Where(x => x.ForumId == forumId);
            }

            if (!showHidden && !customer.IsForumModerator())
            {
                query = query.Where(x => x.Published || x.CustomerId == customer.Id);
            }

            query = query
                .OrderByDescending(x => x.TopicTypeId)
                .ThenByDescending(x => x.LastPostTime)
                .ThenByDescending(x => x.Id);

            var topics = new PagedList<ForumTopic>(query, pageIndex, pageSize);
            return topics;
        }

        public virtual IList<ForumTopic> GetActiveTopics(int forumId, int count, bool showHidden = false)
        {
            var joinApplied = false;
            var customer = _services.WorkContext.CurrentCustomer;

            var query =
                from ft in _forumTopicRepository.Table
                where (forumId == 0 || ft.ForumId == forumId) && ft.LastPostTime.HasValue
                select ft;

            if (!showHidden && !customer.IsForumModerator())
            {
                query = query.Where(x => x.Published || x.CustomerId == customer.Id);
            }

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

                joinApplied = true;
            }

            if (!showHidden && !QuerySettings.IgnoreAcl)
            {
                var allowedCustomerRolesIds = customer.GetRoleIds();

                query =
                    from ft in query
                    join ff in _forumRepository.Table on ft.ForumId equals ff.Id
                    join fg in _forumGroupRepository.Table on ff.ForumGroupId equals fg.Id
                    join a in _aclRepository.Table on new { a1 = fg.Id, a2 = "ForumGroup" } equals new { a1 = a.EntityId, a2 = a.EntityName } into fg_acl
                    from a in fg_acl.DefaultIfEmpty()
                    where !fg.SubjectToAcl || allowedCustomerRolesIds.Contains(a.CustomerRoleId)
                    select ft;

                joinApplied = true;
            }

            if (joinApplied)
            {
                query =
                    from ft in query
                    group ft by ft.Id into ftGroup
                    orderby ftGroup.Key
                    select ftGroup.FirstOrDefault();
            }

            query = query.OrderByDescending(x => x.LastPostTime);

            var forumTopics = query.Take(() => count).ToList();
            return forumTopics;
        }

        public virtual void InsertTopic(ForumTopic topic, bool sendNotifications)
        {
            Guard.NotNull(topic, nameof(topic));

            _forumTopicRepository.Insert(topic);

            var forum = topic.Forum ?? GetForumById(topic.ForumId);

            UpdateForumStatistics(forum);

            if (sendNotifications)
            {
                var subscriptions = GetAllSubscriptions(0, forum.Id, 0, 0, int.MaxValue);
                var languageId = _services.WorkContext.WorkingLanguage.Id;

                foreach (var subscription in subscriptions)
                {
                    if (subscription.CustomerId == topic.CustomerId)
                    {
                        continue;
                    }

                    if (subscription.Customer.Email.HasValue())
                    {
                        _services.MessageFactory.SendNewForumTopicMessage(subscription.Customer, topic, languageId);
                    }
                }
            }
        }

        public virtual void UpdateTopic(ForumTopic topic, bool updateStatistics)
        {
            Guard.NotNull(topic, nameof(topic));

            _forumTopicRepository.Update(topic);

            if (updateStatistics)
            {
                var forum = topic.Forum ?? GetForumById(topic.ForumId);
                var customer = topic.Customer ?? _customerService.GetCustomerById(topic.CustomerId);

                UpdateForumStatistics(forum);
                UpdateTopicStatistics(topic);
                UpdateCustomerStatistics(customer);
            }
        }

        public virtual void DeleteTopic(ForumTopic topic)
        {
            if (topic == null)
            {
                return;
            }

            var forum = topic.Forum ?? GetForumById(topic.ForumId);
            var customer = topic.Customer ?? _customerService.GetCustomerById(topic.CustomerId);

            _forumTopicRepository.Delete(topic);

            // Delete forum subscriptions.
            var subscriptions = _forumSubscriptionRepository.Table.Where(x => x.TopicId == topic.Id).ToList();
            foreach (var subscription in subscriptions)
            {
                _forumSubscriptionRepository.Delete(subscription);
            }

            UpdateForumStatistics(forum);
            UpdateCustomerStatistics(customer);
        }

        public virtual ForumTopic MoveTopic(int topicId, int newForumId)
        {
            var topic = GetTopicById(topicId);
            if (topic != null && IsCustomerAllowedToMoveTopic(_services.WorkContext.CurrentCustomer, topic))
            {
                var previousForumId = topic.ForumId;
                var newForum = GetForumById(newForumId);

                if (newForum != null && previousForumId != newForumId)
                {
                    topic.ForumId = newForum.Id;
                    UpdateTopic(topic, false);

                    UpdateForumStatistics(GetForumById(previousForumId));
                    UpdateForumStatistics(newForum);
                }
            }

            return topic;
        }

        public virtual int CalculateTopicPageIndex(int topicId, int pageSize, int postId)
        {
            if (pageSize > 0 && postId != 0)
            {
                var query = GetPostQuery(topicId, 0, true, true);
                var postIds = query.Select(x => x.Id).ToList();

                for (var i = 0; i < postIds.Count; ++i)
                {
                    if (postIds[i] == postId)
                    {
                        return i / pageSize;
                    }
                }
            }

            return 0;
        }

        #endregion

        #region Post

        protected virtual IQueryable<ForumPost> GetPostQuery(
            int topicId,
            int customerId,
            bool ascSort,
            bool showHidden = false,
            bool untracked = false)
        {
            var customer = _services.WorkContext.CurrentCustomer;
            var query = untracked ? _forumPostRepository.TableUntracked : _forumPostRepository.Table;

            if (topicId > 0)
            {
                query = query.Where(x => topicId == x.TopicId);
            }
            if (customerId > 0)
            {
                query = query.Where(x => customerId == x.CustomerId);
            }
            if (!showHidden && !customer.IsForumModerator())
            {
                query = query.Where(x => x.Published || x.CustomerId == customer.Id);
            }

            if (ascSort)
            {
                query = query.OrderBy(x => x.CreatedOnUtc).ThenBy(x => x.Id);
            }
            else
            {
                query = query.OrderByDescending(x => x.CreatedOnUtc).ThenBy(x => x.Id);
            }

            return query;
        }

        public virtual ForumPost GetPostById(int postId)
        {
            if (postId == 0)
            {
                return null;
            }

            var forumPost = _forumPostRepository.Table
                .Expand(x => x.ForumTopic)
                .Expand(x => x.ForumTopic.Forum)
                .Expand(x => x.ForumTopic.Forum.ForumGroup)
                .FirstOrDefault(x => x.Id == postId);

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

        public virtual IPagedList<ForumPost> GetAllPosts(
            int topicId,
            int customerId,
            bool ascSort,
            int pageIndex,
            int pageSize,
            bool showHidden = false)
        {
            var query = GetPostQuery(topicId, customerId, ascSort, showHidden);
            var forumPosts = new PagedList<ForumPost>(query, pageIndex, pageSize);
            return forumPosts;
        }

        public virtual void InsertPost(ForumPost post, bool sendNotifications)
        {
            Guard.NotNull(post, nameof(post));

            _forumPostRepository.Insert(post);

            var topic = post.ForumTopic ?? GetTopicById(post.TopicId);
            var forum = topic.Forum ?? GetForumById(topic.ForumId);
            var customer = post.Customer ?? _customerService.GetCustomerById(post.CustomerId);

            UpdateTopicStatistics(topic);
            UpdateForumStatistics(forum);
            UpdateCustomerStatistics(customer);

            if (sendNotifications)
            {
                var languageId = _services.WorkContext.WorkingLanguage.Id;
                var subscriptions = GetAllSubscriptions(0, 0, topic.Id, 0, int.MaxValue);
                var friendlyTopicPageIndex = CalculateTopicPageIndex(
                    post.TopicId,
                    _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 20,
                    post.Id) + 1;

                foreach (var subscription in subscriptions)
                {
                    if (subscription.CustomerId == post.CustomerId)
                    {
                        continue;
                    }

                    if (subscription.Customer.Email.HasValue())
                    {
                        _services.MessageFactory.SendNewForumPostMessage(subscription.Customer, post, friendlyTopicPageIndex, languageId);
                    }
                }
            }
        }

        public virtual void UpdatePost(ForumPost post, bool updateStatistics)
        {
            Guard.NotNull(post, nameof(post));

            _forumPostRepository.Update(post);

            if (updateStatistics)
            {
                var topic = post.ForumTopic ?? GetTopicById(post.TopicId);
                var forum = topic.Forum ?? GetForumById(topic.ForumId);
                var customer = post.Customer ?? _customerService.GetCustomerById(post.CustomerId);

                UpdateForumStatistics(forum);
                UpdateTopicStatistics(topic);
                UpdateCustomerStatistics(customer);
            }
        }

        public virtual void DeletePost(ForumPost post)
        {
            if (post == null)
            {
                return;
            }

            var topic = post.ForumTopic ?? GetTopicById(post.TopicId);
            var forum = topic.Forum ?? GetForumById(topic.ForumId);
            var customer = post.Customer ?? _customerService.GetCustomerById(post.CustomerId);

            // Delete topic if it was the first post.
            var firstPost = topic.GetFirstPost(this);
            var deleteTopic = firstPost != null && firstPost.Id == post.Id;

            // Delete forum post.
            _forumPostRepository.Delete(post);

            // Delete topic.
            if (deleteTopic)
            {
                DeleteTopic(topic);
            }
            else
            {
                UpdateTopicStatistics(topic);
            }

            UpdateForumStatistics(forum);
            UpdateCustomerStatistics(customer);
        }

        #endregion

        #region Private message

        public virtual PrivateMessage GetPrivateMessageById(int messageId)
        {
            if (messageId == 0)
            {
                return null;
            }

            var privateMessage = _forumPrivateMessageRepository.Table.FirstOrDefault(x => x.Id == messageId);
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

        public virtual void InsertPrivateMessage(PrivateMessage message)
        {
            Guard.NotNull(message, nameof(message));

            _forumPrivateMessageRepository.Insert(message);

            var customerTo = _customerService.GetCustomerById(message.ToCustomerId);
            if (customerTo == null)
            {
                throw new SmartException("Recipient could not be loaded");
            }

            _genericAttributeService.SaveAttribute(customerTo, SystemCustomerAttributeNames.NotifiedAboutNewPrivateMessages, false, message.StoreId);

            if (_forumSettings.NotifyAboutPrivateMessages)
            {
                _services.MessageFactory.SendPrivateMessageNotification(customerTo, message, _services.WorkContext.WorkingLanguage.Id);
            }
        }

        public virtual void UpdatePrivateMessage(PrivateMessage message)
        {
            Guard.NotNull(message, nameof(message));

            if (message.IsDeletedByAuthor && message.IsDeletedByRecipient)
            {
                _forumPrivateMessageRepository.Delete(message);
            }
            else
            {
                _forumPrivateMessageRepository.Update(message);
            }
        }

        public virtual void DeletePrivateMessage(PrivateMessage message)
        {
            if (message != null)
            {
                _forumPrivateMessageRepository.Delete(message);
            }
        }

        #endregion

        #region Subscription

        public virtual ForumSubscription GetSubscriptionById(int subscriptionId)
        {
            if (subscriptionId == 0)
            {
                return null;
            }

            var forumSubscription = _forumSubscriptionRepository.Table.FirstOrDefault(x => x.Id == subscriptionId);
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

        public virtual void InsertSubscription(ForumSubscription subscription)
        {
            Guard.NotNull(subscription, nameof(subscription));

            _forumSubscriptionRepository.Insert(subscription);
        }

        public virtual void UpdateSubscription(ForumSubscription subscription)
        {
            Guard.NotNull(subscription, nameof(subscription));

            _forumSubscriptionRepository.Update(subscription);
        }

        public virtual void DeleteSubscription(ForumSubscription subscription)
        {
            if (subscription != null)
            {
                _forumSubscriptionRepository.Delete(subscription);
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
            if (customer != null && topic != null)
            {
                if (customer.IsForumModerator())
                {
                    return true;
                }

                if (_forumSettings.AllowCustomersToEditPosts && topic.Published)
                {
                    var ownTopic = customer.Id == topic.CustomerId;
                    return ownTopic;
                }
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToMoveTopic(Customer customer, ForumTopic topic)
        {
            if (customer != null && customer.IsForumModerator())
            {
                return true;
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToDeleteTopic(Customer customer, ForumTopic topic)
        {
            if (topic != null && customer != null)
            {
                if (customer.IsForumModerator())
                {
                    return true;
                }

                if (_forumSettings.AllowCustomersToDeletePosts && topic.Published)
                {
                    var ownTopic = customer.Id == topic.CustomerId;
                    return ownTopic;
                }
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToCreatePost(Customer customer, ForumTopic topic)
        {
            if (topic == null || customer == null)
            {
                return false;
            }

            if (!_forumSettings.AllowGuestsToCreatePosts && customer.IsGuest())
            {
                return false;
            }

            return true;
        }

        public virtual bool IsCustomerAllowedToEditPost(Customer customer, ForumPost post)
        {
            if (post != null && customer != null)
            {
                if (customer.IsForumModerator())
                {
                    return true;
                }

                if (_forumSettings.AllowCustomersToEditPosts && post.Published)
                {
                    var ownPost = customer.Id == post.CustomerId;
                    return ownPost;
                }
            }

            return false;
        }

        public virtual bool IsCustomerAllowedToDeletePost(Customer customer, ForumPost post)
        {
            if (post != null && customer != null)
            {
                if (customer.IsForumModerator())
                {
                    return true;
                }

                if (_forumSettings.AllowCustomersToDeletePosts && post.Published)
                {
                    var ownPost = customer.Id == post.CustomerId;
                    return ownPost;
                }
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

        #region XML Sitemap

        public XmlSitemapProvider PublishXmlSitemap(XmlSitemapBuildContext context)
        {
            if (!context.LoadSetting<SeoSettings>().XmlSitemapIncludesForum || !context.LoadSetting<ForumSettings>().ForumsEnabled)
                return null;

            return new ForumXmlSitemapResult(context, this, _urlHelper.Value, _urlRecordService.Value);
        }

        class ForumXmlSitemapResult : XmlSitemapProvider
        {
            private readonly IForumService _forumService;
            private readonly XmlSitemapBuildContext _context;
            private readonly UrlHelper _urlHelper;
            private readonly IUrlRecordService _urlRecordService;

            private readonly IList<ForumGroup> _groups;
            private readonly IList<Forum> _forums;
            private readonly IQueryable<ForumTopic> _topicsQuery;

            public ForumXmlSitemapResult(
                XmlSitemapBuildContext context,
                IForumService forumService,
                UrlHelper urlHelper,
                IUrlRecordService urlRecordService)
            {
                _forumService = forumService;
                _context = context;
                _urlHelper = urlHelper;
                _urlRecordService = urlRecordService;

                _groups = _forumService.GetAllForumGroups(context.RequestStoreId, false);
                _forums = _groups.SelectMany(x => x.Forums).ToList();
                _topicsQuery = _forumService.GetAllTopics(0, 0, int.MaxValue, false).SourceQuery;
            }

            public override int GetTotalCount()
            {
                // INFO: we gonna create nodes for all groups, forums within groups and all topics
                return _groups.Count + _forums.Count + _topicsQuery.Count();
            }

            public override XmlSitemapNode CreateNode(UrlHelper urlHelper, string baseUrl, NamedEntity entity, UrlRecordCollection slugs, Language language)
            {
                var path = string.Empty;

                switch (entity.EntityName)
                {
                    case nameof(ForumGroup):
                        path = urlHelper.RouteUrl("ForumGroupSlug", new { id = entity.Id, slug = slugs.GetSlug(language.Id, entity.Id, true) });
                        break;
                    case nameof(Forum):
                        path = urlHelper.RouteUrl("ForumSlug", new { id = entity.Id, slug = slugs.GetSlug(language.Id, entity.Id, true) });
                        break;
                    case nameof(ForumTopic):
                        path = urlHelper.RouteUrl("TopicSlug", new { id = entity.Id, slug = entity.Slug });
                        break;
                }

                if (path.HasValue())
                {
                    return new XmlSitemapNode
                    {
                        LastMod = entity.LastMod,
                        Loc = baseUrl + path.TrimStart('/')
                    };
                }

                return null;
            }

            public override IEnumerable<NamedEntity> Enlist()
            {
                // Enlist forum groups
                foreach (var group in _groups)
                {
                    yield return new NamedEntity { EntityName = nameof(ForumGroup), Id = group.Id, LastMod = group.UpdatedOnUtc };
                }

                // Enlist forums
                foreach (var forum in _forums)
                {
                    yield return new NamedEntity { EntityName = nameof(Forum), Id = forum.Id, LastMod = forum.UpdatedOnUtc };
                }

                // Enlist topics
                var pager = new FastPager<ForumTopic>(_topicsQuery.AsNoTracking(), _context.MaximumNodeCount);

                while (pager.ReadNextPage(x => new { x.Id, x.UpdatedOnUtc, x.Subject }, x => x.Id, out var topics))
                {
                    if (_context.CancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    foreach (var x in topics)
                    {
                        yield return new NamedEntity
                        {
                            EntityName = nameof(ForumTopic),
                            Slug = (new ForumTopic { Subject = x.Subject }).GetSeName(),
                            Id = x.Id,
                            LastMod = x.UpdatedOnUtc
                        };
                    }
                }
            }

            public override int Order => 1000;
        }

        #endregion
    }
}
