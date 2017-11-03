using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SmartStore.Core.Data
{
    public interface IDbContext 
    {
        DbSet<TEntity> Set<TEntity>() where TEntity : BaseEntity;

        int SaveChanges();
		Task<int> SaveChangesAsync();

        IList<TEntity> ExecuteStoredProcedureList<TEntity>(string commandText, params object[] parameters) 
            where TEntity : BaseEntity, new();

        /// <summary>
        /// Creates a raw SQL query that will return elements of the given generic type.  The type can be any type that has properties that match the names of the columns returned from the query, or can be a simple primitive type. The type does not have to be an entity type. The results of this query are never tracked by the context even if the type of object returned is an entity type.
        /// </summary>
        /// <typeparam name="TElement">The type of object returned by the query.</typeparam>
        /// <param name="sql">The SQL query string.</param>
        /// <param name="parameters">The parameters to apply to the SQL query string.</param>
        /// <returns>Result</returns>
        IEnumerable<TElement> SqlQuery<TElement>(string sql, params object[] parameters);
 
        /// <summary>
        /// Executes the given DDL/DML command against the database.
        /// </summary>
        /// <param name="sql">The command string</param>
        /// <param name="timeout">Timeout value, in seconds. A null value indicates that the default value of the underlying provider will be used</param>
        /// <param name="parameters">The parameters to apply to the command string.</param>
        /// <returns>The result returned by the database after executing the command.</returns>
        int ExecuteSqlCommand(string sql, bool doNotEnsureTransaction = false, int? timeout = null, params object[] parameters);

        string Alias { get; }

        // increasing performance on bulk operations
        bool ProxyCreationEnabled { get; set; }
		bool LazyLoadingEnabled { get; set; }
		bool AutoDetectChangesEnabled { get; set; }
        bool ValidateOnSaveEnabled { get; set; }
		bool HooksEnabled { get; set; }
        bool HasChanges { get; }

		/// <summary>
		/// Gets or sets a value indicating whether entities returned from queries
		/// or created from stored procedures
		/// should automatically be attached to the <c>DbContext</c>.
		/// </summary>
		/// <remarks>
		/// Set this to <c>true</c> only during long running processes (like export)
		/// </remarks>
		bool ForceNoTracking { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether database write operations
		/// originating from repositories should be committed immediately.
		/// </summary>
		bool AutoCommitEnabled { get; set; }

		/// <summary>
		/// Detects changes made to the properties and relationships of POCO entities. 
		/// Please note that normally DetectChanges is called automatically by many of the methods of 
		/// DbContext and its related classes such that it is rare that this method will need to be called explicitly. 
		/// However, it may be desirable, usually for performance reasons, to turn off this automatic 
		/// calling of DetectChanges using the AutoDetectChangesEnabled flag. 
		/// </summary>
		void DetectChanges();

		/// <summary>
		/// Checks whether the underlying ORM mapper is currently in the process of detecting changes.
		/// </summary>
		/// <returns></returns>
		bool IsDetectingChanges();

		/// <summary>
		/// Gets a value indicating whether the given entity was modified since it has been attached to the context
		/// </summary>
		/// <param name="entity">The entity to check</param>
		/// <returns><c>true</c> if the entity was modified, <c>false</c> otherwise</returns>
		bool IsModified(BaseEntity entity);

		/// <summary>
		/// Determines whether an entity property has changed since it was attached.
		/// </summary>
		/// <param name="entity">Entity</param>
		/// <param name="propertyName">The property name to check</param>
		/// <param name="originalValue">The previous/original property value if change was detected</param>
		/// <returns><c>true</c> if property has changed, <c>false</c> otherwise</returns>
		bool TryGetModifiedProperty(BaseEntity entity, string propertyName, out object originalValue);

		/// <summary>
		/// Gets a list of modified properties for the specified entity
		/// </summary>
		/// <param name="entity">The entity instance for which to get modified properties for</param>
		/// <returns>
		/// A dictionary, where the key is the name of the modified property
		/// and the value is its ORIGINAL value (which was tracked when the entity
		/// was attached to the context the first time)
		/// Returns an empty dictionary if no modification could be detected.
		/// </returns>
		IDictionary<string, object> GetModifiedProperties(BaseEntity entity);

        /// <summary>
        /// Determines whether the given entity is already attached to the current object context
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="entity">The entity instance to attach</param>
        /// <returns><c>true</c> when the entity is attched already, <c>false</c> otherwise</returns>
        bool IsAttached<TEntity>(TEntity entity) where TEntity : BaseEntity;

		/// <summary>
		/// Attaches an entity to the context or returns an already attached entity (if it was already attached)
		/// </summary>
		/// <typeparam name="TEntity">Type of entity</typeparam>
		/// <param name="entity">Entity</param>
		/// <returns>Attached entity</returns>
		TEntity Attach<TEntity>(TEntity entity) where TEntity : BaseEntity;

		/// <summary>
		/// Detaches an entity from the current object context if it's attached
		/// </summary>
		/// <typeparam name="TEntity">Type of entity</typeparam>
		/// <param name="entity">The entity instance to detach</param>
		void DetachEntity<TEntity>(TEntity entity) where TEntity : BaseEntity;

		/// <summary>
		/// Detaches all entities of type <c>TEntity</c> from the current object context
		/// </summary>
		/// <param name="unchangedEntitiesOnly">When <c>true</c>, only entities in unchanged state get detached.</param>
		/// <returns>The count of detached entities</returns>
		int DetachEntities<TEntity>(bool unchangedEntitiesOnly = true) where TEntity : class;

		/// <summary>
		/// Detaches all entities matching the passed <paramref name="predicate"/> from the current object context
		/// </summary>
		/// <param name="unchangedEntitiesOnly">When <c>true</c>, only entities in unchanged state get detached.</param>
		/// <returns>The count of detached entities</returns>
		int DetachEntities(Func<object, bool> predicate, bool unchangedEntitiesOnly = true);

		/// <summary>
		/// Change the state of an entity object
		/// </summary>
		/// <typeparam name="TEntity">Type of entity</typeparam>
		/// <param name="entity">The entity instance</param>
		/// <param name="requestedState">The requested new state</param>
		void ChangeState<TEntity>(TEntity entity, System.Data.Entity.EntityState requestedState) where TEntity : BaseEntity;

		/// <summary>
		/// Reloads the entity from the database overwriting any property values with values from the database. 
		/// The entity will be in the Unchanged state after calling this method. 
		/// </summary>
		/// <typeparam name="TEntity">Type of entity</typeparam>
		/// <param name="entity">The entity instance</param>
		void ReloadEntity<TEntity>(TEntity entity) where TEntity : BaseEntity;

		/// <summary>
		/// Begins a transaction on the underlying store connection using the specified isolation level 
		/// </summary>
		/// <param name="isolationLevel">The database isolation level with which the underlying store transaction will be created</param>
		/// <returns>A transaction object wrapping access to the underlying store's transaction object</returns>
		ITransaction BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);

		/// <summary>
		/// Enables the user to pass in a database transaction created outside of the Database object if you want the Entity Framework to execute commands within that external transaction. Alternatively, pass in null to clear the framework's knowledge of that transaction.
		/// </summary>
		/// <param name="transaction">the external transaction</param>
		void UseTransaction(DbTransaction transaction);
    }

}
