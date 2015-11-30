using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Core.Domain.Common;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Security;
using SmartStore.Services.Stores;
using SmartStore.Utilities;

namespace SmartStore.Admin.Controllers
{
	[AdminAuthorize]
	public class NewsLetterSubscriptionController : AdminControllerBase
	{
		private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;
		private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly AdminAreaSettings _adminAreaSettings;
		private readonly IStoreService _storeService;

		public NewsLetterSubscriptionController(INewsLetterSubscriptionService newsLetterSubscriptionService,
			IDateTimeHelper dateTimeHelper,ILocalizationService localizationService,
            IPermissionService permissionService, AdminAreaSettings adminAreaSettings,
			IStoreService storeService)
		{
			this._newsLetterSubscriptionService = newsLetterSubscriptionService;
			this._dateTimeHelper = dateTimeHelper;
            this._localizationService = localizationService;
            this._permissionService = permissionService;
            this._adminAreaSettings = adminAreaSettings;
			this._storeService = storeService;
		}

		private void PrepareNewsLetterSubscriptionListModel(NewsLetterSubscriptionListModel model)
		{
			var stores = _storeService.GetAllStores().ToList();

			model.GridPageSize = _adminAreaSettings.GridPageSize;

			model.AvailableStores.Add(new SelectListItem() { Text = _localizationService.GetResource("Admin.Common.All"), Value = "0" });
			model.AvailableStores.AddRange(stores.ToSelectListItems());

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
					m.StoreName = store != null ? store.Name : "Unknown";

					return m;
				}),
				Total = newsletterSubscriptions.TotalCount
			};
			return View(model);
		}

		[HttpPost, GridAction(EnableCustomBinding = true)]
		public ActionResult SubscriptionList(GridCommand command, NewsLetterSubscriptionListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var newsletterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(
				model.SearchEmail, command.Page - 1, command.PageSize, true, model.StoreId);

            var gridModel = new GridModel<NewsLetterSubscriptionModel>
            {
                Data = newsletterSubscriptions.Select(x =>
				{
					var m = x.ToModel();
					var store = _storeService.GetStoreById(x.StoreId);

					m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
					m.StoreName = store != null ? store.Name : "Unknown";

					return m;
				}),
                Total = newsletterSubscriptions.TotalCount
            };
            return new JsonResult
            {
                Data = gridModel
            };
		}

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SubscriptionUpdate(NewsLetterSubscriptionModel model, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();
            
            if (!ModelState.IsValid)
            {
                //display the first model error
                var modelStateErrors = this.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
                return Content(modelStateErrors.FirstOrDefault());
            }

            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(model.Id);
            subscription.Email = model.Email;
            subscription.Active = model.Active;

            _newsLetterSubscriptionService.UpdateNewsLetterSubscription(subscription);

			var listModel = new NewsLetterSubscriptionListModel();
			PrepareNewsLetterSubscriptionListModel(listModel);

            return SubscriptionList(command, listModel);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult SubscriptionDelete(int id, GridCommand command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(id);
            if (subscription == null)
                throw new ArgumentException("No subscription found with the specified id");

            _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

			var listModel = new NewsLetterSubscriptionListModel();
			PrepareNewsLetterSubscriptionListModel(listModel);

			return SubscriptionList(command, listModel);
        }

		public ActionResult ExportCsv(NewsLetterSubscriptionListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

			string fileName = String.Format("newsletter_emails_{0}_{1}.txt", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), CommonHelper.GenerateRandomDigitCode(4));

			var sb = new StringBuilder();
			var newsLetterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(model.SearchEmail, 0, int.MaxValue, true, model.StoreId);

			if (newsLetterSubscriptions.Count == 0)
			{
				NotifyInfo(_localizationService.GetResource("Admin.Common.ExportNoData"));
				return RedirectToAction("List");
			}

			foreach (var subscription in newsLetterSubscriptions)
			{
				sb.Append(subscription.Email);
                sb.Append(",");
                sb.Append(subscription.Active);
				sb.Append(",");
				sb.Append(subscription.StoreId);
                sb.Append(Environment.NewLine);
			}
			string result = sb.ToString();

			return File(Encoding.UTF8.GetBytes(result), "text/csv", fileName);
		}

        [HttpPost]
        public ActionResult ImportCsv(FormCollection form)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageNewsletterSubscribers))
                return AccessDeniedView();

            try
            {            
                var file = Request.Files["importcsvfile"];
                if (file != null && file.ContentLength > 0)
                {
                    var result = _newsLetterSubscriptionService.ImportSubscribers(file.InputStream);

					NotifySuccess(
						_localizationService.GetResource("Admin.Promotions.NewsLetterSubscriptions.ImportEmailsSuccess").FormatWith(result.NewRecords, result.ModifiedRecords)
					);

                    return RedirectToAction("List");
                }

				NotifyError(_localizationService.GetResource("Admin.Common.UploadFile"));
                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                NotifyError(exc);
                return RedirectToAction("List");
            }
        }
	}
}
