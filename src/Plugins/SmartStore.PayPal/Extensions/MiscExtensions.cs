using System.Net;
using System.Web;
using SmartStore.PayPal.Services;
using SmartStore.PayPal.Settings;
using SmartStore.Services.Orders;

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

		public static HttpWebRequest GetPayPalWebRequest(this PayPalSettingsBase settings)
		{
			var request = (HttpWebRequest)WebRequest.Create(GetPayPalUrl(settings));
			return request;
		}

		public static PayPalSessionData GetPayPalSessionData(this HttpContextBase httpContext, CheckoutState state = null)
		{
			if (state == null)
				state = httpContext.GetCheckoutState();

			if (!state.CustomProperties.ContainsKey(PayPalPlusProvider.SystemName))
				state.CustomProperties.Add(PayPalPlusProvider.SystemName, new PayPalSessionData());

			return state.CustomProperties.Get(PayPalPlusProvider.SystemName) as PayPalSessionData;
		}
	}
}