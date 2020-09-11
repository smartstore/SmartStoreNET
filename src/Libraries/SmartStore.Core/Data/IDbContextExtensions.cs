using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
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
            return ctx.DetachEntities<BaseEntity>(unchangedEntitiesOnly, false);
        }

        public static void DetachEntities<TEntity>(this IDbContext ctx, IEnumerable<TEntity> entities, bool deep = false) where TEntity : BaseEntity
        {
            Guard.NotNull(ctx, nameof(ctx));

            using (new DbContextScope(ctx, autoDetectChanges: false, lazyLoading: false))
            {
                entities.Each(x => ctx.DetachEntity(x, deep));
            }
        }

        /// <summary>
        /// Changes the object state to unchanged
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="ctx"></param>
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
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

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
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

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
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var dbContext = ctx as DbContext;
            if (dbContext == null)
            {
                throw new NotSupportedException("The IDbContext instance does not inherit from DbContext (EF)");
            }

            var entry = dbContext.Entry(entity);
            var collection = entry.Collection(navigationProperty);

            // Avoid System.InvalidOperationException: Member 'IsLoaded' cannot be called for property...
            if (entry.State == System.Data.Entity.EntityState.Detached)
            {
                ctx.Attach(entity);
            }

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
            bool force = false,
            Func<IQueryable<TProperty>, IQueryable<TProperty>> queryAction = null)
            where TEntity : BaseEntity
            where TProperty : BaseEntity
        {
            Guard.NotNull(entity, nameof(entity));
            Guard.NotNull(navigationProperty, nameof(navigationProperty));

            var dbContext = ctx as DbContext;
            if (dbContext == null)
            {
                throw new NotSupportedException("The IDbContext instance does not inherit from DbContext (EF)");
            }

            var entry = dbContext.Entry(entity);
            var reference = entry.Reference(navigationProperty);

            // Avoid System.InvalidOperationException: Member 'IsLoaded' cannot be called for property...
            if (entry.State == System.Data.Entity.EntityState.Detached)
            {
                ctx.Attach(entity);
            }

            if (force)
            {
                reference.IsLoaded = false;
            }

            if (!reference.IsLoaded)
            {
                if (queryAction != null || ctx.ForceNoTracking)
                {
                    var query = !ctx.ForceNoTracking
                        ? reference.Query()
                        : reference.Query().AsNoTracking();

                    var myQuery = queryAction != null
                        ? queryAction(query)
                        : query;

                    reference.CurrentValue = myQuery.FirstOrDefault();
                }
                else
                {
                    reference.Load();
                }

                reference.IsLoaded = true;
            }
        }

        public static void AttachRange<TEntity>(this IDbContext ctx, IEnumerable<TEntity> entities) where TEntity : BaseEntity
        {
            entities.Each(x => ctx.Attach(x));
        }

    }
}
