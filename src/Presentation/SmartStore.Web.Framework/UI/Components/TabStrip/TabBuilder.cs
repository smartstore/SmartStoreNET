using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
    public class TabBuilder : NavigationItemtWithContentBuilder<Tab, TabBuilder>
    {
        public TabBuilder(Tab item, HtmlHelper htmlHelper)
            : base(item, htmlHelper)
        {
        }

        public TabBuilder Name(string value)
        {
            this.Item.Name = value;
            return this;
        }

        public TabBuilder Pull(TabPull value)
        {
            this.Item.Pull = value;
            return this;
        }
    }
}
