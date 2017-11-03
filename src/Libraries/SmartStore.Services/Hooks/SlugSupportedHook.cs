using System;
using System.Collections.Generic;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Seo;
using SmartStore.Services.Seo;

namespace SmartStore.Services.Common
{
	public class SlugSupportedHook : DbSaveHook<ISlugSupported>
	{
		private readonly Lazy<IUrlRecordService> _urlRecordService;
		private readonly HashSet<UrlRecord> _toDelete = new HashSet<UrlRecord>();

		public SlugSupportedHook(Lazy<IUrlRecordService> urlRecordService)
		{
			_urlRecordService = urlRecordService;
		}

		protected override void OnDeleted(ISlugSupported entity, IHookedEntity entry)
		{
			var entityType = entry.EntityType;
			var records = _urlRecordService.Value.GetUrlRecordsFor(entityType.Name, entry.Entity.Id);
			_toDelete.AddRange(records);
		}

		public override void OnAfterSaveCompleted()
		{
			if (_toDelete.Count == 0)
				return;

			using (var scope = new DbContextScope(autoCommit: false))
			{
				using (_urlRecordService.Value.BeginScope())
				{
					_toDelete.Each(x => _urlRecordService.Value.DeleteUrlRecord(x));
				}

				scope.Commit();
				_toDelete.Clear();
			}
		}
	}
}
