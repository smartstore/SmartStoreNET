using OffAmazonPaymentsService;
using SmartStore.AmazonPay.Settings;

namespace SmartStore.AmazonPay.Services
{
	public class AmazonPayApiClient
	{
		public AmazonPayApiClient(AmazonPaySettings settings, string appVersion)
		{
			var appName = "SmartStore.Net " + AmazonPayPlugin.SystemName;
			var config = new OffAmazonPaymentsServiceConfig();

			config.ServiceURL = settings.UseSandbox
				? "https://mws-eu.amazonservices.com/OffAmazonPayments_Sandbox/2013-01-01/"
				: "https://mws-eu.amazonservices.com/OffAmazonPayments/2013-01-01/";

			config.SetUserAgent(appName, appVersion ?? "1.0");

			Settings = settings;
			Service = new OffAmazonPaymentsServiceClient(appName, appVersion ?? "1.0", settings.AccessKey, settings.SecretKey, config);
		}

		public IOffAmazonPaymentsService Service { get; private set; }
		public AmazonPaySettings Settings { get; private set; }
	}
}