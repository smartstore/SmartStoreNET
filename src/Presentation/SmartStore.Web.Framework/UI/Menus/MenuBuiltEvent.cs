using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
    public class MenuBuiltEvent
    {
        public MenuBuiltEvent(string name, TreeNode<MenuItem> root)
        {
            Name = name;
            Root = root;
        }

        public string Name { get; private set; }
        public TreeNode<MenuItem> Root { get; private set; }
    }
}
