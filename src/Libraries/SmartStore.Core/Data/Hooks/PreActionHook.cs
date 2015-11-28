using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
