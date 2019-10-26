﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.News;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Html;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.News;
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
    public class NewsController : AdminControllerBase
    {
        #region Fields

        private readonly INewsService _newsService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerContentService _customerContentService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;

        #endregion

        #region Constructors

        public NewsController(
            INewsService newsService,
            ILanguageService languageService,
            IDateTimeHelper dateTimeHelper,
            ICustomerContentService customerContentService,
            IUrlRecordService urlRecordService,
            IStoreService storeService,
            IStoreMappingService storeMappingService,
            ICustomerService customerService)
        {
            _newsService = newsService;
            _languageService = languageService;
            _dateTimeHelper = dateTimeHelper;
            _customerContentService = customerContentService;
            _urlRecordService = urlRecordService;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _customerService = customerService;
        }

        #endregion

        #region Utilities

        [NonAction]
        private void PrepareStoresMappingModel(NewsItemModel model, NewsItem newsItem, bool excludeProperties)
        {
            Guard.NotNull(model, nameof(model));

            if (!excludeProperties)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(newsItem);
            }
        }

        #endregion

        #region News items

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Cms.News.Read)]
        public ActionResult List()
        {
            var model = new NewsItemListModel();

            foreach (var s in _storeService.GetAllStores())
            {
                model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
            }

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.News.Read)]
        public ActionResult List(GridCommand command, NewsItemListModel model)
        {
            var gridModel = new GridModel<NewsItemModel>();

            var news = _newsService.GetAllNews(0, model.SearchStoreId, command.Page - 1, command.PageSize, true);

            gridModel.Data = news.Select(x =>
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

            gridModel.Total = news.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Cms.News.Create)]
        public ActionResult Create()
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new NewsItemModel();
            //Stores
            PrepareStoresMappingModel(model, null, false);
            //default values
            model.Published = true;
            model.AllowComments = true;
            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Cms.News.Create)]
        public ActionResult Create(NewsItemModel model, bool continueEditing, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var newsItem = model.ToEntity();
                
                MediaHelper.UpdatePictureTransientStateFor(newsItem, c => c.PictureId);
                MediaHelper.UpdatePictureTransientStateFor(newsItem, c => c.PreviewPictureId);

                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                newsItem.CreatedOnUtc = DateTime.UtcNow;
                _newsService.InsertNews(newsItem);

                // Search engine name.
                var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(newsItem, seName, newsItem.LanguageId);

                SaveStoreMappings(newsItem, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, newsItem, form));

                NotifySuccess(T("Admin.ContentManagement.News.NewsItems.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            PrepareStoresMappingModel(model, null, true);

            return View(model);
        }

        [Permission(Permissions.Cms.News.Read)]
        public ActionResult Edit(int id)
        {
            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
                return RedirectToAction("List");

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            var model = newsItem.ToModel();
            model.StartDate = newsItem.StartDateUtc;
            model.EndDate = newsItem.EndDateUtc;

            //stores
            PrepareStoresMappingModel(model, newsItem, false);

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Cms.News.Update)]
        public ActionResult Edit(NewsItemModel model, bool continueEditing, FormCollection form)
        {
            var newsItem = _newsService.GetNewsById(model.Id);
            if (newsItem == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                newsItem = model.ToEntity(newsItem);

                MediaHelper.UpdatePictureTransientStateFor(newsItem, c => c.PictureId);
                MediaHelper.UpdatePictureTransientStateFor(newsItem, c => c.PreviewPictureId);

                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                _newsService.UpdateNews(newsItem);

                // Search engine name.
                var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(newsItem, seName, newsItem.LanguageId);

                SaveStoreMappings(newsItem, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, newsItem, form));

                NotifySuccess(T("Admin.ContentManagement.News.NewsItems.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            PrepareStoresMappingModel(model, newsItem, true);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [Permission(Permissions.Cms.News.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
                return RedirectToAction("List");

            _newsService.DeleteNews(newsItem);

            NotifySuccess(T("Admin.ContentManagement.News.NewsItems.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        [Permission(Permissions.Cms.News.Delete)]
        public ActionResult DeleteSelected(ICollection<int> selectedIds)
        {
            if (selectedIds != null)
            {
                var news = _newsService.GetNewsByIds(selectedIds.ToArray()).ToList();

                news.ForEach(x => _newsService.DeleteNews(x));
            }

            return Json(new { Result = true });
        }

        #endregion

        #region Comments

        [Permission(Permissions.Cms.News.Read)]
        public ActionResult Comments(int? filterByNewsItemId)
        {
            ViewBag.FilterByNewsItemId = filterByNewsItemId;
            var model = new GridModel<NewsCommentModel>();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.News.Read)]
        public ActionResult Comments(int? filterByNewsItemId, GridCommand command)
        {
            var gridModel = new GridModel<NewsCommentModel>();

            IList<NewsComment> comments;
            if (filterByNewsItemId.HasValue)
            {
                //filter comments by news item
                var newsItem = _newsService.GetNewsById(filterByNewsItemId.Value);
                comments = newsItem.NewsComments.OrderBy(bc => bc.CreatedOnUtc).ToList();
            }
            else
            {
                //load all news comments
                comments = _customerContentService.GetAllCustomerContent<NewsComment>(0, null);
            }

            gridModel.Data = comments.PagedForCommand(command).Select(newsComment =>
            {
                var commentModel = new NewsCommentModel();
                var customer = _customerService.GetCustomerById(newsComment.CustomerId);

                commentModel.Id = newsComment.Id;
                commentModel.NewsItemId = newsComment.NewsItemId;
                commentModel.NewsItemTitle = newsComment.NewsItem.Title;
                commentModel.CustomerId = newsComment.CustomerId;
                commentModel.IpAddress = newsComment.IpAddress;
                commentModel.CreatedOn = _dateTimeHelper.ConvertToUserTime(newsComment.CreatedOnUtc, DateTimeKind.Utc);
                commentModel.CommentTitle = newsComment.CommentTitle;
                commentModel.CommentText = HtmlUtils.ConvertPlainTextToHtml(newsComment.CommentText.HtmlEncode());

                if (customer == null)
                    commentModel.CustomerName = "".NaIfEmpty();
                else
                    commentModel.CustomerName = customer.GetFullName();

                return commentModel;
            });

            gridModel.Total = comments.Count;


            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.News.Update)]
        public ActionResult CommentDelete(int? filterByNewsItemId, int id, GridCommand command)
        {
            var comment = _customerContentService.GetCustomerContentById(id) as NewsComment;

            var newsItem = comment.NewsItem;
            _customerContentService.DeleteCustomerContent(comment);

            _newsService.UpdateCommentTotals(newsItem);

            return Comments(filterByNewsItemId, command);
        }

        #endregion
    }
}
