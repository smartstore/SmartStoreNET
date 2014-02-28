using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using SmartStore.Data.Initializers;

namespace SmartStore.Data
{
	public class SqlCeDataProvider : IEfDataProvider
    {
        /// <summary>
        /// Get connection factory
        /// </summary>
        /// <returns>Connection factory</returns>
        public virtual IDbConnectionFactory GetConnectionFactory()
        {
            return new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
        }

        /// <summary>
        /// Get database initializer
        /// </summary>
		public virtual IDatabaseInitializer<SmartObjectContext> GetDatabaseInitializer()
        {
            var initializer = new CreateCeDatabaseIfNotExists<SmartObjectContext>();
			return initializer;
        }

        /// <summary>
        /// A value indicating whether this data provider supports stored procedures
        /// </summary>
        public bool StoredProceduresSupported
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a support database parameter object (used by stored procedures)
        /// </summary>
        /// <returns>Parameter</returns>
        public DbParameter GetParameter()
        {
            return new SqlParameter();
        }
    }
}
