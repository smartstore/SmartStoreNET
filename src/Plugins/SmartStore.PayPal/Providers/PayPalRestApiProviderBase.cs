using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal
{
	public abstract class PayPalRestApiProviderBase<TSetting> : PaymentMethodBase, IConfigurable where TSetting : PayPalApiSettingsBase, ISettings, new()
    {
        protected PayPalRestApiProviderBase()
		{
			Logger = NullLogger.Instance;
		}

		public ILogger Logger { get; set; }
		public HttpContextBase HttpContext { get; set; }
		public ICommonServices Services { get; set; }
		public IOrderService OrderService { get; set; }
        public IOrderTotalCalculationService OrderTotalCalculationService { get; set; }
		public IPayPalService PayPalService { get; set; }

		protected string GetControllerName()
		{
			return GetControllerType().Name.EmptyNull().Replace("Controller", "");
		}

		public static string CheckoutCompletedKey
		{
			get { return "PayPalCheckoutCompleted"; }
		}

		public override bool SupportCapture
		{
			get { return true; }
		}

		public override bool SupportPartiallyRefund
		{
			get { return true; }
		}

		public override bool SupportRefund
		{
			get { return true; }
		}

		public override bool SupportVoid
		{
			get { return true; }
		}

		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
			var result = decimal.Zero;
			try
			{
				var settings = Services.Settings.LoadSetting<TSetting>();

				result = this.CalculateAdditionalFee(OrderTotalCalculationService, cart, settings.AdditionalFee, settings.AdditionalFeePercentage);
			}
			catch (Exception)
			{
			}
			return result;
        }

		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = new ProcessPaymentResult
			{
				NewPaymentStatus = PaymentStatus.Pending
			};

			HttpContext.Session.SafeRemove(CheckoutCompletedKey);

			var settings = Services.Settings.LoadSetting<TSetting>(processPaymentRequest.StoreId);
			var session = HttpContext.GetPayPalSessionData();

			if (session.AccessToken.IsEmpty() || session.PaymentId.IsEmpty())
			{
				session.SessionExpired = true;

				// Do not place order because we cannot execute the payment.
				result.AddError(T("Plugins.SmartStore.PayPal.SessionExpired"));

				// Redirect to payment wall and create new payment (we need the payment id).
				var response = HttpContext.Response;
				var urlHelper = new UrlHelper(HttpContext.Request.RequestContext);
				var isSecure = Services.WebHelper.IsCurrentConnectionSecured();

				response.Status = "302 Found";
				response.RedirectLocation = urlHelper.Action("PaymentMethod", "Checkout", new { area = "" }, isSecure ? "https" : "http");
				response.End();

				return result;
			}

			processPaymentRequest.OrderGuid = session.OrderGuid;

			var apiResult = PayPalService.ExecutePayment(settings, session);

			if (apiResult.Success && apiResult.Json != null)
			{
				var state = (string)apiResult.Json.state;
				string reasonCode = null;
				dynamic relatedObject = null;

				if (!state.IsCaseInsensitiveEqual("failed"))
				{
					// the payment id is required to find the order during webhook message processing
					result.AuthorizationTransactionCode = apiResult.Id;

					// intent: "sale" for immediate payment, "authorize" for pre-authorized payments and "order" for an order.
					// info required cause API has different endpoints for different intents.
					var intent = (string)apiResult.Json.intent;

					if (intent.IsCaseInsensitiveEqual("sale"))
					{
						relatedObject = apiResult.Json.transactions[0].related_resources[0].sale;

						session.PaymentInstruction = PayPalService.ParsePaymentInstruction(apiResult.Json.payment_instruction) as PayPalPaymentInstruction;

						// Test session data:
						//session.PaymentInstruction = new PayPalPaymentInstruction
						//{
						//	ReferenceNumber = "123456789",
						//	Type = "PAY_UPON_INVOICE",
						//	Amount = 9.99M,
						//	AmountCurrencyCode = "EUR",
						//	Note = "This is a test instruction!",
						//	RecipientBanking = new PayPalPaymentInstruction.RecipientBankingInstruction
						//	{
						//		BankName = "John Pierpont Morgan & Company",
						//		AccountHolderName = "Max Mustermann",
						//		AccountNumber = "987654321",
						//		Iban = "DE654321987654321",
						//		Bic = "DUDEXX321654"
						//	}
						//};
					}
					else
					{
						relatedObject = apiResult.Json.transactions[0].related_resources[0].authorization;
					}

					if (relatedObject != null)
					{
						state = (string)relatedObject.state;
						reasonCode = (string)relatedObject.reason_code;

						// see PayPalService.Refund()
						result.AuthorizationTransactionResult = "{0} ({1})".FormatInvariant(state.NaIfEmpty(), intent.NaIfEmpty());
						result.AuthorizationTransactionId = (string)relatedObject.id;

						result.NewPaymentStatus = PayPalService.GetPaymentStatus(state, reasonCode, PaymentStatus.Authorized);

						if (result.NewPaymentStatus == PaymentStatus.Paid)
						{
							result.CaptureTransactionResult = result.AuthorizationTransactionResult;
							result.CaptureTransactionId = result.AuthorizationTransactionId;
						}
					}
				}
				else
				{
					var failureReason = (string)apiResult.Json.failure_reason;

					result.Errors.Add(T("Plugins.SmartStore.PayPal.PaymentExecuteFailed").Text.Grow(failureReason, " "));
				}
			}

			if (!apiResult.Success)
			{
				result.Errors.Add(apiResult.ErrorMessage);
			}

			return result;
		}

		public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
		{
			var instruction = PayPalService.CreatePaymentInstruction(HttpContext.GetPayPalSessionData().PaymentInstruction);

			if (instruction.HasValue())
			{
				HttpContext.Session[CheckoutCompletedKey] = instruction;

				OrderService.AddOrderNote(postProcessPaymentRequest.Order, instruction, true);
			}
		}

		public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
			var result = new CapturePaymentResult
			{
				NewPaymentStatus = capturePaymentRequest.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(capturePaymentRequest.Order.StoreId);
			var session = new PayPalSessionData();

			var apiResult = PayPalService.EnsureAccessToken(session, settings);
			if (apiResult.Success)
			{
				apiResult = PayPalService.Capture(settings, session, capturePaymentRequest);

				if (apiResult.Success)
					result.NewPaymentStatus = PaymentStatus.Paid;
			}

			if (!apiResult.Success)
				result.Errors.Add(apiResult.ErrorMessage);

			return result;
        }

        public override RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
			var result = new RefundPaymentResult
			{
				NewPaymentStatus = refundPaymentRequest.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(refundPaymentRequest.Order.StoreId);
			var session = new PayPalSessionData();

			var apiResult = PayPalService.EnsureAccessToken(session, settings);
			if (apiResult.Success)
			{
				apiResult = PayPalService.Refund(settings, session, refundPaymentRequest);
				if (apiResult.Success)
				{
					if (refundPaymentRequest.IsPartialRefund)
						result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
					else
						result.NewPaymentStatus = PaymentStatus.Refunded;
				}
			}

			if (!apiResult.Success)
				result.Errors.Add(apiResult.ErrorMessage);

			return result;
        }

        public override VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
			var result = new VoidPaymentResult
			{
				NewPaymentStatus = voidPaymentRequest.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(voidPaymentRequest.Order.StoreId);
			var session = new PayPalSessionData();

			var apiResult = PayPalService.EnsureAccessToken(session, settings);
			if (apiResult.Success)
			{
				apiResult = PayPalService.Void(settings, session, voidPaymentRequest);
				if (apiResult.Success)
				{
					result.NewPaymentStatus = PaymentStatus.Voided;
				}
			}

			if (!apiResult.Success)
				result.Errors.Add(apiResult.ErrorMessage);

			return result;
        }

        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
			actionName = "Configure";
            controllerName = GetControllerName();
            routeValues = new RouteValueDictionary { { "area", "SmartStore.PayPal" } };
        }

        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = GetControllerName();
            routeValues = new RouteValueDictionary { { "area", "SmartStore.PayPal" } };
        }
    }
}

