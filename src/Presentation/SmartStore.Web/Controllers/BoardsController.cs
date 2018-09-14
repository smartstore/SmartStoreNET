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
using SmartStore.Services.Seo;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Models.Boards;
using SmartStore.Web.Models.Search;

namespace SmartStore.Web.Controllers
{
    [RequireHttpsByConfig(SslRequirement.No)]
    public partial class BoardsController : PublicControllerBase
    {
        private readonly IForumService _forumService;
        private readonly IPictureService _pictureService;
        private readonly ICountryService _countryService;
        private readonly IForumSearchService _forumSearchService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ForumSettings _forumSettings;
        private readonly ForumSearchSettings _searchSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly MediaSettings _mediaSettings;
        private readonly IDateTimeHelper _dateTimeHelper;
		private readonly IBreadcrumb _breadcrumb;
        private readonly Lazy<IFacetTemplateProvider> _templateProvider;
        private readonly IForumSearchQueryFactory _queryFactory;

        public BoardsController(
            IForumService forumService,
            IPictureService pictureService,
            ICountryService countryService,
            IForumSearchService forumSearchService,
            IGenericAttributeService genericAttributeService,
            ForumSettings forumSettings,
            ForumSearchSettings searchSettings,
            CustomerSettings customerSettings,
            MediaSettings mediaSettings,
            IDateTimeHelper dateTimeHelper,
			IBreadcrumb breadcrumb,
            Lazy<IFacetTemplateProvider> templateProvider,
            IForumSearchQueryFactory queryFactory)
        {
            _forumService = forumService;
            _pictureService = pictureService;
            _countryService = countryService;
            _forumSearchService = forumSearchService;
            _genericAttributeService = genericAttributeService;
            _forumSettings = forumSettings;
            _searchSettings = searchSettings;
            _customerSettings = customerSettings;
            _mediaSettings = mediaSettings;
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
            var topicModel = new ForumTopicRowModel
            {
                Id = topic.Id,
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
                CustomerName = topic.Customer.FormatUserName(true),
                IsCustomerGuest = topic.Customer.IsGuest(),
                PostsPageSize = _forumSettings.PostsPageSize
            };

            if (topic.LastPostId != 0 && lastPosts.TryGetValue(topic.LastPostId, out var lastPost))
            {
                PrepareLastPostModel(topicModel.LastPost, lastPost);
            }

            return topicModel;
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

        private ForumGroupModel PrepareForumGroupModel(ForumGroup forumGroup)
        {
            var forumGroupModel = new ForumGroupModel
            {
                Id = forumGroup.Id,
                Name = forumGroup.GetLocalized(x => x.Name),
                Description = forumGroup.GetLocalized(x => x.Description),
				SeName = forumGroup.GetSeName()
            };

            var forums = _forumService.GetAllForumsByGroupId(forumGroup.Id);

            var lastPostIds = forums
                .Where(x => x.LastPostId != 0)
                .Select(x => x.LastPostId)
                .Distinct()
                .ToArray();

            var lastPosts = _forumService.GetPostsByIds(lastPostIds).ToDictionary(x => x.Id);

            foreach (var forum in forums)
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

        private IEnumerable<SelectListItem> ForumGroupsForumsList()
        {
            var forumsList = new List<SelectListItem>();
            var separator = "--";
            var store = Services.StoreContext.CurrentStore;
            var forumGroups = _forumService.GetAllForumGroups(store.Id);

            foreach (var fg in forumGroups)
            {
                // Add the forum group with Value of 0 so it won't be used as a target forum.
                forumsList.Add(new SelectListItem { Text = fg.GetLocalized(x => x.Name), Value = "0" });

                var forums = _forumService.GetAllForumsByGroupId(fg.Id);
                foreach (var f in forums)
                {
                    forumsList.Add(new SelectListItem { Text = string.Format("{0}{1}", separator, f.GetLocalized(x => x.Name)), Value = f.Id.ToString() });
                }
            }

            return forumsList;
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
				var forumName = group.GetLocalized(x => x.Name);
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
                    _genericAttributeService.SaveAttribute(
                        customer,
                        SystemCustomerAttributeNames.LastForumVisit,
                        DateTime.UtcNow,
                        Services.StoreContext.CurrentStore.Id);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
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
            var forumGroups = _forumService.GetAllForumGroups(store.Id);

            var model = new BoardsIndexModel
            {
                CurrentTime = _dateTimeHelper.ConvertToUserTime(DateTime.UtcNow)
            };

            foreach (var forumGroup in forumGroups)
            {
                var forumGroupModel = PrepareForumGroupModel(forumGroup);
                model.ForumGroups.Add(forumGroupModel);
            }

            return View(model);
        }

        public ActionResult ForumGroup(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
				return HttpNotFound();
            }

            var forumGroup = _forumService.GetForumGroupById(id);
            if (forumGroup == null)
            {
                return HttpNotFound();
            }

            var model = PrepareForumGroupModel(forumGroup);
			CreateForumBreadcrumb(group: forumGroup);

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
            if (forum == null)
            {
                return HttpNotFound();
            }

            var model = new ForumPageModel();
            model.Id = forum.Id;
            model.Name = forum.GetLocalized(x => x.Name);
            model.SeName = forum.GetSeName();
            model.Description = forum.GetLocalized(x => x.Description);

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

            var pageSize = _forumSettings.TopicsPageSize > 0 ? _forumSettings.TopicsPageSize : 10;
            var topics = _forumService.GetAllTopics(forum.Id, (page - 1), pageSize);

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

            model.TopicPageSize = topics.PageSize;
            model.TopicTotalRecords = topics.TotalCount;
            model.TopicPageIndex = topics.PageIndex;
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);
            model.ForumFeedsEnabled = _forumSettings.ForumFeedsEnabled;
            model.PostsPageSize = _forumSettings.PostsPageSize;

			CreateForumBreadcrumb(forum: forum);
            SaveLastForumVisit(customer);

            return View(model);
        }

		[Compress]
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
            if (forum == null)
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

            if (forum == null)
            {
                return Json(new { Subscribed = subscribed, Text = returnText, Error = true });
            }

            if (!_forumService.IsCustomerAllowedToSubscribe(customer))
            {
                return Json(new { Subscribed = subscribed, Text = returnText, Error = true });
            }

            var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, forum.Id, 0, 0, 1).FirstOrDefault();
            if (forumSubscription == null)
            {
                forumSubscription = new ForumSubscription
                {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    ForumId = forum.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _forumService.InsertSubscription(forumSubscription);
                subscribed = true;
                returnText = T("Forum.UnwatchForum");
            }
            else
            {
                _forumService.DeleteSubscription(forumSubscription);
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

        [Compress]
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
            var forumTopic = _forumService.GetTopicById(id);

            if (forumTopic != null)
            {
                var posts = _forumService.GetAllPosts(forumTopic.Id, 0, true, page - 1, _forumSettings.PostsPageSize);

                // If no posts area loaded, redirect to the first page.
                if (posts.Count == 0 && page > 1)
                {
                    return RedirectToRoute("TopicSlug", new { id = forumTopic.Id, slug = forumTopic.GetSeName() });
                }

                // Update view count.
                try
                {
                    if (!customer.Deleted && customer.Active && !customer.IsSystemAccount)
                    {
                        forumTopic.Views += 1;
                        _forumService.UpdateTopic(forumTopic);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }

                var model = new ForumTopicPageModel();
                model.Id = forumTopic.Id;
                model.Subject= forumTopic.Subject;
                model.SeName = forumTopic.GetSeName();
                model.IsCustomerAllowedToEditTopic = _forumService.IsCustomerAllowedToEditTopic(customer, forumTopic);
                model.IsCustomerAllowedToDeleteTopic = _forumService.IsCustomerAllowedToDeleteTopic(customer, forumTopic);
                model.IsCustomerAllowedToMoveTopic = _forumService.IsCustomerAllowedToMoveTopic(customer, forumTopic);
                model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);

                if (model.IsCustomerAllowedToSubscribe)
                {
                    model.WatchTopicText = T("Forum.WatchTopic");

                    var forumTopicSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, forumTopic.Id, 0, 1).FirstOrDefault();
                    if (forumTopicSubscription != null)
                    {
                        model.WatchTopicText = T("Forum.UnwatchTopic");
                    }
                }
                
				model.PostsPageIndex = posts.PageIndex;
                model.PostsPageSize = posts.PageSize;
                model.PostsTotalRecords = posts.TotalCount;

                foreach (var post in posts)
                {
                    var forumPostModel = new ForumPostModel
                    {
                        Id = post.Id,
                        ForumTopicId =  post.TopicId,
                        ForumTopicSeName = forumTopic.GetSeName(),
                        FormattedText = post.FormatPostText(),
                        IsCurrentCustomerAllowedToEditPost = _forumService.IsCustomerAllowedToEditPost(customer, post),
                        IsCurrentCustomerAllowedToDeletePost = _forumService.IsCustomerAllowedToDeletePost(customer, post),
                        CustomerId = post.CustomerId,
                        AllowViewingProfiles = _customerSettings.AllowViewingProfiles,
                        CustomerName = post.Customer.FormatUserName(true),
                        IsCustomerForumModerator = post.Customer.IsForumModerator(),
                        IsCustomerGuest= post.Customer.IsGuest(),
                        ShowCustomersPostCount = _forumSettings.ShowCustomersPostCount,
                        ForumPostCount = post.Customer.GetAttribute<int>(SystemCustomerAttributeNames.ForumPostCount),
                        ShowCustomersJoinDate = _customerSettings.ShowCustomersJoinDate,
                        CustomerJoinDate = post.Customer.CreatedOnUtc,
                        AllowPrivateMessages = _forumSettings.AllowPrivateMessages,
                        SignaturesEnabled = _forumSettings.SignaturesEnabled,
                        FormattedSignature = post.Customer.GetAttribute<string>(SystemCustomerAttributeNames.Signature).FormatForumSignatureText(),
                    };

                    forumPostModel.PostCreatedOnStr = _forumSettings.RelativeDateTimeFormattingEnabled
                        ? post.CreatedOnUtc.RelativeFormat(true, "f")
                        : _dateTimeHelper.ConvertToUserTime(post.CreatedOnUtc, DateTimeKind.Utc).ToString("f");
                    
                    if (_customerSettings.AllowCustomersToUploadAvatars)
                    {
                        var avatarId = post.Customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId);
                        forumPostModel.CustomerAvatarUrl = _pictureService.GetUrl(avatarId, _mediaSettings.AvatarPictureSize, FallbackPictureType.NoFallback);
                        if (forumPostModel.CustomerAvatarUrl.IsEmpty() && _customerSettings.DefaultAvatarEnabled)
                        {
                            forumPostModel.CustomerAvatarUrl = _pictureService.GetFallbackUrl(_mediaSettings.AvatarPictureSize, FallbackPictureType.Avatar);
                        }
                    }

                    // Location.
                    forumPostModel.ShowCustomersLocation = _customerSettings.ShowCustomersLocation;
                    if (_customerSettings.ShowCustomersLocation)
                    {
                        var countryId = post.Customer.GetAttribute<int>(SystemCustomerAttributeNames.CountryId);
                        var country = _countryService.GetCountryById(countryId);
                        forumPostModel.CustomerLocation = country != null ? country.GetLocalized(x => x.Name) : string.Empty;
                    }

                    // Page number is needed for creating post link in _ForumPost partial view.
                    forumPostModel.CurrentTopicPage = page;
                    model.ForumPostModels.Add(forumPostModel);
                }

				CreateForumBreadcrumb(topic: forumTopic);
                SaveLastForumVisit(customer);

                return View(model);
            }

            return RedirectToRoute("Boards");
        }

        [HttpPost]
        public ActionResult TopicWatch(int id)
        {
            var subscribed = false;
            var returnText = T("Forum.WatchTopic").Text;
            var customer = Services.WorkContext.CurrentCustomer;
            var forumTopic = _forumService.GetTopicById(id);

            if (forumTopic == null)
            {
                return Json(new { Subscribed = subscribed, Text = returnText, Error = true });
            }

            if (!_forumService.IsCustomerAllowedToSubscribe(customer))
            {
                return Json(new { Subscribed = subscribed, Text = returnText, Error = true });
            }

            var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, forumTopic.Id, 0, 1).FirstOrDefault();

            if (forumSubscription == null)
            {
                forumSubscription = new ForumSubscription
                {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    TopicId = forumTopic.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                _forumService.InsertSubscription(forumSubscription);
                subscribed = true;
                returnText = T("Forum.UnwatchTopic");
            }
            else
            {
                _forumService.DeleteSubscription(forumSubscription);
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

            var forumTopic = _forumService.GetTopicById(id);

            if (forumTopic == null)
            {
				return HttpNotFound();
            }

            var model = new TopicMoveModel();
            model.ForumList = ForumGroupsForumsList();
            model.Id = forumTopic.Id;
            model.TopicSeName = forumTopic.GetSeName();
            model.ForumSelected = forumTopic.ForumId;

			CreateForumBreadcrumb(topic: forumTopic);

			return View(model);
        }

        [HttpPost]
        public ActionResult TopicMove(TopicMoveModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return RedirectToRoute("HomePage");
            }

            var forumTopic = _forumService.GetTopicById(model.Id);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            var newForumId = model.ForumSelected;
            var forum = _forumService.GetForumById(newForumId);

            if (forum != null && forumTopic.ForumId != newForumId)
            {
                _forumService.MoveTopic(forumTopic.Id, newForumId);
            }

            return RedirectToRoute("TopicSlug", new { id = forumTopic.Id, slug = forumTopic.GetSeName() });
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
            if (forum == null)
            {
				return HttpNotFound();
            }

            if (_forumService.IsCustomerAllowedToCreateTopic(customer, forum) == false)
            {
                return new HttpUnauthorizedResult();
            }

            var model = new EditForumTopicModel();
            model.Id = 0;
            model.IsEdit = false;
            model.ForumId = forum.Id;
            model.ForumName = forum.GetLocalized(x => x.Name);
            model.ForumSeName = forum.GetSeName();
            model.ForumEditor = _forumSettings.ForumEditor;
            model.IsCustomerAllowedToSetTopicPriority = _forumService.IsCustomerAllowedToSetTopicPriority(customer);
            model.TopicPriorities = ForumTopicTypesList();
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);
            model.Subscribed = false;

			CreateForumBreadcrumb(forum: forum);

			return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
		[GdprConsent]
		public ActionResult TopicCreate(EditForumTopicModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
				return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var forum = _forumService.GetForumById(model.ForumId);
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!_forumService.IsCustomerAllowedToCreateTopic(customer, forum))
                    {
                        return new HttpUnauthorizedResult();
                    }

                    var subject = model.Subject;
                    var maxSubjectLength = _forumSettings.TopicSubjectMaxLength;
                    if (maxSubjectLength > 0 && subject.Length > maxSubjectLength)
                    {
                        subject = subject.Substring(0, maxSubjectLength);
                    }

                    var text = model.Text;
                    var maxPostLength = _forumSettings.PostMaxLength;
                    if (maxPostLength > 0 && text.Length > maxPostLength)
                    {
                        text = text.Substring(0, maxPostLength);
                    }

                    var topicType = ForumTopicType.Normal;
					var utcNow = DateTime.UtcNow;
                    var ipAddress = Services.WebHelper.GetCurrentIpAddress();

                    if (_forumService.IsCustomerAllowedToSetTopicPriority(customer))
                    {
                        topicType = (ForumTopicType)Enum.ToObject(typeof (ForumTopicType), model.TopicTypeId);
                    }

                    var forumTopic = new ForumTopic
                    {
                        ForumId = forum.Id,
                        CustomerId = customer.Id,
                        TopicTypeId = (int) topicType,
                        Subject = subject
                    };
                    _forumService.InsertTopic(forumTopic, true);

                    var forumPost = new ForumPost
                    {
                        TopicId = forumTopic.Id,
                        CustomerId = customer.Id,
                        Text = text,
                        IPAddress = ipAddress,
                    };
                    _forumService.InsertPost(forumPost, false);

                    forumTopic.NumPosts = 1;
                    forumTopic.LastPostId = forumPost.Id;
                    forumTopic.LastPostCustomerId = forumPost.CustomerId;
                    forumTopic.LastPostTime = forumPost.CreatedOnUtc;

                    _forumService.UpdateTopic(forumTopic);

                    // Subscription.
                    if (_forumService.IsCustomerAllowedToSubscribe(customer))
                    {
                        if (model.Subscribed)
                        {
                            var forumSubscription = new ForumSubscription
                            {
                                SubscriptionGuid = Guid.NewGuid(),
                                CustomerId = customer.Id,
                                TopicId = forumTopic.Id,
                                CreatedOnUtc = utcNow
                            };

                            _forumService.InsertSubscription(forumSubscription);
                        }
                    }

                    return RedirectToRoute("TopicSlug", new {id = forumTopic.Id, slug = forumTopic.GetSeName()});
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Redisplay form.
            model.TopicPriorities = ForumTopicTypesList();
            model.IsEdit = false;
            model.ForumId = forum.Id;
            model.ForumName = forum.GetLocalized(x => x.Name);
            model.ForumSeName = forum.GetSeName();
            model.Id = 0;
            model.IsCustomerAllowedToSetTopicPriority = _forumService.IsCustomerAllowedToSetTopicPriority(customer);
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
            var forumTopic = _forumService.GetTopicById(id);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            if (!_forumService.IsCustomerAllowedToEditTopic(customer, forumTopic))
            {
                return new HttpUnauthorizedResult();
            }

            var forum = forumTopic.Forum;
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

			var firstPost = forumTopic.GetFirstPost(_forumService);
            var model = new EditForumTopicModel();

            model.IsEdit = true;
            model.TopicPriorities = ForumTopicTypesList();
            model.ForumName = forum.GetLocalized(x => x.Name);
            model.ForumSeName = forum.GetSeName();
            model.Text = firstPost.Text;
            model.Subject = forumTopic.Subject;
            model.TopicTypeId = forumTopic.TopicTypeId;
            model.Id = forumTopic.Id;
            model.ForumId = forum.Id;
            model.ForumEditor = _forumSettings.ForumEditor;

            model.IsCustomerAllowedToSetTopicPriority = _forumService.IsCustomerAllowedToSetTopicPriority(customer);
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);

            // Subscription.
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, forumTopic.Id, 0, 1).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

			CreateForumBreadcrumb(forum: forum, topic: forumTopic);

			return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult TopicEdit(EditForumTopicModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
				return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var forumTopic = _forumService.GetTopicById(model.Id);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }
            var forum = forumTopic.Forum;
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!_forumService.IsCustomerAllowedToEditTopic(customer, forumTopic))
                    {
                        return new HttpUnauthorizedResult();
                    }

                    var subject = model.Subject;
                    var maxSubjectLength = _forumSettings.TopicSubjectMaxLength;
                    if (maxSubjectLength > 0 && subject.Length > maxSubjectLength)
                    {
                        subject = subject.Substring(0, maxSubjectLength);
                    }

                    var text = model.Text;
                    var maxPostLength = _forumSettings.PostMaxLength;
                    if (maxPostLength > 0 && text.Length > maxPostLength)
                    {
                        text = text.Substring(0, maxPostLength);
                    }

                    var topicType = ForumTopicType.Normal;
                    var ipAddress = Services.WebHelper.GetCurrentIpAddress();
                    var utcNow = DateTime.UtcNow;

                    if (_forumService.IsCustomerAllowedToSetTopicPriority(customer))
                    {
                        topicType = (ForumTopicType) Enum.ToObject(typeof (ForumTopicType), model.TopicTypeId);
                    }

                    forumTopic.TopicTypeId = (int) topicType;
                    forumTopic.Subject = subject;

                    _forumService.UpdateTopic(forumTopic);

                    var firstPost = forumTopic.GetFirstPost(_forumService);
                    if (firstPost != null)
                    {
                        firstPost.Text = text;
                        _forumService.UpdatePost(firstPost);
                    }
                    else
                    {
                        firstPost = new ForumPost
                        {
                            TopicId = forumTopic.Id,
                            CustomerId = forumTopic.CustomerId,
                            Text = text,
                            IPAddress = ipAddress,
                        };

                        _forumService.InsertPost(firstPost, false);
                    }

                    // Subscription.
                    if (_forumService.IsCustomerAllowedToSubscribe(customer))
                    {
                        var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, forumTopic.Id, 0, 1).FirstOrDefault();

                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription
                                {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = customer.Id,
                                    TopicId = forumTopic.Id,
                                    CreatedOnUtc = utcNow
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
                    return RedirectToRoute("TopicSlug", new {id = forumTopic.Id, slug = forumTopic.GetSeName()});
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Redisplay form.
            model.TopicPriorities = ForumTopicTypesList();
            model.IsEdit = true;
            model.ForumName = forum.GetLocalized(x => x.Name);
            model.ForumSeName = forum.GetSeName();
            model.ForumId = forum.Id;
            model.ForumEditor = _forumSettings.ForumEditor;

            model.IsCustomerAllowedToSetTopicPriority = _forumService.IsCustomerAllowedToSetTopicPriority(customer);
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);

            return View(model);
        }

        public ActionResult TopicDelete(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var forumTopic = _forumService.GetTopicById(id);
            if (forumTopic != null)
            {
                if (!_forumService.IsCustomerAllowedToDeleteTopic(Services.WorkContext.CurrentCustomer, forumTopic))
                {
                    return new HttpUnauthorizedResult();
                }

                var forum = _forumService.GetForumById(forumTopic.ForumId);

                _forumService.DeleteTopic(forumTopic);

                if (forum != null)
                {
                    return RedirectToRoute("ForumSlug", new { id = forum.Id, slug = forum.GetSeName() });
                }
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
            var forumTopic = _forumService.GetTopicById(id);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            if (!_forumService.IsCustomerAllowedToCreatePost(customer, forumTopic))
            {
                return new HttpUnauthorizedResult();
            }

            var forum = forumTopic.Forum;
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            var model = new EditForumPostModel
            {
                Id = 0,
                ForumTopicId = forumTopic.Id,
                IsEdit = false,
                ForumEditor = _forumSettings.ForumEditor,
                ForumName = forum.GetLocalized(x => x.Name),
                ForumTopicSubject = forumTopic.Subject,
                ForumTopicSeName = forumTopic.GetSeName(),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer),
                Subscribed = false,
            };
            
            // Subscription.
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, forumTopic.Id, 0, 1).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

            // Insert the quoted text.
            var text = string.Empty;
            if (quote.HasValue)
            {
                var quotePost = _forumService.GetPostById(quote.Value);
                if (quotePost != null && quotePost.TopicId == forumTopic.Id)
                {
                    var quotePostText = quotePost.Text;

                    switch (_forumSettings.ForumEditor)
                    {
                        case EditorType.SimpleTextBox:
                            text = String.Format("{0}:\n{1}\n", quotePost.Customer.FormatUserName(), quotePostText);
                            break;
                        case EditorType.BBCodeEditor:
                            text = String.Format("[quote={0}]{1}[/quote]", quotePost.Customer.FormatUserName(), BBCodeHelper.RemoveQuotes(quotePostText));
                            break;
                    }
                    model.Text = text;
                }
            }

			CreateForumBreadcrumb(forum: forum, topic: forumTopic);

			return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
		[GdprConsent]
		public ActionResult PostCreate(EditForumPostModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
				return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var forumTopic = _forumService.GetTopicById(model.ForumTopicId);
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!_forumService.IsCustomerAllowedToCreatePost(customer, forumTopic))
                    {
                        return new HttpUnauthorizedResult();
                    }

                    var text = model.Text;
                    var maxPostLength = _forumSettings.PostMaxLength;
                    if (maxPostLength > 0 && text.Length > maxPostLength)
                    {
                        text = text.Substring(0, maxPostLength);
                    }

					var utcNow = DateTime.UtcNow;
                    var ipAddress = Services.WebHelper.GetCurrentIpAddress();

                    var forumPost = new ForumPost
                    {
                        TopicId = forumTopic.Id,
                        CustomerId = customer.Id,
                        Text = text,
                        IPAddress = ipAddress
                    };

                    _forumService.InsertPost(forumPost, true);

                    // Subscription.
                    if (_forumService.IsCustomerAllowedToSubscribe(customer))
                    {
                        var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, forumPost.TopicId, 0, 1).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription
                                {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = customer.Id,
                                    TopicId = forumPost.TopicId,
                                    CreatedOnUtc = utcNow
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

                    var pageSize = _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10;
                    var pageIndex = (_forumService.CalculateTopicPageIndex(forumPost.TopicId, pageSize, forumPost.Id) + 1);
                    var url = string.Empty;

                    if (pageIndex > 1)
                    {
                        url = Url.RouteUrl("TopicSlug", new { id = forumPost.TopicId, slug = forumPost.ForumTopic.GetSeName(), page = pageIndex });
                    }
                    else
                    {
                        url = Url.RouteUrl("TopicSlug", new { id = forumPost.TopicId, slug = forumPost.ForumTopic.GetSeName() });
                    }

                    return Redirect(string.Format("{0}#{1}", url, forumPost.Id));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Redisplay form.
            var forum = forumTopic.Forum;
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            model.IsEdit = false;
            model.ForumName = forum.GetLocalized(x => x.Name);
            model.ForumTopicId = forumTopic.Id;
            model.ForumTopicSubject = forumTopic.Subject;
            model.ForumTopicSeName = forumTopic.GetSeName();
            model.Id = 0;
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
            var forumPost = _forumService.GetPostById(id);

            if (forumPost == null)
            {
                return RedirectToRoute("Boards");
            }
            if (!_forumService.IsCustomerAllowedToEditPost(customer, forumPost))
            {
                return new HttpUnauthorizedResult();
            }

            var forumTopic = forumPost.ForumTopic;
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            var forum = forumTopic.Forum;
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            var model = new EditForumPostModel
            {
                Id = forumPost.Id,
                ForumTopicId = forumTopic.Id,
                IsEdit = true,
                ForumEditor = _forumSettings.ForumEditor,
                ForumName = forum.GetLocalized(x => x.Name),
                ForumTopicSubject = forumTopic.Subject,
                ForumTopicSeName = forumTopic.GetSeName(),
                IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer),
                Subscribed = false,
                Text = forumPost.Text,
            };

            // Subscription.
            if (model.IsCustomerAllowedToSubscribe)
            {
                var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, forumTopic.Id, 0, 1).FirstOrDefault();
                model.Subscribed = forumSubscription != null;
            }

			CreateForumBreadcrumb(forum: forum, topic: forumTopic);

			return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult PostEdit(EditForumPostModel model)
        {
            if (!_forumSettings.ForumsEnabled)
            {
				return HttpNotFound();
            }

            var customer = Services.WorkContext.CurrentCustomer;
            var forumPost = _forumService.GetPostById(model.Id);
            if (forumPost == null)
            {
                return RedirectToRoute("Boards");
            }

            if (!_forumService.IsCustomerAllowedToEditPost(customer, forumPost))
            {
                return new HttpUnauthorizedResult();
            }

            var forumTopic = forumPost.ForumTopic;
            if (forumTopic == null)
            {
                return RedirectToRoute("Boards");
            }

            var forum = forumTopic.Forum;
            if (forum == null)
            {
                return RedirectToRoute("Boards");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var utcNow = DateTime.UtcNow;
                    var text = model.Text;
                    var maxPostLength = _forumSettings.PostMaxLength;
                    if (maxPostLength > 0 && text.Length > maxPostLength)
                    {
                        text = text.Substring(0, maxPostLength);
                    }

                    forumPost.Text = text;

                    _forumService.UpdatePost(forumPost);

                    // Subscription.
                    if (_forumService.IsCustomerAllowedToSubscribe(customer))
                    {
                        var forumSubscription = _forumService.GetAllSubscriptions(customer.Id, 0, forumPost.TopicId, 0, 1).FirstOrDefault();
                        if (model.Subscribed)
                        {
                            if (forumSubscription == null)
                            {
                                forumSubscription = new ForumSubscription
                                {
                                    SubscriptionGuid = Guid.NewGuid(),
                                    CustomerId = customer.Id,
                                    TopicId = forumPost.TopicId,
                                    CreatedOnUtc = utcNow
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

                    var pageSize = _forumSettings.PostsPageSize > 0 ? _forumSettings.PostsPageSize : 10;
                    var pageIndex = (_forumService.CalculateTopicPageIndex(forumPost.TopicId, pageSize, forumPost.Id) + 1);
                    var url = string.Empty;

                    if (pageIndex > 1)
                    {
                        url = Url.RouteUrl("TopicSlug", new { id = forumPost.TopicId, slug = forumPost.ForumTopic.GetSeName(), page = pageIndex });
                    }
                    else
                    {
                        url = Url.RouteUrl("TopicSlug", new { id = forumPost.TopicId, slug = forumPost.ForumTopic.GetSeName() });
                    }

                    return Redirect(string.Format("{0}#{1}", url, forumPost.Id));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            // Redisplay form.
            model.IsEdit = true;
            model.ForumName = forum.GetLocalized(x => x.Name);
            model.ForumTopicId = forumTopic.Id;
            model.ForumTopicSubject = forumTopic.Subject;
            model.ForumTopicSeName = forumTopic.GetSeName();
            model.Id = forumPost.Id;
            model.IsCustomerAllowedToSubscribe = _forumService.IsCustomerAllowedToSubscribe(customer);
            model.ForumEditor = _forumSettings.ForumEditor;
            
            return View(model);
        }

        public ActionResult PostDelete(int id)
        {
            if (!_forumSettings.ForumsEnabled)
            {
                return HttpNotFound();
            }

            var forumPost = _forumService.GetPostById(id);
            if (forumPost != null)
            {
                if (!_forumService.IsCustomerAllowedToDeletePost(Services.WorkContext.CurrentCustomer, forumPost))
                {
                    return new HttpUnauthorizedResult();
                }

                var forumTopic = forumPost.ForumTopic;
                var forumId = forumTopic.Forum.Id;
                var forumSlug = forumTopic.Forum.GetSeName();
                var url = string.Empty;

                _forumService.DeletePost(forumPost);

                // Get topic one more time because it can be deleted (first or only post deleted).
                forumTopic = _forumService.GetTopicById(forumPost.TopicId);
                if (forumTopic == null)
                {
                    return RedirectToRoute("ForumSlug", new { id = forumId, slug = forumSlug });
                }
                else
                {
                    return RedirectToRoute("TopicSlug", new { id = forumTopic.Id, slug = forumTopic.GetSeName() });
                }
            }

            return RedirectToRoute("Boards");
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

        [HttpPost, ValidateInput(false)]
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

        [RequireHttpsByConfig(SslRequirement.No), ValidateInput(false)]
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

            model.AddSpellCheckerSuggestions(model.SearchResult.SpellCheckerSuggestions, T, x => Url.RouteUrl("BoardSearch", new { q = x }));

            return View(model);
        }

        // Ajax.
        [HttpPost, ValidateInput(false)]
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
