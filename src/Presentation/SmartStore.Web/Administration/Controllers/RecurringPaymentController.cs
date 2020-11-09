using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Orders;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Security;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;
using Telerik.Web.Mvc;

namespace SmartStore.Admin.Controllers
{
    [AdminAuthorize]
    public class RecurringPaymentController : AdminControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPaymentService _paymentService;

        public RecurringPaymentController(
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILocalizationService localizationService,
            IWorkContext workContext,
            IDateTimeHelper dateTimeHelper,
            IPaymentService paymentService)
        {
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _localizationService = localizationService;
            _workContext = workContext;
            _dateTimeHelper = dateTimeHelper;
            _paymentService = paymentService;
        }

        private void PrepareRecurringPaymentModel(RecurringPaymentModel model, RecurringPayment recurringPayment, bool includeHistory)
        {
            Guard.NotNull(model, nameof(model));
            Guard.NotNull(recurringPayment, nameof(recurringPayment));

            model.Id = recurringPayment.Id;
            model.CycleLength = recurringPayment.CycleLength;
            model.CyclePeriodId = recurringPayment.CyclePeriodId;
            model.CyclePeriodStr = recurringPayment.CyclePeriod.GetLocalizedEnum(_localizationService, _workContext);
            model.TotalCycles = recurringPayment.TotalCycles;
            model.StartDate = _dateTimeHelper.ConvertToUserTime(recurringPayment.StartDateUtc, DateTimeKind.Utc).ToString();
            model.IsActive = recurringPayment.IsActive;
            model.NextPaymentDate = recurringPayment.NextPaymentDate.HasValue ? _dateTimeHelper.ConvertToUserTime(recurringPayment.NextPaymentDate.Value, DateTimeKind.Utc).ToString() : "";
            model.CyclesRemaining = recurringPayment.CyclesRemaining;
            model.InitialOrderId = recurringPayment.InitialOrder.Id;

            var customer = recurringPayment.InitialOrder.Customer;
            model.CustomerId = customer.Id;
            model.CustomerEmail = customer.IsGuest() ? T("Admin.Customers.Guest").Text : customer.Email;
            model.PaymentType = _paymentService.GetRecurringPaymentType(recurringPayment.InitialOrder.PaymentMethodSystemName).GetLocalizedEnum(_localizationService, _workContext);
            model.CanCancelRecurringPayment = _orderProcessingService.CanCancelRecurringPayment(_workContext.CurrentCustomer, recurringPayment);

            if (includeHistory)
            {
                foreach (var rph in recurringPayment.RecurringPaymentHistory.OrderBy(x => x.CreatedOnUtc))
                {
                    var rphModel = new RecurringPaymentModel.RecurringPaymentHistoryModel();
                    PrepareRecurringPaymentHistoryModel(rphModel, rph);
                    model.History.Add(rphModel);
                }
            }
        }

        private void PrepareRecurringPaymentHistoryModel(RecurringPaymentModel.RecurringPaymentHistoryModel model, RecurringPaymentHistory history)
        {
            Guard.NotNull(model, nameof(model));
            Guard.NotNull(history, nameof(history));

            var order = _orderService.GetOrderById(history.OrderId);

            model.Id = history.Id;
            model.OrderId = history.OrderId;
            model.RecurringPaymentId = history.RecurringPaymentId;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(history.CreatedOnUtc, DateTimeKind.Utc);
        }

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        [Permission(Permissions.Order.Read)]
        public ActionResult List()
        {
            var gridModel = new GridModel<RecurringPaymentModel>();
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult List(GridCommand command)
        {
            var gridModel = new GridModel<RecurringPaymentModel>();
            var payments = _orderService.SearchRecurringPayments(0, 0, 0, null, true);

            gridModel.Data = payments.PagedForCommand(command).Select(x =>
            {
                var m = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(m, x, false);
                return m;
            });

            gridModel.Total = payments.Count;

            return new JsonResult
            {
                Data = gridModel
            };
        }

        [Permission(Permissions.Order.Read)]
        public ActionResult Edit(int id)
        {
            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null || payment.Deleted)
            {
                return RedirectToAction("List");
            }

            var model = new RecurringPaymentModel();
            PrepareRecurringPaymentModel(model, payment, true);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.EditRecurringPayment)]
        public ActionResult Edit(RecurringPaymentModel model, bool continueEditing)
        {
            var payment = _orderService.GetRecurringPaymentById(model.Id);
            if (payment == null || payment.Deleted)
            {
                return RedirectToAction("List");
            }

            payment.CycleLength = model.CycleLength;
            payment.CyclePeriodId = model.CyclePeriodId;
            payment.TotalCycles = model.TotalCycles;
            payment.IsActive = model.IsActive;

            _orderService.UpdateRecurringPayment(payment);

            NotifySuccess(T("Admin.RecurringPayments.Updated"));
            return continueEditing ? RedirectToAction("Edit", payment.Id) : RedirectToAction("List");
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.EditRecurringPayment)]
        public ActionResult DeleteConfirmed(int id)
        {
            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null)
            {
                return RedirectToAction("List");
            }

            _orderService.DeleteRecurringPayment(payment);

            NotifySuccess(T("Admin.RecurringPayments.Deleted"));
            return RedirectToAction("List");
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        [Permission(Permissions.Order.Read)]
        public ActionResult HistoryList(int recurringPaymentId, GridCommand command)
        {
            var model = new GridModel<RecurringPaymentModel.RecurringPaymentHistoryModel>();
            var payment = _orderService.GetRecurringPaymentById(recurringPaymentId);

            var historyModel = payment.RecurringPaymentHistory.OrderBy(x => x.CreatedOnUtc)
                .Select(x =>
                {
                    var m = new RecurringPaymentModel.RecurringPaymentHistoryModel();
                    PrepareRecurringPaymentHistoryModel(m, x);
                    return m;
                })
                .ToList();

            model.Data = historyModel;
            model.Total = historyModel.Count;

            return new JsonResult
            {
                Data = model
            };
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("processnextpayment")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.EditRecurringPayment)]
        public ActionResult ProcessNextPayment(int id)
        {
            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                _orderProcessingService.ProcessNextRecurringPayment(payment);

                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment, true);

                NotifySuccess(T("Admin.RecurringPayments.NextPaymentProcessed"), false);
                return View(model);
            }
            catch (Exception ex)
            {
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment, true);
                NotifyError(ex, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelpayment")]
        [ValidateAntiForgeryToken]
        [Permission(Permissions.Order.EditRecurringPayment)]
        public ActionResult CancelRecurringPayment(int id)
        {
            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null)
            {
                return RedirectToAction("List");
            }

            try
            {
                var errors = _orderProcessingService.CancelRecurringPayment(payment);
                var model = new RecurringPaymentModel();

                PrepareRecurringPaymentModel(model, payment, true);

                if (errors.Count > 0)
                {
                    foreach (var error in errors)
                    {
                        NotifyError(error, false);
                    }
                }
                else
                {
                    NotifySuccess(T("Admin.RecurringPayments.Cancelled"), false);
                }

                return View(model);
            }
            catch (Exception ex)
            {
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment, true);
                NotifyError(ex, false);
                return View(model);
            }
        }
    }
}
