using System;

namespace SmartStore.Core.Data.Hooks
{
    /// <summary>
    /// A strongly typed PreActionHook.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity this hook must watch for.</typeparam>
    public abstract class PreActionHook<TEntity> : IPreActionHook
    {
        /// <summary>
        /// Gets a value indicating whether the hook is only used after successful <typeparamref name="TEntity"/> validation.
        /// </summary>
        /// <value>
        ///   <c>true</c> if requires validation; otherwise, <c>false</c>.
        /// </value>
        public abstract bool RequiresValidation { get; }

        /// <summary>
        /// Entity States that this hook must be registered to listen for.
        /// </summary>
        public abstract EntityState HookStates { get; }

		/// <summary>
		/// Indicates whether the hook instance can be processed for the given <see cref="EntityState"/>
		/// </summary>
		/// <param name="state">The state of the entity</param>
		/// <returns><c>true</c> when the hook should be processed, <c>false</c> otherwise</returns>
		public virtual bool CanProcess(EntityState state)
		{
			return state == HookStates;
		}

		/// <summary>
		/// The logic to perform per entity before the registered action gets performed.
		/// This gets run once per entity that has been changed.
		/// </summary>
		/// <param name="entity">The entity that is processed by Entity Framework.</param>
		/// <param name="metadata">Metadata about the entity in the context of this hook - such as state.</param>
		public abstract void Hook(TEntity entity, HookEntityMetadata metadata);

        /// <summary>
        /// Implements the interface.  This causes the hook to only run for objects that are assignable to TEntity.
        /// </summary>
        public void HookObject(object entity, HookEntityMetadata metadata)
        {
            if (typeof(TEntity).IsAssignableFrom(entity.GetType()))
            {
                this.Hook((TEntity)entity, metadata);
            }
        }

		public virtual void OnCompleted()
		{
		}
	}

	/// <summary>
	/// Implements a hook that will run before an entity gets inserted into the database.
	/// </summary>
	public abstract class PreInsertHook<TEntity> : PreActionHook<TEntity>
	{
		/// <summary>
		/// Returns <see cref="EntityState.Added"/> as the hookstate to listen for.
		/// </summary>
		public override EntityState HookStates
		{
			get { return EntityState.Added; }
		}
	}

	/// <summary>
	/// Implements a hook that will run before an entity gets updated in the database.
	/// </summary>
	public abstract class PreUpdateHook<TEntity> : PreActionHook<TEntity>
	{
		/// <summary>
		/// Returns <see cref="EntityState.Modified"/> as the hookstate to listen for.
		/// </summary>
		public override EntityState HookStates
		{
			get { return EntityState.Modified; }
		}
	}

	/// <summary>
	/// Implements a hook that will run before an entity gets deleted from the database.
	/// </summary>
	public abstract class PreDeleteHook<TEntity> : PreActionHook<TEntity>
	{
		/// <summary>
		/// Returns <see cref="EntityState.Deleted"/> as the hookstate to listen for.
		/// </summary>
		public override EntityState HookStates
		{
			get { return EntityState.Deleted; }
		}
	}
}
