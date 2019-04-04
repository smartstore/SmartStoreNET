using System.Web.Routing;
using Newtonsoft.Json;
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
            if (request.Origin.IsCaseInsensitiveEqual("EditMenu"))
            {
                node.Value.BadgeText = T("Providers.MenuItems.FriendlyName.Route");
                node.Value.Icon = "fas fa-directions";
            }

            try
            {
                if (request.Entity.Model.HasValue())
                {
                    var routeValues = JsonConvert.DeserializeObject<RouteValueDictionary>(request.Entity.Model);
                    var routeName = string.Empty;

                    if (routeValues.TryGetValue("routename", out var val))
                    {
                        routeName = val as string;
                        routeValues.Remove("routename");
                    }

                    if (routeName.HasValue())
                    {
                        node.Value.Route(routeName, routeValues);
                    }
                    else
                    {
                        node.Value.Action(routeValues);
                    }
                }
            }
            catch { }			
		}
	}
}
