using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc.Html;
using SmartStore.Collections;
using SmartStore.Core.Infrastructure;

namespace SmartStore.Web.Framework.UI
{
    public class MenuRenderer : ViewBasedComponentRenderer<Menu>
    {
        protected IMenuService _menuService;

        public MenuRenderer()
            : this(EngineContext.Current.Resolve<IMenuService>())
        {
        }

        public MenuRenderer(IMenuService menuService)
        {
            _menuService = menuService;
        }

        public override void Render()
        {
            var model = CreateModel();
            HtmlHelper.RenderPartial(GetViewName(), model);
        }

        public override string ToHtmlString()
        {
            var model = CreateModel();
            return HtmlHelper.Partial(GetViewName(), model).ToString();
        }

        protected virtual MenuModel CreateModel()
        {
            var model = new MenuModel { Name = Component.Name };
            var menu = _menuService.GetMenu(Component.Name);

            if (menu != null)
            {
                model.Root = menu.Root;

                var currentNode = menu.ResolveCurrentNode(HtmlHelper.ViewContext?.Controller?.ControllerContext);

                model.Path = currentNode != null
                    ? currentNode.Trail.Where(x => !x.IsRoot).ToList()
                    : new List<TreeNode<MenuItem>>();

                if (Component.ResolveCounts)
                {
                    menu.ResolveElementCounts(model.SelectedNode, false);
                }
            }

            return model;
        }

        protected override string GetViewName()
        {
            return Component.ViewName.NullEmpty() ?? ("Menus/" + Component.Name);
        }
    }
}
