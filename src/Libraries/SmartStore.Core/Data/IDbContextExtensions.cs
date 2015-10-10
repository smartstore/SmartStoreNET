using System;
using System.Collections.Generic;
using SmartStore.Core;
using SmartStore.Core.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Data.Entity;

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

		public static IQueryable<TCollection> QueryForCollection<TEntity, TCollection>(
			this IDbContext ctx,
			TEntity entity,
			Expression<Func<TEntity, ICollection<TCollection>>> navigationProperty)
			where TEntity : BaseEntity
			where TCollection : BaseEntity
		{
			Guard.ArgumentNotNull(() => entity);
			Guard.ArgumentNotNull(() => navigationProperty);

			var dbContext = ctx as DbContext;
			if (dbContext == null)
			{
				throw new NotSupportedException("The IDbContext instance does not inherit from DbContext (EF)");
			}

			return dbContext.Entry(entity).Collection(navigationProperty).Query();
		}

		public static IQueryable<TProperty> QueryForReference<TEntity, TProperty>(
			this IDbContext ctx,
			TEntity entity,
			Expression<Func<TEntity, TProperty>> navigationProperty)
			where TEntity : BaseEntity
			where TProperty : BaseEntity
		{
			Guard.ArgumentNotNull(() => entity);
			Guard.ArgumentNotNull(() => navigationProperty);

			var dbContext = ctx as DbContext;
			if (dbContext == null)
			{
				throw new NotSupportedException("The IDbContext instance does not inherit from DbContext (EF)");
			}

			return dbContext.Entry(entity).Reference(navigationProperty).Query();
		}

		public static void LoadCollection<TEntity, TCollection>(
			this IDbContext ctx,
			TEntity entity,
			Expression<Func<TEntity, ICollection<TCollection>>> navigationProperty,
			bool force = false,
			Func<IQueryable<TCollection>, IQueryable<TCollection>> queryAction = null)
			where TEntity : BaseEntity
			where TCollection : BaseEntity
		{
			Guard.ArgumentNotNull(() => entity);
			Guard.ArgumentNotNull(() => navigationProperty);

			var dbContext = ctx as DbContext;
			if (dbContext == null)
			{
				throw new NotSupportedException("The IDbContext instance does not inherit from DbContext (EF)");
			}

			var entry = dbContext.Entry(entity);
			var collection = entry.Collection(navigationProperty);

			if (force)
			{
				collection.IsLoaded = false;
			}

			if (!collection.IsLoaded)
			{
				if (queryAction != null || ctx.ForceNoTracking)
				{
					var query = !ctx.ForceNoTracking 
						? collection.Query() 
						: collection.Query().AsNoTracking();

                    var myQuery = queryAction != null 
						? queryAction(query) 
						: query;

					collection.CurrentValue = myQuery.ToList();
				}
				else
				{
					collection.Load();
				}

				collection.IsLoaded = true;
			}
		}

		public static void LoadReference<TEntity, TProperty>(
			this IDbContext ctx,
			TEntity entity,
			Expression<Func<TEntity, TProperty>> navigationProperty,
			bool force = false)
			where TEntity : BaseEntity
			where TProperty : BaseEntity
		{
			Guard.ArgumentNotNull(() => entity);
			Guard.ArgumentNotNull(() => navigationProperty);

			var dbContext = ctx as DbContext;
			if (dbContext == null)
			{
				throw new NotSupportedException("The IDbContext instance does not inherit from DbContext (EF)");
			}

			var entry = dbContext.Entry(entity);
			var reference = entry.Reference(navigationProperty);

			if (force)
			{
				reference.IsLoaded = false;
			}

			if (!reference.IsLoaded)
			{
				reference.Load();
				reference.IsLoaded = true;
			}
		}

	}
}
