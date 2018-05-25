using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web.Routing;
using SmartStore.Core.Configuration;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.PayPalSvc;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal
{
	public abstract class PayPalProviderBase<TSetting> : PaymentMethodBase, IConfigurable where TSetting : PayPalApiSettingsBase, ISettings, new()
    {
        protected PayPalProviderBase()
		{
			Logger = NullLogger.Instance;
		}

		public static string ApiVersion
		{
			get { return "109"; }
		}

		public ILogger Logger { get; set; }
		public ICommonServices Services { get; set; }
		public IOrderService OrderService { get; set; }
        public IOrderTotalCalculationService OrderTotalCalculationService { get; set; }

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

		protected abstract string GetResourceRootKey();

		protected PayPalAPIAASoapBinding GetApiAaService(TSetting settings)
		{
			var service = new PayPalAPIAASoapBinding();

			service.Url = settings.UseSandbox ? "https://api-3t.sandbox.paypal.com/2.0/" : "https://api-3t.paypal.com/2.0/";

			service.RequesterCredentials = GetApiCredentials(settings);

			return service;
		}

		protected PayPalAPISoapBinding GetApiService(TSetting settings)
		{
			var service = new PayPalAPISoapBinding();

			service.Url = settings.UseSandbox ? "https://api-3t.sandbox.paypal.com/2.0/" : "https://api-3t.paypal.com/2.0/";

			service.RequesterCredentials = GetApiCredentials(settings);

			return service;
		}

		protected CustomSecurityHeaderType GetApiCredentials(PayPalApiSettingsBase settings)
		{
			var customSecurityHeaderType = new CustomSecurityHeaderType();

			customSecurityHeaderType.Credentials = new UserIdPasswordType();
			customSecurityHeaderType.Credentials.Username = settings.ApiAccountName;
			customSecurityHeaderType.Credentials.Password = settings.ApiAccountPassword;
			customSecurityHeaderType.Credentials.Signature = settings.Signature;
			customSecurityHeaderType.Credentials.Subject = "";

			return customSecurityHeaderType;
		}

		protected CurrencyCodeType GetApiCurrency(Currency currency)
		{
			var currencyCodeType = CurrencyCodeType.USD;
			try
			{
				currencyCodeType = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), currency.CurrencyCode, true);
			}
			catch {	}

			return currencyCodeType;
		}

		protected bool IsSuccess(AbstractResponseType abstractResponse, out string errorMsg)
		{
			var success = false;
			var sb = new StringBuilder();

			switch (abstractResponse.Ack)
			{
				case AckCodeType.Success:
				case AckCodeType.SuccessWithWarning:
					success = true;
					break;
				default:
					break;
			}

			if (null != abstractResponse.Errors)
			{
				foreach (ErrorType errorType in abstractResponse.Errors)
				{
					if (errorType.ShortMessage.IsEmpty())
						continue;

					if (sb.Length > 0)
						sb.Append(Environment.NewLine);

					sb.Append("{0}: {1}".FormatInvariant(Services.Localization.GetResource("Admin.System.Log.Fields.ShortMessage"), errorType.ShortMessage));
					sb.AppendLine(" ({0}).".FormatInvariant(errorType.ErrorCode));

					if (errorType.LongMessage.HasValue() && errorType.LongMessage != errorType.ShortMessage)
						sb.AppendLine("{0}: {1}".FormatInvariant(Services.Localization.GetResource("Admin.System.Log.Fields.FullMessage"), errorType.LongMessage));
				}
			}

			errorMsg = sb.ToString();
			return success;
		}

		protected abstract string GetControllerName();

		/// <summary>
		/// Gets additional handling fee
		/// </summary>
		/// <param name="cart">Shoping cart</param>
		/// <returns>Additional handling fee</returns>
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

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
			var result = new CapturePaymentResult
			{
				NewPaymentStatus = capturePaymentRequest.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(capturePaymentRequest.Order.StoreId);
			var currencyCode = Services.WorkContext.WorkingCurrency.CurrencyCode ?? "EUR";

			var authorizationId = capturePaymentRequest.Order.AuthorizationTransactionId;			

            var req = new DoCaptureReq();
            req.DoCaptureRequest = new DoCaptureRequestType();
            req.DoCaptureRequest.Version = ApiVersion;
            req.DoCaptureRequest.AuthorizationID = authorizationId;
            req.DoCaptureRequest.Amount = new BasicAmountType();
            req.DoCaptureRequest.Amount.Value = Math.Round(capturePaymentRequest.Order.OrderTotal, 2).ToString("N", new CultureInfo("en-us"));
            req.DoCaptureRequest.Amount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), currencyCode, true);
            req.DoCaptureRequest.CompleteType = CompleteCodeType.Complete;

            using (var service = GetApiAaService(settings))
            {
                var response = service.DoCapture(req);

                var error = "";
                var success = IsSuccess(response, out error);

                if (success)
                {
                    result.NewPaymentStatus = PaymentStatus.Paid;
                    result.CaptureTransactionId = response.DoCaptureResponseDetails.PaymentInfo.TransactionID;
                    result.CaptureTransactionResult = response.Ack.ToString();
                }
                else
                {
                    result.AddError(error);
                }
            }
            return result;
        }

        public override RefundPaymentResult Refund(RefundPaymentRequest request)
        {
			// "Transaction refused (10009). You can not refund this type of transaction.":
			// merchant must accept the payment in his PayPal account
			var result = new RefundPaymentResult
			{
				NewPaymentStatus = request.Order.PaymentStatus
			};

			var settings = Services.Settings.LoadSetting<TSetting>(request.Order.StoreId);

			var transactionId = request.Order.CaptureTransactionId;

			var req = new RefundTransactionReq();
            req.RefundTransactionRequest = new RefundTransactionRequestType();

			if (request.IsPartialRefund)
			{
				var store = Services.StoreService.GetStoreById(request.Order.StoreId);
				var currencyCode = store.PrimaryStoreCurrency.CurrencyCode;

				req.RefundTransactionRequest.RefundType = RefundType.Partial;

				req.RefundTransactionRequest.Amount = new BasicAmountType();
				req.RefundTransactionRequest.Amount.Value = Math.Round(request.AmountToRefund, 2).ToString("N", new CultureInfo("en-us"));
				req.RefundTransactionRequest.Amount.currencyID = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), currencyCode, true);

				// see https://developer.paypal.com/docs/classic/express-checkout/digital-goods/ECDGIssuingRefunds/
				// https://developer.paypal.com/docs/classic/api/merchant/RefundTransaction_API_Operation_NVP/
				var memo = Services.Localization.GetResource("Plugins.SmartStore.PayPal.PartialRefundMemo", 0, false, "", true);
				if (memo.HasValue())
				{
					req.RefundTransactionRequest.Memo = memo.FormatInvariant(req.RefundTransactionRequest.Amount.Value);
				}
			}
			else
			{
				req.RefundTransactionRequest.RefundType = RefundType.Full;
			}

            req.RefundTransactionRequest.RefundTypeSpecified = true;
            req.RefundTransactionRequest.Version = ApiVersion;
            req.RefundTransactionRequest.TransactionID = transactionId;

            using (var service = GetApiService(settings))
            {
                var response = service.RefundTransaction(req);

                var error = "";
                var Success = IsSuccess(response, out error);

                if (Success)
                {
					if (request.IsPartialRefund)
						result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;
					else
						result.NewPaymentStatus = PaymentStatus.Refunded;

                    //cancelPaymentResult.RefundTransactionID = response.RefundTransactionID;
                }
                else
                {
                    result.AddError(error);
                }
            }

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
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

            var req = new DoVoidReq();
            req.DoVoidRequest = new DoVoidRequestType();
            req.DoVoidRequest.Version = ApiVersion;
            req.DoVoidRequest.AuthorizationID = transactionId;

            using (var service = GetApiAaService(settings))
            {
                var response = service.DoVoid(req);

                var error = "";
                var success = IsSuccess(response, out error);

                if (success)
                {
                    result.NewPaymentStatus = PaymentStatus.Voided;
                    //result.VoidTransactionID = response.RefundTransactionID;
                }
                else
                {
                    result.AddError(error);
                }
            }
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public override CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest request)
        {
            var result = new CancelRecurringPaymentResult();
            var order = request.Order;
			var settings = Services.Settings.LoadSetting<TSetting>(order.StoreId);

            var req = new ManageRecurringPaymentsProfileStatusReq();
            req.ManageRecurringPaymentsProfileStatusRequest = new ManageRecurringPaymentsProfileStatusRequestType();
            req.ManageRecurringPaymentsProfileStatusRequest.Version = ApiVersion;
            var details = new ManageRecurringPaymentsProfileStatusRequestDetailsType();
            req.ManageRecurringPaymentsProfileStatusRequest.ManageRecurringPaymentsProfileStatusRequestDetails = details;

            details.Action = StatusChangeActionType.Cancel;
            //Recurring payments profile ID returned in the CreateRecurringPaymentsProfile response
            details.ProfileID = order.SubscriptionTransactionId;

            using (var service = GetApiAaService(settings))
            {
                var response = service.ManageRecurringPaymentsProfileStatus(req);

                string error = "";
                if (!IsSuccess(response, out error))
                {
                    result.AddError(error);
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = GetControllerName();
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.PayPal" } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = GetControllerName();
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.PayPal" } };
        }
    }
}

