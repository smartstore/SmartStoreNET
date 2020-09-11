using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
    public abstract class CatalogMenuProvider : IMenuProvider
    {
        public abstract void BuildMenu(TreeNode<MenuItem> rootNode);

        public string MenuName => "catalog";

        public virtual int Ordinal => 0;

    }
}
