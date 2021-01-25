using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using SmartStore.ComponentModel;
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
using SmartStore.Web.Framework.Seo;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Media;
using SmartStore.Web.Models.News;

namespace SmartStore.Web.Controllers
{
    [RewriteUrl(SslRequirement.No)]
    public partial class NewsController : PublicControllerBase
    {
        private readonly ICommonServices _services;
        private readonly INewsService _newsService;
        private readonly IMediaService _mediaService;
        private readonly ICustomerContentService _customerContentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IGenericAttributeService _genericAttributeService;

        private readonly MediaSettings _mediaSettings;
        private readonly NewsSettings _newsSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;

        public NewsController(
            ICommonServices services,
            INewsService newsService,
            IMediaService mediaService,
            ICustomerContentService customerContentService,
            IDateTimeHelper dateTimeHelper,
            IWebHelper webHelper,
            ICacheManager cacheManager,
            ICustomerActivityService customerActivityService,
            IStoreMappingService storeMappingService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings,
            NewsSettings newsSettings,
            LocalizationSettings localizationSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings)
        {
            _services = services;
            _newsService = newsService;
            _mediaService = mediaService;
            _customerContentService = customerContentService;
            _dateTimeHelper = dateTimeHelper;
            _webHelper = webHelper;
            _cacheManager = cacheManager;
            _customerActivityService = customerActivityService;
            _storeMappingService = storeMappingService;
            _genericAttributeService = genericAttributeService;

            _mediaSettings = mediaSettings;
            _newsSettings = newsSettings;
            _localizationSettings = localizationSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
        }

        #region Utilities

        [NonAction]
        protected NewsItemListModel PrepareNewsItemListModel(NewsPagingFilteringModel command)
        {
            Guard.NotNull(command, nameof(command));

            if (command.PageSize <= 0)
                command.PageSize = _newsSettings.NewsArchivePageSize;
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            var model = PrepareNewsItemListModel(true, null, false, command.PageNumber - 1, command.PageSize, true);
            return model;
        }

        [NonAction]
        protected void PrepareNewsItemModel(NewsItemModel model, NewsItem newsItem, bool prepareComments)
        {
            Guard.NotNull(newsItem, nameof(newsItem));
            Guard.NotNull(model, nameof(model));

            Services.DisplayControl.Announce(newsItem);

            MiniMapper.Map(newsItem, model);

            model.Title = newsItem.GetLocalized(x => x.Title);
            model.Short = newsItem.GetLocalized(x => x.Short);
            model.Full = newsItem.GetLocalized(x => x.Full, true);
            model.MetaTitle = newsItem.GetLocalized(x => x.MetaTitle);
            model.MetaDescription = newsItem.GetLocalized(x => x.MetaDescription);
            model.MetaKeywords = newsItem.GetLocalized(x => x.MetaKeywords);
            model.SeName = newsItem.GetSeName(ensureTwoPublishedLanguages: false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsItem.CreatedOnUtc, DateTimeKind.Utc);
            model.CreatedOnUTC = newsItem.CreatedOnUtc;
            model.AddNewComment.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnNewsCommentPage;
            model.DisplayAdminLink = _services.Permissions.Authorize(Permissions.System.AccessBackend, _services.WorkContext.CurrentCustomer);
            model.PictureModel = PrepareNewsItemPictureModel(newsItem, newsItem.MediaFileId);
            model.PreviewPictureModel = PrepareNewsItemPictureModel(newsItem, newsItem.PreviewMediaFileId);

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

                    commentModel.Avatar = nc.Customer.ToAvatarModel(_genericAttributeService, _customerSettings, _mediaSettings, commentModel.CustomerName);

                    model.Comments.Comments.Add(commentModel);
                }
            }
        }

        #endregion

        #region Methods

        public ActionResult HomePageNews()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowNewsOnMainPage)
            {
                return new EmptyResult();
            }

            var storeId = _services.StoreContext.CurrentStore.Id;
            var languageId = _services.WorkContext.WorkingLanguage.Id;
            var includeHidden = _services.WorkContext.CurrentCustomer.IsAdmin();
            var cacheKey = string.Format(ModelCacheEventConsumer.HOMEPAGE_NEWSMODEL_KEY, languageId, storeId, _newsSettings.MainPageNewsCount, includeHidden);

            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var newsItems = _newsService.GetAllNews(storeId, 0, _newsSettings.MainPageNewsCount, languageId, includeHidden);

                Services.DisplayControl.AnnounceRange(newsItems);

                return new HomePageNewsItemsModel
                {
                    NewsItems = newsItems.Select(x =>
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
            {
                return HttpNotFound();
            }

            var model = PrepareNewsItemListModel(command);
            var storeId = _services.StoreContext.CurrentStore.Id;
            
            model.MetaTitle = _newsSettings.GetLocalizedSetting(x => x.MetaTitle, storeId);
            model.MetaDescription = _newsSettings.GetLocalizedSetting(x => x.MetaDescription, storeId);
            model.MetaKeywords = _newsSettings.GetLocalizedSetting(x => x.MetaKeywords, storeId);

            if (!model.MetaTitle.HasValue())
            {
                model.MetaTitle = T("PageTitle.NewsArchive").Text;
            }
            
            return View(model);
        }

        [ActionName("rss")]
        public ActionResult ListRss()
        {
            DateTime? maxAge = null;
            var language = _services.WorkContext.WorkingLanguage;
            var store = _services.StoreContext.CurrentStore;
            var protocol = _webHelper.IsCurrentConnectionSecured() ? "https" : "http";
            var selfLink = Url.Action("rss", "News", null, protocol);
            var newsLink = Url.RouteUrl("NewsArchive", null, protocol);
            var title = "{0} - News".FormatInvariant(store.Name);

            if (_newsSettings.MaxAgeInDays > 0)
            {
                maxAge = DateTime.UtcNow.Subtract(new TimeSpan(_newsSettings.MaxAgeInDays, 0, 0, 0));
            }

            var feed = new SmartSyndicationFeed(new Uri(newsLink), title);

            feed.AddNamespaces(true);
            feed.Init(selfLink, _services.WorkContext.WorkingLanguage);

            if (!_newsSettings.Enabled)
            {
                return new RssActionResult { Feed = feed };
            }

            var items = new List<SyndicationItem>();
            var newsItems = _newsService.GetAllNews(store.Id, 0, int.MaxValue, language.Id, false, maxAge);

            foreach (var news in newsItems)
            {
                var newsUrl = Url.RouteUrl("NewsItem", new { SeName = news.GetSeName(ensureTwoPublishedLanguages: false) }, protocol);
                var content = news.GetLocalized(x => x.Full, true).Value;

                if (content.HasValue())
                {
                    content = WebHelper.MakeAllUrlsAbsolute(content, Request);
                }

                var item = feed.CreateItem(
                    news.GetLocalized(x => x.Title),
                    news.GetLocalized(x => x.Short),
                    newsUrl,
                    news.CreatedOnUtc,
                    content);

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
            {
                return HttpNotFound();
            }

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null)
            {
                return HttpNotFound();
            }

            if (!newsItem.Published ||
                (newsItem.LanguageId.HasValue && newsItem.LanguageId != _services.WorkContext.WorkingLanguage.Id) ||
                (newsItem.StartDateUtc.HasValue && newsItem.StartDateUtc.Value >= DateTime.UtcNow) ||
                (newsItem.EndDateUtc.HasValue && newsItem.EndDateUtc.Value <= DateTime.UtcNow) ||
                !_storeMappingService.Authorize(newsItem))
            {
                if (!_services.WorkContext.CurrentCustomer.IsAdmin())
                {
                    return HttpNotFound();
                }
            }

            var model = new NewsItemModel();
            PrepareNewsItemModel(model, newsItem, true);

            return View(model);
        }

        [HttpPost, ActionName("NewsItem")]
        [ValidateCaptcha]
        [GdprConsent]
        public ActionResult NewsCommentAdd(int newsItemId, NewsItemModel model, string captchaError)
        {
            if (!_newsSettings.Enabled)
            {
                return HttpNotFound();
            }

            var newsItem = _newsService.GetNewsById(newsItemId);
            if (newsItem == null || !newsItem.Published || !newsItem.AllowComments)
            {
                return HttpNotFound();
            }

            if (_captchaSettings.ShowOnNewsCommentPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            if (_services.WorkContext.CurrentCustomer.IsGuest() && !_newsSettings.AllowNotRegisteredUsersToLeaveComments)
            {
                ModelState.AddModelError("", T("News.Comments.OnlyRegisteredUsersLeaveComments"));
            }

            if (ModelState.IsValid)
            {
                var comment = new NewsComment
                {
                    NewsItemId = newsItem.Id,
                    CustomerId = _services.WorkContext.CurrentCustomer.Id,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    CommentTitle = model.AddNewComment.CommentTitle,
                    CommentText = model.AddNewComment.CommentText,
                    IsApproved = true
                };
                _customerContentService.InsertCustomerContent(comment);

                _newsService.UpdateCommentTotals(newsItem);

                // Notify a store owner.
                if (_newsSettings.NotifyAboutNewNewsComments)
                {
                    Services.MessageFactory.SendNewsCommentNotificationMessage(comment, _localizationSettings.DefaultAdminLanguageId);
                }

                _customerActivityService.InsertActivity("PublicStore.AddNewsComment", T("ActivityLog.PublicStore.AddNewsComment"));
                NotifySuccess(T("News.Comments.SuccessfullyAdded"));

                return RedirectToRoute("NewsItem", new { SeName = newsItem.GetSeName(ensureTwoPublishedLanguages: false) });
            }

            // If we got this far, something failed, redisplay form.
            PrepareNewsItemModel(model, newsItem, true);
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult RssHeaderLink()
        {
            if (!_newsSettings.Enabled || !_newsSettings.ShowHeaderRssUrl)
            {
                return new EmptyResult();
            }

            var url = Url.Action("rss", null, null, _webHelper.IsCurrentConnectionSecured() ? "https" : "http");
            var link = $"<link href=\"{url}\" rel=\"alternate\" type=\"application/rss+xml\" title=\"{_services.StoreContext.CurrentStore.Name}: News\" />";

            return Content(link);
        }

        [NonAction]
        protected PictureModel PrepareNewsItemPictureModel(NewsItem newsItem, int? fileId)
        {
            var file = _mediaService.GetFileById(fileId ?? 0, MediaLoadFlags.AsNoTracking);

            var pictureModel = new PictureModel
            {
                PictureId = newsItem.MediaFileId.GetValueOrDefault(),
                Size = MediaSettings.ThumbnailSizeLg,
                FullSizeImageWidth = file?.Dimensions.Width,
                FullSizeImageHeight = file?.Dimensions.Height,
                Title = file?.File?.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? newsItem.GetLocalized(x => x.Title),
                AlternateText = file?.File?.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? newsItem.GetLocalized(x => x.Title),
                File = file
            };

            _services.DisplayControl.Announce(file?.File);

            return pictureModel;
        }

        [ChildActionOnly]
        public ActionResult NewsSummary(
            bool renderHeading, 
            string newsHeading, 
            bool disableCommentCount,
            int? maxPostAmount = null,
            bool displayPaging = false,
            int? maxAgeInDays = null)
        {
            var model = PrepareNewsItemListModel(renderHeading, newsHeading, disableCommentCount, 0, maxPostAmount, displayPaging, maxAgeInDays);
            model.RssToLinkButton = true;

            return PartialView(model);
        }

        [NonAction]
        protected NewsItemListModel PrepareNewsItemListModel(
            bool renderHeading, 
            string newsHeading, 
            bool disableCommentCount, 
            int? pageIndex = null, 
            int? maxPostAmount = null,
            bool displayPaging = false,
            int? maxAgeInDays = null)
        {
            var model = new NewsItemListModel
            {
                NewsHeading = newsHeading,
                RenderHeading = renderHeading,
                DisableCommentCount = disableCommentCount
            };

            DateTime? maxAge = null;
            if (maxAgeInDays.HasValue)
            {
                maxAge = DateTime.UtcNow.AddDays(-maxAgeInDays.Value);
            }

            var newsItems = _newsService.GetAllNews(
                _services.StoreContext.CurrentStore.Id, 
                pageIndex ?? 0, 
                maxPostAmount ?? _newsSettings.NewsArchivePageSize,
                _services.WorkContext.WorkingLanguage.Id,
                _services.WorkContext.CurrentCustomer.IsAdmin(), 
                maxAge);

            if (displayPaging)
            {
                model.PagingFilteringContext.LoadPagedList(newsItems);
            }

            model.NewsItems = newsItems
                .Select(x =>
                {
                    var newsItemModel = new NewsItemModel();
                    PrepareNewsItemModel(newsItemModel, x, false);
                    return newsItemModel;
                })
                .ToList();

            Services.DisplayControl.AnnounceRange(newsItems);

            return model;
        }

        #endregion
    }
}
