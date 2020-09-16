using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Forums;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Core.Search;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Forums;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Search;
using SmartStore.Services.Search.Modelling;
using SmartStore.Services.Search.Rendering;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Models.Boards;
using SmartStore.Web.Models.Search;

namespace SmartStore.Web.Controllers
{
    [RewriteUrl(SslRequirement.No)]
    public partial class BoardsController : PublicControllerBase
    {
        private readonly IForumService _forumService;
        private readonly IMediaService _mediaService;
        private readonly ICountryService _countryService;
        private readonly IForumSearchService _forumSearchService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IAclService _aclService;
        private readonly ICustomerContentService _customerContentService;
        private readonly ForumSettings _forumSettings;
        private readonly ForumSearchSettings _searchSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IBreadcrumb _breadcrumb;
        private readonly Lazy<IFacetTemplateProvider> _templateProvider;
        private readonly IForumSearchQueryFactory _queryFactory;

        public BoardsController(
            IForumService forumService,
            IMediaService mediaService,
            ICountryService countryService,
            IForumSearchService forumSearchService,
            IGenericAttributeService genericAttributeService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            ICustomerContentService customerContentService,
            ForumSettings forumSettings,
            ForumSearchSettings searchSettings,
            CustomerSettings customerSettings,
            MediaSettings mediaSettings,
            CaptchaSettings captchaSettings,
            IDateTimeHelper dateTimeHelper,
            IBreadcrumb breadcrumb,
            Lazy<IFacetTemplateProvider> templateProvider,
            IForumSearchQueryFactory queryFactory)
        {
            _forumService = forumService;
            _mediaService = mediaService;
            _countryService = countryService;
            _forumSearchService = forumSearchService;
            _genericAttributeService = genericAttributeService;
            _storeMappingService = storeMappingService;
            _aclService = aclService;
            _customerContentService = customerContentService;
            _forumSettings = forumSettings;
            _searchSettings = searchSettings;
            _customerSettings = customerSettings;
            _mediaSettings = mediaSettings;
            _captchaSettings = captchaSettings;
            _dateTimeHelper = dateTimeHelper;
            _breadcrumb = breadcrumb;
            _templateProvider = templateProvider;
            _queryFactory = queryFactory;
        }

        #region Utilities

        private ForumTopicRowModel PrepareForumTopicRowModel(
            ForumTopic topic,
            Dictionary<int, ForumPost> lastPosts,
            ForumPost firstPost = null)
        {
            var customer = topic.Customer;

            var model = new ForumTopicRowModel
            {
                Id = topic.Id,
                Published = topic.Published,
                Subject = topic.Subject,
                SeName = topic.GetSeName(),
                FirstPostId = firstPost?.Id ?? topic.FirstPostId,
                LastPostId = topic.LastPostId,
                NumPosts = topic.NumPosts,
                Views = topic.Views,
                NumReplies = topic.NumReplies,
                ForumTopicType = topic.ForumTopicType,
                CustomerId = topic.CustomerId,
                AllowViewingProfiles = _customerSettings.AllowViewingProfiles,
                CustomerName = customer.FormatUserName(_customerSettings, T, true),
                IsCustomerGuest = customer.IsGuest(),
                PostsPageSize = _forumSettings.PostsPageSize
            };

            model.Avatar = customer.ToAvatarModel(_genericAttributeService, _customerSettings, _mediaSettings, model.CustomerName);

            if (topic.LastPostId != 0 && lastPosts.TryGetValue(topic.LastPostId, out var lastPost))
            {
                PrepareLastPostModel(model.LastPost, lastPost);
            }

            return model;
        }

        private ForumRowModel PrepareForumRowModel(Forum forum, Dictionary<int, ForumPost> lastPosts)
        {
            var forumModel = new ForumRowModel
            {
                Id = forum.Id,
                Name = forum.GetLocalized(x => x.Name),
                SeName = forum.GetSeName(),
                Description = forum.GetLocalized(x => x.Description),
                NumTopics = forum.NumTopics,
                NumPosts = forum.NumPosts,
                LastPostId = forum.LastPostId,
            };

            if (forum.LastPostId != 0 && lastPosts.TryGetValue(forum.LastPostId, out var lastPost))
            {
                PrepareLastPostModel(forumModel.LastPost, lastPost);
            }

            return forumModel;
        }

        private ForumGroupModel PrepareForumGroupModel(ForumGroup group)
        {
            var forumGroupModel = new ForumGroupModel
            {
                Id = group.Id,
                Name = group.GetLocalized(x => x.Name),
                Description = group.GetLocalized(x => x.Description),
                SeName = group.GetSeName()
            };

            var lastPostIds = group.Forums
                .Where(x => x.LastPostId != 0)
                .Select(x => x.LastPostId)
                .Distinct()
                .ToArray();

            var lastPosts = _forumService.GetPostsByIds(lastPostIds).ToDictionary(x => x.Id);

            foreach (var forum in group.Forums.OrderBy(x => x.DisplayOrder))
            {
                var forumModel = PrepareForumRowModel(forum, lastPosts);
                forumModel.LastPost.ShowTopic = true;

                forumGroupModel.Forums.Add(forumModel);
            }

            return forumGroupModel;
        }

        private void PrepareLastPostModel(LastPostModel model, ForumPost post)
        {
            if (post != null)
            {
                model.Id = post.Id;
                model.ForumTopicId = post.TopicId;
                model.ForumTopicSeName = post.ForumTopic.GetSeName();
                model.ForumTopicSubject = post.ForumTopic.StripTopicSubject();
                model.CustomerId = post.CustomerId;
                model.AllowViewingProfiles = _customerSettings.AllowViewingProfiles;
                model.CustomerName = post.Customer.FormatUserName(true);
                model.IsCustomerGuest = post.Customer.IsGuest();
                model.Published = post.Published;

                model.PostCreatedOnStr = _forumSettings.RelativeDateTimeFormattingEnabled
                    ? post.CreatedOnUtc.RelativeFormat(true, "f")
                    : _dateTimeHelper.ConvertToUserTime(post.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
            }
        }

        private IEnumerable<SelectListItem> ForumTopicTypesList()
        {
            var list = new List<SelectListItem>();

            list.Add(new SelectListItem
            {
                Text = T("Forum.Normal"),
                Value = ((int)ForumTopicType.Normal).ToString()
            });

            list.Add(new SelectListItem
            {
                Text = T("Forum.Sticky"),
                Value = ((int)ForumTopicType.Sticky).ToString()
            });

            list.Add(new SelectListItem
            {
                Text = T("Forum.Announcement"),
                Value = ((int)ForumTopicType.Announcement).ToString()
            });

            return list;
        }

        private void CreateForumBreadcrumb(ForumGroup group = null, Forum forum = null, ForumTopic topic = null)
        {
            _breadcrumb.Track(new MenuItem
            {
                Text = T("Forum.Forums"),
                Rtl = Services.WorkContext.WorkingLanguage.Rtl,
                Url = Url.RouteUrl("Boards")
            });

            group = group ?? forum?.ForumGroup ?? topic?.Forum?.ForumGroup;
            if (group != null)
            {
                var groupName = group.GetLocalized(x => x.Name);
                _breadcrumb.Track(new MenuItem
                {
                    Text = groupName,
                    Rtl = groupName.CurrentLanguage.Rtl,
                    Url = Url.RouteUrl("ForumGroupSlug", new { id = group.Id, slug = group.GetSeName() })
                });
            }

            forum = forum ?? topic?.Forum;
            if (forum != null)
            {
                var forumName = forum.GetLocalized(x => x.Name);
                _breadcrumb.Track(new MenuItem
                {
                    Text = forumName,
                    Rtl = forumName.CurrentLanguage.Rtl,
                    Url = Url.RouteUrl("ForumSlug", new { id = forum.Id, slug = forum.GetSeName() })
                });
            }

            if (topic != null)
            {
                _breadcrumb.Track(new MenuItem
                {
                    Text = topic.Subject,
                    Rtl = Services.WorkContext.WorkingLanguage.Rtl,
                    Url = Url.RouteUrl("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() })
                });
            }
        }

        private void SaveLastForumVisit(Customer customer)
        {
            try
            {
                if (!customer.Deleted && customer.Active && !customer.IsSystemAccount)
                {
                    customer.LastForumVisit = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private bool IsTopicVisible(ForumTopic topic, Customer customer)
        {
            if (topic == null)
            {
                return false;
            }
            if (!topic.Published && topic.CustomerId != customer.Id && !customer.IsForumModerator())
            {
                return false;
            }
            if (!_storeMappingService.Authorize(topic.Forum.ForumGroup))
            {
                return false;
            }
            if (!_aclService.Authorize(topic.Forum.ForumGroup))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Forum group

        public ActionResult Index()
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var store = Services.StoreContext.CurrentStore;
            var groups = _forumService.GetAllForumGroups(store.Id);

            var model = new BoardsIndexModel
            {
                CurrentTime = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow)
            };

            foreach (var group in groups)
            {
                var groupModel = PrepareForumGroupModel(group);
                model.ForumGroups.Add(groupModel);
            }

            model.MetaTitle = _forumSettings.GetLocalizedSetting(x => x.MetaTitle, store.Id);
            model.MetaDescription = _forumSettings.GetLocalizedSetting(x => x.MetaDescription, store.Id);
            model.MetaKeywords = _forumSettings.GetLocalizedSetting(x => x.MetaKeywords, store.Id);

            if (!model.MetaTitle.HasValue())
                model.MetaTitle = T("Forum.PageTitle.Default").Text;

            return View(model);
        }

        public ActionResult ForumGroup(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var group = _forumService.GetForumGroupById(id);
            if (group == null || !_storeMappingService.Authorize(group) || !_aclService.Authorize(group))
            {
                return HttpNotFound();
            }

            var model = PrepareForumGroupModel(group);
            CreateForumBreadcrumb(group: group);

            return View(model);
        }

        #endregion

        #region Forum

        public ActionResult Forum(int id, int page = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var forum = _forumService.GetForumById(id);
            if (forum == null || !_storeMappingService.Authorize(forum.ForumGroup) || !_aclService.Authorize(forum.ForumGroup))
            {
                return HttpNotFound();
            }

            var pageSize = _forumSettings.TopicsPageSize > 0 ? _forumSettings.TopicsPageSize : 20;
            var topics = _forumService.GetAllTopics(forum.Id, page - 1, pageSize);

            var model = new ForumPageModel
            {
                Id = forum.Id,
                Name = forum.GetLocalized(x => x.Name),
                SeName = forum.GetSeName(),
                Description = forum.GetLocalized(x => x.Description),
                TopicPageSize = topics.PageSize,
                TopicTotalRecords = topics.TotalCount,
                TopicPageIndex = topics.PageIndex,
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer),
                ForumFeedsEnabled = _forumSettings.ForumFeedsEnabled,
                PostsPageSize = _forumSettings.PostsPageSize
            };

            // Subscription.
            if (_forumService.IsCustomerAllowedToSubscribe(customer))
            {
                model.WatchForumText = T("Forum.WatchForum");

                var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, forum.Id, 0, 0, 1).FirstOrDefault();
                if (forumSubscription != null)
                {
                    model.WatchForumText = T("Forum.UnwatchForum");
                    model.WatchForumSubscribed = true;
                }
            }

            var lastPostIds = topics
                .Where(x => x.LastPostId != 0)
                .Select(x => x.LastPostId)
                .Distinct()
                .ToArray();
            var lastPosts = _forumService.GetPostsByIds(lastPostIds).ToDictionary(x => x.Id);

            foreach (var topic in topics)
            {
                var topicModel = PrepareForumTopicRowModel(topic, lastPosts);
                model.ForumTopics.Add(topicModel);
            }

            CreateForumBreadcrumb(forum: forum);
            SaveLastForumVisit(customer);

            return View(model);
        }

        public ActionResult ForumRss(int id = 0)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var store = Services.StoreContext.CurrentStore;
            var language = Services.WorkContext.WorkingLanguage;
            var protocol = Services.WebHelper.IsCurrentConnectionSecured() ? "https" : "http";
            var selfLink = Url.Action("ForumRSS", "Boards", null, protocol);
            var forumLink = Url.Action("Forum", "Boards", new { id }, protocol);
            var feed = new SmartSyndicationFeed(new Uri(forumLink), store.Name, T("Forum.ForumFeedDescription"));

            feed.AddNamespaces(false);
            feed.Init(selfLink, language);

            if (!_forumSettings.ForumFeedsEnabled)
            {
                return new RssActionResult { Feed = feed };
            }

            var forum = _forumService.GetForumById(id);
            if (forum == null || !_storeMappingService.Authorize(forum.ForumGroup) || !_aclService.Authorize(forum.ForumGroup))
            {
                return new RssActionResult { Feed = feed };
            }

            feed.Title = new TextSyndicationContent("{0} - {1}".FormatInvariant(store.Name, forum.GetLocalized(x => x.Name, language)));

            var items = new List<SyndicationItem>();
            var topics = _forumService.GetAllTopics(id, 0, _forumSettings.ForumFeedCount);
            var viewsText = T("Forum.Views");
            var repliesText = T("Forum.Replies");

            foreach (var topic in topics)
            {
                var topicUrl = Url.RouteUrl("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() }, protocol);
                var synopsis = "{0}: {1}, {2}: {3}".FormatInvariant(repliesText, topic.NumReplies, viewsText, topic.Views);

                var item = feed.CreateItem(topic.Subject, synopsis, topicUrl, topic.LastPostTime ?? topic.UpdatedOnUtc);
                items.Add(item);
            }

            feed.Items = items;

            return new RssActionResult { Feed = feed };
        }

        [HttpPost]
        public ActionResult ForumWatch(int id)
        {
            var subscribed = false;
            var returnText = T("Forum.WatchForum").Text;
            var customer = Services.WorkContext.CurrentCustomer;
            var forum = _forumService.GetForumById(id);

            if (forum == null ||
                !_storeMappingService.Authorize(forum.ForumGroup) ||
                !_aclService.Authorize(forum.ForumGroup) ||
                !_forumService.IsCustomerAllowedToSubscribe(customer))
            {
                return Json(new { Subscribed = subscribed, Text = returnText, Error = true });
            }

            var subscription = _forumService.GetAllSubscriptions(customer.Id, forum.Id, 0, 0, 1).FirstOrDefault();
            if (subscription == null)
            {
                subscription = new ForumSubscription
                {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    ForumId = forum.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _forumService.InsertSubscription(subscription);
                subscribed = true;
                returnText = T("Forum.UnwatchForum");
            }
            else
            {
                _forumService.DeleteSubscription(subscription);
                subscribed = false;
            }

            return Json(new { Subscribed = subscribed, Text = returnText, Error = false });
        }

        #endregion

        #region Active discussion

        [ChildActionOnly]
        public ActionResult ActiveDiscussionsSmall()
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var topics = _forumService.GetActiveTopics(0, _forumSettings.HomePageActiveDiscussionsTopicCount);
            if (!topics.Any())
            {
                return new EmptyResult();
            }

            var model = new ActiveDiscussionsModel();
            var lastPostIds = topics
                .Where(x => x.LastPostId != 0)
                .Select(x => x.LastPostId)
                .Distinct()
                .ToArray();

            var lastPosts = _forumService.GetPostsByIds(lastPostIds).ToDictionary(x => x.Id);

            foreach (var topic in topics)
            {
                var topicModel = PrepareForumTopicRowModel(topic, lastPosts);
                model.ForumTopics.Add(topicModel);
            }

            model.ViewAllLinkEnabled = true;
            model.ActiveDiscussionsFeedEnabled = _forumSettings.ActiveDiscussionsFeedEnabled;
            model.PostsPageSize = _forumSettings.PostsPageSize;

            return PartialView("_ActiveTopics", model);
        }

        public ActionResult ActiveDiscussions(int forumId = 0)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var model = new ActiveDiscussionsModel();
            var topics = _forumService.GetActiveTopics(forumId, _forumSettings.ActiveDiscussionsPageTopicCount);

            var lastPostIds = topics
                .Where(x => x.LastPostId != 0)
                .Select(x => x.LastPostId)
                .Distinct()
                .ToArray();

            var lastPosts = _forumService.GetPostsByIds(lastPostIds).ToDictionary(x => x.Id);

            foreach (var topic in topics)
            {
                var topicModel = PrepareForumTopicRowModel(topic, lastPosts);
                model.ForumTopics.Add(topicModel);
            }

            model.ViewAllLinkEnabled = false;
            model.ActiveDiscussionsFeedEnabled = _forumSettings.ActiveDiscussionsFeedEnabled;
            model.PostsPageSize = _forumSettings.PostsPageSize;

            return View(model);
        }

        public ActionResult ActiveDiscussionsRss(int forumId = 0)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var store = Services.StoreContext.CurrentStore;
            var language = Services.WorkContext.WorkingLanguage;
            var protocol = Services.WebHelper.IsCurrentConnectionSecured() ? "https" : "http";
            var selfLink = Url.Action("ActiveDiscussionsRSS", "Boards", null, protocol);
            var discussionLink = Url.Action("ActiveDiscussions", "Boards", null, protocol);

            var title = "{0} - {1}".FormatInvariant(store.Name, T("Forum.ActiveDiscussionsFeedTitle"));
            var feed = new SmartSyndicationFeed(new Uri(discussionLink), title, T("Forum.ActiveDiscussionsFeedDescription"));

            feed.AddNamespaces(false);
            feed.Init(selfLink, language);

            if (!_forumSettings.ActiveDiscussionsFeedEnabled)
            {
                return new RssActionResult { Feed = feed };
            }

            var items = new List<SyndicationItem>();
            var topics = _forumService.GetActiveTopics(forumId, _forumSettings.ActiveDiscussionsFeedCount);
            var viewsText = T("Forum.Views");
            var repliesText = T("Forum.Replies");

            foreach (var topic in topics)
            {
                var topicUrl = Url.RouteUrl("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() }, protocol);
                var synopsis = "{0}: {1}, {2}: {3}".FormatInvariant(repliesText, topic.NumReplies, viewsText, topic.Views);

                var item = feed.CreateItem(topic.Subject, synopsis, topicUrl, topic.LastPostTime ?? topic.UpdatedOnUtc);
                items.Add(item);
            }

            feed.Items = items;

            return new RssActionResult { Feed = feed };
        }

        #endregion

        #region Topic

        public ActionResult Topic(int id, int page = 1)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var topic = _forumService.GetTopicById(id);

            if (!IsTopicVisible(topic, customer))
            {
                return HttpNotFound();
            }

            var posts = _forumService.GetAllPosts(topic.Id, 0, true, page - 1, _forumSettings.PostsPageSize);

            // If no posts area loaded, redirect to the first page.
            if (posts.Count == 0 && page > 1)
            {
                return RedirectToRoute("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() });
            }

            // Update view count.
            try
            {
                if (!customer.Deleted && customer.Active && !customer.IsSystemAccount)
                {
                    topic.Views += 1;
                    _forumService.UpdateTopic(topic, false);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            var model = new ForumTopicPageModel
            {
                Id = topic.Id,
                Subject = topic.Subject,
                SeName = topic.GetSeName(),
                IsCustomerAllowedToEditTopic = _forumService.IsCustomerAllowedToEditTopic(customer, topic),
                IsCustomerAllowedToDeleteTopic = _forumService.IsCustomerAllowedToDeleteTopic(customer, topic),
                IsCustomerAllowedToMoveTopic = _forumService.IsCustomerAllowedToMoveTopic(customer, topic),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer),
                PostsPageIndex = posts.PageIndex,
                PostsPageSize = posts.PageSize,
                PostsTotalRecords = posts.TotalCount
            };

            if (model.IsCustomerAllowedToSubscribe)
            {
                model.WatchTopicText = T("Forum.WatchTopic");

                var forumTopicSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, topic.Id, 0, 1).FirstOrDefault();
                if (forumTopicSubscription != null)
                {
                    model.WatchTopicText = T("Forum.UnwatchTopic");
                }
            }

            foreach (var post in posts)
            {
                var postModel = new ForumPostModel
                {
                    Id = post.Id,
                    Published = post.Published,
                    ForumTopicId = post.TopicId,
                    ForumTopicSeName = topic.GetSeName(),
                    FormattedText = post.FormatPostText(),
                    IsCurrentCustomerAllowedToEditPost = _forumService.IsCustomerAllowedToEditPost(customer, post),
                    IsCurrentCustomerAllowedToDeletePost = _forumService.IsCustomerAllowedToDeletePost(customer, post),
                    CustomerId = post.CustomerId,
                    AllowViewingProfiles = _customerSettings.AllowViewingProfiles,
                    CustomerName = post.Customer.FormatUserName(_customerSettings, T, false),
                    IsCustomerForumModerator = post.Customer.IsForumModerator(),
                    IsCustomerGuest = post.Customer.IsGuest(),
                    ShowCustomersPostCount = _forumSettings.ShowCustomersPostCount,
                    ForumPostCount = post.Customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount),
                    ShowCustomersJoinDate = _customerSettings.ShowCustomersJoinDate,
                    CustomerJoinDate = post.Customer.CreatedOnUtc,
                    AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                    SignaturesEnabled = _forumSettings.SignaturesEnabled,
                    FormattedSignature = post.Customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature).FormatForumSignatureText(),
                    AllowVoting = _forumSettings.AllowCustomersToVoteOnPosts && post.CustomerId != customer.Id
                };

                if (postModel.AllowVoting)
                {
                    if (!_forumSettings.AllowGuestsToVoteOnPosts && customer.IsGuest())
                    {
                        postModel.AllowVoting = false;
                    }
                    else
                    {
                        postModel.Vote = post.ForumPostVotes.FirstOrDefault(x => x.CustomerId == customer.Id)?.Vote ?? false;
                        postModel.VoteCount = post.ForumPostVotes.Count;
                    }
                }

                postModel.PostCreatedOnStr = _forumSettings.RelativeDateTimeFormattingEnabled
                    ? post.CreatedOnUtc.RelativeFormat(true, "f")
                    : _dateTimeHelper.ConvertToUserTime(post.CreatedOnUtc, DateTimeKind.Utc).ToString("f");

                postModel.Avatar = post.Customer.ToAvatarModel(_genericAttributeService, _customerSettings, _mediaSettings, postModel.CustomerName, true);

                // Location.
                postModel.ShowCustomersLocation = _customerSettings.ShowCustomersLocation;
                if (_customerSettings.ShowCustomersLocation)
                {
                    var countryId = post.Customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
                    var country = _countryService.GetCountryById(countryId);
                    postModel.CustomerLocation = country != null ? country.GetLocalized(x => x.Name) : string.Empty;
                }

                // Page number is needed for creating post link in _ForumPost partial view.
                postModel.CurrentTopicPage = page;
                model.ForumPostModels.Add(postModel);
            }

            CreateForumBreadcrumb(topic: topic);
            SaveLastForumVisit(customer);

            return View(model);
        }

        [HttpPost]
        public ActionResult TopicWatch(int id)
        {
            var subscribed = false;
            var returnText = T("Forum.WatchTopic").Text;
            var customer = Services.WorkContext.CurrentCustomer;
            var topic = _forumService.GetTopicById(id);

            if (!IsTopicVisible(topic, customer) || !_forumService.IsCustomerAllowedToSubscribe(customer))
            {
                return Json(new { Subscribed = subscribed, Text = returnText, Error = true });
            }

            var subscription = _forumService.GetAllSubscriptions(customer.Id, 0, topic.Id, 0, 1).FirstOrDefault();
            if (subscription == null)
            {
                subscription = new ForumSubscription
                {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    TopicId = topic.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _forumService.InsertSubscription(subscription);
                subscribed = true;
                returnText = T("Forum.UnwatchTopic");
            }
            else
            {
                _forumService.DeleteSubscription(subscription);
                subscribed = false;
            }

            return Json(new { Subscribed = subscribed, Text = returnText, Error = false });
        }

        public ActionResult TopicMove(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var topic = _forumService.GetTopicById(id);

            if (!IsTopicVisible(topic, customer))
            {
                return HttpNotFound();
            }

            var model = new TopicMoveModel
            {
                Id = topic.Id,
                TopicSeName = topic.GetSeName(),
                ForumSelected = topic.ForumId,
                CustomerId = topic.CustomerId,
                IsCustomerAllowedToEdit = _forumService.IsCustomerAllowedToMoveTopic(customer, topic)
            };

            if (!model.IsCustomerAllowedToEdit && customer.Id != topic.CustomerId)
            {
                return new HttpUnauthorizedResult();
            }

            // Forums select box.
            model.Forums = new List<SelectListItem>();
            var groups = _forumService.GetAllForumGroups(Services.StoreContext.CurrentStore.Id);
            foreach (var group in groups)
            {
                var optGroup = new SelectListGroup { Name = group.GetLocalized(x => x.Name) };
                foreach (var forum in group.Forums.OrderBy(x => x.DisplayOrder))
                {
                    model.Forums.Add(new SelectListItem
                    {
                        Text = forum.GetLocalized(x => x.Name),
                        Value = forum.Id.ToString(),
                        Group = optGroup
                    });
                }
            }

            CreateForumBreadcrumb(topic: topic);

            return View(model);
        }

        [HttpPost]
        public ActionResult TopicMove(TopicMoveModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var topic = _forumService.GetTopicById(model.Id);

            if (!IsTopicVisible(topic, customer))
            {
                return HttpNotFound();
            }
            if (!_forumService.IsCustomerAllowedToMoveTopic(customer, topic))
            {
                return new HttpUnauthorizedResult();
            }

            var newForumId = model.ForumSelected;
            var forum = _forumService.GetForumById(newForumId);
            if (forum != null && topic.ForumId != newForumId)
            {
                _forumService.MoveTopic(topic.Id, newForumId);
            }

            return RedirectToRoute("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() });
        }

        [GdprConsent]
        public ActionResult TopicCreate(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var forum = _forumService.GetForumById(id);

            if (forum == null || !_storeMappingService.Authorize(forum.ForumGroup) || !_aclService.Authorize(forum.ForumGroup))
            {
                return HttpNotFound();
            }
            if (!_forumService.IsCustomerAllowedToCreateTopic(customer, forum))
            {
                return new HttpUnauthorizedResult();
            }

            var model = new EditForumTopicModel
            {
                Id = 0,
                IsEdit = false,
                Published = true,
                SeName = string.Empty,
                DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnForumPage,
                ForumId = forum.Id,
                ForumName = forum.GetLocalized(x => x.Name),
                ForumSeName = forum.GetSeName(),
                ForumEditor = _forumSettings.ForumEditor,
                IsModerator = customer.IsForumModerator(),
                TopicPriorities = ForumTopicTypesList(),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer),
                Subscribed = false,
            };

            CreateForumBreadcrumb(forum: forum);

            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [GdprConsent]
        public ActionResult TopicCreate(EditForumTopicModel model, string captchaError)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var forum = _forumService.GetForumById(model.ForumId);
            if (forum == null || !_storeMappingService.Authorize(forum.ForumGroup) || !_aclService.Authorize(forum.ForumGroup))
            {
                return HttpNotFound();
            }
            if (!_forumService.IsCustomerAllowedToCreateTopic(customer, forum))
            {
                return new HttpUnauthorizedResult();
            }

            if (_captchaSettings.ShowOnForumPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var topic = new ForumTopic
                    {
                        ForumId = forum.Id,
                        CustomerId = customer.Id,
                        Published = true,
                        TopicTypeId = (int)ForumTopicType.Normal
                    };

                    if (customer.IsForumModerator())
                    {
                        topic.Published = model.Published;
                        topic.TopicTypeId = model.TopicTypeId;
                    }

                    topic.Subject = _forumSettings.TopicSubjectMaxLength > 0 && model.Subject.Length > _forumSettings.TopicSubjectMaxLength
                        ? model.Subject.Substring(0, _forumSettings.TopicSubjectMaxLength)
                        : model.Subject;

                    _forumService.InsertTopic(topic, true);

                    var post = new ForumPost
                    {
                        TopicId = topic.Id,
                        CustomerId = customer.Id,
                        IPAddress = Services.WebHelper.GetCurrentIpAddress(),
                        Published = true
                    };

                    post.Text = _forumSettings.PostMaxLength > 0 && model.Text.Length > _forumSettings.PostMaxLength
                        ? model.Text.Substring(0, _forumSettings.PostMaxLength)
                        : model.Text;

                    _forumService.InsertPost(post, false);

                    topic.NumPosts = topic.Published ? 1 : 0;
                    topic.LastPostId = post.Id;
                    topic.LastPostCustomerId = post.CustomerId;
                    topic.LastPostTime = post.CreatedOnUtc;

                    _forumService.UpdateTopic(topic, false);

                    // Subscription.
                    if (_forumService.IsCustomerAllowedToSubscribe(customer))
                    {
                        if (model.Subscribed)
                        {
                            var forumSubscription = new ForumSubscription
                            {
                                SubscriptionGuid = Guid.NewGuid(),
                                CustomerId = customer.Id,
                                TopicId = topic.Id,
                                CreatedOnUtc = DateTime.UtcNow
                            };

                            _forumService.InsertSubscription(forumSubscription);
                        }
                    }

                    return RedirectToRoute("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Redisplay form.
            model.Id = 0;
            model.TopicPriorities = ForumTopicTypesList();
            model.IsEdit = false;
            model.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnForumPage;
            model.ForumId = forum.Id;
            model.ForumName = forum.GetLocalized(x => x.Name);
            model.ForumSeName = forum.GetSeName();
            model.IsModerator = customer.IsForumModerator();
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);
            model.ForumEditor = _forumSettings.ForumEditor;

            return View(model);
        }

        public ActionResult TopicEdit(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var topic = _forumService.GetTopicById(id);

            if (!IsTopicVisible(topic, customer))
            {
                return HttpNotFound();
            }

            var firstPost = topic.GetFirstPost(_forumService);
            var model = new EditForumTopicModel
            {
                Id = topic.Id,
                IsEdit = true,
                Published = topic.Published,
                SeName = topic.GetSeName(),
                DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnForumPage,
                TopicPriorities = ForumTopicTypesList(),
                ForumName = topic.Forum.GetLocalized(x => x.Name),
                ForumSeName = topic.Forum.GetSeName(),
                Text = firstPost?.Text,
                Subject = topic.Subject,
                TopicTypeId = topic.TopicTypeId,
                ForumId = topic.Forum.Id,
                ForumEditor = _forumSettings.ForumEditor,
                CustomerId = topic.CustomerId,
                IsModerator = customer.IsForumModerator(),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer),
                IsCustomerAllowedToEdit = _forumService.IsCustomerAllowedToEditTopic(customer, topic)
            };

            if (!model.IsCustomerAllowedToEdit)
            {
                return new HttpUnauthorizedResult();
            }

            // Subscription.
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, topic.Id, 0, 1).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

            CreateForumBreadcrumb(forum: topic.Forum, topic: topic);

            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        public ActionResult TopicEdit(EditForumTopicModel model, string captchaError)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var topic = _forumService.GetTopicById(model.Id);

            if (!IsTopicVisible(topic, customer))
            {
                return HttpNotFound();
            }
            if (!_forumService.IsCustomerAllowedToEditTopic(customer, topic))
            {
                return new HttpUnauthorizedResult();
            }

            if (_captchaSettings.ShowOnForumPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updateStatistics = false;
                    if (customer.IsForumModerator())
                    {
                        updateStatistics = topic.Published != model.Published;
                        topic.Published = model.Published;
                        topic.TopicTypeId = model.TopicTypeId;
                    }

                    topic.Subject = _forumSettings.TopicSubjectMaxLength > 0 && model.Subject.Length > _forumSettings.TopicSubjectMaxLength
                        ? model.Subject.Substring(0, _forumSettings.TopicSubjectMaxLength)
                        : model.Subject;

                    _forumService.UpdateTopic(topic, updateStatistics);

                    var text = _forumSettings.PostMaxLength > 0 && model.Text.Length > _forumSettings.PostMaxLength
                        ? model.Text.Substring(0, _forumSettings.PostMaxLength)
                        : model.Text;

                    var firstPost = topic.GetFirstPost(_forumService);
                    if (firstPost != null)
                    {
                        firstPost.Text = text;
                        _forumService.UpdatePost(firstPost, false);
                    }
                    else
                    {
                        firstPost = new ForumPost
                        {
                            TopicId = topic.Id,
                            CustomerId = topic.CustomerId,
                            Text = text,
                            IPAddress = Services.WebHelper.GetCurrentIpAddress(),
                            Published = true
                        };

                        _forumService.InsertPost(firstPost, false);
                    }

                    // Subscription.
                    if (_forumService.IsCustomerAllowedToSubscribe(customer))
                    {
                        var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, topic.Id, 0, 1).FirstOrDefault();

                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription
                                {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = customer.Id,
                                    TopicId = topic.Id,
                                    CreatedOnUtc = DateTime.UtcNow
                                };

                                _forumService.InsertSubscription(forumSubscription);
                            }
                        }
                        else
                        {
                            if (forumSubscription != null)
                            {
                                _forumService.DeleteSubscription(forumSubscription);
                            }
                        }
                    }

                    // Redirect to the topic page with the topic slug.
                    return RedirectToRoute("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Redisplay form.
            model.TopicPriorities = ForumTopicTypesList();
            model.IsEdit = true;
            model.Published = topic.Published;
            model.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnForumPage;
            model.ForumName = topic.Forum.GetLocalized(x => x.Name);
            model.ForumSeName = topic.Forum.GetSeName();
            model.ForumId = topic.Forum.Id;
            model.ForumEditor = _forumSettings.ForumEditor;
            model.CustomerId = customer.Id;
            model.IsModerator = customer.IsForumModerator();
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);

            return View(model);
        }

        public ActionResult TopicDelete(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var topic = _forumService.GetTopicById(id);

            if (!IsTopicVisible(topic, customer))
            {
                return HttpNotFound();
            }
            if (!_forumService.IsCustomerAllowedToDeleteTopic(customer, topic))
            {
                return new HttpUnauthorizedResult();
            }

            var forum = _forumService.GetForumById(topic.ForumId);
            _forumService.DeleteTopic(topic);

            if (forum != null)
            {
                return RedirectToRoute("ForumSlug", new { id = forum.Id, slug = forum.GetSeName() });
            }

            return RedirectToRoute("Boards");
        }

        #endregion

        #region Forum post

        [GdprConsent]
        public ActionResult PostCreate(int id, int? quote)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var topic = _forumService.GetTopicById(id);

            if (topic == null || !_storeMappingService.Authorize(topic.Forum.ForumGroup) || !_aclService.Authorize(topic.Forum.ForumGroup))
            {
                return HttpNotFound();
            }
            if (!_forumService.IsCustomerAllowedToCreatePost(customer, topic))
            {
                return new HttpUnauthorizedResult();
            }

            var model = new EditForumPostModel
            {
                Id = 0,
                ForumTopicId = topic.Id,
                IsEdit = false,
                Published = true,
                DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnForumPage,
                ForumEditor = _forumSettings.ForumEditor,
                ForumName = topic.Forum.GetLocalized(x => x.Name),
                ForumTopicSubject = topic.Subject,
                ForumTopicSeName = topic.GetSeName(),
                IsModerator = customer.IsForumModerator(),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer),
                Subscribed = false
            };

            // Subscription.
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, topic.Id, 0, 1).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

            // Insert the quoted text.
            var text = string.Empty;
            if (quote.HasValue)
            {
                var quotePost = _forumService.GetPostById(quote.Value);
                if (quotePost != null && quotePost.TopicId == topic.Id)
                {
                    var quotePostText = quotePost.Text;

                    switch (_forumSettings.ForumEditor)
                    {
                        case EditorType.SimpleTextBox:
                            text = string.Format("{0}:\n{1}\n", quotePost.Customer.FormatUserName(), quotePostText);
                            break;
                        case EditorType.BBCodeEditor:
                            text = string.Format("[quote={0}]{1}[/quote]", quotePost.Customer.FormatUserName(), BBCodeHelper.RemoveQuotes(quotePostText));
                            break;
                    }
                    model.Text = text;
                }
            }

            CreateForumBreadcrumb(forum: topic.Forum, topic: topic);
            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        [GdprConsent]
        public ActionResult PostCreate(EditForumPostModel model, string captchaError)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var topic = _forumService.GetTopicById(model.ForumTopicId);

            if (topic == null || !_storeMappingService.Authorize(topic.Forum.ForumGroup) || !_aclService.Authorize(topic.Forum.ForumGroup))
            {
                return HttpNotFound();
            }
            if (!_forumService.IsCustomerAllowedToCreatePost(customer, topic))
            {
                return new HttpUnauthorizedResult();
            }

            if (_captchaSettings.ShowOnForumPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var post = new ForumPost
                    {
                        TopicId = topic.Id,
                        CustomerId = customer.Id,
                        IPAddress = Services.WebHelper.GetCurrentIpAddress(),
                        Published = true
                    };

                    if (customer.IsForumModerator())
                    {
                        post.Published = model.Published;
                    }

                    post.Text = _forumSettings.PostMaxLength > 0 && model.Text.Length > _forumSettings.PostMaxLength
                        ? model.Text.Substring(0, _forumSettings.PostMaxLength)
                        : model.Text;

                    _forumService.InsertPost(post, true);

                    // Subscription.
                    if (_forumService.IsCustomerAllowedToSubscribe(customer))
                    {
                        var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, post.TopicId, 0, 1).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription
                                {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = customer.Id,
                                    TopicId = post.TopicId,
                                    CreatedOnUtc = DateTime.UtcNow
                                };

                                _forumService.InsertSubscription(forumSubscription);
                            }
                        }
                        else
                        {
                            if (forumSubscription != null)
                            {
                                _forumService.DeleteSubscription(forumSubscription);
                            }
                        }
                    }

                    var pageSize = _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 20;
                    var pageIndex = _forumService.CalculateTopicPageIndex(post.TopicId, pageSize, post.Id) + 1;

                    var url = pageIndex > 1
                        ? Url.RouteUrl("TopicSlug", new { id = post.TopicId, slug = post.ForumTopic.GetSeName(), page = pageIndex })
                        : Url.RouteUrl("TopicSlug", new { id = post.TopicId, slug = post.ForumTopic.GetSeName() });

                    return Redirect(string.Concat(url, "#", post.Id));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Redisplay form.
            model.Id = 0;
            model.IsEdit = false;
            model.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnForumPage;
            model.ForumName = topic.Forum.GetLocalized(x => x.Name);
            model.ForumTopicId = topic.Id;
            model.ForumTopicSubject = topic.Subject;
            model.ForumTopicSeName = topic.GetSeName();
            model.IsModerator = customer.IsForumModerator();
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);
            model.ForumEditor = _forumSettings.ForumEditor;

            return View(model);
        }

        public ActionResult PostEdit(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var post = _forumService.GetPostById(id);

            if (post == null || !_storeMappingService.Authorize(post.ForumTopic.Forum.ForumGroup) || !_aclService.Authorize(post.ForumTopic.Forum.ForumGroup))
            {
                return HttpNotFound();
            }

            var firstPost = post.ForumTopic.GetFirstPost(_forumService);

            var model = new EditForumPostModel
            {
                Id = post.Id,
                IsEdit = true,
                Published = post.Published,
                IsFirstPost = firstPost?.Id == post.Id,
                ForumTopicId = post.ForumTopic.Id,
                DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnForumPage,
                ForumEditor = _forumSettings.ForumEditor,
                ForumName = post.ForumTopic.Forum.GetLocalized(x => x.Name),
                ForumTopicSubject = post.ForumTopic.Subject,
                ForumTopicSeName = post.ForumTopic.GetSeName(),
                Subscribed = false,
                Text = post.Text,
                CustomerId = customer.Id,
                IsModerator = customer.IsForumModerator(),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer),
                IsCustomerAllowedToEdit = _forumService.IsCustomerAllowedToEditPost(customer, post)
            };

            if (!model.IsCustomerAllowedToEdit)
            {
                return new HttpUnauthorizedResult();
            }

            // Subscription.
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, post.ForumTopic.Id, 0, 1).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

            CreateForumBreadcrumb(forum: post.ForumTopic.Forum, topic: post.ForumTopic);

            return View(model);
        }

        [HttpPost]
        [ValidateCaptcha]
        public ActionResult PostEdit(EditForumPostModel model, string captchaError)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var post = _forumService.GetPostById(model.Id);

            if (post == null || !_storeMappingService.Authorize(post.ForumTopic.Forum.ForumGroup) || !_aclService.Authorize(post.ForumTopic.Forum.ForumGroup))
            {
                return HttpNotFound();
            }
            if (!_forumService.IsCustomerAllowedToEditPost(customer, post))
            {
                return new HttpUnauthorizedResult();
            }

            if (_captchaSettings.ShowOnForumPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var updateStatistics = false;
                    if (customer.IsForumModerator())
                    {
                        // Do not allow to unpublish first post. NumReplies would be wrong. Unpublish topic instead.
                        var firstPost = post.ForumTopic.GetFirstPost(_forumService);
                        if (firstPost?.Id != post.Id)
                        {
                            updateStatistics = post.Published != model.Published;
                            post.Published = model.Published;
                        }
                    }

                    post.Text = _forumSettings.PostMaxLength > 0 && model.Text.Length > _forumSettings.PostMaxLength
                        ? model.Text.Substring(0, _forumSettings.PostMaxLength)
                        : model.Text;

                    _forumService.UpdatePost(post, updateStatistics);

                    // Subscription.
                    if (_forumService.IsCustomerAllowedToSubscribe(customer))
                    {
                        var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, post.TopicId, 0, 1).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription
                                {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = customer.Id,
                                    TopicId = post.TopicId,
                                    CreatedOnUtc = DateTime.UtcNow
                                };

                                _forumService.InsertSubscription(forumSubscription);
                            }
                        }
                        else
                        {
                            if (forumSubscription != null)
                            {
                                _forumService.DeleteSubscription(forumSubscription);
                            }
                        }
                    }

                    var pageSize = _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 20;
                    var pageIndex = _forumService.CalculateTopicPageIndex(post.TopicId, pageSize, post.Id) + 1;

                    var url = pageIndex > 1
                        ? Url.RouteUrl("TopicSlug", new { id = post.TopicId, slug = post.ForumTopic.GetSeName(), page = pageIndex })
                        : Url.RouteUrl("TopicSlug", new { id = post.TopicId, slug = post.ForumTopic.GetSeName() });

                    return Redirect(string.Concat(url, "#", post.Id));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Redisplay form.
            model.IsEdit = true;
            model.Published = post.Published;
            model.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnForumPage;
            model.ForumName = post.ForumTopic.Forum.GetLocalized(x => x.Name);
            model.ForumTopicId = post.ForumTopic.Id;
            model.ForumTopicSubject = post.ForumTopic.Subject;
            model.ForumTopicSeName = post.ForumTopic.GetSeName();
            model.Id = post.Id;
            model.ForumEditor = _forumSettings.ForumEditor;
            model.CustomerId = customer.Id;
            model.IsModerator = customer.IsForumModerator();
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);

            return View(model);
        }

        public ActionResult PostDelete(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var post = _forumService.GetPostById(id);

            if (post == null || !_storeMappingService.Authorize(post.ForumTopic.Forum.ForumGroup) || !_aclService.Authorize(post.ForumTopic.Forum.ForumGroup))
            {
                return HttpNotFound();
            }
            if (!_forumService.IsCustomerAllowedToDeletePost(Services.WorkContext.CurrentCustomer, post))
            {
                return new HttpUnauthorizedResult();
            }

            var topic = post.ForumTopic;
            var forumId = topic.Forum.Id;
            var forumSlug = topic.Forum.GetSeName();

            _forumService.DeletePost(post);

            // Get topic one more time because it can be deleted (first or only post deleted).
            topic = _forumService.GetTopicById(post.TopicId);
            if (topic == null)
            {
                return RedirectToRoute("ForumSlug", new { id = forumId, slug = forumSlug });
            }
            else
            {
                return RedirectToRoute("TopicSlug", new { id = topic.Id, slug = topic.GetSeName() });
            }
        }

        [HttpPost]
        public ActionResult PostVote(int id, bool vote)
        {
            if (!_forumSettings.ForumsEnabled || !_forumSettings.AllowCustomersToVoteOnPosts)
            {
                return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var post = _forumService.GetPostById(id);

            if (post == null || !_storeMappingService.Authorize(post.ForumTopic.Forum.ForumGroup) || !_aclService.Authorize(post.ForumTopic.Forum.ForumGroup))
            {
                return HttpNotFound();
            }

            if (!_forumSettings.AllowGuestsToVoteOnPosts && customer.IsGuest())
            {
                return Json(new { success = false, message = T("Forum.Post.Vote.OnlyRegistered").Text });
            }

            // Do not allow to vote for own posts.
            if (post.CustomerId == customer.Id)
            {
                return Json(new { success = false, message = T("Forum.Post.Vote.OwnPostNotAllowed").Text });
            }

            var voteEntity = post.ForumPostVotes.FirstOrDefault(x => x.CustomerId == customer.Id);
            var voteCount = post.ForumPostVotes.Count;

            if (vote)
            {
                if (voteEntity == null)
                {
                    voteEntity = new ForumPostVote
                    {
                        ForumPostId = post.Id,
                        Vote = true,
                        CustomerId = customer.Id,
                        IpAddress = Services.WebHelper.GetCurrentIpAddress()
                    };
                    _customerContentService.InsertCustomerContent(voteEntity);
                    ++voteCount;
                }
                else
                {
                    voteEntity.Vote = true;
                    _customerContentService.UpdateCustomerContent(voteEntity);
                }
            }
            else
            {
                if (voteEntity != null)
                {
                    _customerContentService.DeleteCustomerContent(voteEntity);
                    --voteCount;
                }
            }

            return Json(new
            {
                success = true,
                message = T("Forum.Post.Vote.SuccessfullyVoted").Text,
                voteCount,
                voteCountString = voteCount.ToString("N0")
            });
        }

        #endregion

        #region Search

        [ChildActionOnly]
        public ActionResult SearchBox()
        {
            var currentTerm = _queryFactory.Current?.Term;

            var model = new SearchBoxModel
            {
                Origin = "Boards/Search",
                SearchUrl = Url.RouteUrl("BoardSearch"),
                InstantSearchUrl = Url.Action("InstantSearch", "Boards"),
                InputPlaceholder = T("Forum.SearchForumsTooltip"),
                InstantSearchEnabled = _searchSettings.InstantSearchEnabled,
                SearchTermMinimumLength = _searchSettings.InstantSearchTermMinLength,
                CurrentQuery = currentTerm
            };

            return PartialView("~/Views/Search/Partials/SearchBox.cshtml", model);
        }

        [ChildActionOnly]
        public ActionResult Filters(IForumSearchResultModel model)
        {
            if (model == null)
            {
                return new EmptyResult();
            }

            // Set facet counters to 0 because they refer to posts, not topics, and would confuse here.
            foreach (var group in model.SearchResult.Facets.Values)
            {
                group.Facets.Each(x => x.HitCount = 0);
            }

            ViewBag.TemplateProvider = _templateProvider.Value;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult InstantSearch(ForumSearchQuery query)
        {
            if (!_forumSettings.ForumsEnabled || string.IsNullOrWhiteSpace(query.Term) || query.Term.Length < _searchSettings.InstantSearchTermMinLength)
            {
                return Content(string.Empty);
            }

            query
                .BuildFacetMap(false)
                .Slice(0, Math.Min(16, _searchSettings.InstantSearchNumberOfHits))
                .SortBy(ForumTopicSorting.Relevance);

            var result = _forumSearchService.Search(query) ?? new ForumSearchResult(query);

            var model = new ForumSearchResultModel(query)
            {
                SearchResult = result,
                Term = query.Term,
                TotalCount = result.TotalHitsCount
            };

            model.AddSpellCheckerSuggestions(result.SpellCheckerSuggestions, T, x => Url.RouteUrl("BoardSearch", new { q = x }));

            if (result.Hits.Any())
            {
                var processedIds = new HashSet<int>();
                var hitGroup = new SearchResultModelBase.HitGroup(model)
                {
                    Name = "InstantSearchHits",
                    DisplayName = T("Search.Hits"),
                    Ordinal = 1
                };

                foreach (var post in result.Hits)
                {
                    if (processedIds.Add(post.TopicId))
                    {
                        hitGroup.Hits.Add(new SearchResultModelBase.HitItem
                        {
                            Label = post.ForumTopic.Subject,
                            Url = Url.RouteUrl("TopicSlug", new { id = post.TopicId, slug = post.ForumTopic.GetSeName() }) + string.Concat("#", post.Id)
                        });
                    }
                }

                model.HitGroups.Add(hitGroup);
            }

            return PartialView(model);
        }

        [RewriteUrl(SslRequirement.No)]
        public ActionResult Search(ForumSearchQuery query)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            CreateForumBreadcrumb();
            _breadcrumb.Track(new MenuItem { Text = T("Forum.Search") });

            ForumSearchResult result = null;
            var language = Services.WorkContext.WorkingLanguage;
            var model = new ForumSearchResultModel(query);
            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.AllowSorting = _forumSettings.AllowSorting;

            // Sorting.
            if (model.AllowSorting)
            {
                model.CurrentSortOrder = query?.CustomData.Get("CurrentSortOrder").Convert<int?>();

                model.AvailableSortOptions = Services.Cache.Get("pres:forumsortoptions-{0}".FormatInvariant(language.Id), () =>
                {
                    var dict = new Dictionary<int, string>();
                    foreach (ForumTopicSorting val in Enum.GetValues(typeof(ForumTopicSorting)))
                    {
                        if (val == ForumTopicSorting.Initial)
                            continue;

                        dict[(int)val] = val.GetLocalizedEnum(Services.Localization, Services.WorkContext);
                    }

                    return dict;
                });

                if (model.CurrentSortOrderName.IsEmpty())
                {
                    model.CurrentSortOrderName = model.AvailableSortOptions.Get(model.CurrentSortOrder ?? 1) ?? model.AvailableSortOptions.First().Value;
                }
            }

            if (query.Term.HasValue() && query.Term.Length < _searchSettings.InstantSearchTermMinLength)
            {
                model.SearchResult = new ForumSearchResult(query);
                model.Error = T("Search.SearchTermMinimumLengthIsNCharacters", _searchSettings.InstantSearchTermMinLength);
                return View(model);
            }

            try
            {
                if (query.Term.HasValue())
                {
                    result = _forumSearchService.Search(query);

                    if (result.TotalHitsCount == 0 && result.SpellCheckerSuggestions.Any())
                    {
                        // No matches, but spell checker made a suggestion.
                        // We implicitly search again with the first suggested term.
                        var oldSuggestions = result.SpellCheckerSuggestions;
                        var oldTerm = query.Term;
                        query.Term = oldSuggestions[0];

                        result = _forumSearchService.Search(query);

                        if (result.TotalHitsCount > 0)
                        {
                            model.AttemptedTerm = oldTerm;
                            // Restore the original suggestions.
                            result.SpellCheckerSuggestions = oldSuggestions.Where(x => x != query.Term).ToArray();
                        }
                        else
                        {
                            query.Term = oldTerm;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                model.Error = ex.ToString();
            }

            model.SearchResult = result ?? new ForumSearchResult(query);
            model.Term = query.Term;
            model.TotalCount = model.SearchResult.TotalHitsCount;

            PrepareSearchResult(model, null);

            return View(model);
        }

        // Ajax.
        [HttpPost]
        public ActionResult Search(ForumSearchQuery query, int[] renderedTopicIds)
        {
            if (!_forumSettings.ForumsEnabled || query.Term.IsEmpty() || query.Term.Length < _searchSettings.InstantSearchTermMinLength)
            {
                return Content(string.Empty);
            }

            query.BuildFacetMap(false).CheckSpelling(0);

            var model = new ForumSearchResultModel(query);

            try
            {
                model.SearchResult = _forumSearchService.Search(query);
            }
            catch (Exception ex)
            {
                model.SearchResult = new ForumSearchResult(query);
                model.Error = ex.ToString();
            }

            model.PostsPageSize = _forumSettings.PostsPageSize;
            model.Term = query.Term;
            model.TotalCount = model.SearchResult.TotalHitsCount;

            PrepareSearchResult(model, renderedTopicIds);

            return PartialView("SearchHits", model);
        }

        private void PrepareSearchResult(ForumSearchResultModel model, int[] renderedTopicIds)
        {
            // The search result may contain duplicate topics.
            // Make sure that no topic is rendered more than once.
            var hits = model.SearchResult.Hits;
            var lastPostIds = hits
                .Where(x => x.ForumTopic.LastPostId != 0)
                .Select(x => x.ForumTopic.LastPostId)
                .Distinct()
                .ToArray();

            var lastPosts = _forumService.GetPostsByIds(lastPostIds).ToDictionary(x => x.Id);
            var renderedIds = new HashSet<int>(renderedTopicIds ?? new int[0]);
            var hitModels = new List<ForumTopicRowModel>();

            foreach (var post in hits)
            {
                if (renderedIds.Add(post.TopicId))
                {
                    var hitModel = PrepareForumTopicRowModel(post.ForumTopic, lastPosts, post);
                    hitModels.Add(hitModel);
                }
            }

            model.PagedList = new PagedList<ForumTopicRowModel>(
                hitModels,
                hits.PageIndex,
                hits.PageSize,
                model.TotalCount);

            model.CumulativeHitCount = renderedIds.Count;
        }

        #endregion
    }
}
