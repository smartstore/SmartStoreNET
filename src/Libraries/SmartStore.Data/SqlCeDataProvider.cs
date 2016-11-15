using System.Data.Common;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;

namespace SmartStore.Data
{
	public class SqlCeDataProvider : IEfDataProvider
    {
        public virtual IDbConnectionFactory GetConnectionFactory()
        {
			return new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
		}

        public bool StoredProceduresSupported
        {
            get { return false; }
        }

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
