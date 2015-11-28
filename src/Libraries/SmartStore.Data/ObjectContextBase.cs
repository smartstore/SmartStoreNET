using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using Microsoft.SqlServer;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Events;
using System.Data.Entity.Core.Objects;

namespace SmartStore.Data
{
    /// <summary>
    /// Object context
    /// </summary>
	[DbConfigurationType(typeof(SmartDbConfiguration))]
    public abstract class ObjectContextBase : DbContext, IDbContext
    {
		private static bool? s_isSqlServer2012OrHigher = null;

        #region Ctor

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
			this.EventPublisher = NullEventPublisher.Instance;
        }

        #endregion

		#region Properties

		public IEventPublisher EventPublisher
		{
			get;
			set;
		}

		#endregion

		#region Hooks

		private readonly IList<DbEntityEntry> _hookedEntries = new List<DbEntityEntry>();

		private void PerformPreSaveActions(out IList<DbEntityEntry> modifiedEntries, out HookedEntityEntry[] modifiedHookEntries)
		{
			modifiedHookEntries = null;

			modifiedEntries = this.ChangeTracker.Entries()
				.Where(x => x.State != System.Data.Entity.EntityState.Unchanged && x.State != System.Data.Entity.EntityState.Detached)
				.Except(_hookedEntries)
				.ToList();

			// prevents stack overflow
			_hookedEntries.AddRange(modifiedEntries);

			var hooksEnabled = this.HooksEnabled && modifiedEntries.Any();
			if (hooksEnabled)
			{
				modifiedHookEntries = modifiedEntries
								.Select(x => new HookedEntityEntry
								{
									Entity = x.Entity,
									PreSaveState = (SmartStore.Core.Data.EntityState)((int)x.State)
								})
								.ToArray();

				// Regardless of validation (possible fixing validation errors too)
				this.EventPublisher.Publish(new PreActionHookEvent { ModifiedEntries = modifiedHookEntries, RequiresValidation = false });
			}

			if (this.Configuration.ValidateOnSaveEnabled)
			{
				var results = from entry in this.ChangeTracker.Entries()
							  where this.ShouldValidateEntity(entry)
							  let validationResult = entry.GetValidationResult()
							  where !validationResult.IsValid
							  select validationResult;

				if (results.Any())
				{

					var fail = new DbEntityValidationException(FormatValidationExceptionMessage(results), results);
					//Debug.WriteLine(fail.Message, fail);
					throw fail;
				}
			}

			if (hooksEnabled)
			{
				this.EventPublisher.Publish(new PreActionHookEvent { ModifiedEntries = modifiedHookEntries, RequiresValidation = true });
			}

			modifiedEntries.Each(x => _hookedEntries.Remove(x));

			IgnoreMergedData(modifiedEntries, true);
		}

		private void PerformPostSaveActions(IList<DbEntityEntry> modifiedEntries, HookedEntityEntry[] modifiedHookEntries)
		{
			IgnoreMergedData(modifiedEntries, false);

			if (this.HooksEnabled && modifiedHookEntries != null && modifiedHookEntries.Any())
			{
				this.EventPublisher.Publish(new PostActionHookEvent { ModifiedEntries = modifiedHookEntries });
			}
		}

		public bool HooksEnabled
		{
			get;
			set;
		}

        #endregion

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
						result[i] = AttachEntity(result[i]);
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
						result[i] = AttachEntity(result[i]);
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
            Guard.ArgumentNotEmpty(sql, "sql");

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
			Guard.ArgumentNotEmpty(sql, "sql");

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
                           .Where(x => x.State != System.Data.Entity.EntityState.Unchanged && x.State != System.Data.Entity.EntityState.Detached)
                           .Any();
            }
        }

		public IDictionary<string, object> GetModifiedProperties(BaseEntity entity)
		{
			var props = new Dictionary<string, object>();

			var entry = this.Entry(entity);
			var modifiedPropertyNames = from p in entry.CurrentValues.PropertyNames
										where entry.Property(p).IsModified
										select p;
			foreach (var name in modifiedPropertyNames)
			{
				props.Add(name, entry.Property(name).OriginalValue);
			}

			return props;
		}

        public override int SaveChanges()
        {
			IList<DbEntityEntry> modifiedEntries;
			HookedEntityEntry[] modifiedHookEntries;
			PerformPreSaveActions(out modifiedEntries, out modifiedHookEntries);

			// SAVE NOW!!!
			bool validateOnSaveEnabled = this.Configuration.ValidateOnSaveEnabled;
			this.Configuration.ValidateOnSaveEnabled = false;
            int result = base.SaveChanges();
            this.Configuration.ValidateOnSaveEnabled = validateOnSaveEnabled;

			PerformPostSaveActions(modifiedEntries, modifiedHookEntries);

            return result;
        }

		public override Task<int> SaveChangesAsync()
		{
			IList<DbEntityEntry> modifiedEntries;
			HookedEntityEntry[] modifiedHookEntries;
			PerformPreSaveActions(out modifiedEntries, out modifiedHookEntries);

			// SAVE NOW!!!
			bool validateOnSaveEnabled = this.Configuration.ValidateOnSaveEnabled;
			this.Configuration.ValidateOnSaveEnabled = false;
			var result = base.SaveChangesAsync();

			result.ContinueWith((t) =>
			{
				this.Configuration.ValidateOnSaveEnabled = validateOnSaveEnabled;
				PerformPostSaveActions(modifiedEntries, modifiedHookEntries);
			});

			return result;
		}

        // codehint: sm-add (required for UoW implementation)
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
		/// <remarks>This helper is called from parameterless DbContext constructors which are required for EF tooling support.</remarks>
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

        /// <summary>
        /// Attach an entity to the context or return an already attached entity (if it was already attached)
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Attached entity</returns>
        protected virtual TEntity AttachEntity<TEntity>(TEntity entity) where TEntity : BaseEntity, new()
        {
			// little hack here until Entity Framework really supports stored procedures
			// otherwise, navigation properties of loaded entities are not loaded until an entity is attached to the context
			var dbSet = Set<TEntity>();
			var alreadyAttached = dbSet.Local.Where(x => x.Id == entity.Id).FirstOrDefault();
			if (alreadyAttached == null)
			{
				// attach new entity
				dbSet.Attach(entity);
				return entity;
			}
			else
			{
				// entity is already loaded.
				return alreadyAttached;
			}
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
			Func<DbEntityEntry, bool> predicate = x => 
			{
				if (x.Entity is TEntity)
				{
					if (x.State == System.Data.Entity.EntityState.Detached)
						return false;

					if (unchangedEntitiesOnly)
						return x.State == System.Data.Entity.EntityState.Unchanged;

					return true;
				}

				return false;
			};
			
			var attachedEntities = this.ChangeTracker.Entries().Where(predicate).ToList();
			attachedEntities.Each(entry => entry.State = System.Data.Entity.EntityState.Detached);
			return attachedEntities.Count;
		}

		public void ChangeState<TEntity>(TEntity entity, System.Data.Entity.EntityState newState) where TEntity : BaseEntity
		{
			this.Entry(entity).State = newState;
		}

		public void ReloadEntity<TEntity>(TEntity entity) where TEntity : BaseEntity
		{
			this.Entry(entity).Reload();
		}

        private string FormatValidationExceptionMessage(IEnumerable<DbEntityValidationResult> results)
        {
            var sb = new StringBuilder();
            sb.Append("Entity validation failed" + Environment.NewLine);

            foreach (var res in results)
            {
                var baseEntity = res.Entry.Entity as BaseEntity;
                sb.AppendFormat("Entity Name: {0} - Id: {0} - State: {1}",
                    res.Entry.Entity.GetType().Name,
                    baseEntity != null ? baseEntity.Id.ToString() : "N/A",
                    res.Entry.State.ToString());
                sb.AppendLine();

                foreach (var validationError in res.ValidationErrors)
                {
                    sb.AppendFormat("\tProperty: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

		private void IgnoreMergedData(IList<DbEntityEntry> entries, bool ignore)
		{
			try
			{
				foreach (var entry in entries)
				{
					var entityWithPossibleMergedData = entry.Entity as IMergedData;
					if (entityWithPossibleMergedData != null)
					{
						entityWithPossibleMergedData.MergedDataIgnore = ignore;
					}
				}
			}
			catch { }
		}

        #endregion

		#region Nested classes

		private class DbContextTransactionWrapper : ITransaction
		{
			private readonly DbContextTransaction _tx;

			public DbContextTransactionWrapper(DbContextTransaction tx)
			{
				Guard.ArgumentNotNull(() => tx);

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