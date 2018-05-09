using System;

namespace SmartStore.Core.Data.Hooks
{
	public abstract class DbSaveHook<TEntity> : IDbSaveHook 
		where TEntity : class
	{
		public void OnBeforeSave(HookedEntity entry)
		{
			var entity = entry.Entity as TEntity;
			switch (entry.InitialState)
			{
				case EntityState.Added:
					OnInserting(entity, entry);
					break;
				case EntityState.Modified:
					OnUpdating(entity, entry);
					break;
				case EntityState.Deleted:
					OnDeleting(entity, entry);
					break;
			}
		}

		protected virtual void OnInserting(TEntity entity, HookedEntity entry)
		{
		}

		protected virtual void OnUpdating(TEntity entity, HookedEntity entry)
		{
		}

		protected virtual void OnDeleting(TEntity entity, HookedEntity entry)
		{
		}

		public virtual void OnBeforeSaveCompleted()
		{
		}


		public void OnAfterSave(HookedEntity entry)
		{
			var entity = entry.Entity as TEntity;
			switch (entry.InitialState)
			{
				case EntityState.Added:
					OnInserted(entity, entry);
					break;
				case EntityState.Modified:
					OnUpdated(entity, entry);
					break;
				case EntityState.Deleted:
					OnDeleted(entity, entry);
					break;
			}
		}

		protected virtual void OnInserted(TEntity entity, HookedEntity entry)
		{
		}

		protected virtual void OnUpdated(TEntity entity, HookedEntity entry)
		{
		}

		protected virtual void OnDeleted(TEntity entity, HookedEntity entry)
		{
		}

		public virtual void OnAfterSaveCompleted()
		{
		}
	}
}
