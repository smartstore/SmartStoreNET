using System;
using System.Collections.Generic;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Security;
using SmartStore.Services.Security;

namespace SmartStore.Services.Common
{
	public class AclEntityHook : DbSaveHook<IAclSupported>
	{
		private readonly Lazy<IAclService> _aclService;
		private readonly HashSet<AclRecord> _toDelete = new HashSet<AclRecord>();

		public AclEntityHook(Lazy<IAclService> aclService)
		{
			_aclService = aclService;
		}

		protected override void OnDeleted(IAclSupported entity, IHookedEntity entry)
		{
			var entityType = entry.EntityType;

			var records = _aclService.Value.GetAclRecordsFor(entityType.Name, entry.Entity.Id);
			_toDelete.AddRange(records);
		}

		public override void OnAfterSaveCompleted()
		{
			if (_toDelete.Count == 0)
				return;

			using (var scope = new DbContextScope(autoCommit: false))
			{
				_toDelete.Each(x => _aclService.Value.DeleteAclRecord(x));
				scope.Commit();
			}

			_toDelete.Clear();
		}
	}
}
