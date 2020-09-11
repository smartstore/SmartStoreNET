using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.AmazonPay.Services;
using SmartStore.Core.Html;
using SmartStore.Core.Logging;
using SmartStore.Services.Common;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.AmazonPay.Controllers
{
    public class AmazonPayCheckoutController : AmazonPayControllerBase
    {
        private readonly HttpContextBase _httpContext;
        private readonly IAmazonPayService _apiService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IOrderProcessingService _orderProcessingService;

        public AmazonPayCheckoutController(
            HttpContextBase httpContext,
            IAmazonPayService apiService,
            IGenericAttributeService genericAttributeService,
            IOrderProcessingService orderProcessingService)
        {
            _httpContext = httpContext;
            _apiService = apiService;
            _genericAttributeService = genericAttributeService;
            _orderProcessingService = orderProcessingService;
        }

        public ActionResult OrderReferenceCreated(string orderReferenceId/*, string accessToken*/)
        {
            var success = false;
            var error = string.Empty;

            try
            {
                var state = _httpContext.GetAmazonPayState(Services.Localization);
                state.OrderReferenceId = orderReferenceId;

                //if (accessToken.HasValue())
                //{
                //	state.AccessToken = accessToken;
                //}

                if (state.OrderReferenceId.IsEmpty())
                {
                    success = false;
                    error = T("Plugins.Payments.AmazonPay.MissingOrderReferenceId");
                }

                if (state.AccessToken.IsEmpty())
                {
                    success = false;
                    error = error.Grow(T("Plugins.Payments.AmazonPay.MissingAddressConsentToken"), " ");
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return new JsonResult { Data = new { success = success, error = error } };
        }

        public ActionResult BillingAddress()
        {
            return RedirectToAction("ShippingAddress", "Checkout", new { area = "" });
        }

        public ActionResult ShippingAddress()
        {
            var model = _apiService.CreateViewModel(AmazonPayRequestType.Address, TempData);

            return GetActionResult(model);
        }

        public ActionResult PaymentMethod()
        {
            var model = _apiService.CreateViewModel(AmazonPayRequestType.PaymentMethod, TempData);

            return GetActionResult(model);
        }

        [HttpPost]
        public ActionResult PaymentMethod(FormCollection form)
        {
            // Display biling address on confirm page.
            _apiService.GetBillingAddress();

            var customer = Services.WorkContext.CurrentCustomer;
            if (customer.BillingAddress == null)
            {
                NotifyError(T("Plugins.Payments.AmazonPay.MissingBillingAddress"));
                return RedirectToAction("Cart", "ShoppingCart", new { area = "" });
            }

            return RedirectToAction("Confirm", "Checkout", new { area = "" });
        }

        public ActionResult PaymentInfo()
        {
            return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
        }

        public ActionResult CheckoutCompleted()
        {
            var note = _httpContext.Session["AmazonPayCheckoutCompletedNote"] as string;
            if (note.HasValue())
            {
                return Content(note);
            }

            return new EmptyResult();
        }

        #region Confirmation flow

        // Ajax.
        public ActionResult ConfirmOrder(string formData)
        {
            string redirectUrl = null;
            var messages = new List<string>();
            var success = false;

            try
            {
                var store = Services.StoreContext.CurrentStore;
                var customer = Services.WorkContext.CurrentCustomer;
                var processPaymentRequest = (_httpContext.Session["OrderPaymentInfo"] as ProcessPaymentRequest) ?? new ProcessPaymentRequest();

                processPaymentRequest.StoreId = store.Id;
                processPaymentRequest.CustomerId = customer.Id;
                processPaymentRequest.PaymentMethodSystemName = AmazonPayPlugin.SystemName;

                // We must check here if an order can be placed to avoid creating unauthorized Amazon payment objects.
                var warnings = _orderProcessingService.GetOrderPlacementWarnings(processPaymentRequest);

                if (!warnings.Any())
                {
                    if (_orderProcessingService.IsMinimumOrderPlacementIntervalValid(customer, store))
                    {
                        if (_apiService.ConfirmOrderReference())
                        {
                            success = true;

                            var state = _httpContext.GetAmazonPayState(Services.Localization);
                            state.FormData = formData.EmptyNull();
                        }
                        else
                        {
                            _httpContext.Session["AmazonPayFailedPaymentReason"] = "PaymentMethodNotAllowed";

                            redirectUrl = Url.Action("PaymentMethod", "Checkout", new { area = "" });
                        }
                    }
                    else
                    {
                        messages.Add(T("Checkout.MinOrderPlacementInterval"));
                    }
                }
                else
                {
                    messages.AddRange(warnings.Select(x => HtmlUtils.ConvertPlainTextToHtml(x)));
                }
            }
            catch (Exception ex)
            {
                messages.Add(ex.Message);
                Logger.Error(ex);
            }

            return Json(new { success, redirectUrl, messages });
        }

        public ActionResult ConfirmationResult(string authenticationStatus)
        {
            var state = _httpContext.GetAmazonPayState(Services.Localization);
            state.SubmitForm = false;

            if (authenticationStatus.IsCaseInsensitiveEqual("Success"))
            {
                state.SubmitForm = true;
                return RedirectToAction("Confirm", "Checkout", new { area = "" });
            }
            else if (authenticationStatus.IsCaseInsensitiveEqual("Failure"))
            {
                // The buyer has exhausted their retry attempts and payment instrument selection on the Amazon Pay page.
                // Review: redirect back to shopping cart (like AmazonRejected).
                _httpContext.Session["AmazonPayFailedPaymentReason"] = "AuthenticationStatusFailure";

                return RedirectToRoute("ShoppingCart");
            }
            else
            {
                // authenticationStatus == "Abandoned": The buyer took action to close/cancel the MFA challenge.
                // Review: redirect to checkout payment page (like InvalidPaymentMethod).
                state.IsConfirmed = false;
                state.FormData = null;

                _httpContext.Session["AmazonPayFailedPaymentReason"] = "AuthenticationStatusAbandoned";

                return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
            }
        }

        #endregion
    }
}