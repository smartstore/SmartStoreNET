using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Web.Framework.UI
{
	public class DatabaseMenuResolver : IMenuResolver
	{
		public bool Exists(string menuName)
		{
			// TODO: Check whether a menu with passed name exists in the database.
			// Make it FAST, e.g. by caching the existing menu names in a HashSet (don't forget invalidation).

			throw new NotImplementedException();
		}

		public IMenu Resolve(string name)
		{
			// TODO: Resolve menus from database > MenuRecord

			throw new NotImplementedException();
		}
	}
}
