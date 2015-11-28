using System;
using SmartStore.Core;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Security;
using SmartStore.Services.Security;

namespace SmartStore.Services.Common
{
	public class AclEntityPostDeleteHook : PostDeleteHook<IAclSupported>
	{
		private readonly Lazy<IAclService> _aclService;

		public AclEntityPostDeleteHook(Lazy<IAclService> aclService)
		{
			this._aclService = aclService;
		}

		public override void Hook(IAclSupported entity, HookEntityMetadata metadata)
		{
			var baseEntity = entity as BaseEntity;

			if (baseEntity == null)
				return;

			var entityType = baseEntity.GetUnproxiedType();

			var records = _aclService.Value.GetAclRecordsFor(entityType.Name, baseEntity.Id);
			records.Each(x => _aclService.Value.DeleteAclRecord(x));
		}
	}
}
