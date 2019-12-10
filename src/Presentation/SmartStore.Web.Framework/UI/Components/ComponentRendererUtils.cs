using System;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{    
    public static class ComponentRendererUtils
    {
        public static IDictionary<string, object> AppendCssClass(this IDictionary<string, object> attributes, string @class)
        {
            return attributes.AppendInValue("class", " ", @class);
        }

        public static IDictionary<string, object> PrependCssClass(this IDictionary<string, object> attributes, string @class)
        {
            return attributes.PrependInValue("class", " ", @class);
        }
    }
}
