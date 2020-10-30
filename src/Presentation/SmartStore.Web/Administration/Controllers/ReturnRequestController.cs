using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Orders;
using SmartStore.Core.Domain.Common;
using SmartStore.Core.Domain.Localization;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Security;
using SmartStore.Services.Catalog;
using SmartStore.Services.Customers;
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
    public class ReturnRequestController : AdminControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly OrderSettings _orderSettings;
        private readonly AdminAreaSettings _adminAreaSettings;

        public ReturnRequestController(
            IOrderService orderService,
            ICustomerService customerService,
            LocalizationSettings localizationSettings,
            IOrderProcessingService orderProcessingService,
            IPriceFormatter priceFormatter,
            OrderSettings orderSettings,
            AdminAreaSettings adminAreaSettings)
        {
            _orderService = orderService;
            _customerService = customerService;
            _localizationSettings = localizationSettings;
            _orderProcessingService = orderProcessingService;
            _priceFormatter = priceFormatter;
            _orderSettings = orderSettings;
            _adminAreaSettings = adminAreaSettings;
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Order.ReturnRequest.Read)]
        public ActionResult List()
        {
            var model = new ReturnRequestListModel();
            PrepareReturnRequestListModel(model);

            return View(model);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.ReturnRequest.Read)]
        public ActionResult List(GridCommand command, ReturnRequestListModel model)
        {
            var gridModel = new GridModel<ReturnRequestModel>();
            var data = new List<ReturnRequestModel>();
            var allStores = Services.StoreService.GetAllStores().ToDictionary(x => x.Id);

            var returnRequests = _orderService.SearchReturnRequests(model.SearchStoreId, 0, 0, model.SearchReturnRequestStatus,
                command.Page - 1, command.PageSize, model.SearchId ?? 0);

            foreach (var rr in returnRequests)
            {
                var m = new ReturnRequestModel();
                if (PrepareReturnRequestModel(m, rr, allStores, false, true))
                {
                    data.Add(m);
                }
            }

            gridModel.Data = data;
            gridModel.Total = returnRequests.TotalCount;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Order.ReturnRequest.Read)]
        public ActionResult Edit(int id)
        {
            var returnRequest = _orderService.GetReturnRequestById(id);
            if (returnRequest == null)
            {
                return RedirectToAction("List");
            }

            var model = new ReturnRequestModel();
            var allStores = Services.StoreService.GetAllStores().ToDictionary(x => x.Id);
            PrepareReturnRequestModel(model, returnRequest, allStores);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.ReturnRequest.Update)]
        public ActionResult Edit(ReturnRequestModel model, bool continueEditing)
        {
            var returnRequest = _orderService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
            {
                return RedirectToAction("List");
            }

            if (ModelState.IsValid)
            {
                var utcNow = DateTime.UtcNow;

                if (returnRequest.RequestedAction != model.RequestedAction)
                {
                    returnRequest.RequestedActionUpdatedOnUtc = utcNow;
                }

                returnRequest.Quantity = model.Quantity;
                returnRequest.ReasonForReturn = model.ReasonForReturn;
                returnRequest.RequestedAction = model.RequestedAction;
                returnRequest.CustomerComments = model.CustomerComments;
                returnRequest.StaffNotes = model.StaffNotes;
                returnRequest.AdminComment = model.AdminComment;
                returnRequest.ReturnRequestStatusId = model.ReturnRequestStatusId;
                returnRequest.UpdatedOnUtc = utcNow;

                if (returnRequest.ReasonForReturn == null)
                {
                    returnRequest.ReasonForReturn = "";
                }
                if (returnRequest.RequestedAction == null)
                {
                    returnRequest.RequestedAction = "";
                }

                _customerService.UpdateCustomer(returnRequest.Customer);

                Services.CustomerActivity.InsertActivity("EditReturnRequest", T("ActivityLog.EditReturnRequest"), returnRequest.Id);

                NotifySuccess(T("Admin.ReturnRequests.Updated"));
                return continueEditing ? RedirectToAction("Edit", returnRequest.Id) : RedirectToAction("List");
            }

            // If we got this far, something failed, redisplay form.
            var allStores = Services.StoreService.GetAllStores().ToDictionary(x => x.Id);
            PrepareReturnRequestModel(model, returnRequest, allStores, true);

            return View(model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("notify-customer")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.ReturnRequest.Update)]
        public ActionResult NotifyCustomer(ReturnRequestModel model)
        {
            var returnRequest = _orderService.GetReturnRequestById(model.Id);
            if (returnRequest == null)
            {
                return RedirectToAction("List");
            }

            var orderItem = _orderService.GetOrderItemById(returnRequest.OrderItemId);
            var msg = Services.MessageFactory.SendReturnRequestStatusChangedCustomerNotification(returnRequest, orderItem, _localizationSettings.DefaultAdminLanguageId);

            if (msg?.Email?.Id != null)
            {
                NotifySuccess(T("Admin.ReturnRequests.Notified"));
            }

            return RedirectToAction("Edit", returnRequest.Id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.ReturnRequest.Delete)]
        public ActionResult DeleteConfirmed(int id)
        {
            var returnRequest = _orderService.GetReturnRequestById(id);
            if (returnRequest == null)
            {
                return RedirectToAction("List");
            }

            _orderService.DeleteReturnRequest(returnRequest);

            Services.CustomerActivity.InsertActivity("DeleteReturnRequest", T("ActivityLog.DeleteReturnRequest"), returnRequest.Id);

            NotifySuccess(T("Admin.ReturnRequests.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.ReturnRequest.Accept)]
        public ActionResult Accept(AutoUpdateOrderItemModel model)
        {
            var returnRequest = _orderService.GetReturnRequestById(model.Id);
            var oi = _orderService.GetOrderItemById(returnRequest.OrderItemId);
            var cancelQuantity = returnRequest.Quantity > oi.Quantity ? oi.Quantity : returnRequest.Quantity;

            var context = new AutoUpdateOrderItemContext
            {
                OrderItem = oi,
                QuantityOld = oi.Quantity,
                QuantityNew = Math.Max(oi.Quantity - cancelQuantity, 0),
                AdjustInventory = model.AdjustInventory,
                UpdateRewardPoints = model.UpdateRewardPoints,
                UpdateTotals = model.UpdateTotals
            };

            returnRequest.ReturnRequestStatus = ReturnRequestStatus.ReturnAuthorized;
            _customerService.UpdateCustomer(returnRequest.Customer);

            _orderProcessingService.AutoUpdateOrderDetails(context);

            TempData[AutoUpdateOrderItemContext.InfoKey] = context.ToString(Services.Localization);

            return RedirectToAction("Edit", new { id = returnRequest.Id });
        }

        private void PrepareReturnRequestListModel(ReturnRequestListModel model)
        {
            model.GridPageSize = _adminAreaSettings.GridPageSize;
            model.AvailableStores = Services.StoreService.GetAllStores().ToSelectListItems();
            model.AvailableReturnRequestStatus = ReturnRequestStatus.Pending.ToSelectList(false).ToList();
        }

        private bool PrepareReturnRequestModel(
            ReturnRequestModel model,
            ReturnRequest returnRequest,
            Dictionary<int, Store> allStores,
            bool excludeProperties = false,
            bool forList = false)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (returnRequest == null)
                throw new ArgumentNullException("returnRequest");

            var orderItem = _orderService.GetOrderItemById(returnRequest.OrderItemId);
            if (orderItem == null)
                return false;

            allStores.TryGetValue(returnRequest.StoreId, out Store store);

            model.Id = returnRequest.Id;
            model.ProductId = orderItem.ProductId;
            model.ProductSku = orderItem.Product.Sku;
            model.ProductName = orderItem.Product.Name;
            model.ProductTypeName = orderItem.Product.GetProductTypeLabel(Services.Localization);
            model.ProductTypeLabelHint = orderItem.Product.ProductTypeLabelHint;
            model.AttributeInfo = orderItem.AttributeDescription;
            model.OrderId = orderItem.OrderId;
            model.OrderNumber = orderItem.Order.GetOrderNumber();
            model.CustomerId = returnRequest.CustomerId;
            model.CustomerFullName = returnRequest.Customer.GetFullName().NaIfEmpty();
            model.CanSendEmailToCustomer = returnRequest.Customer.FindEmail().HasValue();
            model.Quantity = returnRequest.Quantity;
            model.ReturnRequestStatusStr = returnRequest.ReturnRequestStatus.GetLocalizedEnum(Services.Localization, Services.WorkContext);
            model.CreatedOn = Services.DateTimeHelper.ConvertToUserTime(returnRequest.CreatedOnUtc, DateTimeKind.Utc);
            model.UpdatedOn = Services.DateTimeHelper.ConvertToUserTime(returnRequest.UpdatedOnUtc, DateTimeKind.Utc);

            if (allStores.Count > 1)
            {
                model.StoreName = store?.Name;
            }

            if (!excludeProperties)
            {
                model.ReasonForReturn = returnRequest.ReasonForReturn;
                model.RequestedAction = returnRequest.RequestedAction;

                if (returnRequest.RequestedActionUpdatedOnUtc.HasValue)
                {
                    model.RequestedActionUpdated = Services.DateTimeHelper.ConvertToUserTime(returnRequest.RequestedActionUpdatedOnUtc.Value, DateTimeKind.Utc);
                }

                model.CustomerComments = returnRequest.CustomerComments;
                model.StaffNotes = returnRequest.StaffNotes;
                model.AdminComment = returnRequest.AdminComment;
                model.ReturnRequestStatusId = returnRequest.ReturnRequestStatusId;
            }

            if (!forList)
            {
                // That's what we also offer in frontend.
                string returnRequestReasons = _orderSettings.GetLocalizedSetting(x => x.ReturnRequestReasons, orderItem.Order.CustomerLanguageId, store?.Id, true, false);
                string returnRequestActions = _orderSettings.GetLocalizedSetting(x => x.ReturnRequestActions, orderItem.Order.CustomerLanguageId, store?.Id, true, false);
                string unspec = T("Common.Unspecified");

                model.AvailableReasonForReturn.Add(new SelectListItem { Text = unspec, Value = "" });
                foreach (var rrr in returnRequestReasons.SplitSafe(","))
                {
                    model.AvailableReasonForReturn.Add(new SelectListItem { Text = rrr, Value = rrr, Selected = (rrr == returnRequest.ReasonForReturn) });
                }

                model.AvailableRequestedAction.Add(new SelectListItem { Text = unspec, Value = "" });
                foreach (var rra in returnRequestActions.SplitSafe(","))
                {
                    model.AvailableRequestedAction.Add(new SelectListItem { Text = rra, Value = rra, Selected = (rra == returnRequest.RequestedAction) });
                }

                var urlHelper = new UrlHelper(Request.RequestContext);

                model.AutoUpdateOrderItem.Id = returnRequest.Id;
                model.AutoUpdateOrderItem.Caption = T("Admin.ReturnRequests.Accept.Caption");
                model.AutoUpdateOrderItem.PostUrl = urlHelper.Action("Accept", "ReturnRequest");
                model.AutoUpdateOrderItem.ShowUpdateTotals = (orderItem.Order.OrderStatusId <= (int)OrderStatus.Pending);
                model.AutoUpdateOrderItem.ShowUpdateRewardPoints = (orderItem.Order.OrderStatusId > (int)OrderStatus.Pending && orderItem.Order.RewardPointsWereAdded);
                model.AutoUpdateOrderItem.UpdateTotals = model.AutoUpdateOrderItem.ShowUpdateTotals;
                model.AutoUpdateOrderItem.UpdateRewardPoints = orderItem.Order.RewardPointsWereAdded;

                model.ReturnRequestInfo = TempData[AutoUpdateOrderItemContext.InfoKey] as string;

                // The maximum amount that can be refunded for this return request.
                var maxRefundAmount = Math.Max(orderItem.UnitPriceInclTax * returnRequest.Quantity, 0);
                if (maxRefundAmount > decimal.Zero)
                {
                    model.MaxRefundAmount = _priceFormatter.FormatPrice(maxRefundAmount, true, store.PrimaryStoreCurrency, Services.WorkContext.WorkingLanguage, true, true);
                }
            }

            return true;
        }
    }
}
