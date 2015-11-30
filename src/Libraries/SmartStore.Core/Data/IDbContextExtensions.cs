using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Data;

namespace SmartStore
{
	public static class IDbContextExtensions
	{

		/// <summary>
		/// Detaches all entities from the current object context
		/// </summary>
		/// <param name="unchangedEntitiesOnly">When <c>true</c>, only entities in unchanged state get detached.</param>
		/// <returns>The count of detached entities</returns>
		public static int DetachAll(this IDbContext ctx, bool unchangedEntitiesOnly = true)
		{
			return ctx.DetachEntities<BaseEntity>(unchangedEntitiesOnly);
		}

		public static void DetachEntities<TEntity>(this IDbContext ctx, IEnumerable<TEntity> entities) where TEntity : BaseEntity
		{
			Guard.ArgumentNotNull(() => ctx);

			entities.Each(x => ctx.DetachEntity(x));
		}

		/// <summary>
		/// Changes the object state to unchanged
		/// </summary>
		/// <typeparam name="TEntity">Type of entity</typeparam>
		/// <param name="entity">The entity instance</param>
		/// <returns>true on success, false on failure</returns>
		public static bool SetToUnchanged<TEntity>(this IDbContext ctx, TEntity entity) where TEntity : BaseEntity
		{
			try
			{
				ctx.ChangeState<TEntity>(entity, System.Data.Entity.EntityState.Unchanged);
				return true;
			}
			catch (Exception ex)
			{
				ex.Dump();
				return false;
			}
		}

	}
}
