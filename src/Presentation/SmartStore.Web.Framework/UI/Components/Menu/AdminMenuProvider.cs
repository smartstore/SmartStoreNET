using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
    public abstract class AdminMenuProvider : IMenuProvider
    {
        public void BuildMenu(TreeNode<MenuItem> rootNode)
        {
            var pluginsNode = rootNode.SelectNodeById("plugins");
            BuildMenuCore(pluginsNode);
        }

        protected abstract void BuildMenuCore(TreeNode<MenuItem> pluginsNode);

        public string MenuName => "admin";

        public virtual int Ordinal => 0;

    }
}
