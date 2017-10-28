using System;
using SmartStore.Core;
using SmartStore.Core.Data.Hooks;

namespace SmartStore.Services.Hooks
{
	[Important]
	public class AuditableHook : DbSaveHook<IAuditable>
	{
		protected override void OnInserting(IAuditable entity, IHookedEntity entry)
		{
			var now = DateTime.UtcNow;

			if (entity.CreatedOnUtc == DateTime.MinValue)
				entity.CreatedOnUtc = now;

			if (entity.UpdatedOnUtc == DateTime.MinValue)
				entity.UpdatedOnUtc = now;
		}

		protected override void OnUpdating(IAuditable entity, IHookedEntity entry)
		{
			entity.UpdatedOnUtc = DateTime.UtcNow;
		}
	}
}
