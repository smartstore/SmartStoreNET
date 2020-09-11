using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace SmartStore.Web.Framework.Security
{
    public static class HtmlHoneypotExtensions
    {
        public static MvcHtmlString HoneypotField(this HtmlHelper html)
        {
            var token = Honeypot.CreateToken();
            var serializedToken = Honeypot.SerializeToken(token);

            var textField = html.TextBox(token.Name, string.Empty, new { @class = "required-text-input", autocomplete = "off" }).ToHtmlString();
            var hiddenField = html.Hidden(Honeypot.TokenFieldName, serializedToken).ToHtmlString();

            return MvcHtmlString.Create(textField + hiddenField);
        }
    }
}
