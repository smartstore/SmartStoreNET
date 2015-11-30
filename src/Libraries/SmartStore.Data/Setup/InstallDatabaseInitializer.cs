using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Data.Migrations;
using SmartStore.Utilities;

namespace SmartStore.Data.Setup
{

	/// <summary>
	///     An implementation of <see cref="IDatabaseInitializer{TContext}" /> that will use Code First Migrations
	///     to setup and seed the database.
	/// </summary>
	public class InstallDatabaseInitializer : MigrateDatabaseInitializer<SmartObjectContext, MigrationsConfiguration>
	{
		#region Ctor

		public InstallDatabaseInitializer()
			: base()
		{
		}

		public InstallDatabaseInitializer(string connectionString)
			: base(connectionString)
		{
		}

		#endregion

		#region Interface members

		/// <summary>
		/// Initializes the database.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <inheritdoc />
		public override void InitializeDatabase(SmartObjectContext context)
		{
			// we don't use DbSeedingMigrator here because we don't care
			// about Migration seeds during installation.
			// The installation seeder contains ALL required seed data already.
			var migrator = new DbMigrator(base.CreateConfiguration());

			// Run all migrations including the initial one
			migrator.Update();

			// seed install data
			this.Seed(context);
		}

		#endregion

	}

}
