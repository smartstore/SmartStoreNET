using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Web.Framework.UI
{
	public class DependencyMenuResolver : IMenuResolver
	{
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
