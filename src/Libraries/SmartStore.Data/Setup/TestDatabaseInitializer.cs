using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;

namespace SmartStore.Data.Tests
{
	public class TestDatabaseInitializer<TContext, TConfig> : IDatabaseInitializer<TContext>
		where TContext : DbContext, new()
		where TConfig : DbMigrationsConfiguration<TContext>, new()
	{
		private readonly string _connectionString;

		public TestDatabaseInitializer(string connectionString)
		{
			Guard.ArgumentNotEmpty(() => connectionString);
			this._connectionString = connectionString;
		}

		protected virtual TConfig CreateConfiguration()
		{
			var config = new TConfig();
			config.TargetDatabase = new DbConnectionInfo(_connectionString, "System.Data.SqlServerCe.4.0");
			return config;
		}


		public void InitializeDatabase(TContext context)
		{
			var config = this.CreateConfiguration();
			new DbMigrator(config).Update();
		}
	}
}
