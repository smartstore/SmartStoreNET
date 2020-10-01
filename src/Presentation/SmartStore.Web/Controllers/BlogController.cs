using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Blogs;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
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
using SmartStore.Web.Models.Blogs;
using SmartStore.Web.Models.Common;
using SmartStore.Web.Models.Media;

namespace SmartStore.Web.Controllers
{
    [RewriteUrl(SslRequirement.No)]
    public partial class BlogController : PublicControllerBase
    {
        private readonly ICommonServices _services;
        private readonly IBlogService _blogService;
        private readonly IMediaService _mediaService;
        private readonly ICustomerContentService _customerContentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ILanguageService _languageService;
        private readonly IGenericAttributeService _genericAttributeService;

        private readonly MediaSettings _mediaSettings;
        private readonly BlogSettings _blogSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly SeoSettings _seoSettings;

        public BlogController(
            ICommonServices services,
            IBlogService blogService,
            IMediaService mediaService,
            ICustomerContentService customerContentService,
            IDateTimeHelper dateTimeHelper,
            IWebHelper webHelper,
            ICacheManager cacheManager,
            ICustomerActivityService customerActivityService,
            IStoreMappingService storeMappingService,
            ILanguageService languageService,
            IGenericAttributeService genericAttributeService,
            MediaSettings mediaSettings,
            BlogSettings blogSettings,
            LocalizationSettings localizationSettings,
            CustomerSettings customerSettings,
            CaptchaSettings captchaSettings,
            SeoSettings seoSettings)
        {
            _services = services;
            _blogService = blogService;
            _mediaService = mediaService;
            _customerContentService = customerContentService;
            _dateTimeHelper = dateTimeHelper;
            _webHelper = webHelper;
            _cacheManager = cacheManager;
            _customerActivityService = customerActivityService;
            _storeMappingService = storeMappingService;
            _languageService = languageService;
            _genericAttributeService = genericAttributeService;

            _mediaSettings = mediaSettings;
            _blogSettings = blogSettings;
            _localizationSettings = localizationSettings;
            _customerSettings = customerSettings;
            _captchaSettings = captchaSettings;
            _seoSettings = seoSettings;
        }

        #region Utilities

        [NonAction]
        protected PictureModel PrepareBlogPostPictureModel(BlogPost blogPost, int? fileId)
        {
            var file = _mediaService.GetFileById(fileId ?? 0, MediaLoadFlags.AsNoTracking);

            var pictureModel = new PictureModel
            {
                PictureId = blogPost.MediaFileId.GetValueOrDefault(),
                Size = 512,
                ImageUrl = _mediaService.GetUrl(file, 512, null, false),
                FullSizeImageUrl = _mediaService.GetUrl(file, 0, null, false),
                FullSizeImageWidth = file?.Dimensions.Width,
                FullSizeImageHeight = file?.Dimensions.Height,
                Title = file?.File?.GetLocalized(x => x.Title)?.Value.NullEmpty() ?? blogPost.Title,
                AlternateText = file?.File?.GetLocalized(x => x.Alt)?.Value.NullEmpty() ?? blogPost.Title,
                File = file
            };

            _services.DisplayControl.Announce(file?.File);

            return pictureModel;
        }

        [NonAction]
        protected void PrepareBlogPostModel(BlogPostModel model, BlogPost blogPost, bool prepareComments)
        {
            Guard.NotNull(blogPost, nameof(blogPost));
            Guard.NotNull(model, nameof(model));

            MiniMapper.Map(blogPost, model);

            model.SeName = blogPost.GetSeName(blogPost.LanguageId, ensureTwoPublishedLanguages: false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(blogPost.CreatedOnUtc, DateTimeKind.Utc);
            model.AddNewComment.DisplayCaptcha = _captchaSettings.CanDisplayCaptcha && _captchaSettings.ShowOnBlogCommentPage;
            model.Comments.AllowComments = blogPost.AllowComments;
            model.Comments.NumberOfComments = blogPost.ApprovedCommentCount;
            model.Comments.AllowCustomersToUploadAvatars = _customerSettings.AllowCustomersToUploadAvatars;
            model.DisplayAdminLink = _services.Permissions.Authorize(Permissions.System.AccessBackend, _services.WorkContext.CurrentCustomer);

            model.HasBgImage = blogPost.PreviewDisplayType == PreviewDisplayType.DefaultSectionBg || blogPost.PreviewDisplayType == PreviewDisplayType.PreviewSectionBg;

            model.PictureModel = PrepareBlogPostPictureModel(blogPost, blogPost.MediaFileId);

            if (blogPost.PreviewDisplayType == PreviewDisplayType.Default || blogPost.PreviewDisplayType == PreviewDisplayType.DefaultSectionBg)
            {
                model.PreviewPictureModel = PrepareBlogPostPictureModel(blogPost, blogPost.MediaFileId);
            }
            else if (blogPost.PreviewDisplayType == PreviewDisplayType.Preview || blogPost.PreviewDisplayType == PreviewDisplayType.PreviewSectionBg)
            {
                model.PreviewPictureModel = PrepareBlogPostPictureModel(blogPost, blogPost.PreviewMediaFileId);
            }

            if (blogPost.PreviewDisplayType == PreviewDisplayType.Preview ||
                blogPost.PreviewDisplayType == PreviewDisplayType.Default ||
                blogPost.PreviewDisplayType == PreviewDisplayType.Bare)
            {
                model.SectionBg = string.Empty;
            }

            // tags 
            model.Tags = blogPost.ParseTags().Select(x => new BlogPostTagModel
            {
                Name = x,
                SeName = SeoHelper.GetSeName(x,
                _seoSettings.ConvertNonWesternChars,
                _seoSettings.AllowUnicodeCharsInUrls,
                true,
                _seoSettings.SeoNameCharConversion)
            }).ToList();

            if (prepareComments)
            {
                var blogComments = blogPost.BlogComments.Where(pr => pr.IsApproved).OrderBy(pr => pr.CreatedOnUtc);
                foreach (var bc in blogComments)
                {
                    var isGuest = bc.Customer.IsGuest();

                    var commentModel = new CommentModel(model.Comments)
                    {
                        Id = bc.Id,
                        CustomerId = bc.CustomerId,
                        CustomerName = bc.Customer.FormatUserName(_customerSettings, T, false),
                        CommentText = bc.CommentText,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(bc.CreatedOnUtc, DateTimeKind.Utc),
                        CreatedOnPretty = bc.CreatedOnUtc.RelativeFormat(true, "f"),
                        AllowViewingProfiles = _customerSettings.AllowViewingProfiles && !isGuest
                    };

                    commentModel.Avatar = bc.Customer.ToAvatarModel(_genericAttributeService, _customerSettings, _mediaSettings, commentModel.CustomerName);

                    model.Comments.Comments.Add(commentModel);
                }
            }

            Services.DisplayControl.Announce(blogPost);
        }

        [NonAction]
        protected BlogPostListModel PrepareBlogPostListModel(BlogPagingFilteringModel command)
        {
            if (command == null)
                throw new ArgumentNullException("command");

            var storeId = _services.StoreContext.CurrentStore.Id;
            var workingLanguageId = _services.WorkContext.WorkingLanguage.Id;
            var model = new BlogPostListModel();
            model.PagingFilteringContext.Tag = command.Tag;
            model.PagingFilteringContext.Month = command.Month;
            model.WorkingLanguageId = workingLanguageId;

            if (command.PageSize <= 0)
                command.PageSize = _blogSettings.PostsPageSize;
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            DateTime? dateFrom = command.GetFromMonth();
            DateTime? dateTo = command.GetToMonth();

            IPagedList<BlogPost> blogPosts;
            if (!command.Tag.HasValue())
            {
                blogPosts = _blogService.GetAllBlogPosts(storeId, workingLanguageId, dateFrom, dateTo, command.PageNumber - 1, command.PageSize, _services.WorkContext.CurrentCustomer.IsAdmin());
            }
            else
            {
                blogPosts = _blogService.GetAllBlogPostsByTag(storeId, workingLanguageId, command.Tag, command.PageNumber - 1, command.PageSize, _services.WorkContext.CurrentCustomer.IsAdmin());
            }

            model.PagingFilteringContext.LoadPagedList(blogPosts);

            // Prepare SEO model
            var parsedMonth = model.PagingFilteringContext.GetParsedMonth();
            var tag = model.PagingFilteringContext.Tag;

            if (parsedMonth == null && tag == null)
            {
                model.MetaTitle = _blogSettings.GetLocalizedSetting(x => x.MetaTitle, storeId);
                model.MetaDescription = _blogSettings.GetLocalizedSetting(x => x.MetaDescription, storeId);
                model.MetaKeywords = _blogSettings.GetLocalizedSetting(x => x.MetaKeywords, storeId);
            }
            else
            {
                model.MetaTitle = parsedMonth != null ?
                    T("PageTitle.Blog.Month").Text.FormatWith(parsedMonth.Value.ToNativeString("MMMM", CultureInfo.InvariantCulture) + " " + parsedMonth.Value.Year) :
                    T("PageTitle.Blog.Tag").Text.FormatWith(tag);

                model.MetaDescription = parsedMonth != null ?
                    T("Metadesc.Blog.Month").Text.FormatWith(parsedMonth.Value.ToNativeString("MMMM", CultureInfo.InvariantCulture) + " " + parsedMonth.Value.Year) :
                    T("Metadesc.Blog.Tag").Text.FormatWith(tag);

                model.MetaKeywords = parsedMonth != null ? parsedMonth.Value.ToNativeString("MMMM", CultureInfo.InvariantCulture) + " " + parsedMonth.Value.Year : tag;
            }

            Services.DisplayControl.AnnounceRange(blogPosts);

            model.BlogPosts = blogPosts
                .Select(x =>
                {
                    var blogPostModel = new BlogPostModel();
                    PrepareBlogPostModel(blogPostModel, x, false);
                    return blogPostModel;
                })
                .ToList();

            return model;
        }

        [NonAction]
        protected BlogPostListModel PrepareBlogPostListModel(int? maxPostAmount, int? maxAgeInDays, bool renderHeading, string blogHeading, bool disableCommentCount, string postsWithTag)
        {
            var storeId = _services.StoreContext.CurrentStore.Id;
            var workingLanguageId = _services.WorkContext.WorkingLanguage.Id;
            var model = new BlogPostListModel
            {
                BlogHeading = blogHeading,
                RenderHeading = renderHeading,
                RssToLinkButton = renderHeading,
                DisableCommentCount = disableCommentCount
            };

            DateTime? maxAge = null;
            if (maxAgeInDays.HasValue)
            {
                maxAge = DateTime.UtcNow.AddDays(-maxAgeInDays.Value);
            }

            IPagedList<BlogPost> blogPosts;
            if (!postsWithTag.IsEmpty())
            {
                blogPosts = _blogService.GetAllBlogPostsByTag(storeId, workingLanguageId, postsWithTag, 0, maxPostAmount ?? 100, _services.WorkContext.CurrentCustomer.IsAdmin(), maxAge);
            }
            else
            {
                blogPosts = _blogService.GetAllBlogPosts(storeId, workingLanguageId, null, null, 0, maxPostAmount ?? 100, _services.WorkContext.CurrentCustomer.IsAdmin(), maxAge);
            }

            Services.DisplayControl.AnnounceRange(blogPosts);

            model.BlogPosts = blogPosts
                .Select(x =>
                {
                    var blogPostModel = new BlogPostModel();
                    PrepareBlogPostModel(blogPostModel, x, false);
                    return blogPostModel;
                })
                .ToList();

            return model;
        }

        #endregion

        #region Methods

        public ActionResult List(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
                return HttpNotFound();

            var model = PrepareBlogPostListModel(command);

            return View("List", model);
        }

        [ChildActionOnly]
        public ActionResult BlogSummary(int? maxPostAmount, int? maxAgeInDays, bool renderHeading, string blogHeading, bool disableCommentCount, string postsWithTag)
        {
            var model = PrepareBlogPostListModel(maxPostAmount, maxAgeInDays, renderHeading, blogHeading, disableCommentCount, postsWithTag);

            return PartialView(model);
        }

        public ActionResult BlogByTag(string tag, BlogPagingFilteringModel command)
        {
            // INFO: param 'tag' redundant, because OutputCache does not include
            // complex type params in cache key computing

            if (!_blogSettings.Enabled)
                return HttpNotFound();

            var model = PrepareBlogPostListModel(command);
            return View("List", model);
        }

        public ActionResult BlogByMonth(string month, BlogPagingFilteringModel command)
        {
            // INFO: param 'month' redundant, because OutputCache does not include
            // complex type params in cache key computing

            if (!_blogSettings.Enabled)
                return HttpNotFound();

            var model = PrepareBlogPostListModel(command);
            return View("List", model);
        }

        public ActionResult ListRss(int? languageId)
        {
            languageId = languageId ?? _services.WorkContext.WorkingLanguage.Id;

            DateTime? maxAge = null;
            var protocol = _webHelper.IsCurrentConnectionSecured() ? "https" : "http";
            var selfLink = Url.RouteUrl("BlogRSS", new { languageId }, protocol);
            var blogLink = Url.RouteUrl("Blog", null, protocol);
            var currentStore = _services.StoreContext.CurrentStore;
            var title = "{0} - Blog".FormatInvariant(currentStore.Name);

            if (_blogSettings.MaxAgeInDays > 0)
            {
                maxAge = DateTime.UtcNow.Subtract(new TimeSpan(_blogSettings.MaxAgeInDays, 0, 0, 0));
            }

            var language = _languageService.GetLanguageById(languageId.Value);
            var feed = new SmartSyndicationFeed(new Uri(blogLink), title);

            feed.AddNamespaces(false);
            feed.Init(selfLink, language);

            if (!_blogSettings.Enabled)
            {
                return new RssActionResult { Feed = feed };
            }

            var items = new List<SyndicationItem>();
            var blogPosts = _blogService.GetAllBlogPosts(currentStore.Id, languageId.Value, null, null, 0, int.MaxValue, false, maxAge);

            foreach (var blogPost in blogPosts)
            {
                var blogPostUrl = Url.RouteUrl("BlogPost", new { SeName = blogPost.GetSeName(blogPost.LanguageId, ensureTwoPublishedLanguages: false) }, protocol);

                var item = feed.CreateItem(blogPost.Title, blogPost.Body, blogPostUrl, blogPost.CreatedOnUtc);

                items.Add(item);

                Services.DisplayControl.Announce(blogPost);
            }

            feed.Items = items;

            Services.DisplayControl.AnnounceRange(blogPosts);

            return new RssActionResult { Feed = feed };
        }

        [GdprConsent]
        public ActionResult BlogPost(int blogPostId)
        {
            if (!_blogSettings.Enabled)
                return HttpNotFound();

            var blogPost = _blogService.GetBlogPostById(blogPostId);
            if (blogPost == null || (!blogPost.IsPublished && !_services.WorkContext.CurrentCustomer.IsAdmin()) ||
                (blogPost.StartDateUtc.HasValue && blogPost.StartDateUtc.Value >= DateTime.UtcNow) ||
                (blogPost.EndDateUtc.HasValue && blogPost.EndDateUtc.Value <= DateTime.UtcNow))
                return HttpNotFound();

            // Store mapping
            if (!_storeMappingService.Authorize(blogPost))
                return HttpNotFound();

            var model = new BlogPostModel();
            PrepareBlogPostModel(model, blogPost, true);

            return View(model);
        }

        [HttpPost, ActionName("BlogPost")]
        [ValidateCaptcha]
        [GdprConsent]
        public ActionResult BlogCommentAdd(int blogPostId, BlogPostModel model, string captchaError)
        {
            if (!_blogSettings.Enabled)
                return HttpNotFound();

            var blogPost = _blogService.GetBlogPostById(blogPostId);
            if (blogPost == null || !blogPost.AllowComments)
                return HttpNotFound();

            var customer = _services.WorkContext.CurrentCustomer;
            if (customer.IsGuest() && !_blogSettings.AllowNotRegisteredUsersToLeaveComments)
            {
                ModelState.AddModelError("", T("Blog.Comments.OnlyRegisteredUsersLeaveComments"));
            }

            if (_captchaSettings.ShowOnBlogCommentPage && captchaError.HasValue())
            {
                ModelState.AddModelError("", captchaError);
            }

            if (ModelState.IsValid)
            {
                var comment = new BlogComment
                {
                    BlogPostId = blogPost.Id,
                    CustomerId = customer.Id,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    CommentText = model.AddNewComment.CommentText,
                    IsApproved = true
                };
                _customerContentService.InsertCustomerContent(comment);

                _blogService.UpdateCommentTotals(blogPost);

                // Notify a store owner.
                if (_blogSettings.NotifyAboutNewBlogComments)
                {
                    Services.MessageFactory.SendBlogCommentNotificationMessage(comment, _localizationSettings.DefaultAdminLanguageId);
                }

                _customerActivityService.InsertActivity("PublicStore.AddBlogComment", T("ActivityLog.PublicStore.AddBlogComment"));

                NotifySuccess(T("Blog.Comments.SuccessfullyAdded"));

                var url = UrlHelper.GenerateUrl(
                    routeName: "BlogPost",
                    actionName: null,
                    controllerName: null,
                    protocol: null,
                    hostName: null,
                    fragment: "new-comment",
                    routeValues: new RouteValueDictionary(new { blogPostId = blogPost.Id, SeName = blogPost.GetSeName(blogPost.LanguageId, ensureTwoPublishedLanguages: false) }),
                    routeCollection: RouteTable.Routes,
                    requestContext: this.ControllerContext.RequestContext,
                    includeImplicitMvcValues: true /*helps fill in the nulls above*/
                );

                return Redirect(url);
            }

            // If we got this far, something failed, redisplay form.
            PrepareBlogPostModel(model, blogPost, true);
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult BlogTags()
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var storeId = _services.StoreContext.CurrentStore.Id;
            var workingLanguageId = _services.WorkContext.WorkingLanguage.Id;
            var cacheKey = string.Format(ModelCacheEventConsumer.BLOG_TAGS_MODEL_KEY, workingLanguageId, storeId);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = new BlogPostTagListModel();

                //get tags
                var tags = _blogService.GetAllBlogPostTags(storeId, workingLanguageId)
                    .OrderByDescending(x => x.BlogPostCount)
                    .Take(_blogSettings.NumberOfTags)
                    .ToList();

                //sorting
                tags = tags.OrderBy(x => x.Name).ToList();

                foreach (var tag in tags)
                    model.Tags.Add(new BlogPostTagModel()
                    {
                        Name = tag.Name,
                        SeName = tag.GetSeName(),
                        BlogPostCount = tag.BlogPostCount
                    });
                return model;
            });

            return PartialView(cachedModel);
        }

        [ChildActionOnly]
        public ActionResult BlogMonths()
        {
            if (!_blogSettings.Enabled)
                return Content("");

            var storeId = _services.StoreContext.CurrentStore.Id;
            var workingLanguageId = _services.WorkContext.WorkingLanguage.Id;
            var cacheKey = string.Format(ModelCacheEventConsumer.BLOG_MONTHS_MODEL_KEY, workingLanguageId, storeId);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = new List<BlogPostYearModel>();
                var blogPosts = _blogService.GetAllBlogPosts(storeId, workingLanguageId, null, null, 0, int.MaxValue);

                if (blogPosts.Count > 0)
                {
                    var months = new SortedDictionary<DateTime, int>();

                    var first = blogPosts[blogPosts.Count - 1].CreatedOnUtc;
                    while (DateTime.SpecifyKind(first, DateTimeKind.Utc) <= DateTime.UtcNow.AddMonths(1))
                    {
                        var list = blogPosts.GetPostsByDate(new DateTime(first.Year, first.Month, 1), new DateTime(first.Year, first.Month, 1).AddMonths(1).AddSeconds(-1));
                        if (list.Count > 0)
                        {
                            var date = new DateTime(first.Year, first.Month, 1);
                            months.Add(date, list.Count);
                        }

                        first = first.AddMonths(1);
                    }

                    int current = 0;
                    foreach (var kvp in months.Reverse())
                    {
                        var date = kvp.Key;
                        var blogPostCount = kvp.Value;
                        if (current == 0)
                            current = date.Year;

                        if (date.Year < current || model.Count == 0)
                        {
                            var yearModel = new BlogPostYearModel()
                            {
                                Year = date.Year
                            };
                            model.Add(yearModel);
                        }

                        model.Last().Months.Add(new BlogPostMonthModel()
                        {
                            Month = date.Month,
                            BlogPostCount = blogPostCount
                        });

                        current = date.Year;
                    }
                }
                return model;
            });

            return PartialView(cachedModel);
        }

        [ChildActionOnly]
        public ActionResult RssHeaderLink()
        {
            if (!_blogSettings.Enabled || !_blogSettings.ShowHeaderRssUrl)
                return Content("");

            string link = string.Format("<link href=\"{0}\" rel=\"alternate\" type=\"application/rss+xml\" title=\"{1} - Blog\" />",
                Url.RouteUrl("BlogRSS", new { languageId = _services.WorkContext.WorkingLanguage.Id }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http"), _services.StoreContext.CurrentStore.Name);

            return Content(link);
        }

        #endregion
    }
}
