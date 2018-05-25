using System.Text;
using System.Web.Mvc;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Utilities;

namespace SmartStore.Web.Framework.Security
{
	public static class HtmlCaptchaExtensions
	{
        public static string GenerateCaptcha(this HtmlHelper helper)
        {
			var sb = new StringBuilder();
            var captchaSettings = EngineContext.Current.Resolve<CaptchaSettings>();
			var workContext = EngineContext.Current.Resolve<IWorkContext>();
			var widgetUrl = CommonHelper.GetAppSetting<string>("g:RecaptchaWidgetUrl");
			var elementId = "GoogleRecaptchaWidget";

			var url = "{0}?onload=googleRecaptchaOnloadCallback&render=explicit&hl={1}".FormatInvariant(
				widgetUrl,
				workContext.WorkingLanguage.UniqueSeoCode.EmptyNull().ToLower()
			);

			sb.AppendLine("<script type=\"text/javascript\">");
			sb.AppendLine("var googleRecaptchaOnloadCallback = function() {");
			sb.AppendLine("  grecaptcha.render('{0}', {{".FormatInvariant(elementId));
			sb.AppendLine("    'sitekey' : '{0}'".FormatInvariant(captchaSettings.ReCaptchaPublicKey));
			sb.AppendLine("  });");
			sb.AppendLine("};");
			sb.AppendLine("</script>");
			sb.AppendLine("<div id=\"{0}\"></div>".FormatInvariant(elementId));
			sb.AppendLine("<script src=\"{0}\" async defer></script>".FormatInvariant(url));

			return sb.ToString();
        }
    }
}
