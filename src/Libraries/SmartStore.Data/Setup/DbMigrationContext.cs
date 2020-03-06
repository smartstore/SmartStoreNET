using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace SmartStore.Data.Setup
{
	public class DbMigrationContext
	{
		private static DbMigrationContext _dbMigrationContext;
		private readonly ConcurrentDictionary<Type, bool> _map = new ConcurrentDictionary<Type, bool>();

		private DbMigrationContext()
		{
		}

		public static DbMigrationContext Current
		{
			get
			{
				if (_dbMigrationContext == null)
				{
					_dbMigrationContext = new DbMigrationContext();
				}

				return _dbMigrationContext;
			}
		}

		internal void SetSuppressInitialCreate<TContext>(bool suppress) where TContext : DbContext
		{
			_map[typeof(TContext)] = suppress;
		}

		public bool SuppressInitialCreate<TContext>() where TContext : DbContext
		{
			if (_map.TryGetValue(typeof(TContext), out var value))
			{
				return value;
			}
			
			return false;
		}
	}
}
