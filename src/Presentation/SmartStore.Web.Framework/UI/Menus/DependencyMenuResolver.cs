using System;

namespace SmartStore.Web.Framework.UI
{
    public class DependencyMenuResolver : IMenuResolver
	{
        public int Order => 0;

        public bool Exists(string menuName)
		{
			// TODO: Check whether an IMenu dependency with passed name has been registered.

			throw new NotImplementedException();
		}

		public IMenu Resolve(string menuName)
		{
			// TODO: Resolve menus from Autofac like in SiteMapService

			throw new NotImplementedException();
		}
	}
}
