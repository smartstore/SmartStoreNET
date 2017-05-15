using System;

namespace SmartStore.Core.Data.Hooks
{
	/// <summary>
	/// A hook that is executed right after an entity has been loaded from database and materialized.
	/// </summary>
	public interface IDbLoadHook : IDbHook
	{
		void OnLoaded(BaseEntity entity);
	}
}
