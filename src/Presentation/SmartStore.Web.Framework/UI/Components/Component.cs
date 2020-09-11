using System.Collections.Generic;
using System.Web.Routing;

namespace SmartStore.Web.Framework.UI
{
    public abstract class Component : IUiComponent
    {
        protected Component()
        {
            this.HtmlAttributes = new RouteValueDictionary();
        }

        public string Id
        {
            get => !this.HtmlAttributes.ContainsKey("id")
                    ? this.Name
                    : (this.HtmlAttributes["id"] as string);
            set => this.HtmlAttributes["id"] = value;
        }

        public string Name
        {
            get;
            set;
        }

        public IDictionary<string, object> HtmlAttributes
        {
            get;
            private set;
        }



        public virtual bool NameIsRequired => false;
    }
}