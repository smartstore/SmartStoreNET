using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

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
				.Action("ConfigurePlugin", "Plugin", new { systemName = GoogleMerchantCenterFeedPlugin.SystemName, area = "Admin" })
                .ToItem();

            root.Append(menuItem);
        }

    }
}
