using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services.Localization;

namespace SmartStore.Clickatell
{
	public class ClickatellSmsProvider : BasePlugin, IConfigurable
    {
        private readonly ClickatellSettings _clickatellSettings;
        private readonly ILocalizationService _localizationService;
		private readonly ILogger _logger;

		public ClickatellSmsProvider(
			ClickatellSettings clickatellSettings,
            ILocalizationService localizationService,
			ILogger logger)
        {
            _clickatellSettings = clickatellSettings;
            _localizationService = localizationService;
			_logger = logger;			
		}

		public static string SystemName
		{
			get
			{
				return "SmartStore.Clickatell";
			}
		}

		public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "SmsClickatell";
			routeValues = new RouteValueDictionary { { "area", SystemName } };
        }

        public override void Install()
        {
            _localizationService.ImportPluginResourcesFromXml(PluginDescriptor);

            base.Install();
        }

        public override void Uninstall()
        {
            _localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

            base.Uninstall();
        }

		public void SendSms(string text)
		{
			if (text.IsEmpty())
				return;

			string error = null;
			HttpWebResponse webResponse = null;

			try
			{
				// https://www.clickatell.com/developers/api-documentation/rest-api-request-parameters/
				var request = (HttpWebRequest)WebRequest.Create("https://platform.clickatell.com/messages");
				request.Method = "POST";
				request.Accept = "application/json";
				request.ContentType = "application/json";
				request.Headers["Authorization"] = _clickatellSettings.ApiId;

				var data = new Dictionary<string, object>
				{
					{ "content", text },
					{ "to", _clickatellSettings.PhoneNumber.SplitSafe(";") }
				};

				var json = JsonConvert.SerializeObject(data);

				// UTF8 is default encoding
				var bytes = Encoding.UTF8.GetBytes(json);
				request.ContentLength = bytes.Length;

				using (var stream = request.GetRequestStream())
				{
					stream.Write(bytes, 0, bytes.Length);
				}

				webResponse = request.GetResponse() as HttpWebResponse;
			}
			catch (WebException wexc)
			{
				webResponse = wexc.Response as HttpWebResponse;
			}
			catch (Exception exception)
			{
				error = exception.ToString();
			}

			if (webResponse != null)
			{
				using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
				{
					var rawResponse = reader.ReadToEnd();

					if (webResponse.StatusCode == HttpStatusCode.OK || webResponse.StatusCode == HttpStatusCode.Accepted)
					{
						dynamic response = JObject.Parse(rawResponse);

						error = (string)response.error;
						if (error.IsEmpty() && response.messages != null)
							error = response.messages[0].error;
					}
					else
					{
						error = rawResponse;
					}
				}
			}

			if (error.HasValue())
			{
				_logger.Error(error);
				throw new SmartException(error);
			}
		}
	}
}
