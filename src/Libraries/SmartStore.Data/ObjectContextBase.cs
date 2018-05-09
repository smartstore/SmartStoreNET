using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using SmartStore.Core.Events;

namespace SmartStore.Data
{
	/// <summary>
	/// Object context
	/// </summary>
	[DbConfigurationType(typeof(SmartDbConfiguration))]
    public abstract partial class ObjectContextBase : DbContext, IDbContext
    {
		private static bool? s_isSqlServer2012OrHigher = null;

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

			if (DataSettings.DatabaseIsInstalled())
			{
				// listen to 'ObjectMaterialized' for load hooking
				((IObjectContextAdapter)this).ObjectContext.ObjectMaterialized += ObjectMaterialized;
			}
		}

		private void ObjectMaterialized(object sender, ObjectMaterializedEventArgs e)
		{
			var entity = e.Entity as BaseEntity;
			if (entity == null)
				return;

			var hookHandler = this.DbHookHandler;
			var importantHooksOnly = !this.HooksEnabled && hookHandler.HasImportantLoadHooks();

			hookHandler.TriggerLoadHooks(entity, importantHooksOnly);
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

        /// <summary>
        /// Executes the given DDL/DML command against the database.
        /// </summary>
        /// <param name="sql">The command string</param>
        /// <param name="timeout">Timeout value, in seconds. A null value indicates that the default value of the underlying provider will be used</param>
        /// <param name="parameters">The parameters to apply to the command string.</param>
        /// <returns>The result returned by the database after executing the command.</returns>
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
                //Set previous timeout back
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = previousTimeout;
            }

            return result;
        }

		/// <summary>Executes sql by using SQL-Server Management Objects which supports GO statements.</summary>
		public int ExecuteSqlThroughSmo(string sql)
		{
			Guard.NotEmpty(sql, "sql");

			int result = 0;

			try
			{
				bool isSqlServer = DataSettings.Current.IsSqlServer;

				if (!isSqlServer)
				{
					result = ExecuteSqlCommand(sql);
				}
				else
				{
					using (var sqlConnection = new SqlConnection(GetConnectionString()))
					{
						var serverConnection = new ServerConnection(sqlConnection);
						var server = new Server(serverConnection);

						result = server.ConnectionContext.ExecuteNonQuery(sql);
					}
				}
			}
			catch (Exception)
			{
				// remove the GO statements
				sql = Regex.Replace(sql, @"\r{0,1}\n[Gg][Oo]\r{0,1}\n", "\n");

				result = ExecuteSqlCommand(sql);
			}
			return result;
		}

        public bool HasChanges
        {
            get
            {
                return this.ChangeTracker.Entries()
                           .Where(x => x.State > System.Data.Entity.EntityState.Unchanged)
                           .Any();
            }
        }

		public IDictionary<string, object> GetModifiedProperties(BaseEntity entity)
		{
			return GetModifiedProperties(this.Entry(entity));
		}

		private IDictionary<string, object> GetModifiedProperties(DbEntityEntry entry)
		{
			var props = new Dictionary<string, object>();

			// be aware of the entity state. you cannot get modified properties for detached entities.
			if (entry.State == System.Data.Entity.EntityState.Modified)
			{
				var detectChangesEnabled = this.Configuration.AutoDetectChangesEnabled;
				var mergeable = entry.Entity as IMergedData;
				var mergedProps = mergeable?.MergedDataValues;
				var ignoreMerge = false;

				if (mergeable != null)
				{
					ignoreMerge = mergeable.MergedDataIgnore;
					mergeable.MergedDataIgnore = true;
				}

				try
				{
					var modifiedProperties = from p in entry.CurrentValues.PropertyNames
											 let prop = entry.Property(p)
											 // prop.IsModified seems to return true even if values are equal
											 where PropIsModified(prop, detectChangesEnabled, mergedProps)
											 select prop;

					foreach (var prop in modifiedProperties)
					{
						props.Add(prop.Name, prop.OriginalValue);
					}
				}
				finally
				{
					if (mergeable != null)
						mergeable.MergedDataIgnore = ignoreMerge;
				}
			}

			//System.Diagnostics.Debug.WriteLine("GetModifiedProperties: " + String.Join(", ", props.Select(x => x.Key)));

			return props;
		}

		private static bool PropIsModified(DbPropertyEntry prop, bool detectChangesEnabled, IDictionary<string, object> mergedProps)
		{
			var propIsModified = prop.IsModified;

			if (detectChangesEnabled && !propIsModified)
				return false; // Perf

			if (propIsModified && mergedProps != null && mergedProps.ContainsKey(prop.Name))
			{
				// EF "thinks" that prop has changed because merged value differs
				return false;
			}

			// INFO: "CurrentValue" cannot be used for entities in the Deleted state.
			// INFO: "OriginalValues" cannot be used for entities in the Added state.
			return detectChangesEnabled 
				? propIsModified
				: !AreEqual(prop.CurrentValue, prop.OriginalValue);
		}

		private static bool AreEqual(object cur, object orig)
		{
			if (cur == null && orig == null)
				return true;

			return orig != null 
				? orig.Equals(cur)
				: cur.Equals(orig);
		}

        // required for UoW implementation
        public string Alias { get; internal set; }

        // performance on bulk inserts
        public bool AutoDetectChangesEnabled
        {
            get
            {
                return this.Configuration.AutoDetectChangesEnabled;
            }
            set
            {
                this.Configuration.AutoDetectChangesEnabled = value;
            }
        }

        // performance on bulk inserts
        public bool ValidateOnSaveEnabled
        {
            get
            {
                return this.Configuration.ValidateOnSaveEnabled;
            }
            set
            {
                this.Configuration.ValidateOnSaveEnabled = value;
            }
        }

        public bool ProxyCreationEnabled
        {
            get
            {
                return this.Configuration.ProxyCreationEnabled;
            }
            set
            {
                this.Configuration.ProxyCreationEnabled = value;
            }
        }

		public bool LazyLoadingEnabled
		{
			get
			{
				return this.Configuration.LazyLoadingEnabled;
			}
			set
			{
				this.Configuration.LazyLoadingEnabled = value;
			}
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
			if (!s_isSqlServer2012OrHigher.HasValue)
			{
				try
				{
					// TODO: actually we should cache this value by connection (string).
					// But fact is: it's quite unlikely that multiple DB versions are used within a single application scope.
					var info = this.GetSqlServerInfo();
					string productVersion = info.ProductVersion;
					int version = productVersion.Split(new char[] { '.' })[0].ToInt();
					s_isSqlServer2012OrHigher = version >= 11;
				}
				catch
				{
					s_isSqlServer2012OrHigher = false;
				}
			}
			
			return s_isSqlServer2012OrHigher.Value;
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

        public void DetachEntity<TEntity>(TEntity entity) where TEntity : BaseEntity
        {
			this.Entry(entity).State = System.Data.Entity.EntityState.Detached;
        }

		public int DetachEntities<TEntity>(bool unchangedEntitiesOnly = true) where TEntity : class
		{
			return DetachEntities(o => o is TEntity, unchangedEntitiesOnly);
		}

		public int DetachEntities(Func<object, bool> predicate, bool unchangedEntitiesOnly = true)
		{
			Guard.NotNull(predicate, nameof(predicate));

			Func<DbEntityEntry, bool> predicate2 = x =>
			{
				if (x.State > System.Data.Entity.EntityState.Detached && predicate(x.Entity))
				{
					return unchangedEntitiesOnly 
						? x.State == System.Data.Entity.EntityState.Unchanged
						: true;
				}

				return false;
			};

			var attachedEntities = this.ChangeTracker.Entries().Where(predicate2).ToList();
			attachedEntities.Each(entry => entry.State = System.Data.Entity.EntityState.Detached);
			return attachedEntities.Count;
		}

		public void ChangeState<TEntity>(TEntity entity, System.Data.Entity.EntityState newState) where TEntity : BaseEntity
		{
			//Console.WriteLine("ChangeState ORIGINAL");
			this.Entry(entity).State = newState;
		}

		public void ReloadEntity<TEntity>(TEntity entity) where TEntity : BaseEntity
		{
			var entry = this.Entry(entity);

			try
			{
				entry.Reload();
			}
			catch
			{
				// Can occur when entity has been detached in the meantime (for whatever fucking reasons)
				if (entry.State == System.Data.Entity.EntityState.Detached)
				{
					entry.State = System.Data.Entity.EntityState.Unchanged;
					entry.Reload();
				}
			}
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