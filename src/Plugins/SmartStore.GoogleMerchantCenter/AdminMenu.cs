using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using SmartStore.Web.Framework.UI;
using SmartStore.Collections;

namespace SmartStore.GoogleMerchantCenter
{
    public class AdminMenu : AdminMenuProvider
    {
		protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
        {
			var root = pluginsNode.SelectNode(x => x.Value.Id == "promotion-feeds");
			if (root == null)
				return;
			
			var menuItem = new MenuItem().ToBuilder()
                .Text("Google Merchant Center")
				.ResKey("Plugins.FriendlyName.SmartStore.GoogleMerchantCenter")
				.Action("ConfigurePlugin", "Plugin", new { systemName = "SmartStore.GoogleMerchantCenter", area = "Admin" })
                .ToItem();

            root.Append(menuItem);
        }

    }
}
