using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.News;
using SmartStore.Core;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.News;
using SmartStore.Core.Html;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.News;
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
        private readonly AdminAreaSettings _adminAreaSettings;

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
            ICustomerService customerService,
            AdminAreaSettings adminAreaSettings)
        {
            _newsService = newsService;
            _languageService = languageService;
            _dateTimeHelper = dateTimeHelper;
            _customerContentService = customerContentService;
            _urlRecordService = urlRecordService;
            _storeService = storeService;
            _storeMappingService = storeMappingService;
            _customerService = customerService;
            _adminAreaSettings = adminAreaSettings;
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
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            var model = new NewsItemListModel();

            // IsSingleStoreMode & IsMultiLangMode
            model.IsSingleStoreMode = _storeService.IsSingleStoreMode();
            model.IsSingleLangMode = _languageService.GetAllLanguages(true).Count == 1;

            // Set end date to now.
            model.SearchEndDate = DateTime.UtcNow;

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.News.Read)]
        public ActionResult List(GridCommand command, NewsItemListModel model)
        {
            var news = _newsService.GetAllNews(model.SearchLanguageId, model.SearchStoreId, command.Page - 1, command.PageSize, !model.SearchIsPublished ?? true,
                title: model.SearchTitle, intro: model.SearchShort, full: model.SearchFull);

            var gridModel = new GridModel<NewsItemModel>
            {
                Total = news.TotalCount
            };

            gridModel.Data = news.Select(x =>
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

        [Permission(Permissions.Cms.News.Create)]
        public ActionResult Create()
        {
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

            var model = new NewsItemModel
            {
                Published = true,
                AllowComments = true
            };

            PrepareStoresMappingModel(model, null, false);

            //default values
            model.Published = true;
            model.AllowComments = true;
            model.CreatedOn = DateTime.UtcNow;

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [Permission(Permissions.Cms.News.Create)]
        public ActionResult Create(NewsItemModel model, bool continueEditing, FormCollection form)
        {
            if (ModelState.IsValid)
            {
                var newsItem = model.ToEntity();

                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                newsItem.CreatedOnUtc = model.CreatedOn;
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
            model.CreatedOn = newsItem.CreatedOnUtc;

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

                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                newsItem.CreatedOnUtc = model.CreatedOn;
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
            ViewBag.GridPageSize = _adminAreaSettings.GridPageSize;

            return View();
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.News.Read)]
        public ActionResult Comments(int? filterByNewsItemId, GridCommand command)
        {
            IPagedList<NewsComment> comments;

            if (filterByNewsItemId.HasValue)
            {
                // Filter comments by news item.
                var query = _customerContentService.GetAllCustomerContent<NewsComment>(0, null).SourceQuery;
                query = query.Where(x => x.NewsItemId == filterByNewsItemId.Value);

                comments = new PagedList<NewsComment>(query, command.Page - 1, command.PageSize);
            }
            else
            {
                // Load all news comments.
                comments = _customerContentService.GetAllCustomerContent<NewsComment>(0, null, null, null, command.Page - 1, command.PageSize);
            }

            var customerIds = comments.Select(x => x.CustomerId).Distinct().ToArray();
            var customers = _customerService.GetCustomersByIds(customerIds).ToDictionarySafe(x => x.Id);

            var model = new GridModel<NewsCommentModel>
            {
                Total = comments.TotalCount
            };

            model.Data = comments.Select(newsComment =>
            {
                customers.TryGetValue(newsComment.CustomerId, out var customer);

                var commentModel = new NewsCommentModel
                {
                    Id = newsComment.Id,
                    NewsItemId = newsComment.NewsItemId,
                    NewsItemTitle = newsComment.NewsItem.Title,
                    CustomerId = newsComment.CustomerId,
                    IpAddress = newsComment.IpAddress,
                    CreatedOn = _dateTimeHelper.ConvertToUserTime(newsComment.CreatedOnUtc, DateTimeKind.Utc),
                    CommentTitle = newsComment.CommentTitle,
                    CommentText = HtmlUtils.ConvertPlainTextToHtml(newsComment.CommentText.HtmlEncode()),
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
