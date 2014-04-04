using System;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Security;
using SmartStore.Services.Security;

namespace SmartStore.Services.Hooks
{

	public class SoftDeletablePreUpdateHook : PreUpdateHook<ISoftDeletable>
	{
		private readonly Lazy<IAclService> _aclService;
		private readonly Lazy<IDbContext> _dbContext;

		public SoftDeletablePreUpdateHook(Lazy<IAclService> aclService, Lazy<IDbContext> dbContext)
		{
			this._aclService = aclService;
			this._dbContext = dbContext;
		}

		public override void Hook(ISoftDeletable entity, HookEntityMetadata metadata)
		{
			var baseEntity = entity as BaseEntity;

			if (baseEntity == null)
				return;

			var aclEntity = baseEntity as IAclSupported;
			if (aclEntity == null || !aclEntity.SubjectToAcl)
				return;

			var ctx = _dbContext.Value;
			var modProps = ctx.GetModifiedProperties(baseEntity);
			if (modProps.ContainsKey("Deleted"))
			{
				var shouldSetIdle = entity.Deleted;
				var entityType = baseEntity.GetUnproxiedType();

				var records = _aclService.Value.GetAclRecordsFor(entityType.Name, baseEntity.Id);
				foreach (var record in records)
				{
					record.IsIdle = shouldSetIdle;
					_aclService.Value.UpdateAclRecord(record);
				}
			}
		}

		public override bool RequiresValidation
		{
			get { return false; }
		}
	}
}
