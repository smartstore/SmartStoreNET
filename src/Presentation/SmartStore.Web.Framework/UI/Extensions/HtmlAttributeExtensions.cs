using System.Web.Mvc;

namespace SmartStore.Web.Framework
{
    public static class HtmlAttributeExtensions
    {
        public static HtmlAttribute Css(this HtmlHelper html, string value)
        {
            return Css(html, value, true);
        }

        public static HtmlAttribute Css(this HtmlHelper html, string value, bool condition)
        {
            return Css(html, null, value, condition);
        }

        public static HtmlAttribute Css(this HtmlHelper html, string separator, string value, bool condition)
        {
            return new HtmlAttribute("class", separator).Add(value, condition);
        }

        public static HtmlAttribute Attr(this HtmlHelper html, string name, string value)
        {
            return Attr(html, name, value, true);
        }

        public static HtmlAttribute Attr(this HtmlHelper html, string name, string value, bool condition)
        {
            return Attr(html, name, null, value, condition);
        }

        public static HtmlAttribute Attr(this HtmlHelper html, string name, string separator, string value, bool condition)
        {
            return new HtmlAttribute(name, separator).Add(value, condition);
        }
    }
}
