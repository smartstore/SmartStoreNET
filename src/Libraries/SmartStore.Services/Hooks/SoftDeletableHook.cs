using Autofac;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Security;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Data;

namespace SmartStore.Services.Hooks
{
	public class SoftDeletablePreUpdateHook : DbSaveHook<ISoftDeletable>
	{
		private readonly IComponentContext _ctx;

		public SoftDeletablePreUpdateHook(IComponentContext ctx)
		{
			_ctx = ctx;
		}

		protected override void OnUpdating(ISoftDeletable entity, IHookedEntity entry)
		{
			var baseEntity = entry.Entity;

			var prop = entry.Entry.Property("Deleted");
			var deletedModified = !prop.CurrentValue.Equals(prop.OriginalValue);
			if (!deletedModified)
				return;

			var entityType = entry.EntityType;

			// mark orphaned ACL records as idle
			var aclSupported = baseEntity as IAclSupported;
			if (aclSupported != null && aclSupported.SubjectToAcl)
			{
				var shouldSetIdle = entity.Deleted;
				var rsAclRecord = _ctx.Resolve<IRepository<AclRecord>>();

				var aclService = _ctx.Resolve<IAclService>();
				var records = aclService.GetAclRecordsFor(entityType.Name, baseEntity.Id);
				foreach (var record in records)
				{
					record.IsIdle = shouldSetIdle;
					aclService.UpdateAclRecord(record);
				}
			}

			// Delete orphaned inactive UrlRecords.
			// We keep the active ones on purpose in order to be able to fully restore a soft deletable entity once we implemented the "recycle bin" feature
			var slugSupported = baseEntity as ISlugSupported;
			if (slugSupported != null && entity.Deleted)
			{
				var rsUrlRecord = _ctx.Resolve<IRepository<UrlRecord>>();

				var urlRecordService = _ctx.Resolve<IUrlRecordService>();
				var activeRecords = urlRecordService.GetUrlRecordsFor(entityType.Name, baseEntity.Id);
				using (urlRecordService.BeginScope())
				{
					foreach (var record in activeRecords)
					{
						if (!record.IsActive)
						{
							urlRecordService.DeleteUrlRecord(record);
						}
					}
				}
			}
		}
	}
}
