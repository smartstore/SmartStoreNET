using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.AmazonPay.Services;
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
            string message = null;
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
                // ConfirmOrderReference may also send a payment e-mail to the customer, which is irritating for him if the order has not been placed.
                var warnings = _orderProcessingService.GetOrderPlacementWarnings(processPaymentRequest);

                if (!warnings.Any())
                {
                    if (_orderProcessingService.IsMinimumOrderPlacementIntervalValid(customer, store))
                    {
                        success = _apiService.ConfirmOrderReference();
                        if (success)
                        {
                            var settings = Services.Settings.LoadSetting<AmazonPaySettings>(store.Id);
                            var state = _httpContext.GetAmazonPayState(Services.Localization);

                            state.FormData = formData.EmptyNull();
                            state.IsConfirmed = true;
                        }
                        else
                        {
                            redirectUrl = Url.Action("PaymentMethod", "Checkout", new { area = "" });

                            _httpContext.Session["AmazonPayFailedPaymentReason"] = "PaymentMethodNotAllowed";
                        }
                    }
                    else
                    {
                        message = T("Checkout.MinOrderPlacementInterval");
                    }
                }
                else
                {
                    message = string.Join(" ", warnings);
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Json(new { success, redirectUrl });
        }

        public ActionResult ConfirmationResult(string authenticationStatus)
        {
            $"ConfirmationResult {authenticationStatus.NaIfEmpty()}".Dump();

            var state = _httpContext.GetAmazonPayState(Services.Localization);
            state.SubmitForm = false;

            if (authenticationStatus.IsCaseInsensitiveEqual("Success"))
            {
                state.SubmitForm = true;
                return RedirectToAction("Confirm", "Checkout", new { area = "" });
            }
            else if (authenticationStatus.IsCaseInsensitiveEqual("Failure"))
            {
                // "The buyer has exhausted their retry attempts and payment instrument selection on the Amazon Pay page.
                // If this occurs, you should take the buyer back to a page (on your site) where they can choose a different payment method
                // and advise the buyer to checkout using a payment method that is not Amazon Pay or contact their bank."

                _httpContext.RemoveAmazonPayState();
                NotifyWarning(T("Plugins.Payments.AmazonPay.PaymentMethodExhaustedMessage"));

                return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
            }
            else
            {
                // authenticationStatus == "Abandoned":
                // "The buyer took action to close/cancel the MFA challenge. If this occurs, take the buyer back to the page where they 
                // can place the order and advise the buyer to retry placing their order using Amazon Pay and complete the MFA challenge presented."

                state.IsConfirmed = false;
                state.FormData = null;

                _httpContext.Session["AmazonPayFailedPaymentReason"] = "PaymentMethodAbandoned";

                return RedirectToAction("PaymentMethod", "Checkout", new { area = "" });
            }
        }

        #endregion
    }
}