using System;

namespace SmartStore.Core.Data.Hooks
{
	public abstract class DbLoadHook<TEntity> : IDbLoadHook
	{
		public virtual void OnLoaded(BaseEntity entity)
		{
			throw new NotImplementedException();
		}
	}
}
