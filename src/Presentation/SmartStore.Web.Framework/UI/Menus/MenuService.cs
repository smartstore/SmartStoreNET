using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI.Menus
{
    public partial class MenuService : IMenuService
    {
        protected readonly IEnumerable<IMenuResolver> _menuResolvers;

        private TreeNode<MenuItem> _currentNode;
        private bool _currentNodeResolved;

        public MenuService(IEnumerable<IMenuResolver> menuResolvers)
        {
            _menuResolvers = menuResolvers;
        }

        public virtual IMenu GetMenu(string name)
        {
            if (name.HasValue())
            {
                foreach (var resolver in _menuResolvers)
                {
                    if (resolver.Exists(name))
                    {
                        return resolver.Resolve(name);
                    }
                }
            }

            return null;
        }

        public virtual TreeNode<MenuItem> GetRootNode(string menuName)
        {
            return GetMenu(menuName)?.Root;
        }

        public virtual TreeNode<MenuItem> GetCurrentNode(string menuName, ControllerContext controllerContext)
        {
            if (!_currentNodeResolved)
            {
                var menu = GetMenu(menuName);
                if (menu != null)
                {
                    _currentNode = menu.Root.SelectNode(x => x.Value.IsCurrent(controllerContext), true);
                }
                _currentNodeResolved = true;
            }

            return _currentNode;
        }

        public virtual void ResolveElementCounts(string menuName, TreeNode<MenuItem> curNode, bool deep = false)
        {
            GetMenu(menuName)?.ResolveElementCounts(curNode, deep);
        }

        public virtual void ClearCache(string menuName)
        {
            GetMenu(menuName)?.ClearCache();
        }
    }
}
