using System;

namespace SmartStore.Web.Framework.UI
{
    public interface IUiComponent : IHtmlAttributesContainer
    {
        string Id
        {
            get;
        }

        string Name
        {
            get;
        }

        bool NameIsRequired
        {
            get;
        }
    }
}
