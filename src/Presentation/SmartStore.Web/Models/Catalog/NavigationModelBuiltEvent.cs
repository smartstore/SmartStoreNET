using System;
using System.Collections.Generic;
using SmartStore.Collections;
using SmartStore.Web.Framework.UI;

namespace SmartStore.Web.Models.Catalog
{
    public class NavigationModelBuiltEvent
    {
        public NavigationModelBuiltEvent(TreeNode<MenuItem> rootNode)
        {
            this.RootNode = rootNode;
        }

		public TreeNode<MenuItem> RootNode { get; private set; }
    }
}