using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
    
    public interface IMenuProvider
    {
        void BuildMenu(TreeNode<MenuItem> pluginsNode);

    }

}
