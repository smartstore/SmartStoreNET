using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Messages;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Security;
using SmartStore.Services.Helpers;
using SmartStore.Services.Messages;
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
        private readonly AdminAreaSettings _adminAreaSettings;

        public NewsLetterSubscriptionController(
            INewsLetterSubscriptionService newsLetterSubscriptionService,
            IDateTimeHelper dateTimeHelper,
            AdminAreaSettings adminAreaSettings)
        {
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
            _dateTimeHelper = dateTimeHelper;
            _adminAreaSettings = adminAreaSettings;
        }

        private void PrepareNewsLetterSubscriptionListModel(NewsLetterSubscriptionListModel model)
        {
            var stores = Services.StoreService.GetAllStores().ToList();

            model.AvailableStores = stores.ToSelectListItems();
            model.GridPageSize = _adminAreaSettings.GridPageSize;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Promotion.Newsletter.Read)]
        public ActionResult List()
        {
            var newsletterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(string.Empty, 0, _adminAreaSettings.GridPageSize, true);

            var allStores = Services.StoreService.GetAllStores().ToDictionary(x => x.Id);
            var model = new NewsLetterSubscriptionListModel();
            PrepareNewsLetterSubscriptionListModel(model);

            model.NewsLetterSubscriptions = new GridModel<NewsLetterSubscriptionModel>
            {
                Data = newsletterSubscriptions.Select(x =>
                {
                    var m = x.Subscription.ToModel();
                    allStores.TryGetValue(x.Subscription.StoreId, out var store);

                    m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.Subscription.CreatedOnUtc, DateTimeKind.Utc);
                    m.StoreName = store?.Name.NaIfEmpty();

                    return m;
                }),
                Total = newsletterSubscriptions.TotalCount
            };

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Newsletter.Read)]
        public ActionResult SubscriptionList(GridCommand command, NewsLetterSubscriptionListModel model)
        {
            var gridModel = new GridModel<NewsLetterSubscriptionModel>();
            var allStores = Services.StoreService.GetAllStores().ToDictionary(x => x.Id);

            var newsletterSubscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscriptions(
                model.SearchEmail,
                command.Page - 1,
                command.PageSize,
                true,
                model.StoreId != 0 ? new int[] { model.StoreId } : null,
                model.SearchCustomerRoleIds);

            gridModel.Data = newsletterSubscriptions.Select(x =>
            {
                var m = x.Subscription.ToModel();
                allStores.TryGetValue(x.Subscription.StoreId, out var store);

                m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.Subscription.CreatedOnUtc, DateTimeKind.Utc);
                m.StoreName = store?.Name.NaIfEmpty();

                return m;
            });

            gridModel.Total = newsletterSubscriptions.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Newsletter.Update)]
        public ActionResult SubscriptionUpdate(NewsLetterSubscriptionModel model, GridCommand command)
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

            var listModel = new NewsLetterSubscriptionListModel();
            PrepareNewsLetterSubscriptionListModel(listModel);

            return SubscriptionList(command, listModel);
        }

        [GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Promotion.Newsletter.Delete)]
        public ActionResult SubscriptionDelete(int id, GridCommand command)
        {
            var subscription = _newsLetterSubscriptionService.GetNewsLetterSubscriptionById(id);

            _newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription);

            var listModel = new NewsLetterSubscriptionListModel();
            PrepareNewsLetterSubscriptionListModel(listModel);

            return SubscriptionList(command, listModel);
        }
    }
}
