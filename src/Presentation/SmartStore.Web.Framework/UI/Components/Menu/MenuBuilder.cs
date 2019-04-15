using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
    public class MenuBuilder<TModel> : ComponentBuilder<Menu, MenuBuilder<TModel>, TModel>
    {
        public MenuBuilder(Menu component, HtmlHelper<TModel> htmlHelper)
            : base(component, htmlHelper)
        {
        }

        public MenuBuilder<TModel> ViewName(string value)
        {
            Component.ViewName = value;
            return this;
        }

        public MenuBuilder<TModel> ResolveCounts(bool value)
        {
            Component.ResolveCounts = value;
            return this;
        }
    }
}
