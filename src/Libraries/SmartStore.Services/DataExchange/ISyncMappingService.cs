using System.Collections.Generic;
using System.Linq;
using SmartStore.Core;
using SmartStore.Core.Domain.DataExchange;

namespace SmartStore.Services.DataExchange
{
	public partial interface ISyncMappingService
	{
		/// <summary>
		/// Inserts a sync mapping entity
		/// </summary>
		/// <param name="mapping">Sync mapping</param>
		void InsertSyncMapping(SyncMapping mapping);

		/// <summary>
		/// Inserts a range of sync mapping entities
		/// </summary>
		/// <param name="mapping">A sequence of sync mappings</param>
		void InsertSyncMappings(IEnumerable<SyncMapping> mappings);

		/// <summary>
		/// Gets all sync mappings
		/// </summary>
		/// <param name="contextName">The context (external application) name. Leave <c>null</c> to load all records regardless of context.</param>
		/// <param name="entityName">The entity name. Leave <c>null</c> to load all records regardless of entity name.</param>
		/// <param name="entityIds">Array of entity identifiers</param>
		/// <returns>SyncMappings</returns>
		IList<SyncMapping> GetAllSyncMappings(string contextName = null, string entityName = null, int[] entityIds = null);

		/// <summary>
		/// Gets a sync mapping record by (target) entity id, name and context name.
		/// </summary>
		/// <param name="entityId">Entity nd</param>
		/// <param name="entityName">Entity name</param>
		/// <param name="contextName">Context name</param>
		/// <returns>SyncMapping</returns>
		SyncMapping GetSyncMappingByEntity(int entityId, string entityName, string contextName);

		/// <summary>
		/// Gets a sync mapping record by (external) source key, entity name and context name.
		/// </summary>
		/// <param name="sourceKey">Source key</param>
		/// <param name="entityName">Entity name</param>
		/// <param name="contextName">Context name</param>
		/// <returns>SyncMappings</returns>
		SyncMapping GetSyncMappingBySource(string sourceKey, string entityName, string contextName);

		/// <summary>
		/// Deletes a sync mapping entity
		/// </summary>
		/// <param name="mapping">Sync mapping</param>
		void DeleteSyncMapping(SyncMapping mapping);

		/// <summary>
		/// Deletes a range of sync mapping entities
		/// </summary>
		/// <param name="mapping">Sync mappings</param>
		void DeleteSyncMappings(IEnumerable<SyncMapping> mappings);

		/// <summary>
		/// Deletes all sync mapping entities referencing the specified entity
		/// </summary>
		/// <param name="entity">The entity</param>
		void DeleteSyncMappingsFor<T>(T entity) where T : BaseEntity;

		/// <summary>
		/// Deletes a range of sync mapping entities
		/// </summary>
		/// <param name="contextName">The context (external application) name.</param>
		/// <param name="entityName">The entity name. Leave <c>null</c> to delete all context specific mappings regardless of entity name.</param>
		/// <returns>SyncMappings</returns>
		void DeleteSyncMappings(string contextName, string entityName = null);

		/// <summary>
		/// Updates a sync mapping entity
		/// </summary>
		/// <param name="mapping">Sync mapping</param>
		void UpdateSyncMapping(SyncMapping mapping);
	}


	public static class ISyncMappingServiceExtensions
	{

		public static SyncMapping InsertSyncMapping<T>(this ISyncMappingService svc, T entity, string contextName, string sourceKey) where T : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotEmpty(contextName, nameof(contextName));
			Guard.NotEmpty(sourceKey, nameof(sourceKey));

			if (entity is SyncMapping)
			{
				throw Error.InvalidOperation("Cannot insert a sync mapping record for a SyncMapping entity");
			}

			if (entity.IsTransientRecord())
			{
				throw Error.InvalidOperation("Cannot insert a sync mapping record for a transient (unsaved) entity");
			}

			var mapping = new SyncMapping
			{
				ContextName = contextName,
				EntityId = entity.Id,
				EntityName = typeof(T).Name,
				SourceKey = sourceKey
			};

			return mapping;
		}

		public static SyncMapping GetSyncMappingByEntity<T>(this ISyncMappingService svc, T entity, string contextName) where T : BaseEntity
		{
			Guard.NotNull(entity, nameof(entity));
			Guard.NotEmpty(contextName, nameof(contextName));

			if (entity is SyncMapping)
			{
				throw Error.InvalidOperation("Cannot get a sync mapping record for a SyncMapping entity");
			}

			if (entity.IsTransientRecord())
			{
				throw Error.InvalidOperation("Cannot get a sync mapping record for a transient (unsaved) entity");
			}

			return svc.GetSyncMappingByEntity(entity.Id, typeof(T).Name, contextName);
		}

		/// <summary>
		/// Inserts a range of sync mapping entities by specifying two equal sized sequences for entity ids and source keys.
		/// </summary>
		/// <param name="contextName">Context name for all mappings</param>
		/// <param name="entityName">Entity name for all mappings</param>
		/// <param name="entityIds">A sequence of entity ids</param>
		/// <param name="sourceKeys">A sequence of source keys</param>
		/// <returns>List of persisted sync mappings</returns>
		/// <remarks>Both sequences must contain at least one element and must be of equal size.</remarks>
		public static IList<SyncMapping> InsertSyncMappings(this ISyncMappingService svc, string contextName, string entityName, IEnumerable<int> entityIds, IEnumerable<string> sourceKeys) 
		{
			Guard.NotEmpty(contextName, nameof(contextName));
			Guard.NotEmpty(entityName, nameof(entityName));
			Guard.NotNull(entityIds, nameof(entityIds));
			Guard.NotNull(sourceKeys, nameof(sourceKeys));

			if (!entityIds.Any() || !sourceKeys.Any() || entityIds.Count() != sourceKeys.Count())
			{
				throw Error.InvalidOperation("Both sequences must contain at least one element and must be of equal size.");
			}

			var mappings = new List<SyncMapping>();
			var arrIds = entityIds.ToArray();
			var arrKeys = sourceKeys.ToArray();

			for (int i = 0; i < arrIds.Length; i++)
			{
				mappings.Add(new SyncMapping 
				{
 					ContextName = contextName,
					EntityName = entityName,
					EntityId = arrIds[i],
					SourceKey = arrKeys[i]
				});
			}

			svc.InsertSyncMappings(mappings);

			return mappings;
		}

	}
}
