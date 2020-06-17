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
                node.Value.Summary = T("Providers.MenuItems.FriendlyName.Route");
                node.Value.Icon = "fas fa-directions";

                if (!node.Value.HasRoute())
                {
                    node.Value.Text = null;
                    node.Value.ResKey = "Admin.ContentManagement.Menus.SpecifyLinkTarget";
                }
            }
        }
    }
}
