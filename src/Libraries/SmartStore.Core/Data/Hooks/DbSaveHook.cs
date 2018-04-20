using System;

namespace SmartStore.Core.Data.Hooks
{
	public abstract class DbSaveHook<TEntity> : IDbSaveHook where TEntity : class
	{
		public virtual void OnBeforeSave(IHookedEntity entry)
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

		protected virtual void OnInserting(TEntity entity, IHookedEntity entry)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnUpdating(TEntity entity, IHookedEntity entry)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnDeleting(TEntity entity, IHookedEntity entry)
		{
			throw new NotImplementedException();
		}

		public virtual void OnBeforeSaveCompleted()
		{
		}


		public virtual void OnAfterSave(IHookedEntity entry)
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

		protected virtual void OnInserted(TEntity entity, IHookedEntity entry)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnUpdated(TEntity entity, IHookedEntity entry)
		{
			throw new NotImplementedException();
		}

		protected virtual void OnDeleted(TEntity entity, IHookedEntity entry)
		{
			throw new NotImplementedException();
		}

		public virtual void OnAfterSaveCompleted()
		{
		}
	}
}
