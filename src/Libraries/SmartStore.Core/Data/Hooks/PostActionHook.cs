using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartStore.Core.Data.Hooks
{
    /// <summary>
    /// Implements a strongly-typed hook to be run after an action is performed in the database.
    /// </summary>
    /// <typeparam name="TEntity">The type of entity this hook must watch for.</typeparam>
    public abstract class PostActionHook<TEntity> : IPostActionHook
    {
        /// <summary>
        /// Implements the interface.  This causes the hook to only run for objects that are assignable to TEntity.
        /// </summary>
        public void HookObject(object entity, HookEntityMetadata metadata)
        {
            if (typeof(TEntity).IsAssignableFrom(entity.GetType()))
            {
                Hook((TEntity)entity, metadata);
            }
        }

        /// <summary>
        /// The logic to perform per entity after the registered action gets performed.
        /// This gets run once per entity that has been changed.
        /// </summary>
        public abstract void Hook(TEntity entity, HookEntityMetadata metadata);

        /// <summary>
        /// Entity States that this hook must be registered to listen for.
        /// </summary>
        public abstract EntityState HookStates { get; }
    }
}
