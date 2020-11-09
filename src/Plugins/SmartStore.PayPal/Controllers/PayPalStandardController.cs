using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.PayPal.Controllers
{
    public class PayPalStandardController : PayPalControllerBase<PayPalStandardPaymentSettings>
    {
        public PayPalStandardController(
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService) : base(
                paymentService,
                orderService,
                orderProcessingService)
        {
        }

        protected override string ProviderSystemName => PayPalStandardProvider.SystemName;

        [AdminAuthorize, ChildActionOnly, LoadSetting]
        public ActionResult Configure(PayPalStandardPaymentSettings settings, int storeScope)
        {
            var model = new PayPalStandardConfigurationModel();
            model.Copy(settings, true);

            PrepareConfigurationModel(model, storeScope);

            return View(model);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(PayPalStandardConfigurationModel model, FormCollection form)
        {
            var storeDependingSettingHelper = new StoreDependingSettingHelper(ViewData);
            var storeScope = this.GetActiveStoreScopeConfiguration(Services.StoreService, Services.WorkContext);
            var settings = Services.Settings.LoadSetting<PayPalStandardPaymentSettings>(storeScope);

            if (!ModelState.IsValid)
            {
                return Configure(settings, storeScope);
            }

            ModelState.Clear();
            model.Copy(settings, false);

            using (Services.Settings.BeginScope())
            {
                storeDependingSettingHelper.UpdateSettings(settings, form, storeScope, Services.Settings);
            }

            using (Services.Settings.BeginScope())
            {
                // Multistore context not possible, see IPN handling.
                Services.Settings.SaveSetting(settings, x => x.UseSandbox, 0, false);
            }

            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

            return RedirectToConfiguration(PayPalStandardProvider.SystemName, false);
        }

        public ActionResult PaymentInfo()
        {
            return PartialView();
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            return paymentInfo;
        }

        [ValidateInput(false)]
        public ActionResult PDTHandler(FormCollection form)
        {
            Dictionary<string, string> values;
            var tx = Services.WebHelper.QueryString<string>("tx");
            var utcNow = DateTime.UtcNow;
            var orderNumberGuid = Guid.Empty;
            var orderNumber = string.Empty;
            var total = decimal.Zero;
            string response;

            var provider = PaymentService.LoadPaymentMethodBySystemName(PayPalStandardProvider.SystemName, true);
            var processor = provider != null ? provider.Value as PayPalStandardProvider : null;
            if (processor == null)
            {
                Logger.Warn(null, T("Plugins.Payments.PayPal.NoModuleLoading", "PDTHandler"));
                return RedirectToAction("Completed", "Checkout", new { area = "" });
            }

            var settings = Services.Settings.LoadSetting<PayPalStandardPaymentSettings>();

            if (processor.GetPDTDetails(tx, settings, out values, out response))
            {
                values.TryGetValue("custom", out orderNumber);

                try
                {
                    orderNumberGuid = new Guid(orderNumber);
                }
                catch { }

                var order = OrderService.GetOrderByGuid(orderNumberGuid);

                if (order != null)
                {
                    try
                    {
                        total = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, T("Plugins.Payments.PayPalStandard.FailedGetGross"));
                    }

                    values.TryGetValue("payer_status", out string payer_status);
                    values.TryGetValue("payment_status", out string payment_status);
                    values.TryGetValue("pending_reason", out string pending_reason);
                    values.TryGetValue("mc_currency", out string mc_currency);
                    values.TryGetValue("txn_id", out string txn_id);
                    values.TryGetValue("payment_type", out string payment_type);
                    values.TryGetValue("payer_id", out string payer_id);
                    values.TryGetValue("receiver_id", out string receiver_id);
                    values.TryGetValue("invoice", out string invoice);
                    values.TryGetValue("payment_fee", out string payment_fee);

                    var paymentNote = T("Plugins.Payments.PayPalStandard.PaymentNote",
                        total, mc_currency, payer_status, payment_status, pending_reason, txn_id, payment_type, payer_id, receiver_id, invoice, payment_fee);

                    OrderService.AddOrderNote(order, paymentNote);

                    // validate order total... you may get differences if settings.PassProductNamesAndTotals is true
                    if (settings.PdtValidateOrderTotal)
                    {
                        var roundedTotal = Math.Round(total, 2);
                        var roundedOrderTotal = Math.Round(order.OrderTotal, 2);
                        var roundedDifference = Math.Abs(roundedTotal - roundedOrderTotal);

                        if (!roundedTotal.Equals(roundedOrderTotal))
                        {
                            var message = T("Plugins.Payments.PayPalStandard.UnequalTotalOrder",
                                total, roundedOrderTotal.FormatInvariant(), order.OrderTotal, roundedDifference.FormatInvariant());

                            if (settings.PdtValidateOnlyWarn)
                            {
                                OrderService.AddOrderNote(order, message);
                            }
                            else
                            {
                                Logger.Error(message);

                                return RedirectToAction("Index", "Home", new { area = "" });
                            }
                        }
                    }

                    // mark order as paid
                    var newPaymentStatus = GetPaymentStatus(payment_status, pending_reason, total, order.OrderTotal);

                    if (newPaymentStatus == PaymentStatus.Paid)
                    {
                        // note, order can be marked as paid through IPN
                        if (order.AuthorizationTransactionId.IsEmpty())
                        {
                            order.AuthorizationTransactionId = order.CaptureTransactionId = txn_id;
                            order.AuthorizationTransactionResult = order.CaptureTransactionResult = "Success";

                            OrderService.UpdateOrder(order);
                        }

                        if (OrderProcessingService.CanMarkOrderAsPaid(order))
                        {
                            OrderProcessingService.MarkOrderAsPaid(order);
                        }
                    }
                }

                return RedirectToAction("Completed", "Checkout", new { area = "" });
            }
            else
            {
                try
                {
                    values.TryGetValue("custom", out orderNumber);
                    orderNumberGuid = new Guid(orderNumber);

                    var order = OrderService.GetOrderByGuid(orderNumberGuid);
                    OrderService.AddOrderNote(order, "{0} {1}".FormatInvariant(T("Plugins.Payments.PayPalStandard.PdtFailed"), response));
                }
                catch { }

                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        public ActionResult CancelOrder(FormCollection form)
        {
            var order = OrderService.SearchOrders(Services.StoreContext.CurrentStore.Id, Services.WorkContext.CurrentCustomer.Id, null, null, null, null, null, null, null, null, 0, 1)
                .FirstOrDefault();

            if (order != null)
            {
                return RedirectToAction("Details", "Order", new { id = order.Id, area = "" });
            }

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}