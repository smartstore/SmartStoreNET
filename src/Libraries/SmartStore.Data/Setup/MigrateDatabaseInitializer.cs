using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using SmartStore.Data.Migrations;

namespace SmartStore.Data.Setup
{
	/// <summary>
	///     An implementation of <see cref="IDatabaseInitializer{TContext}" /> that will use Code First Migrations
	///     to update the database to the latest version.
	/// </summary>
	public class MigrateDatabaseInitializer<TContext, TConfig> : IDatabaseInitializer<TContext>
		where TContext : DbContext, new()
		where TConfig : DbMigrationsConfiguration<TContext>, new()
	{

		#region Ctor

		public MigrateDatabaseInitializer()
		{
		}

		public MigrateDatabaseInitializer(string connectionString)
		{
			this.ConnectionString = connectionString;
		}

		#endregion

		#region Properties

		public IEnumerable<IDataSeeder<TContext>> DataSeeders
		{
			get;
			set;
		}

		public IEnumerable<string> TablesToCheck
		{
			get;
			set;
		}

		public string ConnectionString
		{
			get;
			private set;
		}

		#endregion

		#region Interface members

		/// <summary>
		/// Initializes the database.
		/// </summary>
		/// <param name="context">The context.</param>
		public virtual void InitializeDatabase(TContext context)
		{
			if (!context.Database.Exists())
			{
				throw Error.InvalidOperation("Database migration failed because the target database does not exist. Ensure the database was initialized and seeded with the 'InstallDatabaseInitializer'.");
			}

			var config = CreateConfiguration();
			
			var migrator = new DbSeedingMigrator<TContext>(config);
			var tablesExist = CheckTables(context);

			if (tablesExist)
			{
				// Tables specific to the model DO exist in the database...
				var hasHistoryEntry = migrator.GetDatabaseMigrations().Any();
				if (!hasHistoryEntry)
				{
					// ...but there is no entry in the __MigrationHistory table (or __MigrationHistory doesn't exist at all)
					// We MUST assume that the database was created with a previous SmartStore version
					// prior integrating EF Migrations.
					// Running the Migrator with initial DDL would crash in this case as
					// the db objects exist already. Therefore we set a suppression flag
					// which we read in the corresponding InitialMigration to exit early.
					DbMigrationContext.Current.SetSuppressInitialCreate<TContext>(true);
				}
			}

			// run all pending migrations
			var appliedCount = migrator.RunPendingMigrations(context);

			if (appliedCount > 0)
			{
				Seed(context);
			}
			else
			{
				var coreConfig = config as MigrationsConfiguration;
				if (coreConfig != null && context is SmartObjectContext)
				{
					// DB is up-to-date and no migration ran.
					// Call the main Seed method anyway (on every startup),
					// we could have locale resources or settings to add/update.
					coreConfig.SeedDatabase(context as SmartObjectContext);
				}
			}

			// not needed anymore
			this.DataSeeders = null;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Seeds the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		protected virtual void Seed(TContext context)
		{
			if (this.DataSeeders == null)
				return;

			this.DataSeeders.Each((x) => x.Seed(context));
		}

		protected virtual TConfig CreateConfiguration()
		{
			var config = new TConfig();
			if (this.ConnectionString.HasValue())
			{
				var dbContextInfo = new DbContextInfo(typeof(TContext));
				config.TargetDatabase = new DbConnectionInfo(this.ConnectionString, dbContextInfo.ConnectionProviderName);
			}

			return config;
		}

		/// <summary>
		/// Checks tables existence
		/// </summary>
		/// <returns>
		/// Returns <c>true</c> if the tables to check exist in the database or the check list is empty.
		/// </returns>
		protected bool CheckTables(TContext context)
		{
			if (this.TablesToCheck == null || !this.TablesToCheck.Any())
				return true;

			var existingTableNames = new List<string>(context.Database.SqlQuery<string>("SELECT table_name FROM INFORMATION_SCHEMA.TABLES WHERE table_type = 'BASE TABLE'"));
			var result = existingTableNames.Intersect(this.TablesToCheck, StringComparer.InvariantCultureIgnoreCase).Count() == 0;
			return !result;
		}

		#endregion
	}

}
