using System;

namespace SmartStore.Core.Data.Hooks
{
    /// <summary>
    /// Implements a strongly-typed hook to be run after an action is performed in the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity this hook must watch for.</typeparam>
    public abstract class PostActionHook<TEntity> : IPostActionHook
    {
        /// <summary>
        /// Implements the interface. This causes the hook to only run for objects that are assignable to TEntity.
        /// </summary>
        public void HookObject(object entity, HookEntityMetadata metadata)
        {
            //if (typeof(TEntity).IsAssignableFrom(entity.GetType()))
            //{
                Hook((TEntity)entity, metadata);
            //}
        }

        /// <summary>
        /// The logic to perform per entity after the registered action gets performed.
        /// This gets run once per entity that has been changed.
        /// </summary>
        public abstract void Hook(TEntity entity, HookEntityMetadata metadata);

		/// <summary>
		/// Indicates whether the hook instance can be processed for the given <see cref="EntityState"/>
		/// </summary>
		/// <param name="state">The state of the entity</param>
		/// <returns><c>true</c> when the hook should be processed, <c>false</c> otherwise</returns>
		public abstract bool CanProcess(EntityState state);

		public virtual void OnCompleted()
		{
		}
	}

	/// <summary>
	/// Implements a hook that will run after an entity gets inserted into the database.
	/// </summary>
	public abstract class PostInsertHook<TEntity> : PostActionHook<TEntity>
	{
		public override bool CanProcess(EntityState state)
		{
			return state == EntityState.Added;
		}
	}

	/// <summary>
	/// Implements a hook that will run after an entity gets updated in the database.
	/// </summary>
	public abstract class PostUpdateHook<TEntity> : PostActionHook<TEntity>
	{
		public override bool CanProcess(EntityState state)
		{
			return state == EntityState.Modified;
		}
	}

	/// <summary>
	/// Implements a hook that will run after an entity gets deleted from the database.
	/// </summary>
	public abstract class PostDeleteHook<TEntity> : PostActionHook<TEntity>
	{
		public override bool CanProcess(EntityState state)
		{
			return state == EntityState.Deleted;
		}
	}
}
