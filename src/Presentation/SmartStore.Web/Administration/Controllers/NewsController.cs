using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.News;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.News;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.News;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
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
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly AdminAreaSettings _adminAreaSettings;
		private readonly IStoreService _storeService;
		private readonly IStoreMappingService _storeMappingService;
		private readonly ICustomerService _customerService;
        
		#endregion

		#region Constructors

        public NewsController(INewsService newsService, ILanguageService languageService,
            IDateTimeHelper dateTimeHelper, ICustomerContentService customerContentService,
            ILocalizationService localizationService, IPermissionService permissionService,
            IUrlRecordService urlRecordService, IStoreService storeService, IStoreMappingService storeMappingService,
			AdminAreaSettings adminAreaSettings,
			ICustomerService customerService)
        {
            this._newsService = newsService;
            this._languageService = languageService;
            this._dateTimeHelper = dateTimeHelper;
            this._customerContentService = customerContentService;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._urlRecordService = urlRecordService;
			this._storeService = storeService;
			this._storeMappingService = storeMappingService;
            this._adminAreaSettings = adminAreaSettings;
			this._customerService = customerService;
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

			model.AvailableStores = _storeService.GetAllStores().ToSelectListItems(model.SelectedStoreIds);
		}

		#endregion

        #region News items

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

			var model = new NewsItemListModel();

			foreach (var s in _storeService.GetAllStores())
			{
				model.AvailableStores.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString() });
			}

			return View(model);
        }

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult List(GridCommand command, NewsItemListModel model)
		{
			var gridModel = new GridModel<NewsItemModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageNews))
			{
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
			}
			else
			{
				gridModel.Data = Enumerable.Empty<NewsItemModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Create()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);
            var model = new NewsItemModel();
			//Stores
			PrepareStoresMappingModel(model, null, false);
            //default values
            model.Published = true;
            model.AllowComments = true;
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(NewsItemModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var newsItem = model.ToEntity();
                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                newsItem.CreatedOnUtc = DateTime.UtcNow;
                _newsService.InsertNews(newsItem);

                //search engine name
                var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(newsItem, seName, newsItem.LanguageId);

				//Stores
				_storeMappingService.SaveStoreMappings<NewsItem>(newsItem, model.SelectedStoreIds);

                NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

			//Stores
			PrepareStoresMappingModel(model, null, true);

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

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

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Edit(NewsItemModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(model.Id);
            if (newsItem == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                newsItem = model.ToEntity(newsItem);
                newsItem.StartDateUtc = model.StartDate;
                newsItem.EndDateUtc = model.EndDate;
                _newsService.UpdateNews(newsItem);

                //search engine name
                var seName = newsItem.ValidateSeName(model.SeName, model.Title, true);
                _urlRecordService.SaveSlug(newsItem, seName, newsItem.LanguageId);

				//Stores
				_storeMappingService.SaveStoreMappings<NewsItem>(newsItem, model.SelectedStoreIds);

                NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Updated"));
                return continueEditing ? RedirectToAction("Edit", new { id = newsItem.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            ViewBag.AllLanguages = _languageService.GetAllLanguages(true);

			//stores
			PrepareStoresMappingModel(model, newsItem, true);

            return View(model);
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            var newsItem = _newsService.GetNewsById(id);
            if (newsItem == null)
                return RedirectToAction("List");

            _newsService.DeleteNews(newsItem);

            NotifySuccess(_localizationService.GetResource("Admin.ContentManagement.News.NewsItems.Deleted"));
            return RedirectToAction("List");
        }

		[HttpPost]
		public ActionResult DeleteSelected(ICollection<int> selectedIds)
		{
			if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
				return AccessDeniedView();

			if (selectedIds != null)
			{
				var news = _newsService.GetNewsByIds(selectedIds.ToArray()).ToList();

				news.ForEach(x => _newsService.DeleteNews(x));
			}

			return Json(new { Result = true });
		}

        #endregion

        #region Comments

        public ActionResult Comments(int? filterByNewsItemId)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNews))
                return AccessDeniedView();

            ViewBag.FilterByNewsItemId = filterByNewsItemId;
            var model = new GridModel<NewsCommentModel>();
            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult Comments(int? filterByNewsItemId, GridCommand command)
        {
			var gridModel = new GridModel<NewsCommentModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageNews))
			{
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
					commentModel.CommentText = Core.Html.HtmlUtils.FormatText(newsComment.CommentText, false, true, false, false, false, false);

					if (customer == null)
						commentModel.CustomerName = "".NaIfEmpty();
					else
						commentModel.CustomerName = customer.GetFullName();

					return commentModel;
				});

				gridModel.Total = comments.Count;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<NewsCommentModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult CommentDelete(int? filterByNewsItemId, int id, GridCommand command)
        {
			if (_permissionService.Authorize(StandardPermissionProvider.ManageNews))
			{
				var comment = _customerContentService.GetCustomerContentById(id) as NewsComment;

				var newsItem = comment.NewsItem;
				_customerContentService.DeleteCustomerContent(comment);
			
				//update totals
				_newsService.UpdateCommentTotals(newsItem);
			}

            return Comments(filterByNewsItemId, command);
        }


        #endregion
    }
}
