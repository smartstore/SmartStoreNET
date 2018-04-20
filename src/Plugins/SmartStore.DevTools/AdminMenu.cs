using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.DevTools
{
	public class AdminMenu : AdminMenuProvider
	{
		protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
		{
			var menuItem = new MenuItem().ToBuilder()
				.Text("Developer Tools")
				.Icon("terminal")
				.Action("ConfigurePlugin", "Plugin", new { systemName = "SmartStore.DevTools", area = "Admin" })
				.ToItem();
			
			pluginsNode.Prepend(menuItem);

			// uncomment to add to admin menu (see plugin sub-menu)
			//var backendExtensionItem = new MenuItem().ToBuilder()
			//	.Text("Backend extension")
			//	.Icon("area-chart")
			//	.Action("BackendExtension", "DevTools", new { area = "SmartStore.DevTools" })
			//	.ToItem();
			//pluginsNode.Append(backendExtensionItem);

			// uncomment to add a sub-menu (see plugin sub-menu)
			//var subMenu = new MenuItem().ToBuilder()
			//	.Text("Sub Menu")
			//	.Action("BackendExtension", "DevTools", new { area = "SmartStore.DevTools" })
			//	.ToItem();
			//var subMenuNode = pluginsNode.Append(subMenu);

			//var subMenuItem = new MenuItem().ToBuilder()
			//	.Text("Sub Menu Item 1")
			//	.Action("BackendExtension", "DevTools", new { area = "SmartStore.DevTools" })
			//	.ToItem();
			//subMenuNode.Append(subMenuItem);
		}
	}
}
