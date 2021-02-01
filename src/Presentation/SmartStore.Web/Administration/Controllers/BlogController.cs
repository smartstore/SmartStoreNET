using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Blogs;
using SmartStore.Core;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Html;
using SmartStore.Core.Security;
using SmartStore.Data.Utilities;
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
        private readonly IBlogService _blogService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerContentService _customerContentService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly AdminAreaSettings _adminAreaSettings;

        public BlogController(
            IBlogService blogService,
            ILanguageService languageService,
            IDateTimeHelper dateTimeHelper,
            ICustomerContentService customerContentService,
            ILocalizedEntityService localizedEntityService,
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
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _customerService = customerService;
            _adminAreaSettings = adminAreaSettings;
        }

        #region Utilities

        private void UpdateLocales(BlogPost blogPost, BlogPostModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(blogPost, x => x.Title, localized.Title, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(blogPost, x => x.Intro, localized.Intro, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(blogPost, x => x.Body, localized.Body, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(blogPost, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(blogPost, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(blogPost, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId);

                var seName = blogPost.ValidateSeName(localized.SeName, localized.Title, false, localized.LanguageId);
                _urlRecordService.SaveSlug(blogPost, seName, localized.LanguageId);
            }
        }

        private void PrepareBlogPostModel(BlogPostModel model, BlogPost blogPost)
        {
            if (blogPost != null)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(blogPost);
                
                model.Tags = blogPost.Tags
                    .SplitSafe(",")
                    .Select(x => x = x.Trim())
                    .ToArray();
            }

            var allTags = _blogService.GetAllBlogPostTags(0, 0, true);
            model.AvailableTags = new MultiSelectList(allTags.Select(x => x.Name).ToList(), model.AvailableTags);

            var allLanguages = _languageService.GetAllLanguages(true);
            model.AvailableLanguages = allLanguages
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();

            model.IsSingleLanguageMode = allLanguages.Count <= 1;
        }

        #endregion

        #region Blog posts

        // AJAX.
        public ActionResult AllBlogPosts(string selectedIds)
        {
            var query = _blogService.GetAllBlogPosts(0, null, null, 0, int.MaxValue, 0, true).SourceQuery;
            var pager = new FastPager<BlogPost>(query, 500);
            var allBlogPosts = new List<dynamic>();
            var ids = selectedIds.ToIntArray().ToList();

            while (pager.ReadNextPage(out var blogPosts))
            {
                foreach (var blogPost in blogPosts)
                {
                    dynamic obj = new
                    {
                        blogPost.Id,
                        blogPost.CreatedOnUtc,
                        Title = blogPost.GetLocalized(x => x.Title).Value
                    };

                    allBlogPosts.Add(obj);
                }
            }

            var data = allBlogPosts
                .OrderByDescending(x => x.CreatedOnUtc)
                .Select(x => new ChoiceListItem
                {
                    Id = x.Id.ToString(),
                    Text = x.Title,
                    Selected = ids.Contains(x.Id)
                })
                .ToList();

            return new JsonResult { Data = data, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult List()
        {
            var allTags = _blogService.GetAllBlogPostTags(0, 0, true)
                .Select(x => x.Name)
                .ToList();

            var allLanguages = _languageService.GetAllLanguages(true);

            var model = new BlogListModel
            {
                GridPageSize = _adminAreaSettings.GridPageSize,
                IsSingleStoreMode = _storeService.IsSingleStoreMode(),
                IsSingleLanguageMode = allLanguages.Count <= 1,
                SearchEndDate = DateTime.UtcNow,
                SearchAvailableTags = new MultiSelectList(allTags)
            };

            model.AvailableLanguages = allLanguages
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult List(GridCommand command, BlogListModel model)
        {
            var blogPosts = _blogService.GetAllBlogPosts(
                model.SearchStoreId, 
                model.SearchStartDate,
                model.SearchEndDate, 
                command.Page - 1, 
                command.PageSize,
                model.SearchLanguageId,
                true,
                null,
                model.SearchTitle,
                model.SearchIntro,
                model.SearchBody,
                model.SearchTags);

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
                m.Comments = x.ApprovedCommentCount + x.NotApprovedCommentCount;

                if (x.LanguageId.HasValue)
                {
                    m.LanguageName = x.Language?.Name;
                }

                return m;
            }).Where(x => x.IsPublished == model.SearchIsPublished || model.SearchIsPublished == null);

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Cms.Blog.Create)]
        public ActionResult Create()
        {
            var model = new BlogPostModel
            {
                CreatedOnUtc = DateTime.UtcNow,
                AllowComments = true
            };

            PrepareBlogPostModel(model, null);
            AddLocales(_languageService, model.Locales);

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
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

                model.SeName = blogPost.ValidateSeName(model.SeName, blogPost.Title, true);
                _urlRecordService.SaveSlug(blogPost, model.SeName, 0);

                UpdateLocales(blogPost, model);

                SaveStoreMappings(blogPost, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, blogPost, form));

                NotifySuccess(T("Admin.ContentManagement.Blog.BlogPosts.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            PrepareBlogPostModel(model, null);

            return View(model);
        }

        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult Edit(int id)
        {
            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost == null)
            {
                return RedirectToAction("List");
            }

            var model = blogPost.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = blogPost.GetLocalized(x => x.Title, languageId, false, false);
                locale.Intro = blogPost.GetLocalized(x => x.Intro, languageId, false, false);
                locale.Body = blogPost.GetLocalized(x => x.Body, languageId, false, false);
                locale.MetaKeywords = blogPost.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = blogPost.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = blogPost.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = blogPost.GetSeName(languageId, false, false);
            });

            model.CreatedOnUtc = blogPost.CreatedOnUtc;
            model.StartDate = blogPost.StartDateUtc;
            model.EndDate = blogPost.EndDateUtc;

            PrepareBlogPostModel(model, blogPost);

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Blog.Update)]
        public ActionResult Edit(BlogPostModel model, bool continueEditing, FormCollection form)
        {
            var blogPost = _blogService.GetBlogPostById(model.Id);
            if (blogPost == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                blogPost = model.ToEntity(blogPost);

                blogPost.CreatedOnUtc = model.CreatedOnUtc;
                blogPost.StartDateUtc = model.StartDate;
                blogPost.EndDateUtc = model.EndDate;

                model.SeName = blogPost.ValidateSeName(model.SeName, blogPost.Title, true);
                _urlRecordService.SaveSlug(blogPost, model.SeName, 0);

                UpdateLocales(blogPost, model);

                _blogService.UpdateBlogPost(blogPost);

                SaveStoreMappings(blogPost, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, blogPost, form));
                NotifySuccess(T("Admin.ContentManagement.Blog.BlogPosts.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = blogPost.Id }) : RedirectToAction("List");
            }

            PrepareBlogPostModel(model, blogPost);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.Blog.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var blogPost = _blogService.GetBlogPostById(id);
            if (blogPost != null)
            {
                _blogService.DeleteBlogPost(blogPost);

                NotifySuccess(T("Admin.ContentManagement.Blog.BlogPosts.Deleted"));
            }

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
                    BlogPostTitle = blogComment.BlogPost.GetLocalized(x => x.Title),
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
