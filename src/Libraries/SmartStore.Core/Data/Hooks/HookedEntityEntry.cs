using System;
using System.Data.Entity.Infrastructure;

namespace SmartStore.Core.Data.Hooks
{
	public class HookedEntityEntry
	{
		public DbEntityEntry Entry { get; set; }
		public EntityState PreSaveState { get; set; }
	}
}
