using System;
using System.Collections.Generic;

namespace SmartStore.Web.Framework.UI
{
    public class DependencyMenuResolver : IMenuResolver
    {
        private readonly IDictionary<string, IMenu> _menus;

        public DependencyMenuResolver(IEnumerable<IMenu> menus)
        {
            _menus = menus.ToDictionarySafe(x => x.Name, x => x, StringComparer.OrdinalIgnoreCase);
        }

        public int Order => 0;

        public bool Exists(string menuName)
        {
            Guard.NotEmpty(menuName, nameof(menuName));

            return _menus.ContainsKey(menuName);
        }

        public IMenu Resolve(string menuName)
        {
            _menus.TryGetValue(menuName, out var menu);
            return menu;
        }
    }
}
