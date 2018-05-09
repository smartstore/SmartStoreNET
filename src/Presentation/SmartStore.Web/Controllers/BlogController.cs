using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core;
using SmartStore.Core.Caching;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Logging;
using SmartStore.Services.Blogs;
using SmartStore.Services.Common;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Messages;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Utilities;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.UI.Captcha;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Blogs;
using SmartStore.Web.Models.Common;
using SmartStore.Core.Domain.Seo;

namespace SmartStore.Web.Controllers
{
    [RequireHttpsByConfigAttribute(SslRequirement.No)]
    public partial class BlogController : PublicControllerBase
    {
        #region Fields

        private readonly IBlogService _blogService;
        private readonly IWorkContext _workContext;
		private readonly IStoreContext _storeContext;
        private readonly IPictureService _pictureService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerContentService _customerContentService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly IWebHelper _webHelper;
        private readonly ICacheManager _cacheManager;
        private readonly ICustomerActivityService _customerActivityService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly ILanguageService _languageService;

		private readonly MediaSettings _mediaSettings;
        private readonly BlogSettings _blogSettings;
        private readonly LocalizationSettings _localizationSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly CaptchaSettings _captchaSettings;
        private readonly SeoSettings _seoSettings;

        #endregion

        #region Constructors

        public BlogController(IBlogService blogService,
            IWorkContext workContext, 
			IStoreContext storeContext,
			IPictureService pictureService,
			ILocalizationService localizationService,
            ICustomerContentService customerContentService,
			IDateTimeHelper dateTimeHelper,
            IWorkflowMessageService workflowMessageService,
			IWebHelper webHelper,
            ICacheManager cacheManager,
			ICustomerActivityService customerActivityService,
			IStoreMappingService storeMappingService,
			ILanguageService languageService,
            MediaSettings mediaSettings,
			BlogSettings blogSettings,
            LocalizationSettings localizationSettings,
			CustomerSettings customerSettings,
            CaptchaSettings captchaSettings,
            SeoSettings seoSettings)
        {
            this._blogService = blogService;
            this._workContext = workContext;
			this._storeContext = storeContext;
            this._pictureService = pictureService;
            this._localizationService = localizationService;
            this._customerContentService = customerContentService;
            this._dateTimeHelper = dateTimeHelper;
            this._workflowMessageService = workflowMessageService;
            this._webHelper = webHelper;
            this._cacheManager = cacheManager;
            this._customerActivityService = customerActivityService;
			this._storeMappingService = storeMappingService;
			this._languageService = languageService;

            this._mediaSettings = mediaSettings;
            this._blogSettings = blogSettings;
            this._localizationSettings = localizationSettings;
            this._customerSettings = customerSettings;
            this._captchaSettings = captchaSettings;
            this._seoSettings = seoSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        protected void PrepareBlogPostModel(BlogPostModel model, BlogPost blogPost, bool prepareComments)
        {
            if (blogPost == null)
                throw new ArgumentNullException("blogPost");

            if (model == null)
                throw new ArgumentNullException("model");

            model.Id = blogPost.Id;
            model.MetaTitle = blogPost.MetaTitle;
            model.MetaDescription = blogPost.MetaDescription;
            model.MetaKeywords = blogPost.MetaKeywords;
            model.SeName = blogPost.GetSeName(blogPost.LanguageId, ensureTwoPublishedLanguages: false);
            model.Title = blogPost.Title;
            model.Body = blogPost.Body;
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(blogPost.CreatedOnUtc, DateTimeKind.Utc);
            model.Tags = blogPost.ParseTags().Select(x => new BlogPostTagModel { Name = x, SeName = SeoHelper.GetSeName(x,
                _seoSettings.ConvertNonWesternChars,
                _seoSettings.AllowUnicodeCharsInUrls,
                _seoSettings.SeoNameCharConversion)
            }).ToList();
            model.AddNewComment.DisplayCaptcha = _captchaSettings.Enabled && _captchaSettings.ShowOnBlogCommentPage;
			model.Comments.AllowComments = blogPost.AllowComments;
			model.Comments.AvatarPictureSize = _mediaSettings.AvatarPictureSize;
			model.Comments.NumberOfComments = blogPost.ApprovedCommentCount;
			model.Comments.AllowCustomersToUploadAvatars = _customerSettings.AllowCustomersToUploadAvatars;

            if (prepareComments)
            {
                var blogComments = blogPost.BlogComments.Where(pr => pr.IsApproved).OrderBy(pr => pr.CreatedOnUtc);
                foreach (var bc in blogComments)
                {
                    var commentModel = new CommentModel(model.Comments)
                    {
                        Id = bc.Id,
                        CustomerId = bc.CustomerId,
                        CustomerName = bc.Customer.FormatUserName(),
                        CommentText = bc.CommentText,
                        CreatedOn = _dateTimeHelper.ConvertToUserTime(bc.CreatedOnUtc, DateTimeKind.Utc),
						CreatedOnPretty = bc.CreatedOnUtc.RelativeFormat(true, "f"),
						AllowViewingProfiles = _customerSettings.AllowViewingProfiles && bc.Customer != null && !bc.Customer.IsGuest()
                    };

                    if (_customerSettings.AllowCustomersToUploadAvatars)
                    {
                        var customer = bc.Customer;
                        string avatarUrl = _pictureService.GetPictureUrl(customer.GetAttribute<int>(SystemCustomerAttributeNames.AvatarPictureId), _mediaSettings.AvatarPictureSize, false);
                        if (String.IsNullOrEmpty(avatarUrl) && _customerSettings.DefaultAvatarEnabled)
                            avatarUrl = _pictureService.GetDefaultPictureUrl(_mediaSettings.AvatarPictureSize, PictureType.Avatar);
                        commentModel.CustomerAvatarUrl = avatarUrl;
                    }

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

            var model = new BlogPostListModel();
            model.PagingFilteringContext.Tag = command.Tag;
            model.PagingFilteringContext.Month = command.Month;
            model.WorkingLanguageId = _workContext.WorkingLanguage.Id;

            if (command.PageSize <= 0)
                command.PageSize = _blogSettings.PostsPageSize;
            if (command.PageNumber <= 0)
                command.PageNumber = 1;

            DateTime? dateFrom = command.GetFromMonth();
            DateTime? dateTo = command.GetToMonth();

            IPagedList<BlogPost> blogPosts;
            if (String.IsNullOrEmpty(command.Tag))
            {
				blogPosts = _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id,
					_workContext.WorkingLanguage.Id,
                    dateFrom, dateTo, command.PageNumber - 1, command.PageSize);
            }
            else
            {
				blogPosts = _blogService.GetAllBlogPostsByTag(_storeContext.CurrentStore.Id,
					_workContext.WorkingLanguage.Id,
                    command.Tag, command.PageNumber - 1, command.PageSize);
            }
            model.PagingFilteringContext.LoadPagedList(blogPosts);

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

        public ActionResult BlogByTag(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
				return HttpNotFound();

            var model = PrepareBlogPostListModel(command);
            return View("List", model);
        }

        public ActionResult BlogByMonth(BlogPagingFilteringModel command)
        {
            if (!_blogSettings.Enabled)
				return HttpNotFound();

            var model = PrepareBlogPostListModel(command);
            return View("List", model);
        }

		[Compress]
        public ActionResult ListRss(int languageId)
        {
			DateTime? maxAge = null;
			var protocol = _webHelper.IsCurrentConnectionSecured() ? "https" : "http";
			var selfLink = Url.RouteUrl("BlogRSS", new { languageId = languageId }, protocol);
			var blogLink = Url.RouteUrl("Blog", null, protocol);

			var title = "{0} - Blog".FormatInvariant(_storeContext.CurrentStore.Name);

			if (_blogSettings.MaxAgeInDays > 0)
				maxAge = DateTime.UtcNow.Subtract(new TimeSpan(_blogSettings.MaxAgeInDays, 0, 0, 0));

			var language = _languageService.GetLanguageById(languageId);
			var feed = new SmartSyndicationFeed(new Uri(blogLink), title);

			feed.AddNamespaces(false);
			feed.Init(selfLink, language);

			if (!_blogSettings.Enabled)
				return new RssActionResult { Feed = feed };

			var items = new List<SyndicationItem>();
			var blogPosts = _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id, languageId, null, null, 0, int.MaxValue, false, maxAge);

			foreach (var blogPost in blogPosts)
			{
				var blogPostUrl = Url.RouteUrl("BlogPost", new { SeName = blogPost.GetSeName(blogPost.LanguageId, ensureTwoPublishedLanguages: false) }, protocol);

				var item = feed.CreateItem(blogPost.Title, blogPost.Body, blogPostUrl, blogPost.CreatedOnUtc);

				items.Add(item);

				Services.DisplayControl.Announce(blogPost);
			}

			feed.Items = items;

			return new RssActionResult { Feed = feed };
        }

        public ActionResult BlogPost(int blogPostId)
        {
            if (!_blogSettings.Enabled)
				return HttpNotFound();

            var blogPost = _blogService.GetBlogPostById(blogPostId);
            if (blogPost == null ||
                (blogPost.StartDateUtc.HasValue && blogPost.StartDateUtc.Value >= DateTime.UtcNow) ||
                (blogPost.EndDateUtc.HasValue && blogPost.EndDateUtc.Value <= DateTime.UtcNow))
				return HttpNotFound();

			//Store mapping
			if (!_storeMappingService.Authorize(blogPost))
				return HttpNotFound();

            var model = new BlogPostModel();
            PrepareBlogPostModel(model, blogPost, true);

            return View(model);
        }

        [HttpPost, ActionName("BlogPost")]
        [FormValueRequired("add-comment")]
        [CaptchaValidator]
        public ActionResult BlogCommentAdd(int blogPostId, BlogPostModel model, bool captchaValid)
        {
            if (!_blogSettings.Enabled)
				return HttpNotFound();

            var blogPost = _blogService.GetBlogPostById(blogPostId);
            if (blogPost == null || !blogPost.AllowComments)
				return HttpNotFound();

            if (_workContext.CurrentCustomer.IsGuest() && !_blogSettings.AllowNotRegisteredUsersToLeaveComments)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Blog.Comments.OnlyRegisteredUsersLeaveComments"));
            }

            //validate CAPTCHA
            if (_captchaSettings.Enabled && _captchaSettings.ShowOnBlogCommentPage && !captchaValid)
            {
                ModelState.AddModelError("", _localizationService.GetResource("Common.WrongCaptcha"));
            }

            if (ModelState.IsValid)
            {
                var comment = new BlogComment
                {
                    BlogPostId = blogPost.Id,
                    CustomerId = _workContext.CurrentCustomer.Id,
                    IpAddress = _webHelper.GetCurrentIpAddress(),
                    CommentText = model.AddNewComment.CommentText,
                    IsApproved = true
                };
                _customerContentService.InsertCustomerContent(comment);

                //update totals
                _blogService.UpdateCommentTotals(blogPost);

                //notify a store owner
                if (_blogSettings.NotifyAboutNewBlogComments)
                    _workflowMessageService.SendBlogCommentNotificationMessage(comment, _localizationSettings.DefaultAdminLanguageId);

                //activity log
                _customerActivityService.InsertActivity("PublicStore.AddBlogComment", _localizationService.GetResource("ActivityLog.PublicStore.AddBlogComment"));

				NotifySuccess(T("Blog.Comments.SuccessfullyAdded"));

                var url = UrlHelper.GenerateUrl(
                    routeName: "BlogPost",
                    actionName: null,
                    controllerName: null,
                    protocol: null,
                    hostName: null,
                    fragment: "new-comment",
                    routeValues: new RouteValueDictionary(new { blogPostId = blogPost.Id, SeName = blogPost.GetSeName(blogPost.LanguageId, ensureTwoPublishedLanguages: false) }),
                    routeCollection: System.Web.Routing.RouteTable.Routes,
                    requestContext: this.ControllerContext.RequestContext,
                    includeImplicitMvcValues: true /*helps fill in the nulls above*/
                );

                return Redirect(url);
            }

            //If we got this far, something failed, redisplay form
            PrepareBlogPostModel(model, blogPost, true);
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult BlogTags()
        {
            if (!_blogSettings.Enabled)
                return Content("");

			var cacheKey = string.Format(ModelCacheEventConsumer.BLOG_TAGS_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = new BlogPostTagListModel();

                //get tags
				var tags = _blogService.GetAllBlogPostTags(_storeContext.CurrentStore.Id, _workContext.WorkingLanguage.Id)
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

			var cacheKey = string.Format(ModelCacheEventConsumer.BLOG_MONTHS_MODEL_KEY, _workContext.WorkingLanguage.Id, _storeContext.CurrentStore.Id);
            var cachedModel = _cacheManager.Get(cacheKey, () =>
            {
                var model = new List<BlogPostYearModel>();

				var blogPosts = _blogService.GetAllBlogPosts(_storeContext.CurrentStore.Id,
					_workContext.WorkingLanguage.Id, null, null, 0, int.MaxValue);
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
                    foreach (var kvp in months)
                    {
                        var date = kvp.Key;
                        var blogPostCount = kvp.Value;
                        if (current == 0)
                            current = date.Year;

                        if (date.Year > current || model.Count == 0)
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
				Url.RouteUrl("BlogRSS", new { languageId = _workContext.WorkingLanguage.Id }, _webHelper.IsCurrentConnectionSecured() ? "https" : "http"), _storeContext.CurrentStore.Name);

			return Content(link);
        }

        #endregion
    }
}
