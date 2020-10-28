using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Orders;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Logging;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
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

        #region Gift card

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Order.GiftCard.Read)]
        public ActionResult List()
        {
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
        [Permission(Permissions.Order.GiftCard.Read)]
        public ActionResult GiftCardList(GridCommand command, GiftCardListModel model)
        {
            var gridModel = new GridModel<GiftCardModel>();

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

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.GiftCard.Read)]
        public ActionResult UsageHistoryList(int giftCardId, GridCommand command)
        {
            var model = new GridModel<GiftCardModel.GiftCardUsageHistoryModel>();

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

            return new JsonResult
            {
                Data = model
            };
        }

        [Permission(Permissions.Order.GiftCard.Create)]
        public ActionResult Create()
        {
            var model = new GiftCardModel();
            model.PrimaryStoreCurrencyCode = _services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.GiftCard.Create)]
        public ActionResult Create(GiftCardModel model, bool continueEditing)
        {
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

        [Permission(Permissions.Order.GiftCard.Read)]
        public ActionResult Edit(int id)
        {
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
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.GiftCard.Update)]
        public ActionResult Edit(GiftCardModel model, bool continueEditing)
        {
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
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.GiftCard.Read)]
        public ActionResult GenerateCouponCode()
        {
            return Json(new { CouponCode = _giftCardService.GenerateGiftCardCode() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.GiftCard.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var giftCard = _giftCardService.GetGiftCardById(id);
            if (giftCard == null)
                return RedirectToAction("List");

            _giftCardService.DeleteGiftCard(giftCard);

            _customerActivityService.InsertActivity("DeleteGiftCard", _services.Localization.GetResource("ActivityLog.DeleteGiftCard"), giftCard.GiftCardCouponCode);

            NotifySuccess(_services.Localization.GetResource("Admin.GiftCards.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("notifyRecipient")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.GiftCard.Notify)]
        public ActionResult NotifyRecipient(GiftCardModel model)
        {
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

        #endregion
    }
}
