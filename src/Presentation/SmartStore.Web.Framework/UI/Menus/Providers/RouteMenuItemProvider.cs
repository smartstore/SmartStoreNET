using System.Web.Routing;
using Newtonsoft.Json;
using SmartStore.Collections;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Plugins;

namespace SmartStore.Web.Framework.UI
{
    [SystemName("route")]
	public class RouteMenuItemProvider : MenuItemProviderBase
	{
		protected override void ApplyLink(TreeNode<MenuItem> node, MenuItemRecord entity)
		{
            if (entity.Model.IsEmpty())
            {
                return;
            }

            try
            {
                var routeValues = JsonConvert.DeserializeObject<RouteValueDictionary>(entity.Model);
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
            catch { }			
		}
	}
}
