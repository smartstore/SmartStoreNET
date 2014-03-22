using System.Collections.Generic;
using System.Data.Entity;
using System.Threading.Tasks;
using SmartStore.Core;

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

		/// <summary>Executes sql by using SQL-Server Management Objects which supports GO statements.</summary>
		int ExecuteSqlThroughSmo(string sql);

        // codehint: sm-add (required for UoW implementation)
        string Alias { get; }

        // codehint: sm-add (increasing performance on bulk inserts)
        bool ProxyCreationEnabled { get; set; }
        bool AutoDetectChangesEnabled { get; set; }
        bool ValidateOnSaveEnabled { get; set; }
		bool HooksEnabled { get; set; }
        bool HasChanges { get; }

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
        bool IsAttached<TEntity>(TEntity entity) where TEntity : BaseEntity, new();

        /// <summary>
        /// Detaches an entity from the current object context if it's attached
        /// </summary>
        /// <typeparam name="TEntity">Type of entity</typeparam>
        /// <param name="entity">The entity instance to detach</param>
        void DetachEntity<TEntity>(TEntity entity) where TEntity : BaseEntity, new();
		void Detach(object entity);
    }
}
