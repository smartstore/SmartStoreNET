using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;
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
			var routeName = string.Empty; // TODO: get from entity
			var routeValues = new RouteValueDictionary(); // TODO: deserialize dict from entity
			
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
}
