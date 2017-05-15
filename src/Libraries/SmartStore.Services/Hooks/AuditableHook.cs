using System;
using SmartStore.Core;
using SmartStore.Core.Data.Hooks;

namespace SmartStore.Services.Hooks
{
	[Important]
	public class AuditableHook : DbSaveHook<IAuditable>
	{
		protected override void OnInserting(IAuditable entity, HookedEntity entry)
		{
			var now = DateTime.UtcNow;

			if (entity.CreatedOnUtc == DateTime.MinValue)
				entity.CreatedOnUtc = now;

			if (entity.UpdatedOnUtc == DateTime.MinValue)
				entity.UpdatedOnUtc = now;
		}

		protected override void OnUpdating(IAuditable entity, HookedEntity entry)
		{
			entity.UpdatedOnUtc = DateTime.UtcNow;
		}
	}
}
