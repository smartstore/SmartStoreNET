using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Data.Setup
{

	public static class SmartObjectContextExtensions
	{

		#region Resource seeding

		public static void MigrateLocaleResources(this SmartObjectContext ctx, Action<LocaleResourcesBuilder> fn, bool updateTouchedResources = false)
		{
			Guard.ArgumentNotNull(() => ctx);
			Guard.ArgumentNotNull(() => fn);

			var builder = new LocaleResourcesBuilder();
			fn(builder);
			var entries = builder.Build();

			var migrator = new LocaleResourcesMigrator(ctx);
			migrator.Migrate(entries, updateTouchedResources);
		}

		#endregion


		#region Settings seeding

		// [...]

		#endregion

	}

}
