using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public static HtmlAttribute Css(this HtmlHelper html, string seperator, string value, bool condition)
        {
            return new HtmlAttribute("class", seperator).Add(value, condition);
        }

        public static HtmlAttribute Attr(this HtmlHelper html, string name, string value)
        {
            return Attr(html, name, value, true);
        }

        public static HtmlAttribute Attr(this HtmlHelper html, string name, string value, bool condition)
        {
            return Attr(html, name, null, value, condition);
        }

        public static HtmlAttribute Attr(this HtmlHelper html, string name, string seperator, string value, bool condition)
        {
            return new HtmlAttribute(name, seperator).Add(value, condition);
        }
    }
}
