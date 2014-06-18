using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Routing;
using System.Web.Mvc;
using SmartStore.Web.Framework.UI;
using SmartStore.Collections;

namespace SmartStore.Plugin.Developer.DevTools
{
    public class AdminMenu : IMenuProvider
    {
        public void BuildMenu(TreeNode<MenuItem> pluginsNode)
        {
            var menuItem = new MenuItem().ToBuilder()
                .Text("DevTools Plugin")
				.Action("Index", "DevTools Admin", new { area = "Developer.DevTools" })
                .ToItem();

            pluginsNode.Append(menuItem);
        }

    }
}
