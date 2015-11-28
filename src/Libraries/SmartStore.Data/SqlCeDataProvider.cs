using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using SmartStore.Core.Data;
using SmartStore.Data.Setup;
using SmartStore.Data.Migrations;

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

		public string ProviderInvariantName
		{
			get { return "System.Data.SqlServerCe.4.0"; }
		}
    }
}
