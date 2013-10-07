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
using SmartStore.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Data.Hooks;
using Microsoft.SqlServer;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using SmartStore.Core.Infrastructure;

// codehint: sm-add (whole file)

namespace SmartStore.Data
{
    /// <summary>
    /// Object context
    /// </summary>
    public abstract class ObjectContextBase : DbContext, IDbContext
    {
        #region Fields

        private IEnumerable<IHook> _hooks;
        private IList<IPreActionHook> _preHooks;
        private IList<IPostActionHook> _postHooks;

        #endregion

        #region Ctor

        protected ObjectContextBase(string nameOrConnectionString, string alias = null)
            : base(nameOrConnectionString)
        {
            //((IObjectContextAdapter) this).ObjectContext.ContextOptions.LazyLoadingEnabled = true;
            //base.Configuration.ProxyCreationEnabled = false;

            this._preHooks = new List<IPreActionHook>();
            this._postHooks = new List<IPostActionHook>();

            this.Alias = null;
        }

        #endregion

        #region Hooks

        public IEnumerable<IHook> Hooks
        {
            get
            {
                return _hooks ?? Enumerable.Empty<IHook>();
            }
            set
            {
                if (value != null)
                {
                    this._preHooks = value.OfType<IPreActionHook>().ToList();
                    this._postHooks = value.OfType<IPostActionHook>().ToList();
                }
                else
                {
                    this._preHooks.Clear();
                    this._postHooks.Clear();
                }
                _hooks = value;
            }
        }

        /// <summary>
        /// Executes the pre action hooks, filtered by <paramref name="requiresValidation"/>.
        /// </summary>
        /// <param name="modifiedEntries">The modified entries to execute hooks for.</param>
        /// <param name="requiresValidation">if set to <c>true</c> executes hooks that require validation, otherwise executes hooks that do NOT require validation.</param>
        private void ExecutePreActionHooks(IEnumerable<HookedEntityEntry> modifiedEntries, bool requiresValidation)
        {
            foreach (var entityEntry in modifiedEntries)
            {
                var entry = entityEntry; // Prevents access to modified closure

                foreach (
                    var hook in
                        _preHooks.Where(x => x.HookStates == entry.PreSaveState && x.RequiresValidation == requiresValidation))
                {
                    var metadata = new HookEntityMetadata(entityEntry.PreSaveState);
                    hook.HookObject(entityEntry.Entity, metadata);

                    if (metadata.HasStateChanged)
                    {
                        entityEntry.PreSaveState = metadata.State;
                    }
                }
            }
        }

        /// <summary>
        /// Executes the post action hooks.
        /// </summary>
        /// <param name="modifiedEntries">The modified entries to execute hooks for.</param>
        private void ExecutePostActionHooks(IEnumerable<HookedEntityEntry> modifiedEntries)
        {
            foreach (var entityEntry in modifiedEntries)
            {
                foreach (var hook in _postHooks.Where(x => x.HookStates == entityEntry.PreSaveState))
                {
                    var metadata = new HookEntityMetadata(entityEntry.PreSaveState);
                    hook.HookObject(entityEntry.Entity, metadata);
                }
            }
        }

        protected virtual bool HooksEnabled
        {
            get { return true; }
        }

        #endregion

        #region IDbContext members

        public virtual string CreateDatabaseScript()
        {
            return ((IObjectContextAdapter)this).ObjectContext.CreateDatabaseScript();
        }

        public new IDbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity
        {
            return base.Set<TEntity>();
        }

        public virtual IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) where TEntity : BaseEntity, new()
        {
            //HACK: Entity Framework Code First doesn't support output parameters
            //That's why we have to manually create command and execute it.
            //just wait until EF Code First starts support them
            //
            //More info: http://weblogs.asp.net/dwahlin/archive/2011/09/23/using-entity-framework-code-first-with-stored-procedures-that-have-output-parameters.aspx

            bool hasOutputParameters = false;
            if (parameters != null)
            {
                foreach (var p in parameters)
                {
                    var outputP = p as DbParameter;
                    if (outputP == null)
                        continue;

                    if (outputP.Direction == ParameterDirection.InputOutput ||
                        outputP.Direction == ParameterDirection.Output)
                        hasOutputParameters = true;
                }
            }

            var detectChanges = this.AutoDetectChangesEnabled;
            this.AutoDetectChangesEnabled = false;

            IList<TEntity> result = null;
            try
            {
                var context = ((IObjectContextAdapter)(this)).ObjectContext;

                if (!hasOutputParameters)
                {
                    //no output parameters
                    result = this.Database.SqlQuery<TEntity>(commandText, parameters).ToList();
                    for (int i = 0; i < result.Count; i++)
                        result[i] = AttachEntityToContext(result[i]);

                    //return result;

                    //var result = context.ExecuteStoreQuery<TEntity>(commandText, parameters).ToList();
                    //foreach (var entity in result)
                    //    Set<TEntity>().Attach(entity);
                    //return result;
                }
                else
                {

                    //var connection = context.Connection;
                    var connection = this.Database.Connection;
                    //Don't close the connection after command execution


                    //open the connection for use
                    if (connection.State == ConnectionState.Closed)
                        connection.Open();
                    //create a command object
                    using (var cmd = connection.CreateCommand())
                    {
                        //command to execute
                        cmd.CommandText = commandText;
                        cmd.CommandType = CommandType.StoredProcedure;

                        // move parameters to command object
                        if (parameters != null)
                            foreach (var p in parameters)
                                cmd.Parameters.Add(p);

                        //database call
                        var reader = cmd.ExecuteReader();
                        //return reader.DataReaderToObjectList<TEntity>();
                        result = context.Translate<TEntity>(reader).ToList();
                        for (int i = 0; i < result.Count; i++)
                            result[i] = AttachEntityToContext(result[i]);
                        //close up the reader, we're done saving results
                        reader.Close();
                        //return result;
                    }

                }
            }
            finally 
            {
                this.AutoDetectChangesEnabled = detectChanges;
            }

            return result;
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
        public int ExecuteSqlCommand(string sql, int? timeout = null, params object[] parameters)
        {
            Guard.ArgumentNotEmpty(sql, "sql");

            int? previousTimeout = null;
            if (timeout.HasValue)
            {
                //store previous timeout
                previousTimeout = ((IObjectContextAdapter)this).ObjectContext.CommandTimeout;
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = timeout;
            }

            // remove the GO statements
            //sql = Regex.Replace(sql, @"\r{0,1}\n[Gg][Oo]\r{0,1}\n", "\n");

            var result = this.Database.ExecuteSqlCommand(sql, parameters);

            if (timeout.HasValue)
            {
                //Set previous timeout back
                ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = previousTimeout;
            }

            return result;
        }

		/// <summary>Executes sql by using SQL-Server Management Objects which supports GO statements.</summary>
		/// <remarks>codehint: sm-add</remarks>
		public int ExecuteSqlThroughSmo(string sql)
		{
			Guard.ArgumentNotEmpty(sql, "sql");

			var dataSettings = EngineContext.Current.Resolve<DataSettings>();

			int result = 0;
			bool isSqlServerCompact = dataSettings.DataProvider.IsCaseInsensitiveEqual("sqlce");

			if (isSqlServerCompact)
			{
				result = ExecuteSqlCommand(sql);
			}
			else
			{
				using (var sqlConnection = new SqlConnection(dataSettings.DataConnectionString))
				{
					var serverConnection = new ServerConnection(sqlConnection);
					var server = new Server(serverConnection);

					result = server.ConnectionContext.ExecuteNonQuery(sql);
				}
			}
			return result;
		}

        // codehint: sm-add
        public bool HasChanges
        {
            get
            {
                return this.ChangeTracker.Entries()
                           .Where(x => x.State != System.Data.EntityState.Unchanged && x.State != System.Data.EntityState.Detached)
                           .Any();
            }
        }

        // codehint: sm-edit (added Hooks)
        public override int SaveChanges()
        {
            HookedEntityEntry[] modifiedEntries = null;
            bool hooksEnabled = this.HooksEnabled && (_preHooks.Count > 0 || _postHooks.Count > 0);
            
            if (hooksEnabled)
            {
                modifiedEntries = this.ChangeTracker.Entries()
                                .Where(x => x.State != System.Data.EntityState.Unchanged && x.State != System.Data.EntityState.Detached)
                                .Select(x => new HookedEntityEntry()
                                {
                                    Entity = x.Entity,
                                    PreSaveState = (SmartStore.Core.Data.EntityState)((int)x.State)
                                })
                                .ToArray();

                if (_preHooks.Count > 0)
                {
                    // Regardless of validation (possible fixing validation errors too)
                    ExecutePreActionHooks(modifiedEntries, false);
                }
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

            if (hooksEnabled && _preHooks.Count > 0)
            {
                ExecutePreActionHooks(modifiedEntries, true);
            }

            bool validateOnSaveEnabled = this.Configuration.ValidateOnSaveEnabled;
            
            // SAVE NOW!!!
            this.Configuration.ValidateOnSaveEnabled = false;
            int result = this.Commit();
            this.Configuration.ValidateOnSaveEnabled = validateOnSaveEnabled;

            if (hooksEnabled && _postHooks.Count > 0)
            {
                ExecutePostActionHooks(modifiedEntries);
            }

            return result;
        }

        private int Commit()
        {
            int result = 0;
            bool commitFailed = false;
            do
            {
                commitFailed = false;

                try
                {
                    result = base.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    commitFailed = true;

                    foreach (var entry in ex.Entries)
                    {
                        entry.Reload();
                    }
                }
            }
            while (commitFailed);

            return result;
        }

        // codehint: sm-add (required for UoW implementation)
        public string Alias { get; internal set; }

        // codehint: sm-add (performance on bulk inserts)
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

        // codehint: sm-add (performance on bulk inserts)
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

        // codehint: sm-add
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

        #endregion

        #region Utils

        /// <summary>
        /// Attach an entity to the context or return an already attached entity (if it was already attached)
        /// </summary>
        /// <typeparam name="TEntity">TEntity</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Attached entity</returns>
        protected virtual TEntity AttachEntityToContext<TEntity>(TEntity entity) where TEntity : BaseEntity, new()
        {
            //little hack here until Entity Framework really supports stored procedures
            //otherwise, navigation properties of loaded entities are not loaded until an entity is attached to the context
            var alreadyAttached = Set<TEntity>().Local.Where(x => x.Id == entity.Id).FirstOrDefault();
            if (alreadyAttached == null)
            {
                //attach new entity
                Set<TEntity>().Attach(entity);
                return entity;
            }
            else
            {
                //entity is already loaded.
                return alreadyAttached;
            }
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

        #endregion

    }
}