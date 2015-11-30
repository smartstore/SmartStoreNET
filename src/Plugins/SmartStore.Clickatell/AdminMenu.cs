using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using SmartStore.Web.Framework.UI;
using SmartStore.Collections;

namespace SmartStore.Clickatell
{
	public class AdminMenu : AdminMenuProvider
	{
		protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
		{
			var menuItem = new MenuItem().ToBuilder()
				.Text("Clickatell SMS Provider")
				.ResKey("Plugins.FriendlyName.Mobile.SMS.Clickatell")
				.Icon("send-o")
				.Action("ConfigurePlugin", "Plugin", new { systemName = "SmartStore.Clickatell", area = "Admin" })
				.ToItem();

			pluginsNode.Prepend(menuItem);
		}

		public override int Ordinal
		{
			get
			{
				return 100;
			}
		}

	}
}
