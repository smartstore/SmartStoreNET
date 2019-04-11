using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SmartStore.Collections;
using SmartStore.Core.Logging;
using SmartStore.Services.Cms;

namespace SmartStore.Web.Framework.UI
{
    public partial class MenuService : IMenuService
    {
        protected readonly IChronometer _chronometer;
        protected readonly IWidgetProvider _widgetProvider;
        protected readonly IMenuStorage _menuStorage;
        protected readonly IMenuResolver[] _menuResolvers;

        private TreeNode<MenuItem> _currentNode;
        private bool _currentNodeResolved;

        public MenuService(
            IChronometer chronometer,
            IWidgetProvider widgetProvider,
            IMenuStorage menuStorage,
            IEnumerable<IMenuResolver> menuResolvers)
        {
            _chronometer = chronometer;
            _widgetProvider = widgetProvider;
            _menuStorage = menuStorage;
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

        public virtual void ProcessMenus()
        {
            using (_chronometer.Step("ProcessMenus"))
            {
                var menusInfo = _menuStorage.GetUserMenusInfo();

                foreach (var info in menusInfo)
                {
                    _widgetProvider.RegisterAction(
                        info.WidgetZones,
                        "UserMenu",
                        "Common",
                        new { area = "", systemName = info.SystemName, template = info.Template },
                        info.DisplayOrder);
                }
            }
        }
    }
}
