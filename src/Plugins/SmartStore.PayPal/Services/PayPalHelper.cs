using System;
using System.Net;
using System.Text;
using System.Web.Routing;
using SmartStore.Core.Domain.Directory;
using SmartStore.Core.Domain.Payments;
using SmartStore.PayPal.PayPalSvc;
using SmartStore.PayPal.Settings;
using SmartStore.Web.Framework.Plugins;

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
		public static bool CheckSuccess(PluginHelper helper, AbstractResponseType abstractResponse, out string errorMsg)
        {
            bool success = false;
            StringBuilder sb = new StringBuilder();
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
                    if (sb.Length <= 0)
                    {
                        sb.Append(Environment.NewLine);
                    }
					sb.AppendLine("{0}: {1}".FormatWith(helper.GetResource("Admin.System.Log.Fields.FullMessage"), errorType.LongMessage));
					sb.AppendLine("{0}: {1}".FormatWith(helper.GetResource("Admin.System.Log.Fields.ShortMessage"), errorType.ShortMessage));
                    sb.Append("Code: ").Append(errorType.ErrorCode).Append(Environment.NewLine);
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

        public static string CheckIfButtonExists(string buttonUrl) 
        { 
        
            HttpWebResponse response = null;
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

        //TODO: join the following two methods, with help of payment method type

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

        /// <summary>
        /// Gets Paypal URL
        /// </summary>
        /// <returns></returns>
        public static string GetPaypalServiceUrl(PayPalSettingsBase settings)
        {
            return settings.UseSandbox ?
                "https://api-3t.sandbox.paypal.com/2.0/" :
                "https://api-3t.paypal.com/2.0/";
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

    }
}

