using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using SmartStore.Web.Framework.UI;
using SmartStore.Collections;

namespace SmartStore.DevTools
{
	public class AdminMenu : AdminMenuProvider
	{
		protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
		{
			var menuItem = new MenuItem().ToBuilder()
				.Text("Developer Tools")
				.Icon("code")
				.Action("ConfigurePlugin", "Plugin", new { systemName = "SmartStore.DevTools", area = "Admin" })
				.ToItem();

			pluginsNode.Prepend(menuItem);
		}
	}
}
