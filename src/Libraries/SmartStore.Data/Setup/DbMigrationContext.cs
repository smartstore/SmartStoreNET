using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using SmartStore.Collections;

namespace SmartStore.Data.Setup
{
    public class DbMigrationContext
    {
        private static DbMigrationContext _instance;
        private readonly ConcurrentDictionary<Type, bool> _map = new ConcurrentDictionary<Type, bool>();
        private Multimap<Type, string> _appliedMigrations;
        private readonly object _lock = new object();

        private DbMigrationContext()
        {
        }

        public static DbMigrationContext Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DbMigrationContext();
                }

                return _instance;
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

        internal void AddAppliedMigration(Type contextType, string migrationName)
        {
            if (_appliedMigrations == null)
            {
                lock (_lock)
                {
                    if (_appliedMigrations == null)
                    {
                        _appliedMigrations = new Multimap<Type, string>();
                    }
                }
            }

            _appliedMigrations.Add(contextType, migrationName);
        }

        public IEnumerable<string> GetAppliedMigrations()
        {
            if (_appliedMigrations != null)
            {
                return _appliedMigrations.SelectMany(x => x.Value);
            }

            return Enumerable.Empty<string>();
        }

        public IEnumerable<string> GetAppliedMigrations<TContext>() where TContext : DbContext
        {
            if (_appliedMigrations != null && _appliedMigrations.ContainsKey(typeof(TContext)))
            {
                return _appliedMigrations[typeof(TContext)].AsReadOnly();
            }

            return Enumerable.Empty<string>();
        }
    }
}
