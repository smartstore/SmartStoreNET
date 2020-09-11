using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Blogs;
using SmartStore.Core;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Html;
using SmartStore.Core.Security;
using SmartStore.Services.Blogs;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class BlogController : AdminControllerBase
    {
        #region Fields

        private readonly IBlogService _blogService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerContentService _customerContentService;
        private readonly ILocalizationService _localizationService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly AdminAreaSettings _adminAreaSettings;

        #endregion

        #region Constructors

        public BlogController(
            IBlogService blogService,
            ILanguageService languageService,
            IDateTimeHelper dateTimeHelper,
            ICustomerContentService customerContentService,
            ILocalizationService localizationService,
            IUrlRecordService urlRecordService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            ICustomerService customerService,
            AdminAreaSettings adminAreaSettings)
        {
            _blogService = blogService;
            _languageService = languageService;
            _dateTimeHelper = dateTimeHelper;
            _customerContentService = customerContentService;
            _localizationService = localizationService;
            _urlRecordService = urlRecordService;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _customerService = customerService;
            _adminAreaSettings = adminAreaSettings;
        }

        #endregion

        #region Utilities

        [NonAction]
        private void PrepareStoresMappingModel(BlogPostModel model, BlogPost blogPost, bool excludeProperties)
        {
            Guard.NotNull(model, nameof(model));

            if (!excludeProperties)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(blogPost);
            }
        }

        #endregion

        #region Blog posts

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult List()
        {
            ViewBag.GridPageSize = _adminAreaSettings.GridPageSize;
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            // PrepareSearchModel
            var model = new BlogListModel();

            // Tags
            var allTags = _blogService.GetAllBlogPostTags(0, 0, true).Select(x => x.Name).ToList();
            model.SearchAvailableTags = new MultiSelectList(allTags);

            // IsSingleStoreMode & IsMultiLangMode
            model.IsSingleStoreMode = _storeService.IsSingleStoreMode();
            model.IsSingleLangMode = _languageService.GetAllLanguages(true).Count == 1;

            // Set end date to now.
            model.SearchEndDate = DateTime.UtcNow;

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult List(GridCommand command, BlogListModel model)
        {
            var blogPosts = _blogService.GetAllBlogPosts(model.SearchStoreId, model.SearchLanguageId, model.SearchStartDate, model.SearchEndDate, command.Page - 1, command.PageSize,
                !model.SearchIsPublished ?? true,
                title: model.SearchTitle, intro: model.SearchIntro, body: model.SearchBody, tag: model.SearchTags);

            var gridModel = new GridModel<BlogPostModel>
            {
                Total = blogPosts.TotalCount
            };

            gridModel.Data = blogPosts.Select(x =>
            {
                var m = x.ToModel();

                if (x.StartDateUtc.HasValue)
                {
                    m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                }
                if (x.EndDateUtc.HasValue)
                {
                    m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                }

                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                m.LanguageName = x.Language.Name;
                m.Comments = x.ApprovedCommentCount + x.NotApprovedCommentCount;

                return m;
            });

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Cms.Blog.Create)]
        public ActionResult Create()
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            var model = new BlogPostModel
            {
                CreatedOnUtc = DateTime.UtcNow,
                AllowComments = true
            };

            var allTags = _blogService.GetAllBlogPostTags(0, 0, true).Select(x => x.Name).ToList();
            model.AvailableTags = new MultiSelectList(allTags, model.AvailableTags);

            PrepareStoresMappingModel(model, null, false);

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Cms.Blog.Create)]
        public ActionResult Create(BlogPostModel model, bool continueEditing, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var blogPost = model.ToEntity();

                blogPost.CreatedOnUtc = model.CreatedOnUtc;
                blogPost.StartDateUtc = model.StartDate;
                blogPost.EndDateUtc = model.EndDate;

                _blogService.InsertBlogPost(blogPost);

                // Search engine name.
                var seName = blogPost.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(blogPost, seName, blogPost.LanguageId);

                SaveStoreMappings(blogPost, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, blogPost, form));

                NotifySuccess(T("Admin.ContentManagement.Blog.BlogPosts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            // Tags
            var allTags = _blogService.GetAllBlogPostTags(0, 0, true).Select(x => x.Name).ToList();
            model.AvailableTags = new MultiSelectList(allTags, model.AvailableTags);

            PrepareStoresMappingModel(model, null, true);

            return View(model);
        }

        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult Edit(int id)
        {
            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            var model = blogPost.ToModel();
            model.CreatedOnUtc = blogPost.CreatedOnUtc;
            model.StartDate = blogPost.StartDateUtc;
            model.EndDate = blogPost.EndDateUtc;

            // Tags
            var allTags = _blogService.GetAllBlogPostTags(0, 0, true).Select(x => x.Name).ToList();
            model.AvailableTags = new MultiSelectList(allTags, model.AvailableTags);
            model.Tags = blogPost.Tags.SplitSafe(",");

            // Store
            PrepareStoresMappingModel(model, blogPost, false);
            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Cms.Blog.Update)]
        public ActionResult Edit(BlogPostModel model, bool continueEditing, FormCollection form)
        {
            var blogPost = _blogService.GetBlogPostById(model.Id);
            if (blogPost == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                blogPost = model.ToEntity(blogPost);

                blogPost.CreatedOnUtc = model.CreatedOnUtc;
                blogPost.StartDateUtc = model.StartDate;
                blogPost.EndDateUtc = model.EndDate;

                _blogService.UpdateBlogPost(blogPost);

                // Search engine name.
                var seName = blogPost.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(blogPost, seName, blogPost.LanguageId);

                SaveStoreMappings(blogPost, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, blogPost, form));
                NotifySuccess(T("Admin.ContentManagement.Blog.BlogPosts.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            // Tags
            var allTags = _blogService.GetAllBlogPostTags(0, 0, true).Select(x => x.Name).ToList();
            model.AvailableTags = new MultiSelectList(allTags, model.AvailableTags);
            model.Tags = blogPost.Tags.SplitSafe(",");

            PrepareStoresMappingModel(model, blogPost, true);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [Permission(Permissions.Cms.Blog.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
                return RedirectToAction("List");

            _blogService.DeleteBlogPost(blogPost);

            NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.Blog.BlogPosts.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region Comments

        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult Comments(int? filterByBlogPostId)
        {
            ViewBag.FilterByBlogPostId = filterByBlogPostId;
            ViewBag.GridPageSize = _adminAreaSettings.GridPageSize;

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult Comments(int? filterByBlogPostId, GridCommand command)
        {
            IPagedList<BlogComment> comments;

            if (filterByBlogPostId.HasValue)
            {
                // Filter comments by blog.
                var query = _customerContentService.GetAllCustomerContent<BlogComment>(0, null).SourceQuery;
                query = query.Where(x => x.BlogPostId == filterByBlogPostId.Value);

                comments = new PagedList<BlogComment>(query, command.Page - 1, command.PageSize);
            }
            else
            {
                // Load all blog comments.
                comments = _customerContentService.GetAllCustomerContent<BlogComment>(0, null, null, null, command.Page - 1, command.PageSize);
            }

            var customerIds = comments.Select(x => x.CustomerId).Distinct().ToArray();
            var customers = _customerService.GetCustomersByIds(customerIds).ToDictionarySafe(x => x.Id);

            var model = new GridModel<BlogCommentModel>
            {
                Total = comments.TotalCount
            };

            model.Data = comments.Select(blogComment =>
            {
                customers.TryGetValue(blogComment.CustomerId, out var customer);

                var commentModel = new BlogCommentModel
                {
                    Id = blogComment.Id,
                    BlogPostId = blogComment.BlogPostId,
                    BlogPostTitle = blogComment.BlogPost.Title,
                    CustomerId = blogComment.CustomerId,
                    IpAddress = blogComment.IpAddress,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(blogComment.CreatedOnUtc, DateTimeKind.Utc),
                    Comment = HtmlUtils.ConvertPlainTextToHtml(blogComment.CommentText.HtmlEncode()),
                    CustomerName = customer.GetDisplayName(T)
                };

                return commentModel;
            });

            return new JsonResult
            {
                Data = model
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Blog.EditComment)]
        public ActionResult CommentDelete(int? filterByBlogPostId, int id, GridCommand command)
        {
            var comment = _customerContentService.GetCustomerContentById(id) as BlogComment;

            var blogPost = comment.BlogPost;
            _customerContentService.DeleteCustomerContent(comment);

            //update totals
            _blogService.UpdateCommentTotals(blogPost);

            return Comments(filterByBlogPostId, command);
        }

        #endregion
    }
}
