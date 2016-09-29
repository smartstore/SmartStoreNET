using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure.Interception;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartStore.Data.Caching
{
	internal class CacheInvalidationInterceptor : IDbCommandTreeInterceptor
	{
		private readonly QueryCache _queryCache;

		public CacheInvalidationInterceptor(QueryCache queryCache)
		{
			_queryCache = queryCache;
		}

		public void TreeCreated(DbCommandTreeInterceptionContext interceptionContext)
		{
			if (!_queryCache.AutoInvalidationEnabled)
			{
				return;
			}

			var tree = interceptionContext.Result as DbModificationCommandTree;
			if (tree == null)
			{
				return;
			}

			var scanExpression = tree.Target.Expression as DbScanExpression;
			if (scanExpression == null)
			{
				return;
			}

			var entitySetName = scanExpression.Target.Name;

			_queryCache.InvalidateSetsInRequest(new[] { entitySetName });
			_queryCache.InvalidateSets(new[] { entitySetName });
		}
	}
}
