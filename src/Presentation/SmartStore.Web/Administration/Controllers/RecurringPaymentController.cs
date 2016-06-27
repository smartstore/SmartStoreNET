using System;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Admin.Models.Orders;
using SmartStore.Core;
using SmartStore.Core.Domain.Orders;
using SmartStore.Services.Customers;
using SmartStore.Services.Helpers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Services.Security;
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
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IPaymentService _paymentService;
        private readonly IPermissionService _permissionService;

        #endregion Fields

        #region Constructors

        public RecurringPaymentController(IOrderService orderService,
            IOrderProcessingService orderProcessingService, ILocalizationService localizationService,
            IWorkContext workContext, IDateTimeHelper dateTimeHelper, IPaymentService paymentService,
            IPermissionService permissionService)
        {
            this._orderService = orderService;
            this._orderProcessingService = orderProcessingService;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._dateTimeHelper = dateTimeHelper;
            this._paymentService = paymentService;
            this._permissionService = permissionService;
        }

        #endregion

        #region Utilities

        [NonAction]
        private void PrepareRecurringPaymentModel(RecurringPaymentModel model, 
            RecurringPayment recurringPayment, bool includeHistory)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (recurringPayment == null)
                throw new ArgumentNullException("recurringPayment");
            
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
            model.CustomerEmail = customer.IsGuest() ? _localizationService.GetResource("Admin.Customers.Guest") : customer.Email;
            model.PaymentType = _paymentService.GetRecurringPaymentType(recurringPayment.InitialOrder.PaymentMethodSystemName).GetLocalizedEnum(_localizationService, _workContext);
            model.CanCancelRecurringPayment = _orderProcessingService.CanCancelRecurringPayment(_workContext.CurrentCustomer, recurringPayment);
                    
            if (includeHistory)
                foreach (var rph in recurringPayment.RecurringPaymentHistory.OrderBy(x => x.CreatedOnUtc))
                {
                    var rphModel = new RecurringPaymentModel.RecurringPaymentHistoryModel();
                    PrepareRecurringPaymentHistoryModel(rphModel, rph);
                    model.History.Add(rphModel);
                }
        }

        [NonAction]
        private void PrepareRecurringPaymentHistoryModel(RecurringPaymentModel.RecurringPaymentHistoryModel model,
            RecurringPaymentHistory history)
        {
            if (model == null)
                throw new ArgumentNullException("model");

            if (history == null)
                throw new ArgumentNullException("history");

            var order = _orderService.GetOrderById(history.OrderId);

            model.Id = history.Id;
            model.OrderId = history.OrderId;
            model.RecurringPaymentId = history.RecurringPaymentId;
            model.OrderStatus = order.OrderStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.PaymentStatus = order.PaymentStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.ShippingStatus = order.ShippingStatus.GetLocalizedEnum(_localizationService, _workContext);
            model.CreatedOn = _dateTimeHelper.ConvertToUserTime(history.CreatedOnUtc, DateTimeKind.Utc);
        }

        #endregion

        #region Recurring payment

        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var gridModel = new GridModel<RecurringPaymentModel>();
            return View(gridModel);
        }

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult List(GridCommand command)
        {
			var gridModel = new GridModel<RecurringPaymentModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
			{
				var payments = _orderService.SearchRecurringPayments(0, 0, 0, null, true);

				gridModel.Data = payments.PagedForCommand(command).Select(x =>
				{
					var m = new RecurringPaymentModel();
					PrepareRecurringPaymentModel(m, x, false);
					return m;
				});

				gridModel.Total = payments.Count;
			}
			else
			{
				gridModel.Data = Enumerable.Empty<RecurringPaymentModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = gridModel
            };
        }

        public ActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null || payment.Deleted)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            var model = new RecurringPaymentModel();
            PrepareRecurringPaymentModel(model, payment, true);
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public ActionResult Edit(RecurringPaymentModel model, bool continueEditing)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(model.Id);
            if (payment == null || payment.Deleted)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            payment.CycleLength = model.CycleLength;
            payment.CyclePeriodId = model.CyclePeriodId;
            payment.TotalCycles = model.TotalCycles;
            payment.IsActive = model.IsActive;
            _orderService.UpdateRecurringPayment(payment);

            NotifySuccess(_localizationService.GetResource("Admin.RecurringPayments.Updated"));
            return continueEditing ? RedirectToAction("Edit", payment.Id) : RedirectToAction("List");
        }

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            _orderService.DeleteRecurringPayment(payment);

            NotifySuccess(_localizationService.GetResource("Admin.RecurringPayments.Deleted"));
            return RedirectToAction("List");
        }

        #endregion

        #region History

        [HttpPost, GridAction(EnableCustomBinding = true)]
        public ActionResult HistoryList(int recurringPaymentId, GridCommand command)
        {
			var model = new GridModel<RecurringPaymentModel.RecurringPaymentHistoryModel>();

			if (_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
			{
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
			}
			else
			{
				model.Data = Enumerable.Empty<RecurringPaymentModel.RecurringPaymentHistoryModel>();

				NotifyAccessDenied();
			}

            return new JsonResult
            {
                Data = model
            };
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("processnextpayment")]
        public ActionResult ProcessNextPayment(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            try
            {
                _orderProcessingService.ProcessNextRecurringPayment(payment);
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment, true);

                NotifySuccess(_localizationService.GetResource("Admin.RecurringPayments.NextPaymentProcessed"), false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment, true);
                NotifyError(exc, false);
                return View(model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelpayment")]
        public ActionResult CancelRecurringPayment(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var payment = _orderService.GetRecurringPaymentById(id);
            if (payment == null)
                //No recurring payment found with the specified id
                return RedirectToAction("List");

            try
            {
                var errors = _orderProcessingService.CancelRecurringPayment(payment);
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment, true);
                if (errors.Count > 0)
                {
                    foreach (var error in errors)
						NotifyError(error, false);
                }
                else
                    NotifySuccess(_localizationService.GetResource("Admin.RecurringPayments.Cancelled"), false);
                return View(model);
            }
            catch (Exception exc)
            {
                //error
                var model = new RecurringPaymentModel();
                PrepareRecurringPaymentModel(model, payment, true);
                NotifyError(exc, false);
                return View(model);
            }
        }

        #endregion
    }
}
