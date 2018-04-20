using System;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{    
    public static class ComponentRendererUtils
    {
        public static void AppendCssClass(this IDictionary<string, object> attributes, string @class)
        {
            attributes.AppendInValue("class", " ", @class);
        }

        public static void PrependCssClass(this IDictionary<string, object> attributes, string @class)
        {
            attributes.PrependInValue("class", " ", @class);
        }
    }
}
