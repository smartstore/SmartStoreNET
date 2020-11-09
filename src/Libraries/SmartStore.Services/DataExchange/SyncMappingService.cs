using System;
using System.Collections.Generic;
using System.Linq;
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
            Guard.NotNull(mapping, nameof(mapping));

            _syncMappingsRepository.Insert(mapping);
        }

        public void InsertSyncMappings(IEnumerable<SyncMapping> mappings)
        {
            Guard.NotNull(mappings, nameof(mappings));

            _syncMappingsRepository.InsertRange(mappings);
        }

        public IList<SyncMapping> GetAllSyncMappings(string contextName = null, string entityName = null, int[] entityIds = null)
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

            if (entityIds != null && entityIds.Any())
            {
                query = query.Where(x => entityIds.Contains(x.EntityId));
            }

            return query.ToList();
        }

        public SyncMapping GetSyncMappingByEntity(int entityId, string entityName, string contextName)
        {
            Guard.IsPositive(entityId, nameof(entityId));
            Guard.NotEmpty(entityName, nameof(entityName));
            Guard.NotEmpty(contextName, nameof(contextName));

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
            Guard.NotEmpty(sourceKey, nameof(sourceKey));
            Guard.NotEmpty(entityName, nameof(entityName));
            Guard.NotEmpty(contextName, nameof(contextName));

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
            Guard.NotNull(mapping, nameof(mapping));

            _syncMappingsRepository.Delete(mapping);
        }

        public void DeleteSyncMappings(IEnumerable<SyncMapping> mappings)
        {
            Guard.NotNull(mappings, nameof(mappings));

            _syncMappingsRepository.DeleteRange(mappings);
        }

        public void DeleteSyncMappingsFor<T>(T entity) where T : BaseEntity
        {
            Guard.NotNull(entity, nameof(entity));

            if (entity is SyncMapping)
            {
                throw Error.InvalidOperation("Cannot delete a sync mapping record for a SyncMapping entity");
            }

            if (entity.IsTransientRecord())
            {
                return;
            }

            _syncMappingsRepository.DeleteAll(x => x.EntityId == entity.Id && x.EntityName == entity.GetEntityName());
        }

        public void DeleteSyncMappings(string contextName, string entityName = null)
        {
            Guard.NotEmpty(contextName, nameof(contextName));

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
            Guard.NotNull(mapping, nameof(mapping));

            mapping.SyncedOnUtc = DateTime.UtcNow;
            _syncMappingsRepository.Update(mapping);
        }

    }

}
