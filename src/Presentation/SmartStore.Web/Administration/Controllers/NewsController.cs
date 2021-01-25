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
using SmartStore.Data.Utilities;
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
        private readonly INewsService _newsService;
        private readonly ILanguageService _languageService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ICustomerContentService _customerContentService;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IStoreService _storeService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly ICustomerService _customerService;
        private readonly AdminAreaSettings _adminAreaSettings;

        public NewsController(
            INewsService newsService,
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
            _newsService = newsService;
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

        private void UpdateLocales(NewsItem newsItem, NewsItemModel model)
        {
            foreach (var localized in model.Locales)
            {
                _localizedEntityService.SaveLocalizedValue(newsItem, x => x.Title, localized.Title, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(newsItem, x => x.Short, localized.Short, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(newsItem, x => x.Full, localized.Full, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(newsItem, x => x.MetaKeywords, localized.MetaKeywords, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(newsItem, x => x.MetaDescription, localized.MetaDescription, localized.LanguageId);
                _localizedEntityService.SaveLocalizedValue(newsItem, x => x.MetaTitle, localized.MetaTitle, localized.LanguageId);

                var seName = newsItem.ValidateSeName(localized.SeName, localized.Title, false, localized.LanguageId);
                _urlRecordService.SaveSlug(newsItem, seName, localized.LanguageId);
            }
        }

        private void PrepareNewsItemModel(NewsItemModel model, NewsItem newsItem)
        {
            if (newsItem != null)
            {
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(newsItem);
            }

            var allLanguages = _languageService.GetAllLanguages(true);
            model.AvailableLanguages = allLanguages
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();

            model.IsSingleLanguageMode = allLanguages.Count <= 1;
        }

        #endregion

        #region News items

        // AJAX.
        public ActionResult AllNews(string selectedIds)
        {
            var query = _newsService.GetAllNews(0, 0, int.MaxValue, 0, true).SourceQuery;
            var pager = new FastPager<NewsItem>(query, 500);
            var allNewsItems = new List<dynamic>();
            var ids = selectedIds.ToIntArray().ToList();

            while (pager.ReadNextPage(out var newsItems))
            {
                foreach (var newsItem in newsItems)
                {
                    dynamic obj = new
                    {
                        newsItem.Id,
                        newsItem.CreatedOnUtc,
                        Title = newsItem.GetLocalized(x => x.Title).Value
                    };

                    allNewsItems.Add(obj);
                }
            }

            var data = allNewsItems
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

        [Permission(Permissions.Cms.News.Read)]
        public ActionResult List()
        {
            var allLanguages = _languageService.GetAllLanguages(true);

            var model = new NewsItemListModel
            {
                IsSingleStoreMode = _storeService.IsSingleStoreMode(),
                IsSingleLanguageMode = allLanguages.Count <= 1,
                GridPageSize = _adminAreaSettings.GridPageSize,
                SearchEndDate = DateTime.UtcNow
            };

            model.AvailableLanguages = allLanguages
                .Select(x => new SelectListItem { Text = x.Name, Value = x.Id.ToString() })
                .ToList();

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Cms.News.Read)]
        public ActionResult List(GridCommand command, NewsItemListModel model)
        {
            var news = _newsService.GetAllNews(
                model.SearchStoreId,
                command.Page - 1,
                command.PageSize,
                model.SearchLanguageId,
                true,
                null,
                model.SearchTitle, 
                model.SearchShort, 
                model.SearchFull);

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
                m.Comments = x.ApprovedCommentCount + x.NotApprovedCommentCount;

                if (x.LanguageId.HasValue)
                {
                    m.LanguageName = x.Language?.Name;
                }

                return m;
            }).Where(x => x.Published == model.SearchIsPublished || model.SearchIsPublished == null);

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Cms.News.Create)]
        public ActionResult Create()
        {
            var model = new NewsItemModel
            {
                Published = true,
                AllowComments = true,
                CreatedOn = DateTime.UtcNow
            };

            AddLocales(_languageService, model.Locales);
            PrepareNewsItemModel(model, null);

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
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

                model.SeName = newsItem.ValidateSeName(model.SeName, newsItem.Title, true);
                _urlRecordService.SaveSlug(newsItem, model.SeName, 0);

                UpdateLocales(newsItem, model);

                SaveStoreMappings(newsItem, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, newsItem, form));

                NotifySuccess(T("Admin.ContentManagement.News.NewsItems.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            PrepareNewsItemModel(model, null);

            return View(model);
        }

        [Permission(Permissions.Cms.News.Read)]
        public ActionResult Edit(int id)
        {
            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
            {
                return RedirectToAction("List");
            }

            var model = newsItem.ToModel();

            AddLocales(_languageService, model.Locales, (locale, languageId) =>
            {
                locale.Title = newsItem.GetLocalized(x => x.Title, languageId, false, false);
                locale.Short = newsItem.GetLocalized(x => x.Short, languageId, false, false);
                locale.Full = newsItem.GetLocalized(x => x.Full, languageId, false, false);
                locale.MetaKeywords = newsItem.GetLocalized(x => x.MetaKeywords, languageId, false, false);
                locale.MetaDescription = newsItem.GetLocalized(x => x.MetaDescription, languageId, false, false);
                locale.MetaTitle = newsItem.GetLocalized(x => x.MetaTitle, languageId, false, false);
                locale.SeName = newsItem.GetSeName(languageId, false, false);
            });

            model.StartDate = newsItem.StartDateUtc;
            model.EndDate = newsItem.EndDateUtc;
            model.CreatedOn = newsItem.CreatedOnUtc;

            PrepareNewsItemModel(model, newsItem);

            return View(model);
        }

        [HttpPost, ValidateInput(false), ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.News.Update)]
        public ActionResult Edit(NewsItemModel model, bool continueEditing, FormCollection form)
        {
            var newsItem = _newsService.GetNewsById(model.Id);
            if (newsItem == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                newsItem = model.ToEntity(newsItem);

                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                newsItem.CreatedOnUtc = model.CreatedOn;

                model.SeName = newsItem.ValidateSeName(model.SeName, newsItem.Title, true);
                _urlRecordService.SaveSlug(newsItem, model.SeName, 0);

                UpdateLocales(newsItem, model);

                _newsService.UpdateNews(newsItem);

                SaveStoreMappings(newsItem, model.SelectedStoreIds);

                Services.EventPublisher.Publish(new ModelBoundEvent(model, newsItem, form));
                NotifySuccess(T("Admin.ContentManagement.News.NewsItems.Updated"));

                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            PrepareNewsItemModel(model, newsItem);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Cms.News.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
            {
                return RedirectToAction("List");
            }

            _newsService.DeleteNews(newsItem);

            NotifySuccess(T("Admin.ContentManagement.News.NewsItems.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
                    NewsItemTitle = newsComment.NewsItem.GetLocalized(x => x.Title),
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
