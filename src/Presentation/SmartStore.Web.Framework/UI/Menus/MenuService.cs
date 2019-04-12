using System.Collections.Generic;
using System.Linq;
using SmartStore.Collections;

namespace SmartStore.Web.Framework.UI
{
    public partial class MenuService : IMenuService
    {
        protected readonly IMenuResolver[] _menuResolvers;

        public MenuService(IEnumerable<IMenuResolver> menuResolvers)
        {
            _menuResolvers = menuResolvers.OrderBy(x => x.Order).ToArray();
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
