using System.Web.Mvc;

namespace SmartStore.Web.Framework.UI
{
    public class MenuBuilder<TModel> : ComponentBuilder<Menu, MenuBuilder<TModel>, TModel>
    {
        public MenuBuilder(Menu component, HtmlHelper<TModel> htmlHelper)
            : base(component, htmlHelper)
        {
        }

        public MenuBuilder<TModel> Template(string value)
        {
            Component.Route = null;
            Component.Template = value;
            return this;
        }

        public MenuBuilder<TModel> Template(string action, string controller, object routeValues = null)
        {
            Component.Template = null;
            Component.Route = new RouteInfo(action, controller, routeValues);
            return this;
        }
    }
}
