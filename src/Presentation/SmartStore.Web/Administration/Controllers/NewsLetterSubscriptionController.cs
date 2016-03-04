using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Helpers;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class NewsLetterSubscriptionController : AdminControllerBase
	{
		private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
		private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPermissionService _permissionService;
        private readonly AdminAreaSettings _adminAreaSettings;
		private readonly IStoreService _storeService;

		public NewsLetterSubscriptionController(INewsLetterSubscriptionService newsLetterSubscriptionService,
			IDateTimeHelper dateTimeHelper,
            IPermissionService permissionService,
			AdminAreaSettings adminAreaSettings,
			IStoreService storeService)
		{
			this._newsLetterSubscriptionService = newsLetterSubscriptionService;
			this._dateTimeHelper = dateTimeHelper;
            this._permissionService = permissionService;
            this._adminAreaSettings = adminAreaSettings;
			this._storeService = storeService;
		}

		private void PrepareNewsLetterSubscriptionListModel(NewsLetterSubscriptionListModel model)
		{
			var stores = _storeService.GetAllStores().ToList();

			model.GridPageSize = _adminAreaSettings.GridPageSize;

			model.AvailableStores = stores.ToSelectListItems();
		}

		public ActionResult Index()
		{
			return RedirectToAction("List");
		}

		public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var newsletterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(String.Empty, 0, _adminAreaSettings.GridPageSize, true);
			var model = new NewsLetterSubscriptionListModel();
			PrepareNewsLetterSubscriptionListModel(model);

			model.NewsLetterSubscriptions = new GridModel<NewsLetterSubscriptionModel>
			{
				Data = newsletterSubscriptions.Select(x => 
				{
					var m = x.ToModel();
					var store = _storeService.GetStoreById(x.StoreId);

					m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
					m.StoreName = store != null ? store.Name : "".NaIfEmpty();

					return m;
				}),
				Total = newsletterSubscriptions.TotalCount
			};
			return View(model);
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult SubscriptionList(GridCommand command, NewsLetterSubscriptionListModel model)
        {
			var gridModel = new GridModel<NewsLetterSubscriptionModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
			{
				var newsletterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(
					model.SearchEmail, command.Page - 1, command.PageSize, true, model.StoreId);

				gridModel.Data = newsletterSubscriptions.Select(x =>
				{
					var m = x.ToModel();
					var store = _storeService.GetStoreById(x.StoreId);

					m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
					m.StoreName = store != null ? store.Name : "".NaIfEmpty();

					return m;
				});

				gridModel.Total = newsletterSubscriptions.TotalCount;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<NewsLetterSubscriptionModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
		}

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SubscriptionUpdate(NewsLetterSubscriptionModel model, GridCommand command)
        {
			if (_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
			{
				if (!ModelState.IsValid)
				{
					var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
					return Content(modelStateErrors.FirstOrDefault());
				}

				var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(model.Id);
				subscription.Email = model.Email;
				subscription.Active = model.Active;

				_newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);
			}

			var listModel = new NewsLetterSubscriptionListModel();
			PrepareNewsLetterSubscriptionListModel(listModel);

			return SubscriptionList(command, listModel);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SubscriptionDelete(int id, GridCommand command)
        {
			if (_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
			{
				var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(id);

				_newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);
			}

			var listModel = new NewsLetterSubscriptionListModel();
			PrepareNewsLetterSubscriptionListModel(listModel);

			return SubscriptionList(command, listModel);
        }
	}
}
