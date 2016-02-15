using System.Text;
using System.Web.Mvc;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.UI.Captcha
{
	public static class HtmlExtensions
    {
        public static string GenerateCaptcha(this HtmlHelper helper)
        {
			var sb = new StringBuilder();
            var captchaSettings = EngineContext.Current.Resolve<CaptchaSettings>();
			var widgetUrl = CommonHelper.GetAppSetting<string>("g:RecaptchaWidgetUrl");
			var elementId = "GoogleRecaptchaWidget";

			sb.AppendLine("<script type=\"text/javascript\">");
			sb.AppendLine("var googleRecaptchaOnloadCallback = function() {");
			sb.AppendLine("  grecaptcha.render('{0}', {{".FormatInvariant(elementId));
			sb.AppendLine("    'sitekey' : '{0}'".FormatInvariant(captchaSettings.ReCaptchaPublicKey));
			sb.AppendLine("  });");
			sb.AppendLine("};");
			sb.AppendLine("</script>");
			sb.AppendLine("<div id=\"{0}\"></div>".FormatInvariant(elementId));
			sb.AppendLine("<script src=\"{0}?onload=googleRecaptchaOnloadCallback&render=explicit\" async defer></script>".FormatInvariant(widgetUrl));

			return sb.ToString();
        }
    }
}
