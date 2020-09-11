using System;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{
    public static class ComponentRendererUtils
    {
        public static IDictionary<string, object> AppendCssClass(this IDictionary<string, object> attributes, Func<string> cssClass)
        {
            return attributes.AppendInValue("class", " ", cssClass());
        }

        public static IDictionary<string, object> PrependCssClass(this IDictionary<string, object> attributes, Func<string> cssClass)
        {
            return attributes.PrependInValue("class", " ", cssClass());
        }

        public static IDictionary<string, object> AppendCssClass(this IDictionary<string, object> attributes, string cssClass)
        {
            return attributes.AppendInValue("class", " ", cssClass);
        }

        public static IDictionary<string, object> PrependCssClass(this IDictionary<string, object> attributes, string cssClass)
        {
            return attributes.PrependInValue("class", " ", cssClass);
        }
    }
}
