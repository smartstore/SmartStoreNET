using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.Configuration;

namespace SmartStore.Data.Setup
{
	
	internal class SettingsMigrator
	{
		private readonly SmartObjectContext _ctx;
		private readonly DbSet<Setting> _settings;

		public SettingsMigrator(SmartObjectContext ctx)
		{
			Guard.ArgumentNotNull(() => ctx);

			_ctx = ctx;
			_settings = _ctx.Set<Setting>();
		}

		public void Migrate(IEnumerable<SettingEntry> entries)
		{
			Guard.ArgumentNotNull(() => entries);

			if (!entries.Any())
				return;

			using (var scope = new DbContextScope(_ctx, autoDetectChanges: false))
			{
				// [...]
			}
		}
	}

}
