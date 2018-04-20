using System;
using System.Collections.Generic;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Domain.Stores;
using SmartStore.Services.Stores;

namespace SmartStore.Services.Common
{
	public class StoreMappingEntityHook : DbSaveHook<IStoreMappingSupported>
	{
		private readonly Lazy<IStoreMappingService> _storeMappingService;
		private readonly HashSet<StoreMapping> _toDelete = new HashSet<StoreMapping>();

		public StoreMappingEntityHook(Lazy<IStoreMappingService> storeMappingService)
		{
			_storeMappingService = storeMappingService;
		}

		protected override void OnDeleted(IStoreMappingSupported entity, IHookedEntity entry)
		{
			var entityType = entry.EntityType;

			var records = _storeMappingService.Value
				.GetStoreMappingsFor(entityType.Name, entry.Entity.Id)
				.ToList();

			_toDelete.AddRange(records);
		}

		public override void OnAfterSaveCompleted()
		{
			if (_toDelete.Count == 0)
				return;

			using (var scope = new DbContextScope(autoCommit: false))
			{
				_toDelete.Each(x => _storeMappingService.Value.DeleteStoreMapping(x));
				scope.Commit();
			}

			_toDelete.Clear();
		}
	}
}
