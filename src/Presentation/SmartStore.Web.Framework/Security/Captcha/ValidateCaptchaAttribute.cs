using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Mvc;
using SmartStore.Core.Logging;
using SmartStore.Services.Localization;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Security
{
	public class ValidateCaptchaAttribute : ActionFilterAttribute
    {
		public ValidateCaptchaAttribute()
		{
			Logger = NullLogger.Instance;
		}

		public Lazy<CaptchaSettings> CaptchaSettings { get; set; }
		public ILogger Logger { get; set; }
		public Lazy<ILocalizationService> LocalizationService { get; set; }

		public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var valid = false;

			try
			{
				var captchaSettings = CaptchaSettings.Value;
				if (captchaSettings.Enabled && captchaSettings.ReCaptchaPrivateKey.HasValue())
				{
					var verifyUrl = CommonHelper.GetAppSetting<string>("g:RecaptchaVerifyUrl");
					var recaptchaResponse = filterContext.HttpContext.Request.Form["g-recaptcha-response"];

					var url = "{0}?secret={1}&response={2}".FormatInvariant(
						verifyUrl,
						HttpUtility.UrlEncode(captchaSettings.ReCaptchaPrivateKey),
						HttpUtility.UrlEncode(recaptchaResponse)
					);

					using (var client = new WebClient())
					{
						var jsonResponse = client.DownloadString(url);
						using (var memoryStream = new MemoryStream(Encoding.Unicode.GetBytes(jsonResponse)))
						{
							var serializer = new DataContractJsonSerializer(typeof(GoogleRecaptchaApiResponse));
							var result = serializer.ReadObject(memoryStream) as GoogleRecaptchaApiResponse;

							if (result == null)
							{
								Logger.Error(LocalizationService.Value.GetResource("Common.CaptchaUnableToVerify"));
							}
							else
							{
								if (result.ErrorCodes == null)
								{
									valid = result.Success;
								}
							}
						}
					}
				}
			}
			catch (Exception exception)
			{
				Logger.ErrorsAll(exception);
			}

			// this will push the result value into a parameter in our Action  
			filterContext.ActionParameters["captchaValid"] = valid;

            base.OnActionExecuting(filterContext);
        }
    }


	[DataContract]
	public class GoogleRecaptchaApiResponse
	{
		[DataMember(Name = "success")]
		public bool Success { get; set; }

		[DataMember(Name = "error-codes")]
		public List<string> ErrorCodes { get; set; }
	}
}
