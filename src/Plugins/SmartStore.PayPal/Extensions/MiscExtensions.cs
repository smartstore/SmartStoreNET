using System.Net;
using SmartStore.PayPal.Settings;

namespace SmartStore.PayPal
{
	public static class MiscExtensions
	{
		public static string GetPayPalUrl(this PayPalSettingsBase settings)
		{
			return settings.UseSandbox ?
				"https://www.sandbox.paypal.com/cgi-bin/webscr" :
				"https://www.paypal.com/cgi-bin/webscr";
		}

		public static HttpWebRequest GetPayPalWebRequest(this PayPalSettingsBase settings)
		{
			if (settings.SecurityProtocol.HasValue)
			{
				ServicePointManager.SecurityProtocol = settings.SecurityProtocol.Value;
			}

			var request = (HttpWebRequest)WebRequest.Create(GetPayPalUrl(settings));
			return request;
		}
	}
}