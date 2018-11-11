using System;
using System.Globalization;
using SmartStore.Core;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.PayPal.Controllers;
using SmartStore.PayPal.PayPalSvc;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Customers;
using SmartStore.Services.Payments;

namespace SmartStore.PayPal
{
	[SystemName("Payments.PayPalDirect")]
    [FriendlyName("PayPal Direct")]
    [DisplayOrder(1)]
    public class PayPalDirectProvider : PayPalProviderBase<PayPalDirectPaymentSettings>
	{
		private readonly ICustomerService _customerService;

        public PayPalDirectProvider(
            ICustomerService customerService)
		{
			_customerService = customerService;
		}

		public static string SystemName { get { return "Payments.PayPalDirect"; } }

		public override bool RequiresInteraction
		{
			get
			{
				return true;
			}
		}

		public override RecurringPaymentType RecurringPaymentType
		{
			get
			{
				return RecurringPaymentType.Automatic;
			}
		}

		public override PaymentMethodType PaymentMethodType
		{
			get
			{
				return PaymentMethodType.Standard;
			}
		}

		private CreditCardTypeType GetCreditCardType(string creditCardType)
		{
			var creditCardTypeType = (CreditCardTypeType)Enum.Parse(typeof(CreditCardTypeType), creditCardType);
			return creditCardTypeType;
		}

		private CountryCodeType GetCountryCodeType(Country country)
		{
			CountryCodeType payerCountry = CountryCodeType.US;
			try
			{
				payerCountry = (CountryCodeType)Enum.Parse(typeof(CountryCodeType), country.TwoLetterIsoCode);
			}
			catch {	}

			return payerCountry;
		}

		protected override string GetControllerName()
		{
			return "PayPalDirect";
		}

		public override Type GetControllerType()
		{
			return typeof(PayPalDirectController);
		}

		protected override string GetResourceRootKey()
		{
			return "Plugins.Payments.PayPalDirect";
		}

		/// <summary>
		/// Process a payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Process payment result</returns>
		public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
		{
            var result = new ProcessPaymentResult();

			var store = Services.StoreService.GetStoreById(processPaymentRequest.StoreId);
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
			var settings = Services.Settings.LoadSetting<PayPalDirectPaymentSettings>(processPaymentRequest.StoreId);
            
            var req = new DoDirectPaymentReq();
            req.DoDirectPaymentRequest = new DoDirectPaymentRequestType();
            req.DoDirectPaymentRequest.Version = ApiVersion;

            var details = new DoDirectPaymentRequestDetailsType();
            req.DoDirectPaymentRequest.DoDirectPaymentRequestDetails = details;
            details.IPAddress = Services.WebHelper.GetCurrentIpAddress();

			if (details.IPAddress.IsEmpty())
                details.IPAddress = "127.0.0.1";

            if (settings.TransactMode == TransactMode.Authorize)
                details.PaymentAction = PaymentActionCodeType.Authorization;
            else
                details.PaymentAction = PaymentActionCodeType.Sale;

            //credit card
            details.CreditCard = new CreditCardDetailsType();
            details.CreditCard.CreditCardNumber = processPaymentRequest.CreditCardNumber;
            details.CreditCard.CreditCardType = GetCreditCardType(processPaymentRequest.CreditCardType);
            details.CreditCard.ExpMonthSpecified = true;
            details.CreditCard.ExpMonth = processPaymentRequest.CreditCardExpireMonth;
            details.CreditCard.ExpYearSpecified = true;
            details.CreditCard.ExpYear = processPaymentRequest.CreditCardExpireYear;
            details.CreditCard.CVV2 = processPaymentRequest.CreditCardCvv2;
            details.CreditCard.CardOwner = new PayerInfoType();
            details.CreditCard.CardOwner.PayerCountry = GetCountryCodeType(customer.BillingAddress.Country);
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
            details.CreditCard.CardOwner.Address.Country = GetCountryCodeType(customer.BillingAddress.Country);
            details.CreditCard.CardOwner.Address.PostalCode = customer.BillingAddress.ZipPostalCode;
            details.CreditCard.CardOwner.Payer = customer.BillingAddress.Email;
            details.CreditCard.CardOwner.PayerName = new PersonNameType();
            details.CreditCard.CardOwner.PayerName.FirstName = customer.BillingAddress.FirstName;
            details.CreditCard.CardOwner.PayerName.LastName = customer.BillingAddress.LastName;
            
			//order totals
            var payPalCurrency = GetApiCurrency(store.PrimaryStoreCurrency);

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
            using (var service = GetApiAaService(settings))
            {
                var response = service.DoDirectPayment(req);

                var error = "";
                var success = IsSuccess(response, out error);

                if (success)
                {
                    result.AvsResult = response.AVSCode;
                    result.AuthorizationTransactionCode = response.CVV2Code;
                    if (settings.TransactMode == TransactMode.Authorize)
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
		/// Process recurring payment
		/// </summary>
		/// <param name="processPaymentRequest">Payment info required for an order processing</param>
		/// <returns>Process payment result</returns>
		public override ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
		{
			var result = new ProcessPaymentResult();

			var store = Services.StoreService.GetStoreById(processPaymentRequest.StoreId);
			var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
			var settings = Services.Settings.LoadSetting<PayPalDirectPaymentSettings>(processPaymentRequest.StoreId);

			var req = new CreateRecurringPaymentsProfileReq();
			req.CreateRecurringPaymentsProfileRequest = new CreateRecurringPaymentsProfileRequestType();
            req.CreateRecurringPaymentsProfileRequest.Version = ApiVersion;
			var details = new CreateRecurringPaymentsProfileRequestDetailsType();
			req.CreateRecurringPaymentsProfileRequest.CreateRecurringPaymentsProfileRequestDetails = details;

			details.CreditCard = new CreditCardDetailsType();
			details.CreditCard.CreditCardNumber = processPaymentRequest.CreditCardNumber;
			details.CreditCard.CreditCardType = GetCreditCardType(processPaymentRequest.CreditCardType);
			details.CreditCard.ExpMonthSpecified = true;
			details.CreditCard.ExpMonth = processPaymentRequest.CreditCardExpireMonth;
			details.CreditCard.ExpYearSpecified = true;
			details.CreditCard.ExpYear = processPaymentRequest.CreditCardExpireYear;
			details.CreditCard.CVV2 = processPaymentRequest.CreditCardCvv2;
			details.CreditCard.CardOwner = new PayerInfoType();
            details.CreditCard.CardOwner.PayerCountry = GetCountryCodeType(customer.BillingAddress.Country);
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
            details.CreditCard.CardOwner.Address.Country = GetCountryCodeType(customer.BillingAddress.Country);
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
			details.ScheduleDetails.Description = T("Plugins.Payments.PayPalDirect.RecurringPayment");
			details.ScheduleDetails.PaymentPeriod = new BillingPeriodDetailsType();
			details.ScheduleDetails.PaymentPeriod.Amount = new BasicAmountType();
			details.ScheduleDetails.PaymentPeriod.Amount.Value = Math.Round(processPaymentRequest.OrderTotal, 2).ToString("N", new CultureInfo("en-us"));
			details.ScheduleDetails.PaymentPeriod.Amount.currencyID = GetApiCurrency(store.PrimaryStoreCurrency);
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
					throw new SmartException(T("Plugins.Payments.PayPalDirect.NotSupportedPeriod"));
			}
			details.ScheduleDetails.PaymentPeriod.TotalBillingCycles = processPaymentRequest.RecurringTotalCycles;
			details.ScheduleDetails.PaymentPeriod.TotalBillingCyclesSpecified = true;

			using (var service = GetApiAaService(settings))
			{
				var response = service.CreateRecurringPaymentsProfile(req);

				var error = "";
                var success = IsSuccess(response, out error);

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
	}
}