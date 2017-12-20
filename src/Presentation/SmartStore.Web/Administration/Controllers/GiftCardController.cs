using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Orders;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Logging;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Messages;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class GiftCardController : AdminControllerBase
    {
        #region Fields

        private readonly IGiftCardService _giftCardService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly LocalizationSettings _localizationSettings;
        private readonly ILanguageService _languageService;
        private readonly ICustomerActivityService _customerActivityService;
		private readonly ICommonServices _services;
		private readonly AdminAreaSettings _adminAreaSettings;

		#endregion

		#region Constructors

		public GiftCardController(IGiftCardService giftCardService,
            IPriceFormatter priceFormatter,
            IDateTimeHelper dateTimeHelper,
			LocalizationSettings localizationSettings,
            ILanguageService languageService,
            ICustomerActivityService customerActivityService,
			ICommonServices services,
			AdminAreaSettings adminAreaSettings)
        {
            _giftCardService = giftCardService;
            _priceFormatter = priceFormatter;
            _dateTimeHelper = dateTimeHelper;
            _localizationSettings = localizationSettings;
            _languageService = languageService;
            _customerActivityService = customerActivityService;
			_services = services;
			_adminAreaSettings = adminAreaSettings;
        }

        #endregion

        #region Methods

        //list
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

			var model = new GiftCardListModel
			{
				GridPageSize = _adminAreaSettings.GridPageSize
			};

			model.ActivatedList.Add(new SelectListItem
            {
                Value = "1",
                Text = _services.Localization.GetResource("Common.Activated", logIfNotFound: false, defaultValue: "Activated")
            });

            model.ActivatedList.Add(new SelectListItem
            {
                Value = "2",
                Text = _services.Localization.GetResource("Common.Deactivated", logIfNotFound: false, defaultValue: "Deactivated")
            });

            return View(model);
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult GiftCardList(GridCommand command, GiftCardListModel model)
        {
			var gridModel = new GridModel<GiftCardModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageGiftCards))
			{
				bool? isGiftCardActivated = null;

				if (model.ActivatedId == 1)
					isGiftCardActivated = true;
				else if (model.ActivatedId == 2)
					isGiftCardActivated = false;

				var giftCards = _giftCardService.GetAllGiftCards(null, null, null, isGiftCardActivated, model.CouponCode);

				gridModel.Data = giftCards.PagedForCommand(command).Select(x =>
				{
					var m = x.ToModel();
					m.RemainingAmountStr = _priceFormatter.FormatPrice(x.GetGiftCardRemainingAmount(), true, false);
					m.AmountStr = _priceFormatter.FormatPrice(x.Amount, true, false);
					m.CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc);
					return m;
				});

				gridModel.Total = giftCards.Count();
			}
			else
			{
				gridModel.Data = Enumerable.Empty<GiftCardModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Create()
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var model = new GiftCardModel();
			model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public ActionResult Create(GiftCardModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var giftCard = model.ToEntity();
                giftCard.CreatedOnUtc = DateTime.UtcNow;
                _giftCardService.InsertGiftCard(giftCard);

                //activity log
                _customerActivityService.InsertActivity("AddNewGiftCard", _services.Localization.GetResource("ActivityLog.AddNewGiftCard"), giftCard.GiftCardCouponCode);

                NotifySuccess(_services.Localization.GetResource("Admin.GiftCards.Added"));
                return continueEditing ? RedirectToAction("Edit", new { id = giftCard.Id }) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
			model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            return View(model);
        }

        public ActionResult Edit(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var giftCard = _giftCardService.GetGiftCardById(id);
            if (giftCard == null)
                //No gift card found with the specified id
                return RedirectToAction("List");

            var model = giftCard.ToModel();
            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? (int?)giftCard.PurchasedWithOrderItem.OrderId : null;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
			model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(GiftCardModel model, bool continueEditing)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var giftCard = _giftCardService.GetGiftCardById(model.Id);

            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? (int?)giftCard.PurchasedWithOrderItem.OrderId : null;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
			model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            if (ModelState.IsValid)
            {
                giftCard = model.ToEntity(giftCard);
                _giftCardService.UpdateGiftCard(giftCard);

                //activity log
                _customerActivityService.InsertActivity("EditGiftCard", _services.Localization.GetResource("ActivityLog.EditGiftCard"), giftCard.GiftCardCouponCode);

                NotifySuccess(_services.Localization.GetResource("Admin.GiftCards.Updated"));
                return continueEditing ? RedirectToAction("Edit", giftCard.Id) : RedirectToAction("List");
            }

            //If we got this far, something failed, redisplay form
            return View(model);
        }
        
        [HttpPost]
        public ActionResult GenerateCouponCode()
        {
            return Json(new { CouponCode = _giftCardService.GenerateGiftCardCode() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("notifyRecipient")]
        public ActionResult NotifyRecipient(GiftCardModel model)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var giftCard = _giftCardService.GetGiftCardById(model.Id);

            model = giftCard.ToModel();
            model.PurchasedWithOrderId = giftCard.PurchasedWithOrderItem != null ? (int?)giftCard.PurchasedWithOrderItem.OrderId : null;
            model.RemainingAmountStr = _priceFormatter.FormatPrice(giftCard.GetGiftCardRemainingAmount(), true, false);
            model.AmountStr = _priceFormatter.FormatPrice(giftCard.Amount, true, false);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(giftCard.CreatedOnUtc, DateTimeKind.Utc);
			model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            try
            {
				if (!giftCard.RecipientEmail.IsEmail())
                    throw new SmartException("Recipient email is not valid");
				if (!giftCard.SenderEmail.IsEmail())
                    throw new SmartException("Sender email is not valid");

                var languageId = 0;
	            var order = giftCard.PurchasedWithOrderItem != null ? giftCard.PurchasedWithOrderItem.Order : null;
	            if (order != null)
	            {
	                var customerLang = _languageService.GetLanguageById(order.CustomerLanguageId);
	                if (customerLang == null)
	                    customerLang = _languageService.GetAllLanguages().FirstOrDefault();
	            }
	            else
	            {
	                languageId = _localizationSettings.DefaultAdminLanguageId;
	            }

	            var msg = Services.MessageFactory.SendGiftCardNotification(giftCard, languageId);

                if (msg?.Email?.Id != null)
                {
                    giftCard.IsRecipientNotified = true;
                    _giftCardService.UpdateGiftCard(giftCard);
					NotifySuccess(T("Admin.Common.TaskSuccessfullyProcessed"));
                }
            }
            catch (Exception exc)
            {
                NotifyError(exc, false);
            }

            return View(model);
        }
        
        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_services.Permissions.Authorize(StandardPermissionProvider.ManageGiftCards))
                return AccessDeniedView();

            var giftCard = _giftCardService.GetGiftCardById(id);
            if (giftCard == null)
                return RedirectToAction("List");

            _giftCardService.DeleteGiftCard(giftCard);

            _customerActivityService.InsertActivity("DeleteGiftCard", _services.Localization.GetResource("ActivityLog.DeleteGiftCard"), giftCard.GiftCardCouponCode);

            NotifySuccess(_services.Localization.GetResource("Admin.GiftCards.Deleted"));
            return RedirectToAction("List");
        }
        
        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult UsageHistoryList(int giftCardId, GridCommand command)
        {
			var model = new GridModel<GiftCardModel.GiftCardUsageHistoryModel>();

			if (_services.Permissions.Authorize(StandardPermissionProvider.ManageGiftCards))
			{
				var giftCard = _giftCardService.GetGiftCardById(giftCardId);

				var usageHistoryModel = giftCard.GiftCardUsageHistory
					.OrderByDescending(gcuh => gcuh.CreatedOnUtc)
					.Select(x =>
					{
						return new GiftCardModel.GiftCardUsageHistoryModel()
						{
							Id = x.Id,
							OrderId = x.UsedWithOrderId,
							UsedValue = _priceFormatter.FormatPrice(x.UsedValue, true, false),
							CreatedOn = _dateTimeHelper.ConvertToUserTime(x.CreatedOnUtc, DateTimeKind.Utc)
						};
					})
					.ToList();

				model.Data = usageHistoryModel.PagedForCommand(command);
				model.Total = usageHistoryModel.Count;
			}
			else
			{
				model.Data = Enumerable.Empty<GiftCardModel.GiftCardUsageHistoryModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        #endregion
    }
}
