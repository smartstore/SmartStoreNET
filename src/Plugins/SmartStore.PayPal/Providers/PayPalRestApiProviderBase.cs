using System;
using System.Collections.Generic;
using System.Web;
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

			processPaymentRequest.OrderGuid = session.OrderGuid;

			var apiResult = PayPalService.ExecutePayment(settings, session);

			if (apiResult.Success && apiResult.Json != null)
			{
				var state = (string)apiResult.Json.state;

				if (!state.IsCaseInsensitiveEqual("failed"))
				{
					// intent: "sale" for immediate payment, "authorize" for pre-authorized payments and "order" for an order.
					// info required cause API has different endpoints for different intents.
					result.AuthorizationTransactionCode = (string)apiResult.Json.intent;

					var sale = apiResult.Json.transactions[0].related_resources[0].sale;

					if (sale != null)
					{
						state = (string)sale.state;

						result.AuthorizationTransactionResult = state;
						result.AuthorizationTransactionId = (string)sale.id;

						result.NewPaymentStatus = PaymentStatus.Authorized;

						if (state.IsCaseInsensitiveEqual("completed") || state.IsCaseInsensitiveEqual("processed"))
						{
							result.CaptureTransactionResult = state;
							result.CaptureTransactionId = (string)sale.id;

							result.NewPaymentStatus = PaymentStatus.Paid;
						}
						else if (state.IsCaseInsensitiveEqual("pending"))
						{
							var reasonCode = (string)sale.reason_code;
							if (reasonCode.IsCaseInsensitiveEqual("ECHECK"))
							{
								result.NewPaymentStatus = PaymentStatus.Pending;
							}
						}

						session.PaymentInstruction = PayPalService.ParsePaymentInstruction(apiResult.Json.payment_instruction) as PayPalPaymentInstruction;
					}
				}
				else
				{
					var failureReason = (string)apiResult.Json.failure_reason;

					result.Errors.Add(T("Plugins.SmartStore.PayPal.PaymentExecuteFailed").Text.Grow(failureReason, " "));
				}
			}
			else
			{
				result.Errors.Add(apiResult.ErrorMessage);
			}

			return result;
		}

		public override void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
		{
			if (postProcessPaymentRequest.Order.PaymentStatus == PaymentStatus.Paid)
				return;

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
			var currencyCode = Services.WorkContext.WorkingCurrency.CurrencyCode ?? "EUR";

			var authorizationId = capturePaymentRequest.Order.AuthorizationTransactionId;		
			
			// TODO	

            return result;
        }

        public override RefundPaymentResult Refund(RefundPaymentRequest request)
        {
			var result = new RefundPaymentResult
			{
				NewPaymentStatus = request.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(request.Order.StoreId);
			var session = new PayPalSessionData();

			var apiResult = PayPalService.EnsureAccessToken(session, settings);
			if (result.Success)
			{
				apiResult = PayPalService.Refund(settings, session, request);

				if (apiResult.Success && apiResult.Json != null)
				{
					if (request.IsPartialRefund)
						result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
					else
						result.NewPaymentStatus = PaymentStatus.Refunded;
				}
				else
				{
					result.Errors.Add(apiResult.ErrorMessage);
				}
			}
			else
			{
				result.Errors.Add(apiResult.ErrorMessage);
			}

			return result;
        }

        public override VoidPaymentResult Void(VoidPaymentRequest request)
        {
			var result = new VoidPaymentResult
			{
				NewPaymentStatus = request.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(request.Order.StoreId);

			var transactionId = request.Order.AuthorizationTransactionId;

            if (transactionId.IsEmpty())
                transactionId = request.Order.CaptureTransactionId;

			// TODO

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

