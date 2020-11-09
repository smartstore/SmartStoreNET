using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace SmartStore.Data
{
    public class SqlServerDataProvider : IEfDataProvider
    {
        /// <summary>
        /// Get connection factory
        /// </summary>
        /// <returns>Connection factory</returns>
        public virtual IDbConnectionFactory GetConnectionFactory()
        {
            return new SqlConnectionFactory();
        }

        /// <summary>
        /// A value indicating whether this data provider supports stored procedures
        /// </summary>
        public bool StoredProceduresSupported => true;

        /// <summary>
        /// Gets a support database parameter object (used by stored procedures)
        /// </summary>
        /// <returns>Parameter</returns>
        public DbParameter GetParameter()
        {
            return new SqlParameter();
        }

        public string ProviderInvariantName => "System.Data.SqlClient";
    }
}
