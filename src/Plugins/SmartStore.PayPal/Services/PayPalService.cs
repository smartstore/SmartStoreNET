using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
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
			var encoding = Encoding.ASCII;
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
			request.ContentType = "application/x-www-form-urlencoded";

			if (HttpContext.Current != null && HttpContext.Current.Request != null)
			{
				request.UserAgent = HttpContext.Current.Request.UserAgent;
			}
			else
			{
				request.UserAgent = Plugin.SystemName;
			}

			if (path.EmptyNull().EndsWith("/token"))
			{
				// see https://github.com/paypal/sdk-core-dotnet/blob/master/Source/SDK/OAuthTokenCredential.cs
				byte[] credentials = Encoding.UTF8.GetBytes("{0}:{1}".FormatInvariant(settings.ClientId, settings.Secret));

				request.Headers.Add("Authorization", "Basic " + Convert.ToBase64String(credentials));
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
					using (var reader = new StreamReader(webResponse.GetResponseStream(), Encoding.UTF8))
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
									var name = (string)result.Json.name;
									var message = (string)result.Json.message;

									if (name.IsEmpty())
										name = (string)result.Json.error;

									if (message.IsEmpty())
										message = (string)result.Json.error_description;

									if (message.IsEmpty())
										message = webResponse.StatusDescription;

									result.ErrorMessage = "{0} ({1}).".FormatInvariant(message.NaIfEmpty(), name.NaIfEmpty());

									LogError(null, result.ErrorMessage, result.Json.ToString(), false, errors);
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

		public PayPalResponse EnsureAccessToken(PayPalSessionData session, PayPalApiSettingsBase settings)
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
				else
				{
					return result;
				}
			}

			return new PayPalResponse
			{
				Success = session.AccessToken.HasValue()
			};
		}

		public PayPalResponse SetCheckoutExperience(PayPalApiSettingsBase settings, Store store)
		{
			var session = new PayPalSessionData();
			var result = EnsureAccessToken(session, settings);

			if (!result.Success)
				return result;

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

			result = CallApi("POST", "/v1/payment-experience/webprofiles", session.AccessToken, settings, JsonConvert.SerializeObject(data));

			return result;
		}
	}


	public class PayPalResponse
	{
		public bool Success { get; set; }
		public dynamic Json { get; set; }
		public string ErrorMessage { get; set; }
	}

	public class PayPalSessionData
	{
		public string AccessToken { get; set; }
		public DateTime TokenExpiration { get; set; }
	}

}