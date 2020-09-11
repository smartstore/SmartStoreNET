using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Models;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Controllers;

namespace SmartStore.PayPal.Controllers
{
    public abstract class PayPalPaymentControllerBase : PaymentControllerBase
    {
        protected abstract string ProviderSystemName { get; }

        protected void PrepareConfigurationModel(ApiConfigurationModel model, int storeScope)
        {
            var store = storeScope == 0
                ? Services.StoreContext.CurrentStore
                : Services.StoreService.GetStoreById(storeScope);

            model.PrimaryStoreCurrencyCode = store.PrimaryStoreCurrency.CurrencyCode;
        }
    }


    public abstract class PayPalControllerBase<TSetting> : PayPalPaymentControllerBase where TSetting : PayPalSettingsBase, ISettings, new()
    {
        public PayPalControllerBase(
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService)
        {
            PaymentService = paymentService;
            OrderService = orderService;
            OrderProcessingService = orderProcessingService;
        }

        protected string SystemName { get; private set; }
        protected IPaymentService PaymentService { get; private set; }
        protected IOrderService OrderService { get; private set; }
        protected IOrderProcessingService OrderProcessingService { get; private set; }

        protected PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason, decimal payPalTotal, decimal orderTotal)
        {
            var result = PaymentStatus.Pending;

            if (paymentStatus == null)
                paymentStatus = string.Empty;

            if (pendingReason == null)
                pendingReason = string.Empty;

            switch (paymentStatus.ToLowerInvariant())
            {
                case "pending":
                    switch (pendingReason.ToLowerInvariant())
                    {
                        case "authorization":
                            result = PaymentStatus.Authorized;
                            break;
                        default:
                            result = PaymentStatus.Pending;
                            break;
                    }
                    break;
                case "processed":
                case "completed":
                case "canceled_reversal":
                    result = PaymentStatus.Paid;
                    break;
                case "denied":
                case "expired":
                case "failed":
                case "voided":
                    result = PaymentStatus.Voided;
                    break;
                case "reversed":
                    result = PaymentStatus.Refunded;
                    break;
                case "refunded":
                    if ((Math.Abs(orderTotal) - Math.Abs(payPalTotal)) > decimal.Zero)
                        result = PaymentStatus.PartiallyRefunded;
                    else
                        result = PaymentStatus.Refunded;
                    break;
                default:
                    break;
            }
            return result;
        }

        protected bool VerifyIPN(PayPalSettingsBase settings, string formString, out Dictionary<string, string> values)
        {
            // Settings: multistore context not possible here. we need the custom value to determine what store it is.
            var request = (HttpWebRequest)WebRequest.Create(settings.GetPayPalUrl());
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = Request.UserAgent;

            var formContent = string.Format("{0}&cmd=_notify-validate", formString);
            request.ContentLength = formContent.Length;

            using (var sw = new StreamWriter(request.GetRequestStream(), Encoding.ASCII))
            {
                sw.Write(formContent);
            }

            string response = null;
            using (var sr = new StreamReader(request.GetResponse().GetResponseStream()))
            {
                response = HttpUtility.UrlDecode(sr.ReadToEnd());
            }

            var success = response.Trim().Equals("VERIFIED", StringComparison.OrdinalIgnoreCase);

            values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in formString.SplitSafe("&"))
            {
                var line = HttpUtility.UrlDecode(item).TrimSafe();
                var equalIndex = line.IndexOf('=');

                if (equalIndex >= 0)
                    values.Add(line.Substring(0, equalIndex), line.Substring(equalIndex + 1));
            }

            return success;
        }

        [ValidateInput(false)]
        public ActionResult IPNHandler()
        {
            byte[] param = Request.BinaryRead(Request.ContentLength);
            var strRequest = Encoding.UTF8.GetString(param);

            if (!PaymentService.IsPaymentMethodActive(ProviderSystemName, Services.StoreContext.CurrentStore.Id))
            {
                Logger.Warn(new SmartException(strRequest), T("Plugins.Payments.PayPal.NoModuleLoading", "IPNHandler"));
                return Content(string.Empty);
            }

            var sb = new StringBuilder();
            Dictionary<string, string> values;
            var settings = Services.Settings.LoadSetting<TSetting>();

            if (VerifyIPN(settings, strRequest, out values))
            {
                #region values

                decimal total = decimal.Zero;
                try
                {
                    total = decimal.Parse(values["mc_gross"], new CultureInfo("en-US"));
                }
                catch { }

                string payer_status = string.Empty;
                values.TryGetValue("payer_status", out payer_status);
                string payment_status = string.Empty;
                values.TryGetValue("payment_status", out payment_status);
                string pending_reason = string.Empty;
                values.TryGetValue("pending_reason", out pending_reason);
                string mc_currency = string.Empty;
                values.TryGetValue("mc_currency", out mc_currency);
                string txn_id = string.Empty;
                values.TryGetValue("txn_id", out txn_id);
                string txn_type = string.Empty;
                values.TryGetValue("txn_type", out txn_type);
                string rp_invoice_id = string.Empty;
                values.TryGetValue("rp_invoice_id", out rp_invoice_id);
                string payment_type = string.Empty;
                values.TryGetValue("payment_type", out payment_type);
                string payer_id = string.Empty;
                values.TryGetValue("payer_id", out payer_id);
                string receiver_id = string.Empty;
                values.TryGetValue("receiver_id", out receiver_id);
                string invoice = string.Empty;
                values.TryGetValue("invoice", out invoice);
                string payment_fee = string.Empty;
                values.TryGetValue("payment_fee", out payment_fee);

                #endregion

                sb.AppendLine("PayPal IPN:");
                foreach (KeyValuePair<string, string> kvp in values.Where(x => x.Value.HasValue()))
                {
                    sb.AppendLine(kvp.Key + ": " + kvp.Value);
                }

                switch (txn_type)
                {
                    case "recurring_payment_profile_created":
                        //do nothing here
                        break;
                    case "recurring_payment":
                        #region Recurring payment
                        {
                            Guid orderNumberGuid = Guid.Empty;
                            try
                            {
                                orderNumberGuid = new Guid(rp_invoice_id);
                            }
                            catch { }

                            var initialOrder = OrderService.GetOrderByGuid(orderNumberGuid);
                            if (initialOrder != null)
                            {
                                var newPaymentStatus = GetPaymentStatus(payment_status, pending_reason, total, initialOrder.OrderTotal);
                                var recurringPayments = OrderService.SearchRecurringPayments(0, 0, initialOrder.Id, null);

                                foreach (var rp in recurringPayments)
                                {
                                    switch (newPaymentStatus)
                                    {
                                        case PaymentStatus.Authorized:
                                        case PaymentStatus.Paid:
                                            {
                                                var recurringPaymentHistory = rp.RecurringPaymentHistory;
                                                if (recurringPaymentHistory.Count == 0)
                                                {
                                                    //first payment
                                                    var rph = new RecurringPaymentHistory
                                                    {
                                                        RecurringPaymentId = rp.Id,
                                                        OrderId = initialOrder.Id,
                                                        CreatedOnUtc = DateTime.UtcNow
                                                    };
                                                    rp.RecurringPaymentHistory.Add(rph);
                                                    OrderService.UpdateRecurringPayment(rp);
                                                }
                                                else
                                                {
                                                    //next payments
                                                    OrderProcessingService.ProcessNextRecurringPayment(rp);
                                                }
                                            }
                                            break;
                                    }
                                }

                                Logger.Info(new SmartException(sb.ToString()), T("Plugins.Payments.PayPal.IpnRecurringPaymentInfo"));
                            }
                            else
                            {
                                if (rp_invoice_id.IsEmpty())
                                    Logger.Warn(new SmartException(sb.ToString()), T("Plugins.Payments.PayPal.IpnIrregular", "rp_invoice_id"));
                                else
                                    Logger.Error(new SmartException(sb.ToString()), T("Plugins.Payments.PayPal.IpnOrderNotFound"));
                            }
                        }
                        #endregion
                        break;
                    default:
                        #region Standard payment
                        {
                            var orderNumber = "";
                            var orderNumberGuid = Guid.Empty;
                            if (!values.TryGetValue("custom", out orderNumber) || orderNumber.IsEmpty())
                            {
                                return Content(string.Empty);
                            }

                            try
                            {
                                orderNumberGuid = new Guid(orderNumber);
                            }
                            catch { }

                            var order = OrderService.GetOrderByGuid(orderNumberGuid);
                            if (order != null)
                            {
                                order.HasNewPaymentNotification = true;

                                OrderService.AddOrderNote(order, sb.ToString());

                                if (settings.IpnChangesPaymentStatus)
                                {
                                    var newPaymentStatus = GetPaymentStatus(payment_status, pending_reason, total, order.OrderTotal);

                                    switch (newPaymentStatus)
                                    {
                                        case PaymentStatus.Pending:
                                            break;
                                        case PaymentStatus.Authorized:
                                            if (OrderProcessingService.CanMarkOrderAsAuthorized(order))
                                            {
                                                OrderProcessingService.MarkAsAuthorized(order);
                                            }
                                            break;
                                        case PaymentStatus.Paid:
                                            if (OrderProcessingService.CanMarkOrderAsPaid(order))
                                            {
                                                OrderProcessingService.MarkOrderAsPaid(order);
                                            }
                                            break;
                                        case PaymentStatus.Refunded:
                                            if (OrderProcessingService.CanRefundOffline(order))
                                            {
                                                OrderProcessingService.RefundOffline(order);
                                            }
                                            break;
                                        case PaymentStatus.PartiallyRefunded:
                                            // We could only process it once cause otherwise order.RefundedAmount would getting wrong.
                                            if (order.RefundedAmount == decimal.Zero && OrderProcessingService.CanPartiallyRefundOffline(order, Math.Abs(total)))
                                            {
                                                OrderProcessingService.PartiallyRefundOffline(order, Math.Abs(total));
                                            }
                                            break;
                                        case PaymentStatus.Voided:
                                            if (OrderProcessingService.CanVoidOffline(order))
                                            {
                                                OrderProcessingService.VoidOffline(order);
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                }
                            }
                            else
                            {
                                Logger.Error(new SmartException(sb.ToString()), T("Plugins.Payments.PayPal.IpnOrderNotFound"));
                            }
                        }
                        #endregion
                        break;
                }
            }
            else
            {
                Logger.Error(new SmartException(strRequest), T("Plugins.Payments.PayPal.IpnFailed"));
            }

            //nothing should be rendered to visitor
            return Content("");
        }
    }
}