using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange
{
	
	public partial class SyncMappingService : ISyncMappingService
	{
		private readonly IRepository<SyncMapping> _syncMappingsRepository;

		public SyncMappingService(IRepository<SyncMapping> syncMappingsRepository)
		{
			this._syncMappingsRepository = syncMappingsRepository;
		}

		public void InsertSyncMapping(SyncMapping mapping)
		{
			Guard.ArgumentNotNull(() => mapping);

			_syncMappingsRepository.Insert(mapping);
		}

		public void InsertSyncMappings(IEnumerable<SyncMapping> mappings)
		{
			Guard.ArgumentNotNull(() => mappings);

			_syncMappingsRepository.InsertRange(mappings);
		}

		public IList<SyncMapping> GetAllSyncMappings(string contextName = null, string entityName = null)
		{
			var query = _syncMappingsRepository.Table;

			if (entityName.HasValue())
			{
				query = query.Where(x => x.EntityName == entityName);
			}

			if (contextName.HasValue())
			{
				query = query.Where(x => x.ContextName == contextName);
			}

			return query.ToList();
		}

		public SyncMapping GetSyncMappingByEntity(int entityId, string entityName, string contextName)
		{
			Guard.ArgumentIsPositive(entityId, "entityId");
			Guard.ArgumentNotEmpty(() => entityName);
			Guard.ArgumentNotEmpty(() => contextName);

			var query = from x in _syncMappingsRepository.Table
						where 
							x.EntityId == entityId 
							&& x.EntityName == entityName 
							&& x.ContextName == contextName
						select x;

			return query.FirstOrDefault();
		}

		public SyncMapping GetSyncMappingBySource(string sourceKey, string entityName, string contextName)
		{
			Guard.ArgumentNotEmpty(() => sourceKey);
			Guard.ArgumentNotEmpty(() => entityName);
			Guard.ArgumentNotEmpty(() => contextName);

			var query = from x in _syncMappingsRepository.Table
						where
							x.SourceKey == sourceKey
							&& x.EntityName == entityName
							&& x.ContextName == contextName
						select x;

			return query.FirstOrDefault();
		}

		public void DeleteSyncMapping(SyncMapping mapping)
		{
			Guard.ArgumentNotNull(() => mapping);

			_syncMappingsRepository.Delete(mapping);
		}

		public void DeleteSyncMappings(IEnumerable<SyncMapping> mappings)
		{
			Guard.ArgumentNotNull(() => mappings);

			_syncMappingsRepository.DeleteRange(mappings);
		}

		public void DeleteSyncMappingsFor<T>(T entity) where T : BaseEntity
		{
			Guard.ArgumentNotNull(() => entity);

			if (entity is SyncMapping)
			{
				throw Error.InvalidOperation("Cannot delete a sync mapping record for a SyncMapping entity");
			}

			if (entity.IsTransientRecord())
			{
				return;
			}

			_syncMappingsRepository.DeleteAll(x => x.EntityId == entity.Id && x.EntityName == typeof(T).Name);
		}

		public void DeleteSyncMappings(string contextName, string entityName = null)
		{
			Guard.ArgumentNotEmpty(() => contextName);

			Expression<Func<SyncMapping, bool>> expression = (x) => x.ContextName == contextName;

			if (entityName.HasValue())
			{
				_syncMappingsRepository.DeleteAll(x => x.ContextName == contextName && x.EntityName == entityName);
			}
			else
			{
				_syncMappingsRepository.DeleteAll(x => x.ContextName == contextName);
			}
		}


		public void UpdateSyncMapping(SyncMapping mapping)
		{
			Guard.ArgumentNotNull(() => mapping);

			mapping.SyncedOnUtc = DateTime.UtcNow;
			_syncMappingsRepository.Update(mapping);
		}

	}

}
