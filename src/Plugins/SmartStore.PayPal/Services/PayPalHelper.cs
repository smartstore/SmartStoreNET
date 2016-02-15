using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Routing;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Payments;
using SmartStore.PayPal.PayPalSvc;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Localization;

namespace SmartStore.PayPal.Services
{
	/// <summary>
	/// Represents paypal helper
	/// </summary>
	public static class PayPalHelper
    {
        /// <summary>
        /// Gets a payment status
        /// </summary>
        /// <param name="paymentStatus">PayPal payment status</param>
        /// <param name="pendingReason">PayPal pending reason</param>
        /// <returns>Payment status</returns>
        public static PaymentStatus GetPaymentStatus(string paymentStatus, string pendingReason)
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
                case "refunded":
                case "reversed":
                    result = PaymentStatus.Refunded;
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// Checks response
        /// </summary>
        /// <param name="abstractResponse">response</param>
        /// <param name="errorMsg">Error message if exists</param>
        /// <returns>True - response OK; otherwise, false</returns>
		public static bool CheckSuccess(ILocalizationService localization, AbstractResponseType abstractResponse, out string errorMsg)
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

					sb.Append("{0}: {1}".FormatInvariant(localization.GetResource("Admin.System.Log.Fields.ShortMessage"), errorType.ShortMessage));
					sb.AppendLine(" ({0}).".FormatInvariant(errorType.ErrorCode));

					if (errorType.LongMessage.HasValue() && errorType.LongMessage != errorType.ShortMessage)
						sb.AppendLine("{0}: {1}".FormatInvariant(localization.GetResource("Admin.System.Log.Fields.FullMessage"), errorType.LongMessage));
				}
            }

            errorMsg = sb.ToString();
            return success;
        }

        /// <summary>
        /// Get Paypal currency code
        /// </summary>
        /// <param name="currency">Currency</param>
        /// <returns>Paypal currency code</returns>
        public static CurrencyCodeType GetPaypalCurrency(Currency currency)
        {
            CurrencyCodeType currencyCodeType = CurrencyCodeType.USD;
            try
            {
                currencyCodeType = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), currency.CurrencyCode, true);
            }
            catch
            {
            }
            return currencyCodeType;
        }

        public static string CheckIfButtonExists(PayPalExpressPaymentSettings settings, string buttonUrl) 
        {
            HttpWebResponse response = null;

			if (settings.SecurityProtocol.HasValue)
			{
				ServicePointManager.SecurityProtocol = settings.SecurityProtocol.Value;
			}

			var request = (HttpWebRequest)WebRequest.Create(buttonUrl);
            request.Method = "HEAD";

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                return buttonUrl;
            }
            catch (WebException)
            {
                /* A WebException will be thrown if the status of the response is not `200 OK` */
                return "https://www.paypalobjects.com/en_US/i/btn/btn_xpressCheckout.gif";
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        public static bool CurrentPageIsBasket(RouteData routeData) 
        {
            return routeData.GetRequiredString("controller").IsCaseInsensitiveEqual("ShoppingCart")
                && routeData.GetRequiredString("action").IsCaseInsensitiveEqual("Cart");
        }

        /// <summary>
        /// Gets Paypal URL
        /// </summary>
        /// <returns></returns>
        public static string GetPaypalUrl(PayPalSettingsBase settings)
        {
            return settings.UseSandbox ?
                "https://www.sandbox.paypal.com/cgi-bin/webscr" :
                "https://www.paypal.com/cgi-bin/webscr";
        }

		public static HttpWebRequest GetPayPalWebRequest(PayPalSettingsBase settings)
		{
			if (settings.SecurityProtocol.HasValue)
			{
				ServicePointManager.SecurityProtocol = settings.SecurityProtocol.Value;
			}

			var request = (HttpWebRequest)WebRequest.Create(GetPaypalUrl(settings));
			return request;
		}

		public static bool VerifyIPN(PayPalSettingsBase settings, string formString, out Dictionary<string, string> values)
		{
			// settings: multistore context not possible here. we need the custom value to determine what store it is.

			var request = PayPalHelper.GetPayPalWebRequest(settings);
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.UserAgent = HttpContext.Current.Request.UserAgent;

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

		public static string GetApiVersion()
        {
            return "109";
        }

        /// <summary>
        /// Gets API credentials
        /// </summary>
        /// <returns></returns>
        public static CustomSecurityHeaderType GetPaypalApiCredentials(PayPalApiSettingsBase settings)
        {
            CustomSecurityHeaderType customSecurityHeaderType = new CustomSecurityHeaderType();

            customSecurityHeaderType.Credentials = new UserIdPasswordType();
            customSecurityHeaderType.Credentials.Username = settings.ApiAccountName;
            customSecurityHeaderType.Credentials.Password = settings.ApiAccountPassword;
            customSecurityHeaderType.Credentials.Signature = settings.Signature;
            customSecurityHeaderType.Credentials.Subject = "";

            return customSecurityHeaderType;
        }
        
		/// <summary>
        /// Get Paypal country code
        /// </summary>
        /// <param name="country">Country</param>
        /// <returns>Paypal country code</returns>
        public static CountryCodeType GetPaypalCountryCodeType(Country country)
        {
            CountryCodeType payerCountry = CountryCodeType.US;
            try
            {
                payerCountry = (CountryCodeType)Enum.Parse(typeof(CountryCodeType), country.TwoLetterIsoCode);
            }
            catch
            {
            }
            return payerCountry;
        }

        /// <summary>
        /// Get Paypal credit card type
        /// </summary>
        /// <param name="creditCardType">Credit card type</param>
        /// <returns>Paypal credit card type</returns>
        public static CreditCardTypeType GetPaypalCreditCardType(string creditCardType)
        {
            var creditCardTypeType = (CreditCardTypeType)Enum.Parse(typeof(CreditCardTypeType), creditCardType);
            return creditCardTypeType;
        }

        public static PaymentActionCodeType GetPaymentAction(PayPalExpressPaymentSettings payPalExpressPaymentSettings)
        {
            if (payPalExpressPaymentSettings.TransactMode == TransactMode.Authorize)
            {
                return PaymentActionCodeType.Authorization;
            }
            else
            {
                return PaymentActionCodeType.Sale;
            }
        }

		public static Dictionary<SecurityProtocolType, string> GetSecurityProtocols()
		{
			var dic = new Dictionary<SecurityProtocolType, string>();

			foreach (SecurityProtocolType protocol in Enum.GetValues(typeof(SecurityProtocolType)))
			{
				string friendlyName = null;
				switch (protocol)
				{
					case SecurityProtocolType.Ssl3:
						friendlyName = "SSL 3.0";
						break;
					case SecurityProtocolType.Tls:
						friendlyName = "TLS 1.0";
						break;
					case SecurityProtocolType.Tls11:
						friendlyName = "TLS 1.1";
						break;
					case SecurityProtocolType.Tls12:
						friendlyName = "TLS 1.2";
						break;
					default:
						friendlyName = protocol.ToString().ToUpper();
						break;
				}

				dic.Add(protocol, friendlyName);
			}
			return dic;
		}

    }
}

