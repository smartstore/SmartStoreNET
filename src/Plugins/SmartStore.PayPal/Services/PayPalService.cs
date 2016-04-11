using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SmartStore.Core.Domain.Logging;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Stores;
using SmartStore.Core.Localization;
using SmartStore.Core.Logging;
using SmartStore.PayPal.Settings;
using SmartStore.Services;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;

namespace SmartStore.PayPal.Services
{
	public class PayPalService : IPayPalService
	{
		private readonly ICommonServices _services;
		private readonly IOrderService _orderService;
		private readonly Lazy<IPictureService> _pictureService;

		public PayPalService(
			ICommonServices services,
			IOrderService orderService,
			Lazy<IPictureService> pictureService)
		{
			_services = services;
			_orderService = orderService;
			_pictureService = pictureService;

			T = NullLocalizer.Instance;
			Logger = NullLogger.Instance;
		}

		public Localizer T { get; set; }
		public ILogger Logger { get; set; }

		public static string GetApiUrl(bool sandbox)
		{
			return sandbox ? "https://api.sandbox.paypal.com" : "https://api.paypal.com";
		}

		public void AddOrderNote(PayPalSettingsBase settings, Order order, string anyString)
		{
			try
			{
				if (order == null || anyString.IsEmpty() || (settings != null && !settings.AddOrderNotes))
					return;

				string[] orderNoteStrings = T("Plugins.SmartStore.PayPal.OrderNoteStrings").Text.SplitSafe(";");
				var faviconUrl = "{0}Plugins/{1}/Content/favicon.png".FormatInvariant(_services.WebHelper.GetStoreLocation(false), Plugin.SystemName);

				var sb = new StringBuilder();
				sb.AppendFormat("<img src=\"{0}\" style=\"float: left; width: 16px; height: 16px;\" />", faviconUrl);

				var note = orderNoteStrings.SafeGet(0).FormatInvariant(anyString);

				sb.AppendFormat("<span style=\"padding-left: 4px;\">{0}</span>", note);

				_orderService.AddOrderNote(order, sb.ToString());
			}
			catch { }
		}

		public void LogError(Exception exception, string shortMessage = null, string fullMessage = null, bool notify = false, IList<string> errors = null, bool isWarning = false)
		{
			try
			{
				if (exception != null)
				{
					shortMessage = exception.ToAllMessages();
					fullMessage = exception.ToString();
				}

				if (shortMessage.HasValue())
				{
					shortMessage = "PayPal. " + shortMessage;
					Logger.InsertLog(isWarning ? LogLevel.Warning : LogLevel.Error, shortMessage, fullMessage.EmptyNull());

					if (notify)
					{
						if (isWarning)
							_services.Notifier.Warning(new LocalizedString(shortMessage));
						else
							_services.Notifier.Error(new LocalizedString(shortMessage));
					}
				}
			}
			catch (Exception) { }

			if (errors != null && shortMessage.HasValue())
			{
				errors.Add(shortMessage);
			}
		}

		public PayPalResponse CallApi(string method, string path, string accessToken, PayPalApiSettingsBase settings, string data, IList<string> errors = null)
		{
			var encoding = Encoding.UTF8;
			var result = new PayPalResponse();
			HttpWebResponse webResponse = null;

			var url = GetApiUrl(settings.UseSandbox) + path.EnsureStartsWith("/");

			if (method.IsCaseInsensitiveEqual("GET") && data.HasValue())
			{
				url = url.EnsureEndsWith("?") + data;
			}

			if (settings.SecurityProtocol.HasValue)
			{
				ServicePointManager.SecurityProtocol = settings.SecurityProtocol.Value;
			}

			var request = (HttpWebRequest)WebRequest.Create(url);
			request.Method = method;
			request.Accept = "application/json";
			request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";

			if (path.EmptyNull().EndsWith("/token"))
			{
				request.Credentials = new NetworkCredential(settings.ClientId, settings.Secret);
			}
			else
			{
				request.Headers["Authorization"] = "Bearer " + accessToken.EmptyNull();
			}


			if ((method.IsCaseInsensitiveEqual("POST") || method.IsCaseInsensitiveEqual("PATCH")) && data.HasValue())
			{
				byte[] bytes = encoding.GetBytes(data);

				request.ContentLength = bytes.Length;

				using (var stream = request.GetRequestStream())
				{
					stream.Write(bytes, 0, bytes.Length);
				}
			}
			else
			{
				request.ContentLength = 0;
			}

			try
			{
				webResponse = request.GetResponse() as HttpWebResponse;
				result.Success = (webResponse.StatusCode == HttpStatusCode.OK);
			}
			catch (WebException wexc)
			{
				result.Success = false;
				webResponse = wexc.Response as HttpWebResponse;
			}
			catch (Exception exception)
			{
				result.Success = false;
				LogError(exception, errors: errors);
			}

			try
			{
				if (webResponse != null)
				{
					using (var reader = new StreamReader(webResponse.GetResponseStream(), encoding))
					{
						var rawResponse = reader.ReadToEnd();
						if (rawResponse.HasValue())
						{
							if (rawResponse.StartsWith("["))
								result.Json = JArray.Parse(rawResponse);
							else
								result.Json = JObject.Parse(rawResponse);

							if (result.Json != null)
							{
								if (!result.Success)
								{
									result.ErrorName = (string)result.Json.name;
									result.ErrorMessage = (string)result.Json.message;

									var details = (string)result.Json.details;
									if (details.IsEmpty())
										details = webResponse.StatusDescription;

									if (details.HasValue())
										result.ErrorMessage = result.ErrorMessage.EnsureEndsWith(".").Grow(details, " ");

									LogError(null, result.ErrorName, result.ErrorMessage, false, errors);
								}
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				LogError(exception, errors: errors);
			}
			finally
			{
				if (webResponse != null)
				{
					webResponse.Close();
					webResponse.Dispose();
				}
			}

			return result;
		}

		public bool EnsureAccessToken(PayPalSessionData session, PayPalApiSettingsBase settings)
		{
			if (session.AccessToken.IsEmpty() || DateTime.UtcNow >= session.TokenExpiration)
			{
				var data = new Dictionary<string, object>();
				data.Add("grant_type", "client_credentials");

				var result = CallApi("POST", "/v1/oauth2/token", null, settings, JsonConvert.SerializeObject(data));

				if (result.Success)
				{
					session.AccessToken = (string)result.Json.access_token;

					var expireSeconds = ((string)result.Json.expires_in).ToInt(5 * 60);

					session.TokenExpiration = DateTime.UtcNow.AddSeconds(expireSeconds);
				}
			}

			return session.AccessToken.HasValue();
		}

		public PayPalResponse SetCheckoutExperience(PayPalApiSettingsBase settings, Store store)
		{
			var session = new PayPalSessionData();
			if (!EnsureAccessToken(session, settings))
				return null;

			var logo = _pictureService.Value.GetPictureById(store.LogoPictureId);

			var data = new Dictionary<string, object>();
			var presentation = new Dictionary<string, object>();
			var inpuFields = new Dictionary<string, object>();

			presentation.Add("brand_name", store.Name);
			presentation.Add("locale_code", _services.WorkContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToUpper());

			if (logo != null)
				presentation.Add("logo_image", _pictureService.Value.GetPictureUrl(logo, showDefaultPicture: false, storeLocation: store.Url));

			inpuFields.Add("allow_note", false);
			inpuFields.Add("no_shipping", 0);
			inpuFields.Add("address_override", 1);

			data.Add("name", store.Name);
			data.Add("presentation", presentation);
			data.Add("input_fields", inpuFields);

			var result = CallApi("POST", "/v1/payment-experience/webprofiles", session.AccessToken, settings, JsonConvert.SerializeObject(data));

			if (result.Success)
			{

				//return (string)result.Json.id;
			}

			return result;
		}
	}


	public class PayPalResponse
	{
		public bool Success { get; set; }
		public dynamic Json { get; set; }
		public string ErrorName { get; set; }
		public string ErrorMessage { get; set; }
	}

	public class PayPalSessionData
	{
		public string AccessToken { get; set; }
		public DateTime TokenExpiration { get; set; }
	}

}