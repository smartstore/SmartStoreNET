using SmartStore.Collections;
using SmartStore.Core.Localization;

namespace SmartStore.Web.Framework.UI
{
    [MenuItemProvider("route")]
    public class RouteMenuItemProvider : MenuItemProviderBase
    {
        public RouteMenuItemProvider()
        {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override void ApplyLink(MenuItemProviderRequest request, TreeNode<MenuItem> node)
        {
            try
            {
                node.ApplyRouteData(request.Entity.Model);
            }
            catch { }

            if (request.IsEditMode)
            {
                var item = node.Value;

                item.Summary = T("Providers.MenuItems.FriendlyName.Route");
                item.Icon = "fas fa-directions";

                if (!item.HasRoute())
                {
                    item.Text = null;
                    item.ResKey = "Admin.ContentManagement.Menus.SpecifyLinkTarget";
                }
            }
        }
    }
}
