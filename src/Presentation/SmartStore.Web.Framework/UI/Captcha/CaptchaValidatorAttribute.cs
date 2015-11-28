using System;
using System.Web.Mvc;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.UI.Captcha
{
    public class CaptchaValidatorAttribute : ActionFilterAttribute
    {
        private const string CHALLENGE_FIELD_KEY = "recaptcha_challenge_field";
        private const string RESPONSE_FIELD_KEY = "recaptcha_response_field";

		public Lazy<CaptchaSettings> CaptchaSettings { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            bool valid = false;
            var captchaChallengeValue = filterContext.HttpContext.Request.Form[CHALLENGE_FIELD_KEY];
            var captchaResponseValue = filterContext.HttpContext.Request.Form[RESPONSE_FIELD_KEY];
            if (!string.IsNullOrEmpty(captchaChallengeValue) && !string.IsNullOrEmpty(captchaResponseValue))
            {
				var captchaSettings = CaptchaSettings.Value;
                if (captchaSettings.Enabled)
                {
                    //validate captcha
                    var captchaValidtor = new Recaptcha.RecaptchaValidator
                    {
                        PrivateKey = captchaSettings.ReCaptchaPrivateKey,
                        RemoteIP = filterContext.HttpContext.Request.UserHostAddress,
                        Challenge = captchaChallengeValue,
                        Response = captchaResponseValue
                    };

                    var recaptchaResponse = captchaValidtor.Validate();
                    valid = recaptchaResponse.IsValid;
                }
            }

            //this will push the result value into a parameter in our Action  
            filterContext.ActionParameters["captchaValid"] = valid;

            base.OnActionExecuting(filterContext);
        }
    }
}
