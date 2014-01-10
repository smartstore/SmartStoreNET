using System;
using System.Collections.Generic;
using SmartStore.Collections;

namespace SmartStore.Web.Models.Catalog
{
    public class NavigationModelBuiltEvent
    {
        public NavigationModelBuiltEvent(TreeNode<CategoryNavigationModel.CategoryModel> rootNode)
        {
            this.RootNode = rootNode;
        }

        public TreeNode<CategoryNavigationModel.CategoryModel> RootNode { get; private set; }
    }
}