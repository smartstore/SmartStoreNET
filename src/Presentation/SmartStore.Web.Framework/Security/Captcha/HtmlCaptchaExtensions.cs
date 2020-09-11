using System.Web;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Security
{
    public static class HtmlCaptchaExtensions
    {
        public static IHtmlString GenerateCaptcha(this HtmlHelper helper)
        {
            var captchaSettings = EngineContext.Current.Resolve<CaptchaSettings>();

            if (captchaSettings.CanDisplayCaptcha)
            {
                var workContext = EngineContext.Current.Resolve<IWorkContext>();
                var widgetUrl = CommonHelper.GetAppSetting<string>("g:RecaptchaWidgetUrl");
                var ident = CommonHelper.GenerateRandomDigitCode(5).ToString();
                var elementId = "recaptcha" + ident;
                var siteKey = captchaSettings.ReCaptchaPublicKey;
                var callbackName = "recaptchaOnload" + ident;

                var url = "{0}?onload={1}&render=explicit&hl={2}".FormatInvariant(
                    widgetUrl,
                    callbackName,
                    workContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToLower()
                );

                var script = new[]
                {
                    "<script>",
                    "	var {0} = function() {{".FormatInvariant(callbackName),
                    "		renderGoogleRecaptcha('{0}', '{1}', {2});".FormatInvariant(elementId, siteKey, captchaSettings.UseInvisibleReCaptcha.ToString().ToLower()),
                    "	};",
                    "</script>",
                    "<div id='{0}' class='g-recaptcha' data-sitekey='{1}'></div>".FormatInvariant(elementId, siteKey),
                    "<script src='{0}' async defer></script>".FormatInvariant(url),
                }.StrJoin("");

                return MvcHtmlString.Create(script);
            }

            return MvcHtmlString.Empty;
        }
    }
}
