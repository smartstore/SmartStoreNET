using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.GoogleMerchantCenter
{
    public class AdminMenu : AdminMenuProvider
    {
		protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
        {
			var menuItem = new MenuItem().ToBuilder()
                .Text("Google Merchant Center")
                .Icon("google")
                .ResKey("Plugins.FriendlyName.SmartStore.GoogleMerchantCenter")
				.Action("ConfigurePlugin", "Plugin", new { systemName = GoogleMerchantCenterFeedPlugin.SystemName, area = "Admin" })
                .ToItem();

            pluginsNode.Prepend(menuItem);
        }

    }
}
