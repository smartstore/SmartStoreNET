using System;
using AmazonPay;
using AmazonPay.CommonRequests;
using SmartStore.AmazonPay.Settings;

namespace SmartStore.AmazonPay.Services
{
	/// <summary>
	/// Helper with utilities to keep the AmazonPayService tidy.
	/// </summary>
	public partial class AmazonPayService
	{
		/// <summary>
		/// Create API client.
		/// </summary>
		/// <param name="settings">AmazonPay settings</param>
		/// <param name="currencyCode">Currency code of primary store currency</param>
		/// <returns>AmazonPay client</returns>
		private Client CreateClient(AmazonPaySettings settings, string currencyCode = null)
		{
			var descriptor = _pluginFinder.GetPluginDescriptorBySystemName(AmazonPayPlugin.SystemName);
			var appVersion = descriptor != null ? descriptor.Version.ToString() : "1.0";

			Regions.supportedRegions region;
			switch (settings.Marketplace.EmptyNull().ToLower())
			{
				case "us":
					region = Regions.supportedRegions.us;
					break;
				case "uk":
					region = Regions.supportedRegions.uk;
					break;
				case "jp":
					region = Regions.supportedRegions.jp;
					break;
				default:
					region = Regions.supportedRegions.de;
					break;
			}

			var config = new Configuration()
				.WithAccessKey(settings.AccessKey)
				.WithClientId(settings.ClientId)
				.WithSandbox(settings.UseSandbox)
				.WithApplicationName("SmartStore.Net " + AmazonPayPlugin.SystemName)
				.WithApplicationVersion(appVersion)
				.WithRegion(region);

			if (currencyCode.HasValue())
			{
				Regions.currencyCode currency;

				switch (currencyCode.ToLower())
				{
					case "usd":
						currency = Regions.currencyCode.USD;
						break;
					case "gbp":
						currency = Regions.currencyCode.GBP;
						break;
					case "jpy":
						currency = Regions.currencyCode.JPY;
						break;
					default:
						currency = Regions.currencyCode.EUR;
						break;
				}

				config = config.WithCurrencyCode(currency);
			}

			var client = new Client(config);
			return client;
		}
	}
}