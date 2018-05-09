﻿using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Clickatell
{
	public class AdminMenu : AdminMenuProvider
	{
		protected override void BuildMenuCore(TreeNode<MenuItem> pluginsNode)
		{
			var menuItem = new MenuItem().ToBuilder()
				.Text("Clickatell SMS Provider")
				.ResKey("Plugins.FriendlyName.SmartStore.Clickatell")
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
