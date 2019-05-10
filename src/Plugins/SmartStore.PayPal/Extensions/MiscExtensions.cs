using System.Web;
using Newtonsoft.Json;
using SmartStore.Core.Domain.Customers;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Common;

namespace SmartStore.PayPal
{
    internal static class MiscExtensions
	{
		public static string GetPayPalUrl(this PayPalSettingsBase settings)
		{
			return settings.UseSandbox ?
				"https://www.sandbox.paypal.com/cgi-bin/webscr" :
				"https://www.paypal.com/cgi-bin/webscr";
		}

		public static PayPalSessionData GetPayPalState(this HttpContextBase httpContext, string key)
		{
            Guard.NotEmpty(key, nameof(key));

			var state = httpContext.GetCheckoutState();

            if (!state.CustomProperties.ContainsKey(key))
            {
                state.CustomProperties.Add(key, new PayPalSessionData());
            }

			var session = state.CustomProperties.Get(key) as PayPalSessionData;
            return session;
		}

        public static PayPalSessionData GetPayPalState(
            this HttpContextBase httpContext,
            string key,
            Customer customer,
            int storeId,
            IGenericAttributeService genericAttributeService)
        {
            Guard.NotNull(httpContext, nameof(httpContext));
            Guard.NotNull(customer, nameof(customer));
            Guard.NotNull(genericAttributeService, nameof(genericAttributeService));

            var session = httpContext.GetPayPalState(key);

            if (session.AccessToken.IsEmpty() || session.PaymentId.IsEmpty())
            {
                try
                {
                    var str = customer.GetAttribute<string>(key + ".SessionData", genericAttributeService, storeId);
                    if (str.HasValue())
                    {
                        var storedSessionData = JsonConvert.DeserializeObject<PayPalSessionData>(str);
                        if (storedSessionData != null)
                        {
                            // Only token and paymentId required.
                            session.AccessToken = storedSessionData.AccessToken;
                            session.PaymentId = storedSessionData.PaymentId;
                        }
                    }
                }
                catch { }
            }

            return session;
        }
    }
}