using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{
    public interface IHtmlAttributesContainer
    {
        IDictionary<string, object> HtmlAttributes
        {
            get;
        }
    }
}
