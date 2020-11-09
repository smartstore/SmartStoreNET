using Autofac;
using SmartStore.Services;
using SmartStore.Services.Cms;

namespace SmartStore.Web.Framework.UI
{
    public class DatabaseMenuResolver : IMenuResolver
    {
        protected readonly IComponentContext _ctx;
        protected readonly IMenuStorage _menuStorage;

        public DatabaseMenuResolver(IComponentContext ctx, IMenuStorage menuStorage)
        {
            _ctx = ctx;
            _menuStorage = menuStorage;
        }

        public int Order => 1;

        public bool Exists(string menuName)
        {
            return _menuStorage.MenuExists(menuName);
        }

        public IMenu Resolve(string name)
        {
            var menu = _ctx.ResolveNamed<IMenu>("database", new NamedParameter("menuName", name));
            return menu;
        }
    }
}
