﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Blogs;
using SmartStore.Core.Domain.Blogs;
using SmartStore.Core.Html;
using SmartStore.Core.Security;
using SmartStore.Services.Blogs;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
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

        #endregion

        #region Constructors

        public BlogController(IBlogService blogService, ILanguageService languageService,
            IDateTimeHelper dateTimeHelper, ICustomerContentService customerContentService,
            ILocalizationService localizationService, IUrlRecordService urlRecordService,
            IStoreService storeService, IStoreMappingService storeMappingService,
            ICustomerService customerService)
        {
            this._blogService = blogService;
            this._languageService = languageService;
            this._dateTimeHelper = dateTimeHelper;
            this._customerContentService = customerContentService;
            this._localizationService = localizationService;
            this._urlRecordService = urlRecordService;
            this._storeService = storeService;
            this._storeMappingService = storeMappingService;
            this._customerService = customerService;
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
            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult List(GridCommand command)
        {
            var model = new GridModel<BlogPostModel>();

            var blogPosts = _blogService.GetAllBlogPosts(0, 0, null, null, command.Page - 1, command.PageSize, true);

            model.Data = blogPosts.Select(x =>
            {
                var m = x.ToModel();
                if (x.StartDateUtc.HasValue)
                    m.StartDate = _dateTimeHelper.ConvertToUserTime(x.StartDateUtc.Value, DateTimeKind.Utc);
                if (x.EndDateUtc.HasValue)
                    m.EndDate = _dateTimeHelper.ConvertToUserTime(x.EndDateUtc.Value, DateTimeKind.Utc);
                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
                m.LanguageName = x.Language.Name;
                m.Comments = x.ApprovedCommentCount + x.NotApprovedCommentCount;
                return m;
            });

            model.Total = blogPosts.TotalCount;

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Cms.Blog.Create)]
        public ActionResult Create()
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            var model = new BlogPostModel();
            model.CreatedOnUtc = DateTime.UtcNow;

            //Stores
            PrepareStoresMappingModel(model, null, false);

            //default values
            model.AllowComments = true;

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Cms.Blog.Create)]
        public ActionResult Create(BlogPostModel model, bool continueEditing, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var blogPost = model.ToEntity();
                
                MediaHelper.UpdatePictureTransientStateFor(blogPost, c => c.PictureId);
                MediaHelper.UpdatePictureTransientStateFor(blogPost, c => c.PreviewPictureId);

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

                MediaHelper.UpdatePictureTransientStateFor(blogPost, c => c.PictureId);
                MediaHelper.UpdatePictureTransientStateFor(blogPost, c => c.PreviewPictureId);

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
            var model = new GridModel<BlogCommentModel>();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.Blog.Read)]
        public ActionResult Comments(int? filterByBlogPostId, GridCommand command)
        {
            var model = new GridModel<BlogCommentModel>();

            IList<BlogComment> comments;
            if (filterByBlogPostId.HasValue)
            {
                //filter comments by blog
                var blogPost = _blogService.GetBlogPostById(filterByBlogPostId.Value);
                comments = blogPost.BlogComments.OrderBy(bc => bc.CreatedOnUtc).ToList();
            }
            else
            {
                //load all blog comments
                comments = _customerContentService.GetAllCustomerContent<BlogComment>(0, null);
            }

            model.Data = comments.PagedForCommand(command).Select(blogComment =>
            {
                var commentModel = new BlogCommentModel();
                var customer = _customerService.GetCustomerById(blogComment.CustomerId);

                commentModel.Id = blogComment.Id;
                commentModel.BlogPostId = blogComment.BlogPostId;
                commentModel.BlogPostTitle = blogComment.BlogPost.Title;
                commentModel.CustomerId = blogComment.CustomerId;
                commentModel.IpAddress = blogComment.IpAddress;
                commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(blogComment.CreatedOnUtc, DateTimeKind.Utc);
                commentModel.Comment = HtmlUtils.ConvertPlainTextToHtml(blogComment.CommentText.HtmlEncode());

                if (customer == null)
                    commentModel.CustomerName = "".NaIfEmpty();
                else
                    commentModel.CustomerName = customer.GetFullName();

                return commentModel;
            });

            model.Total = comments.Count;

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
