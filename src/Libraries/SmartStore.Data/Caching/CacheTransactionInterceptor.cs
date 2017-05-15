using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Infrastructure.Interception;
using System.Linq;

namespace SmartStore.Data.Caching
{
	public class CacheTransactionInterceptor : IDbTransactionInterceptor
	{
		private readonly ConcurrentDictionary<DbTransaction, List<string>> _affectedSetsInTransaction = new ConcurrentDictionary<DbTransaction, List<string>>();
		private readonly IDbCache _cache;

		public CacheTransactionInterceptor(IDbCache cache)
		{
			Guard.NotNull(cache, nameof(cache));

			_cache = cache;
		}

		public virtual bool GetItem(DbTransaction transaction, string key, out object value)
		{
			if (transaction == null)
			{
				return _cache.TryGet(key, out value);
			}

			value = null;

			return false;
		}

		public virtual void PutItem(DbTransaction transaction, string key, object value, IEnumerable<string> dependentEntitySets, TimeSpan? duration)
		{
			if (transaction == null)
			{
				_cache.Put(key, value, dependentEntitySets, duration);
			}
		}

		public virtual void InvalidateSets(DbTransaction transaction, IEnumerable<string> entitySets)
		{
			if (transaction == null)
			{
				_cache.InvalidateSets(entitySets);
				_cache.RequestInvalidateSets(entitySets);
			}
			else
			{
				AddAffectedEntitySets(transaction, entitySets);
			}
		}

		protected void AddAffectedEntitySets(DbTransaction transaction, IEnumerable<string> affectedEntitySets)
		{
			Guard.NotNull(transaction, nameof(transaction));
			Guard.NotNull(affectedEntitySets, nameof(affectedEntitySets));

			var entitySets = _affectedSetsInTransaction.GetOrAdd(transaction, new List<string>());
			entitySets.AddRange(affectedEntitySets);
		}

		private IEnumerable<string> RemoveAffectedEntitySets(DbTransaction transaction)
		{
			List<string> affectedEntitySets;

			_affectedSetsInTransaction.TryRemove(transaction, out affectedEntitySets);

			return affectedEntitySets;
		}

		public void Committed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
		{
			var entitySets = RemoveAffectedEntitySets(transaction);

			if (entitySets != null)
			{
				_cache.InvalidateSets(entitySets);
				_cache.RequestInvalidateSets(entitySets);
			}
		}

		public void Committing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
		{
		}

		public void ConnectionGetting(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext)
		{
		}

		public void ConnectionGot(DbTransaction transaction, DbTransactionInterceptionContext<DbConnection> interceptionContext)
		{
		}

		public void Disposed(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
		{
		}

		public void Disposing(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
		{
		}

		public void IsolationLevelGetting(DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext)
		{
		}

		public void IsolationLevelGot(DbTransaction transaction, DbTransactionInterceptionContext<IsolationLevel> interceptionContext)
		{
		}

		public void RolledBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
		{
			RemoveAffectedEntitySets(transaction);
		}

		public void RollingBack(DbTransaction transaction, DbTransactionInterceptionContext interceptionContext)
		{
		}
	}
}
