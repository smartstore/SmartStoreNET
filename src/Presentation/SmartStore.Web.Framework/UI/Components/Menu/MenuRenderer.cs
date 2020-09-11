using System.Web.Mvc.Html;
using System.Web.UI;

namespace SmartStore.Web.Framework.UI
{
    public class MenuRenderer : ComponentRenderer<Menu>
    {
        protected override void WriteHtmlCore(HtmlTextWriter writer)
        {
            var route = Component.Route;

            if (route != null)
            {
                HtmlHelper.RenderAction(route.Action, route.Controller, route.RouteValues);
            }
            else
            {
                HtmlHelper.RenderAction("Menu", "Menu", new
                {
                    area = "",
                    name = Component.Name,
                    template = Component.Template
                });
            }
        }

        public override void Render()
        {
            var c = Component;
            var route = c.Route;

            if (route != null)
            {
                HtmlHelper.RenderAction(route.Action, route.Controller, route.RouteValues);
            }
            else
            {
                HtmlHelper.RenderAction("Menu", "Menu", new { area = "", name = c.Name, template = c.Template });
            }
        }

        public override string ToHtmlString()
        {
            var c = Component;
            var route = c.Route;

            if (route != null)
            {
                return HtmlHelper.Action(route.Action, route.Controller, route.RouteValues).ToHtmlString();
            }
            else
            {
                return HtmlHelper.Action("Menu", "Menu", new { area = "", name = c.Name, template = c.Template }).ToHtmlString();
            }
        }
    }
}
