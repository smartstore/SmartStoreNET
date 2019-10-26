﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.News;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.News;

namespace SmartStore.Web.Controllers
{
    [RewriteUrl(SslRequirement.No)]
    public partial class NewsController : PublicControllerBase
    {
        #region Fields

        private readonly ICommonServices _services;
        private readonly INewsService _newsService;
        private readonly IPictureService _pictureService;
        private readonly ICustomerContentService _customerContentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerActivityService _customerActivityService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly ILanguageService _languageService;
        private readonly IGenericAttributeService _genericAttributeService;

        private readonly MediaSettings _mediaSettings;
        private readonly NewsSettings _newsSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Constructors

        public NewsController(
            ICommonServices services,
            INewsService newsService,
			IPictureService pictureService, 
            ICustomerContentService customerContentService, 
            IDateTimeHelper dateTimeHelper,
            IWebHelper webHelper,
            ICacheManager cacheManager,
            ICustomerActivityService customerActivityService,
			IStoreMappingService storeMappingService,
			ILanguageService languageService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings, 
            NewsSettings newsSettings,
            LocalizationSettings localizationSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings)
        {
            _services = services;
            _newsService = newsService;
            _pictureService = pictureService;
            _customerContentService = customerContentService;
            _dateTimeHelper = dateTimeHelper;
            _webHelper = webHelper;
            _cacheManager = cacheManager;
            _customerActivityService = customerActivityService;
			_storeMappingService = storeMappingService;
			_languageService = languageService;
            _genericAttributeService = genericAttributeService;

            _mediaSettings = mediaSettings;
            _newsSettings = newsSettings;
            _localizationSettings = localizationSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected void PrepareNewsItemModel(NewsItemModel model, NewsItem newsItem, bool prepareComments)
        {
			Guard.NotNull(newsItem, nameof(newsItem));
			Guard.NotNull(model, nameof(model));

			Services.DisplayControl.Announce(newsItem);

            model.Id = newsItem.Id;
            model.MetaTitle = newsItem.MetaTitle;
            model.MetaDescription = newsItem.MetaDescription;
            model.MetaKeywords = newsItem.MetaKeywords;
            model.SeName = newsItem.GetSeName(newsItem.LanguageId, ensureTwoPublishedLanguages: false);
            model.Title = newsItem.Title;
            model.Short = newsItem.Short;
            model.Full = newsItem.Full;
			model.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.CreatedOnUtc, DateTimeKind.Utc);
            model.AddNewComment.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnNewsCommentPage;
            model.DisplayAdminLink = _services.Permissions.Authorize(Permissions.System.AccessBackend, _services.WorkContext.CurrentCustomer);
            model.PictureModel = PrepareNewsItemPictureModel(newsItem, newsItem.PictureId);
            model.PreviewPictureModel = PrepareNewsItemPictureModel(newsItem, newsItem.PreviewPictureId);

            model.Comments.AllowComments = newsItem.AllowComments;
            model.Comments.NumberOfComments = newsItem.ApprovedCommentCount;
			model.Comments.AllowCustomersToUploadAvatars = _customerSettings.AllowCustomersToUploadAvatars;

            if (prepareComments)
            {
                var newsComments = newsItem.NewsComments.Where(n => n.IsApproved).OrderBy(pr => pr.CreatedOnUtc);
                foreach (var nc in newsComments)
                {
                    var isGuest = nc.Customer.IsGuest();

                    var commentModel = new CommentModel(model.Comments)
					{
						Id = nc.Id,
						CustomerId = nc.CustomerId,
						CustomerName = nc.Customer.FormatUserName(_customerSettings, T, false),
						CommentTitle = nc.CommentTitle,
						CommentText = nc.CommentText,
						CreatedOn = _dateTimeHelper.ConvertToUserTime(nc.CreatedOnUtc, DateTimeKind.Utc),
						CreatedOnPretty = nc.CreatedOnUtc.RelativeFormat(true, "f"),
                        AllowViewingProfiles = _customerSettings.AllowViewingProfiles && !isGuest,
                    };

                    commentModel.Avatar = nc.Customer.ToAvatarModel(_genericAttributeService, _pictureService, _customerSettings, _mediaSettings, Url, commentModel.CustomerName);

                    model.Comments.Comments.Add(commentModel);
                }
            }
        }

        #endregion

        #region Methods

        public ActionResult HomePageNews()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
                return Content("");

            var workingLanguageId = _services.WorkContext.WorkingLanguage.Id;
            var currentStoreId = _services.StoreContext.CurrentStore.Id;
            var cacheKey = string.Format(ModelCacheEventConsumer.HOMEPAGE_NEWSMODEL_KEY, workingLanguageId, currentStoreId);

            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
				var newsItems = _newsService.GetAllNews(workingLanguageId, currentStoreId, 0, _newsSettings.MainPageNewsCount);

				Services.DisplayControl.AnnounceRange(newsItems);

				return new HomePageNewsItemsModel()
                {
                    WorkingLanguageId = workingLanguageId,
                    NewsItems = newsItems
                        .Select(x =>
                        {
                            var newsModel = new NewsItemModel();
                            PrepareNewsItemModel(newsModel, x, false);
                            return newsModel;
                        })
                        .ToList()
                };
            });

            // "Comments" property of "NewsItemModel" object depends on the current customer.
            // Furthermore, we just don't need it for home page news. So let's update reset it.
            // But first we need to clone the cached model (the updated one should not be cached)
            var model = (HomePageNewsItemsModel)cachedModel.Clone();
            foreach (var newsItemModel in model.NewsItems)
			{
				newsItemModel.Comments.Comments.Clear();
			}
                
            return PartialView(model);
        }

        public ActionResult List(NewsPagingFilteringModel command)
        {
            if (!_newsSettings.Enabled)
				return HttpNotFound();

            var workingLanguageId = _services.WorkContext.WorkingLanguage.Id;
            var model = new NewsItemListModel();
            model.WorkingLanguageId = workingLanguageId;

            if (command.PageSize <= 0)
                command.PageSize = _newsSettings.NewsArchivePageSize;
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

			var newsItems = _newsService.GetAllNews(workingLanguageId, _services.StoreContext.CurrentStore.Id, command.PageNumber - 1, command.PageSize);
            model.PagingFilteringContext.LoadPagedList(newsItems);

            model.NewsItems = newsItems
                .Select(x =>
                {
                    var newsModel = new NewsItemModel();
                    PrepareNewsItemModel(newsModel, x, false);
                    return newsModel;
                })
                .ToList();

			Services.DisplayControl.AnnounceRange(newsItems);

            return View(model);
        }

		[ActionName("rss")]
        public ActionResult ListRss(int? languageId)
        {
			languageId = languageId ?? _services.WorkContext.WorkingLanguage.Id;

			DateTime? maxAge = null;
			var protocol = _webHelper.IsCurrentConnectionSecured() ? "https" : "http";
			var selfLink = Url.Action("rss", "News", new { languageId = languageId }, protocol);
			var newsLink = Url.RouteUrl("NewsArchive", null, protocol);

			var title = "{0} - News".FormatInvariant(_services.StoreContext.CurrentStore.Name);

			if (_newsSettings.MaxAgeInDays > 0)
			{
				maxAge = DateTime.UtcNow.Subtract(new TimeSpan(_newsSettings.MaxAgeInDays, 0, 0, 0));
			}

			var language = _languageService.GetLanguageById(languageId.Value);
			var feed = new SmartSyndicationFeed(new Uri(newsLink), title);

			feed.AddNamespaces(true);
			feed.Init(selfLink, language);

			if (!_newsSettings.Enabled)
			{
				return new RssActionResult { Feed = feed };
			}

			var items = new List<SyndicationItem>();
			var newsItems = _newsService.GetAllNews(languageId.Value, _services.StoreContext.CurrentStore.Id, 0, int.MaxValue, false, maxAge);

			foreach (var news in newsItems)
			{
				var newsUrl = Url.RouteUrl("NewsItem", new { SeName = news.GetSeName(news.LanguageId, ensureTwoPublishedLanguages: false) }, protocol);

				var item = feed.CreateItem(news.Title, news.Short, newsUrl, news.CreatedOnUtc, news.Full);

				items.Add(item);
			}

			feed.Items = items;

			Services.DisplayControl.AnnounceRange(newsItems);

            return new RssActionResult { Feed = feed };
        }

		[GdprConsent]
		public ActionResult NewsItem(int newsItemId)
        {
            if (!_newsSettings.Enabled)
				return HttpNotFound();

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null ||
                !newsItem.Published ||
                (newsItem.StartDateUtc.HasValue && newsItem.StartDateUtc.Value >= DateTime.UtcNow) ||
				(newsItem.EndDateUtc.HasValue && newsItem.EndDateUtc.Value <= DateTime.UtcNow) ||
				//Store mapping
				!_storeMappingService.Authorize(newsItem))
				return HttpNotFound();

            var model = new NewsItemModel();
            PrepareNewsItemModel(model, newsItem, true);

            return View(model);
        }

        [HttpPost, ActionName("NewsItem")]
        [FormValueRequired("add-comment")]
        [ValidateCaptcha]
		[GdprConsent]
		public ActionResult NewsCommentAdd(int newsItemId, NewsItemModel model, bool captchaValid)
        {
            if (!_newsSettings.Enabled)
				return HttpNotFound();

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null || !newsItem.Published || !newsItem.AllowComments)
				return HttpNotFound();

            //validate CAPTCHA
            if (_captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnNewsCommentPage && !captchaValid)
            {
                ModelState.AddModelError("", T("Common.WrongCaptcha"));
            }

            if (_services.WorkContext.CurrentCustomer.IsGuest() && !_newsSettings.AllowNotRegisteredUsersToLeaveComments)
            {
                ModelState.AddModelError("", T("News.Comments.OnlyRegisteredUsersLeaveComments"));
            }

            if (ModelState.IsValid)
            {
                var comment = new NewsComment()
                {
                    NewsItemId = newsItem.Id,
                    CustomerId = _services.WorkContext.CurrentCustomer.Id,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    CommentTitle = model.AddNewComment.CommentTitle,
                    CommentText = model.AddNewComment.CommentText,
                    IsApproved = true
                };
                _customerContentService.InsertCustomerContent(comment);

                //update totals
                _newsService.UpdateCommentTotals(newsItem);

                //notify a store owner;
                if (_newsSettings.NotifyAboutNewNewsComments)
                    Services.MessageFactory.SendNewsCommentNotificationMessage(comment, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddNewsComment", T("ActivityLog.PublicStore.AddNewsComment"));

				NotifySuccess(T("News.Comments.SuccessfullyAdded"));

                return RedirectToRoute("NewsItem", new { SeName = newsItem.GetSeName(newsItem.LanguageId, ensureTwoPublishedLanguages: false) });
            }

            //If we got this far, something failed, redisplay form
            PrepareNewsItemModel(model, newsItem, true);
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult RssHeaderLink()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowHeaderRssUrl)
                return Content("");

            var link = string.Format("<link href=\"{0}\" rel=\"alternate\" type=\"application/rss+xml\" title=\"{1}: News\" />",
				Url.Action("rss", null, new { languageId = _services.WorkContext.WorkingLanguage.Id }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http"),
				_services.StoreContext.CurrentStore.Name);

            return Content(link);
        }

        [NonAction]
        protected PictureModel PrepareNewsItemPictureModel(NewsItem newsItem, int? pictureId)
        {
            var pictureInfo = _pictureService.GetPictureInfo(pictureId);

            var pictureModel = new PictureModel
            {
                PictureId = newsItem.PictureId.GetValueOrDefault(),
                Size = 512,
                ImageUrl = _pictureService.GetUrl(pictureInfo, 512, false),
                FullSizeImageUrl = _pictureService.GetUrl(pictureInfo, 0, false),
                FullSizeImageWidth = pictureInfo?.Width,
                FullSizeImageHeight = pictureInfo?.Height,
                Title = newsItem.Title,
                AlternateText = newsItem.Title
            };

            return pictureModel;
        }

        #endregion
    }
}
