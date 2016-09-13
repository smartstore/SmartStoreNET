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
		private static DbMigrationContext s_dbMigrationContext;
		private readonly ConcurrentDictionary<Type, bool> _map = new ConcurrentDictionary<Type, bool>();

		public static DbMigrationContext Current
		{
			get
			{
				if (s_dbMigrationContext == null)
				{
					s_dbMigrationContext = new DbMigrationContext();
				}
				return s_dbMigrationContext;
			}
		}

		internal void SetSuppressInitialCreate<TContext>(bool suppress) where TContext : DbContext
		{
			_map[typeof(TContext)] = suppress;
		}

		public bool SuppressInitialCreate<TContext>() where TContext : DbContext
		{
			bool value;
			if (_map.TryGetValue(typeof(TContext), out value))
			{
				return value;
			}
			
			return false;
		}
	}
}
