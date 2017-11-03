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
	public interface IDbSaveHook : IDbHook
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
}
