using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Routing;
using Autofac;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Services.Directory;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework.Plugins;
using SmartStore.PayPal;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.PayPalSvc;
using SmartStore.PayPal.Settings;
using SmartStore.PayPal.Controllers;

namespace SmartStore.PayPal
{
	/// <summary>
	/// PayPalDirect provider
	/// </summary>
    [SystemName("Payments.PayPalDirect")]
    [FriendlyName("PayPal Direct")]
    [DisplayOrder(1)]
    public class PayPalDirectPaymentProcessor : PaymentMethodBase, IConfigurable
	{
		#region Fields

		private readonly PluginHelper _helper;
		private readonly PayPalDirectSettings _paypalDirectSettings;
		private readonly ISettingService _settingService;
		private readonly ICurrencyService _currencyService;
		private readonly ICustomerService _customerService;
		private readonly CurrencySettings _currencySettings;
		private readonly IWebHelper _webHelper;
		private readonly IOrderTotalCalculationService _orderTotalCalculationService;
		private readonly ILocalizationService _localizationService;
		#endregion

		#region Ctor

		public PayPalDirectPaymentProcessor(PayPalDirectSettings paypalDirectSettings,
			ISettingService settingService,
			ICurrencyService currencyService, ICustomerService customerService,
			CurrencySettings currencySettings, IWebHelper webHelper,
			IOrderTotalCalculationService orderTotalCalculationService,
			ILocalizationService localizationService,
			IComponentContext ctx)
		{
			this._paypalDirectSettings = paypalDirectSettings;
			this._settingService = settingService;
			this._currencyService = currencyService;
			this._customerService = customerService;
			this._currencySettings = currencySettings;
			this._webHelper = webHelper;
			this._orderTotalCalculationService = orderTotalCalculationService;
			this._localizationService = localizationService;

			_helper = new PluginHelper(ctx, "SmartStore.PayPal");
		}

		#endregion

		#region Utilities

		protected ProcessPaymentResult AuthorizeOrSale(ProcessPaymentRequest processPaymentRequest, bool authorizeOnly)
		{
			var result = new ProcessPaymentResult();

			var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

			var req = new DoDirectPaymentReq();
			req.DoDirectPaymentRequest = new DoDirectPaymentRequestType();
			req.DoDirectPaymentRequest.Version = PayPalHelper.GetApiVersion();
			var details = new DoDirectPaymentRequestDetailsType();
			req.DoDirectPaymentRequest.DoDirectPaymentRequestDetails = details;
			details.IPAddress = _webHelper.GetCurrentIpAddress();
			if (details.IPAddress == null || details.IPAddress == "::1")
				details.IPAddress = "127.0.0.1";
			if (authorizeOnly)
				details.PaymentAction = PaymentActionCodeType.Authorization;
			else
				details.PaymentAction = PaymentActionCodeType.Sale;
			//credit card
			details.CreditCard = new CreditCardDetailsType();
			details.CreditCard.CreditCardNumber = processPaymentRequest.CreditCardNumber;
			details.CreditCard.CreditCardType = PayPalHelper.GetPaypalCreditCardType(processPaymentRequest.CreditCardType);
			details.CreditCard.ExpMonthSpecified = true;
			details.CreditCard.ExpMonth = processPaymentRequest.CreditCardExpireMonth;
			details.CreditCard.ExpYearSpecified = true;
			details.CreditCard.ExpYear = processPaymentRequest.CreditCardExpireYear;
			details.CreditCard.CVV2 = processPaymentRequest.CreditCardCvv2;
			details.CreditCard.CardOwner = new PayerInfoType();
            details.CreditCard.CardOwner.PayerCountry = PayPalHelper.GetPaypalCountryCodeType(customer.BillingAddress.Country);
			details.CreditCard.CreditCardTypeSpecified = true;
			//billing address
			details.CreditCard.CardOwner.Address = new AddressType();
			details.CreditCard.CardOwner.Address.CountrySpecified = true;
			details.CreditCard.CardOwner.Address.Street1 = customer.BillingAddress.Address1;
			details.CreditCard.CardOwner.Address.Street2 = customer.BillingAddress.Address2;
			details.CreditCard.CardOwner.Address.CityName = customer.BillingAddress.City;
			if (customer.BillingAddress.StateProvince != null)
				details.CreditCard.CardOwner.Address.StateOrProvince = customer.BillingAddress.StateProvince.Abbreviation;
			else
				details.CreditCard.CardOwner.Address.StateOrProvince = "CA";
            details.CreditCard.CardOwner.Address.Country = PayPalHelper.GetPaypalCountryCodeType(customer.BillingAddress.Country);
			details.CreditCard.CardOwner.Address.PostalCode = customer.BillingAddress.ZipPostalCode;
			details.CreditCard.CardOwner.Payer = customer.BillingAddress.Email;
			details.CreditCard.CardOwner.PayerName = new PersonNameType();
			details.CreditCard.CardOwner.PayerName.FirstName = customer.BillingAddress.FirstName;
			details.CreditCard.CardOwner.PayerName.LastName = customer.BillingAddress.LastName;
			//order totals
			var payPalCurrency = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
			details.PaymentDetails = new PaymentDetailsType();
			details.PaymentDetails.OrderTotal = new BasicAmountType();
			details.PaymentDetails.OrderTotal.Value = Math.Round(processPaymentRequest.OrderTotal, 2).ToString("N", new CultureInfo("en-us"));
			details.PaymentDetails.OrderTotal.currencyID = payPalCurrency;
			details.PaymentDetails.Custom = processPaymentRequest.OrderGuid.ToString();
            details.PaymentDetails.ButtonSource = SmartStoreVersion.CurrentFullVersion;
			//shipping
			if (customer.ShippingAddress != null)
			{
				if (customer.ShippingAddress.StateProvince != null && customer.ShippingAddress.Country != null)
				{
					var shippingAddress = new AddressType();
					shippingAddress.Name = customer.ShippingAddress.FirstName + " " + customer.ShippingAddress.LastName;
					shippingAddress.Street1 = customer.ShippingAddress.Address1;
					shippingAddress.CityName = customer.ShippingAddress.City;
					shippingAddress.StateOrProvince = customer.ShippingAddress.StateProvince.Abbreviation;
					shippingAddress.PostalCode = customer.ShippingAddress.ZipPostalCode;
					shippingAddress.Country = (CountryCodeType)Enum.Parse(typeof(CountryCodeType), customer.ShippingAddress.Country.TwoLetterIsoCode, true);
					shippingAddress.CountrySpecified = true;
					details.PaymentDetails.ShipToAddress = shippingAddress;
				}
			}

			//send request
			using (var service = new PayPalAPIAASoapBinding())
			{
                service.Url = PayPalHelper.GetPaypalServiceUrl(_paypalDirectSettings);
				service.RequesterCredentials = new CustomSecurityHeaderType();
				service.RequesterCredentials.Credentials = new UserIdPasswordType();
                service.RequesterCredentials.Credentials.Username = _paypalDirectSettings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = _paypalDirectSettings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = _paypalDirectSettings.Signature;
				service.RequesterCredentials.Credentials.Subject = "";

				DoDirectPaymentResponseType response = service.DoDirectPayment(req);

				string error = "";
				bool success = PayPalHelper.CheckSuccess(_helper, response, out error);
				if (success)
				{
					result.AvsResult = response.AVSCode;
					result.AuthorizationTransactionCode = response.CVV2Code;
					if (authorizeOnly)
					{
						result.AuthorizationTransactionId = response.TransactionID;
						result.AuthorizationTransactionResult = response.Ack.ToString();

						result.NewPaymentStatus = PaymentStatus.Authorized;
					}
					else
					{
						result.CaptureTransactionId = response.TransactionID;
						result.CaptureTransactionResult = response.Ack.ToString();

						result.NewPaymentStatus = PaymentStatus.Paid;
					}
				}
				else
				{
					result.AddError(error);
				}
			}
			return result;
		}

		/// <summary>
		/// Verifies IPN
		/// </summary>
		/// <param name="formString">Form string</param>
		/// <param name="values">Values</param>
		/// <returns>Result</returns>
		public bool VerifyIPN(string formString, out Dictionary<string, string> values)
		{
            var serviceUrl = PayPalHelper.GetPaypalServiceUrl(_paypalDirectSettings);
            var req = (HttpWebRequest)WebRequest.Create(serviceUrl);
			req.Method = "POST";
			req.ContentType = "application/x-www-form-urlencoded";
			req.UserAgent = HttpContext.Current.Request.UserAgent;

			string formContent = string.Format("{0}&cmd=_notify-validate", formString);
			req.ContentLength = formContent.Length;

			using (var sw = new StreamWriter(req.GetRequestStream(), Encoding.ASCII))
			{
				sw.Write(formContent);
			}

			string response = null;
			using (var sr = new StreamReader(req.GetResponse().GetResponseStream()))
			{
				response = HttpUtility.UrlDecode(sr.ReadToEnd());
			}
			bool success = response.Trim().Equals("VERIFIED", StringComparison.OrdinalIgnoreCase);

			values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (string l in formString.Split('&'))
			{
				string line = HttpUtility.UrlDecode(l).Trim();
				int equalPox = line.IndexOf('=');
				if (equalPox >= 0)
					values.Add(line.Substring(0, equalPox), line.Substring(equalPox + 1));
			}

			return success;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Process a payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Process payment result</returns>
		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
			if (_paypalDirectSettings.TransactMode == TransactMode.Authorize)
			{
				return AuthorizeOrSale(processPaymentRequest, true);
			}
			else
			{
				return AuthorizeOrSale(processPaymentRequest, false);
			}
		}

		/// <summary>
		/// Gets additional handling fee
		/// </summary>
		/// <param name="cart">Shoping cart</param>
		/// <returns>Additional handling fee</returns>
		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
		{
			var result = this.CalculateAdditionalFee(_orderTotalCalculationService, cart,
                _paypalDirectSettings.AdditionalFee, _paypalDirectSettings.AdditionalFeePercentage);
			return result;
		}

		/// <summary>
		/// Captures payment
		/// </summary>
		/// <param name="capturePaymentRequest">Capture payment request</param>
		/// <returns>Capture payment result</returns>
		public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
		{
			var result = new CapturePaymentResult();

			string authorizationId = capturePaymentRequest.Order.AuthorizationTransactionId;
			var req = new DoCaptureReq();
			req.DoCaptureRequest = new DoCaptureRequestType();
            req.DoCaptureRequest.Version = PayPalHelper.GetApiVersion();
			req.DoCaptureRequest.AuthorizationID = authorizationId;
			req.DoCaptureRequest.Amount = new BasicAmountType();
			req.DoCaptureRequest.Amount.Value = Math.Round(capturePaymentRequest.Order.OrderTotal, 2).ToString("N", new CultureInfo("en-us"));
            req.DoCaptureRequest.Amount.currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
			req.DoCaptureRequest.CompleteType = CompleteCodeType.Complete;

			using (var service = new PayPalAPIAASoapBinding())
			{
                service.Url = PayPalHelper.GetPaypalServiceUrl(_paypalDirectSettings);

				service.RequesterCredentials = new CustomSecurityHeaderType();
				service.RequesterCredentials.Credentials = new UserIdPasswordType();
				service.RequesterCredentials.Credentials.Username = _paypalDirectSettings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = _paypalDirectSettings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = _paypalDirectSettings.Signature;
				service.RequesterCredentials.Credentials.Subject = "";

				DoCaptureResponseType response = service.DoCapture(req);

				string error = "";
				bool success = PayPalHelper.CheckSuccess(_helper, response, out error);
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

		/// <summary>
		/// Refunds a payment
		/// </summary>
		/// <param name="refundPaymentRequest">Request</param>
		/// <returns>Result</returns>
		public override RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
		{
			var result = new RefundPaymentResult();

			string transactionId = refundPaymentRequest.Order.CaptureTransactionId;

			var req = new RefundTransactionReq();
			req.RefundTransactionRequest = new RefundTransactionRequestType();
			//NOTE: Specify amount in partial refund
			req.RefundTransactionRequest.RefundType = RefundType.Full;
			req.RefundTransactionRequest.RefundTypeSpecified = true;
            req.RefundTransactionRequest.Version = PayPalHelper.GetApiVersion();
			req.RefundTransactionRequest.TransactionID = transactionId;

			using (var service = new PayPalAPISoapBinding())
			{
                service.Url = PayPalHelper.GetPaypalServiceUrl(_paypalDirectSettings);
				service.RequesterCredentials = new CustomSecurityHeaderType();
				service.RequesterCredentials.Credentials = new UserIdPasswordType();
                service.RequesterCredentials.Credentials.Username = _paypalDirectSettings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = _paypalDirectSettings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = _paypalDirectSettings.Signature;
				service.RequesterCredentials.Credentials.Subject = "";

				RefundTransactionResponseType response = service.RefundTransaction(req);

				string error = string.Empty;
				bool Success = PayPalHelper.CheckSuccess(_helper, response, out error);
				if (Success)
				{
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
		public override VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
		{
			var result = new VoidPaymentResult();

			string transactionId = voidPaymentRequest.Order.AuthorizationTransactionId;
			if (String.IsNullOrEmpty(transactionId))
				transactionId = voidPaymentRequest.Order.CaptureTransactionId;

			var req = new DoVoidReq();
			req.DoVoidRequest = new DoVoidRequestType();
			req.DoVoidRequest.Version = PayPalHelper.GetApiVersion();
			req.DoVoidRequest.AuthorizationID = transactionId;


			using (var service = new PayPalAPIAASoapBinding())
			{
                service.Url = PayPalHelper.GetPaypalServiceUrl(_paypalDirectSettings);
				service.RequesterCredentials = new CustomSecurityHeaderType();
				service.RequesterCredentials.Credentials = new UserIdPasswordType();
                service.RequesterCredentials.Credentials.Username = _paypalDirectSettings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = _paypalDirectSettings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = _paypalDirectSettings.Signature;
				service.RequesterCredentials.Credentials.Subject = "";

				DoVoidResponseType response = service.DoVoid(req);

				string error = "";
                bool success = PayPalHelper.CheckSuccess(_helper, response, out error);
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
		/// Process recurring payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Process payment result</returns>
		public override ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = new ProcessPaymentResult();

			var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

			var req = new CreateRecurringPaymentsProfileReq();
			req.CreateRecurringPaymentsProfileRequest = new CreateRecurringPaymentsProfileRequestType();
            req.CreateRecurringPaymentsProfileRequest.Version = PayPalHelper.GetApiVersion();
			var details = new CreateRecurringPaymentsProfileRequestDetailsType();
			req.CreateRecurringPaymentsProfileRequest.CreateRecurringPaymentsProfileRequestDetails = details;

			details.CreditCard = new CreditCardDetailsType();
			details.CreditCard.CreditCardNumber = processPaymentRequest.CreditCardNumber;
			details.CreditCard.CreditCardType = PayPalHelper.GetPaypalCreditCardType(processPaymentRequest.CreditCardType);
			details.CreditCard.ExpMonthSpecified = true;
			details.CreditCard.ExpMonth = processPaymentRequest.CreditCardExpireMonth;
			details.CreditCard.ExpYearSpecified = true;
			details.CreditCard.ExpYear = processPaymentRequest.CreditCardExpireYear;
			details.CreditCard.CVV2 = processPaymentRequest.CreditCardCvv2;
			details.CreditCard.CardOwner = new PayerInfoType();
            details.CreditCard.CardOwner.PayerCountry = PayPalHelper.GetPaypalCountryCodeType(customer.BillingAddress.Country);
			details.CreditCard.CreditCardTypeSpecified = true;

			details.CreditCard.CardOwner.Address = new AddressType();
			details.CreditCard.CardOwner.Address.CountrySpecified = true;
			details.CreditCard.CardOwner.Address.Street1 = customer.BillingAddress.Address1;
			details.CreditCard.CardOwner.Address.Street2 = customer.BillingAddress.Address2;
			details.CreditCard.CardOwner.Address.CityName = customer.BillingAddress.City;
			if (customer.BillingAddress.StateProvince != null)
				details.CreditCard.CardOwner.Address.StateOrProvince = customer.BillingAddress.StateProvince.Abbreviation;
			else
				details.CreditCard.CardOwner.Address.StateOrProvince = "CA";
            details.CreditCard.CardOwner.Address.Country = PayPalHelper.GetPaypalCountryCodeType(customer.BillingAddress.Country);
			details.CreditCard.CardOwner.Address.PostalCode = customer.BillingAddress.ZipPostalCode;
			details.CreditCard.CardOwner.Payer = customer.BillingAddress.Email;
			details.CreditCard.CardOwner.PayerName = new PersonNameType();
			details.CreditCard.CardOwner.PayerName.FirstName = customer.BillingAddress.FirstName;
			details.CreditCard.CardOwner.PayerName.LastName = customer.BillingAddress.LastName;

			//start date
			details.RecurringPaymentsProfileDetails = new RecurringPaymentsProfileDetailsType();
			details.RecurringPaymentsProfileDetails.BillingStartDate = DateTime.UtcNow;
			details.RecurringPaymentsProfileDetails.ProfileReference = processPaymentRequest.OrderGuid.ToString();

			//schedule
			details.ScheduleDetails = new ScheduleDetailsType();
			details.ScheduleDetails.Description = _helper.GetResource("RecurringPayment");
			details.ScheduleDetails.PaymentPeriod = new BillingPeriodDetailsType();
			details.ScheduleDetails.PaymentPeriod.Amount = new BasicAmountType();
			details.ScheduleDetails.PaymentPeriod.Amount.Value = Math.Round(processPaymentRequest.OrderTotal, 2).ToString("N", new CultureInfo("en-us"));
			details.ScheduleDetails.PaymentPeriod.Amount.currencyID = PayPalHelper.GetPaypalCurrency(_currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId));
			details.ScheduleDetails.PaymentPeriod.BillingFrequency = processPaymentRequest.RecurringCycleLength;
			switch (processPaymentRequest.RecurringCyclePeriod)
			{
				case RecurringProductCyclePeriod.Days:
					details.ScheduleDetails.PaymentPeriod.BillingPeriod = BillingPeriodType.Day;
					break;
				case RecurringProductCyclePeriod.Weeks:
					details.ScheduleDetails.PaymentPeriod.BillingPeriod = BillingPeriodType.Week;
					break;
				case RecurringProductCyclePeriod.Months:
					details.ScheduleDetails.PaymentPeriod.BillingPeriod = BillingPeriodType.Month;
					break;
				case RecurringProductCyclePeriod.Years:
					details.ScheduleDetails.PaymentPeriod.BillingPeriod = BillingPeriodType.Year;
					break;
				default:
					throw new SmartException(_helper.GetResource("NotSupportedPeriod"));
			}
			details.ScheduleDetails.PaymentPeriod.TotalBillingCycles = processPaymentRequest.RecurringTotalCycles;
			details.ScheduleDetails.PaymentPeriod.TotalBillingCyclesSpecified = true;

			using (var service = new PayPalAPIAASoapBinding())
			{
                service.Url = PayPalHelper.GetPaypalServiceUrl(_paypalDirectSettings);
				service.RequesterCredentials = new CustomSecurityHeaderType();
				service.RequesterCredentials.Credentials = new UserIdPasswordType();
                service.RequesterCredentials.Credentials.Username = _paypalDirectSettings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = _paypalDirectSettings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = _paypalDirectSettings.Signature;
				service.RequesterCredentials.Credentials.Subject = "";

				CreateRecurringPaymentsProfileResponseType response = service.CreateRecurringPaymentsProfile(req);

				string error = "";
                bool success = PayPalHelper.CheckSuccess(_helper, response, out error);
				if (success)
				{
					result.NewPaymentStatus = PaymentStatus.Pending;
					if (response.CreateRecurringPaymentsProfileResponseDetails != null)
					{
						result.SubscriptionTransactionId = response.CreateRecurringPaymentsProfileResponseDetails.ProfileID;
					}
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
		public override CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
		{
			var result = new CancelRecurringPaymentResult();
			var order = cancelPaymentRequest.Order;

			var req = new ManageRecurringPaymentsProfileStatusReq();
			req.ManageRecurringPaymentsProfileStatusRequest = new ManageRecurringPaymentsProfileStatusRequestType();
            req.ManageRecurringPaymentsProfileStatusRequest.Version = PayPalHelper.GetApiVersion();
			var details = new ManageRecurringPaymentsProfileStatusRequestDetailsType();
			req.ManageRecurringPaymentsProfileStatusRequest.ManageRecurringPaymentsProfileStatusRequestDetails = details;

			details.Action = StatusChangeActionType.Cancel;
			//Recurring payments profile ID returned in the CreateRecurringPaymentsProfile response
			details.ProfileID = order.SubscriptionTransactionId;

			using (var service = new PayPalAPIAASoapBinding())
			{
                service.Url = PayPalHelper.GetPaypalServiceUrl(_paypalDirectSettings);
				service.RequesterCredentials = new CustomSecurityHeaderType();
				service.RequesterCredentials.Credentials = new UserIdPasswordType();
                service.RequesterCredentials.Credentials.Username = _paypalDirectSettings.ApiAccountName;
                service.RequesterCredentials.Credentials.Password = _paypalDirectSettings.ApiAccountPassword;
                service.RequesterCredentials.Credentials.Signature = _paypalDirectSettings.Signature;
				service.RequesterCredentials.Credentials.Subject = "";

				var response = service.ManageRecurringPaymentsProfileStatus(req);

				string error = "";
				if (!PayPalHelper.CheckSuccess(_helper, response, out error))
				{
					result.AddError(error);
				}
			}

			return result;
		}

		/// <summary>
		/// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
		/// </summary>
		/// <param name="order">Order</param>
		/// <returns>Result</returns>
		public override bool CanRePostProcessPayment(Order order)
		{
			if (order == null)
				throw new ArgumentNullException("order");

			//it's not a redirection payment method. So we always return false
			return false;
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
			controllerName = "PayPalDirect";
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
			controllerName = "PayPalDirect";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.PayPal" } };
		}

		public override Type GetControllerType()
		{
			return typeof(PayPalDirectController);
		}

		#endregion

		#region Properties

		/// <summary>
		/// Gets a value indicating whether capture is supported
		/// </summary>
		public override bool SupportCapture
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether refund is supported
		/// </summary>
		public override bool SupportRefund
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets a value indicating whether void is supported
		/// </summary>
		public override bool SupportVoid
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Gets a recurring payment type of payment method
		/// </summary>
		public override RecurringPaymentType RecurringPaymentType
		{
			get
			{
				return RecurringPaymentType.Automatic;
			}
		}

		/// <summary>
		/// Gets a payment method type
		/// </summary>
		public override PaymentMethodType PaymentMethodType
		{
			get
			{
				return PaymentMethodType.Standard;
			}
		}

		#endregion
	}
}