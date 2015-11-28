using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Infrastructure.DependencyResolution;
using System.Linq;
using SmartStore.Core.Data;
using SmartStore.Data.Setup;

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
