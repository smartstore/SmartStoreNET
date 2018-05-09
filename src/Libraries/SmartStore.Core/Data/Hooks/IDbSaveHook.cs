﻿using System;

namespace SmartStore.Core.Data.Hooks
{
	/// <summary>
	/// A hook that is executed before and after a database save operation.
	/// </summary>
	public interface IDbSaveHook : IDbHook
	{
		void OnBeforeSave(HookedEntity entry);

		void OnAfterSave(HookedEntity entry);

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
