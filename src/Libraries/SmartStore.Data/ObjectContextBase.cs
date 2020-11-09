using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Runtime.CompilerServices;
using SmartStore.ComponentModel;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Utilities;
using EfState = System.Data.Entity.EntityState;

namespace SmartStore.Data
{
    [DbConfigurationType(typeof(SmartDbConfiguration))]
    public abstract partial class ObjectContextBase : DbContext, IDbContext
    {
        private static bool? _isSqlServer2012OrHigher = null;
        private static int? _commandTimeoutInSeconds = CommonHelper.GetAppSetting<int?>("sm:EfCommandTimeout");

        // Instance of the internal ObjectStateManager.TransactionManager
        // required for detecting if EF performs change detection
        private object _transactionManager;

        /// <summary>
        /// Parameterless constructor for tooling support, e.g. EF Migrations.
        /// </summary>
        protected ObjectContextBase()
            : this(GetConnectionString(), null)
        {
        }

        protected ObjectContextBase(string nameOrConnectionString, string alias = null)
            : base(nameOrConnectionString)
        {
            this.HooksEnabled = true;
            this.AutoCommitEnabled = true;
            this.Alias = null;
            this.DbHookHandler = NullDbHookHandler.Instance;

            if (_commandTimeoutInSeconds >= 0 && DataSettings.Current.IsSqlServer)
            {
                Database.CommandTimeout = _commandTimeoutInSeconds;
            }
        }

        public bool HooksEnabled
        {
            get;
            set;
        }

        #region IDbContext members

        public virtual string CreateDatabaseScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }

        public new DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        private IEnumerable<DbParameter> ToParameters(params object[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return Enumerable.Empty<DbParameter>();

            return parameters.Cast<DbParameter>();
        }

        public virtual IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            // Add parameters to command
            var commandText2 = commandText;
            var dbParams = ToParameters(parameters);
            bool firstParam = true;
            bool hasOutputParams = false;
            foreach (var p in dbParams)
            {
                commandText += firstParam ? " " : ", ";
                firstParam = false;

                commandText += "@" + p.ParameterName;
                if (p.Direction == ParameterDirection.InputOutput || p.Direction == ParameterDirection.Output)
                {
                    // output parameter
                    hasOutputParams = true;
                    commandText += " output";
                }
            }


            var isLegacyDb = !this.IsSqlServer2012OrHigher();
            if (isLegacyDb && hasOutputParams)
            {
                // SQL Server 2008 or lower is not capable of handling 
                // stored procedures with output parameters
                return ExecuteStoredProcedureListLegacy<TEntity>(commandText2, dbParams);
            }

            var result = this.Database.SqlQuery<TEntity>(commandText, parameters).ToList();
            if (!ForceNoTracking)
            {
                using (var scope = new DbContextScope(this, autoDetectChanges: false))
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        result[i] = Attach(result[i]);
                    }
                }
            }
            return result;
        }

        private IList<TEntity> ExecuteStoredProcedureListLegacy<TEntity>(string commandText, IEnumerable<DbParameter> parameters) where TEntity : BaseEntity, new()
        {
            var connection = this.Database.Connection;
            // Don't close the connection after command execution

            // open the connection for use
            if (connection.State == ConnectionState.Closed)
                connection.Open();

            // create a command object
            using (var cmd = connection.CreateCommand())
            {
                // command to execute
                cmd.CommandText = commandText;
                cmd.CommandType = CommandType.StoredProcedure;

                // move parameters to command object
                cmd.Parameters.AddRange(parameters.ToArray());

                // database call
                var reader = cmd.ExecuteReader();
                var result = ((IObjectContextAdapter)(this)).ObjectContext.Translate<TEntity>(reader).ToList();
                if (!ForceNoTracking)
                {
                    for (int i = 0; i < result.Count; i++)
                    {
                        result[i] = Attach(result[i]);
                    }
                }
                // close up the reader, we're done saving results
                reader.Close();
                return result;
            }
        }

        /// <summary>
        /// Creates a raw SQL query that will return elements of the given generic type.  
        /// The type can be any type that has properties that match the names of the columns returned from the query, 
        /// or can be a simple primitive type. The type does not have to be an entity type. 
        /// The results of this query are never tracked by the context even if the type of object returned is an entity type.
        /// </summary>
        /// <typeparam name="TElement">The type of object returned by the query.</typeparam>
        /// <param name="sql">The SQL query string.</param>
        /// <param name="parameters">The parameters to apply to the SQL query string.</param>
        /// <returns>Result</returns>
        public IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters)
        {
            return this.Database.SqlQuery<TElement>(sql, parameters);
        }

        public int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            Guard.NotEmpty(sql, "sql");

            int? previousTimeout = null;
            if (timeout.HasValue)
            {
                //store previous timeout
                previousTimeout = ((IObjectContextAdapter)this).ObjectContext.CommandTimeout;
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = timeout;
            }

            var transactionalBehavior = doNotEnsureTransaction
                ? TransactionalBehavior.DoNotEnsureTransaction
                : TransactionalBehavior.EnsureTransaction;
            var result = this.Database.ExecuteSqlCommand(transactionalBehavior, sql, parameters);

            if (timeout.HasValue)
            {
                // Set previous timeout back
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = previousTimeout;
            }

            return result;
        }

        public int? ExecuteSqlCommandSafe(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters)
        {
            try
            {
                return ExecuteSqlCommand(sql, doNotEnsureTransaction, timeout, parameters);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks whether the underlying ORM mapper is currently in the process of detecting changes.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsDetectingChanges()
        {
            if (_transactionManager == null && DataSettings.DatabaseIsInstalled())
            {
                var stateManager = ((IObjectContextAdapter)this).ObjectContext.ObjectStateManager;
                if (stateManager != null)
                {
                    // Get the internal TransactionManager property instance from ObjectStateManager
                    _transactionManager = FastProperty.GetProperty(stateManager.GetType(), "TransactionManager")?.GetValue(stateManager);
                }
            }

            if (_transactionManager != null)
            {
                // Get the "IsDetectChanges" property of the internal TransactionManager
                var prop = FastProperty.GetProperty(_transactionManager.GetType(), "IsDetectChanges");
                if (prop != null)
                {
                    return (bool)prop.GetValue(_transactionManager);
                }
            }

            return false;
        }

        public void DetectChanges()
        {
            base.ChangeTracker.DetectChanges();
        }

        public bool HasChanges => GetChangedEntries().Any();

        public virtual bool IsModified(BaseEntity entity)
        {
            Guard.NotNull(entity, nameof(entity));

            var entry = this.Entry((object)entity);
            return entry.HasChanges(this);
        }

        public bool TryGetModifiedProperty(BaseEntity entity, string propertyName, out object originalValue)
        {
            Guard.NotNull(entity, nameof(entity));

            if (entity.IsTransientRecord())
            {
                originalValue = null;
                return false;
            }

            var entry = this.Entry((object)entity);
            return entry.TryGetModifiedProperty(this, propertyName, out originalValue);
        }

        public IDictionary<string, object> GetModifiedProperties(BaseEntity entity)
        {
            return this.Entry((object)entity).GetModifiedProperties(this);
        }

        // required for UoW implementation
        public string Alias { get; internal set; }

        // performance on bulk inserts
        public bool AutoDetectChangesEnabled
        {
            get => this.Configuration.AutoDetectChangesEnabled;
            set => this.Configuration.AutoDetectChangesEnabled = value;
        }

        // performance on bulk inserts
        public bool ValidateOnSaveEnabled
        {
            get => this.Configuration.ValidateOnSaveEnabled;
            set => this.Configuration.ValidateOnSaveEnabled = value;
        }

        public bool ProxyCreationEnabled
        {
            get => this.Configuration.ProxyCreationEnabled;
            set => this.Configuration.ProxyCreationEnabled = value;
        }

        public bool LazyLoadingEnabled
        {
            get => this.Configuration.LazyLoadingEnabled;
            set => this.Configuration.LazyLoadingEnabled = value;
        }

        public bool ForceNoTracking { get; set; }

        public bool AutoCommitEnabled { get; set; }

        public ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            var dbContextTransaction = this.Database.BeginTransaction(isolationLevel);
            return new DbContextTransactionWrapper(dbContextTransaction);
        }

        public void UseTransaction(DbTransaction transaction)
        {
            this.Database.UseTransaction(transaction);
        }

        private IEnumerable<IMergedData> GetMergeableEntitiesFromChangeTracker()
        {
            return base.ChangeTracker.Entries()
                .Where(x => x.State > EfState.Detached)
                .Select(x => x.Entity)
                .OfType<IMergedData>();
        }

        #endregion

        #region Utils

        /// <summary>
        /// Resolves the connection string from the <c>Settings.txt</c> file
        /// </summary>
        /// <returns>The connection string</returns>
        /// <remarks>This helper is called from parameterless DbContext constructors which are required for EF tooling support or during installation.</remarks>
        public static string GetConnectionString()
        {
            if (DataSettings.Current.IsValid())
            {
                return DataSettings.Current.DataConnectionString;
            }

            throw Error.Application("A connection string could not be resolved for the parameterless constructor of the derived DbContext. Either the database is not installed, or the file 'Settings.txt' does not exist or contains invalid content.");
        }

        protected internal bool IsSqlServer2012OrHigher()
        {
            if (!_isSqlServer2012OrHigher.HasValue)
            {
                try
                {
                    // TODO: actually we should cache this value by connection (string).
                    // But fact is: it's quite unlikely that multiple DB versions are used within a single application scope.
                    var info = this.GetSqlServerInfo();
                    string productVersion = info.ProductVersion;
                    int version = productVersion.Split(new char[] { '.' })[0].ToInt();
                    _isSqlServer2012OrHigher = version >= 11;
                }
                catch
                {
                    _isSqlServer2012OrHigher = false;
                }
            }

            return _isSqlServer2012OrHigher.Value;
        }

        public TEntity Attach<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            var dbSet = Set<TEntity>();
            var alreadyAttached = dbSet.Local.FirstOrDefault(x => x.Id == entity.Id);

            if (alreadyAttached == null)
            {
                dbSet.Attach(entity);
                return entity;
            }

            return alreadyAttached;
        }

        public bool IsAttached<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            if (entity != null)
            {
                return Set<TEntity>().Local.Any(x => x.Id == entity.Id);
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DetachEntity<TEntity>(TEntity entity, bool deep = false) where TEntity : BaseEntity
        {
            return DetachInternal(entity, deep ? new HashSet<BaseEntity>() : null, deep);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int DetachEntities<TEntity>(bool unchangedEntitiesOnly = true, bool deep = false) where TEntity : BaseEntity
        {
            return DetachEntities(o => o is TEntity, unchangedEntitiesOnly, deep);
        }

        public int DetachEntities(Func<BaseEntity, bool> predicate, bool unchangedEntitiesOnly = true, bool deep = false)
        {
            Guard.NotNull(predicate, nameof(predicate));

            var numDetached = 0;

            using (new DbContextScope(this, autoDetectChanges: false, lazyLoading: false))
            {
                var entries = this.ChangeTracker.Entries<BaseEntity>().Where(Match).ToList();

                HashSet<BaseEntity> objSet = deep ? new HashSet<BaseEntity>() : null;

                foreach (var entry in entries)
                {
                    numDetached += DetachInternal(entry, objSet, deep);
                }

                return numDetached;
            }

            bool Match(DbEntityEntry<BaseEntity> entry)
            {
                if (entry.State > EfState.Detached && predicate(entry.Entity))
                {
                    return unchangedEntitiesOnly
                        ? entry.State == EfState.Unchanged
                        : true;
                }

                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int DetachInternal(BaseEntity obj, ISet<BaseEntity> objSet, bool deep)
        {
            if (obj == null)
                return 0;

            return DetachInternal(this.Entry(obj), objSet, deep);
        }

        internal int DetachInternal(DbEntityEntry<BaseEntity> entry, ISet<BaseEntity> objSet, bool deep)
        {
            var obj = entry.Entity;
            int numDetached = 0;

            if (deep)
            {
                // This is to prevent an infinite recursion when the child object has a navigation property
                // that points back to the parent
                if (objSet != null && !objSet.Add(obj))
                    return 0;

                // Recursively detach all navigation properties
                foreach (var prop in FastProperty.GetProperties(obj.GetUnproxiedType()).Values)
                {
                    if (typeof(BaseEntity).IsAssignableFrom(prop.Property.PropertyType))
                    {
                        numDetached += DetachInternal(prop.GetValue(obj) as BaseEntity, objSet, deep);
                    }
                    else if (typeof(IEnumerable<BaseEntity>).IsAssignableFrom(prop.Property.PropertyType))
                    {
                        var val = prop.GetValue(obj);
                        if (val is IEnumerable<BaseEntity> list)
                        {
                            foreach (var item in list.ToList())
                            {
                                numDetached += DetachInternal(item, objSet, deep);
                            }
                        }
                    }
                }
            }

            entry.State = EfState.Detached;
            numDetached++;

            return numDetached;
        }

        public void ChangeState<TEntity>(TEntity entity, EfState requestedState) where TEntity : BaseEntity
        {
            //Console.WriteLine("ChangeState ORIGINAL");
            var entry = this.Entry(entity);

            if (entry.State != requestedState)
            {
                // Only change state when requested state differs,
                // because EF internally sets all properties to modified
                // if necessary, even when requested state equals current state.
                entry.State = requestedState;
            }
        }

        public void ReloadEntity<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
            this.Entry((object)entity).ReloadEntity();
        }

        #endregion

        #region Nested classes

        private class DbContextTransactionWrapper : ITransaction
        {
            private readonly DbContextTransaction _tx;

            public DbContextTransactionWrapper(DbContextTransaction tx)
            {
                Guard.NotNull(tx, nameof(tx));

                _tx = tx;
            }

            public void Commit()
            {
                _tx.Commit();
            }

            public void Rollback()
            {
                if (_tx.UnderlyingTransaction.Connection != null)
                    _tx.Rollback();
            }

            public void Dispose()
            {
                _tx.Dispose();
            }
        }

        #endregion
    }
}