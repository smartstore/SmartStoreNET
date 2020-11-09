using System;

namespace SmartStore.Core.Data.Hooks
{
    /// <summary>
    /// A hook that is executed before and after a database save operation.
    /// An implementor should raise <see cref="NotSupportedException"/> or
    /// <see cref="NotImplementedException"/> to signal the hook handler
    /// that it never should process the hook again for the current
    /// EntityType/State/Stage combination.
    /// </summary>
    public interface IDbSaveHook
    {
        void OnBeforeSave(IHookedEntity entry);

        void OnAfterSave(IHookedEntity entry);

        /// <summary>
        /// Called after all entities in the current unit of work has been handled right before saving changes to the database
        /// </summary>
        void OnBeforeSaveCompleted();

        /// <summary>
        /// Called after all entities in the current unit of work has been handled after saving changes to the database
        /// </summary>
        void OnAfterSaveCompleted();
    }

    /// <inheritdoc/>
    /// <typeparam name="TContext">
    /// Restricts the hook to the specified data context implementation type.
    /// To restrict to the core data context, implement the parameterless <see cref="IDbSaveHook"/> instead.
    /// Abstract base types can also be specified in order to bypass restrictions.
    /// </typeparam>
    public interface IDbSaveHook<TContext> : IDbSaveHook
        where TContext : IDbContext
    {
    }
}
