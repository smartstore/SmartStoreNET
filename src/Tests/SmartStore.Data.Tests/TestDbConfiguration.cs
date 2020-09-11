using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace SmartStore.Data.Tests
{
    public class TestDbConfiguration : DbConfiguration
    {
        public TestDbConfiguration()
        {
            base.SetDefaultConnectionFactory(new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0"));
        }
    }
}
