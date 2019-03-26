using SmartStore.Services.Cms;

namespace SmartStore.Web.Framework.UI
{
    public class DatabaseMenuResolver : IMenuResolver
	{
        protected readonly IMenuStorage _menuStorage;

        public DatabaseMenuResolver(IMenuStorage menuStorage)
        {
            _menuStorage = menuStorage;
        }

        public bool Exists(string menuName)
		{
            return _menuStorage.MenuExists(menuName);
		}

		public IMenu Resolve(string name)
		{
            
            return null;
		}
	}
}
