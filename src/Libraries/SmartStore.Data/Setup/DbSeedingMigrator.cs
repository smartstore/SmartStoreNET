using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Logging;

namespace SmartStore.Data.Setup
{
	/// <summary>
	/// Provides advanced migrations by providing a seeding platform for each migration.
	/// This allows for initial seed data after each new database version (for example when 
	/// deploying new features and you want to include initial data). Seeders will be executing 
	/// in the correct order after all migrations have been completed.
	/// </summary>
	public class DbSeedingMigrator<TContext> : DbMigrator where TContext : DbContext
	{
		private ILogger _logger;

		/// <summary>
		/// Initializes a new instance of the DbMigrator class.
		/// </summary>
		/// <param name="configuration">Configuration to be used for the migration process.</param>
		public DbSeedingMigrator(DbMigrationsConfiguration configuration)
			: base(configuration)
		{ 
		}

		public ILogger Logger
		{
			get
			{
				if (_logger == null)
				{
					try
					{
						_logger = EngineContext.Current.Resolve<ILoggerFactory>().GetLogger(this.GetType());
					}
					catch
					{
						_logger = NullLogger.Instance;
					}
				}

				return _logger;
			}
		}

		/// <summary>
		/// Migrates the database to the latest version
		/// </summary>
		/// <returns>The number of applied migrations</returns>
		public int RunPendingMigrations(TContext context)
		{
			var pendingMigrations = GetPendingMigrations().ToList();
			if (!pendingMigrations.Any())
				return 0;
		
			var coreSeeders = new List<SeederEntry>();
			var externalSeeders = new List<SeederEntry>();
			var isCoreMigration = context is SmartObjectContext;
			var initialMigration = this.GetDatabaseMigrations().LastOrDefault() ?? "[Initial]";
			var lastSuccessfulMigration = initialMigration;

			IDataSeeder<SmartObjectContext> coreSeeder = null;
			IDataSeeder<TContext> externalSeeder = null;

			int result = 0;

			// Apply migrations
			foreach (var migrationId in pendingMigrations)
			{
				if (MigratorUtils.IsAutomaticMigration(migrationId))
					continue;

				if (!MigratorUtils.IsValidMigrationId(migrationId))
					continue;

				// Resolve and instantiate the DbMigration instance from the assembly
				var migration = MigratorUtils.CreateMigrationInstanceByMigrationId(migrationId, Configuration);
				
				// Seeders for the core DbContext must be run in any case 
				// (e.g. for Resource or Setting updates even from external plugins)
				coreSeeder = migration as IDataSeeder<SmartObjectContext>;
				externalSeeder = null;

				if (!isCoreMigration)
				{
					// Context specific seeders should only be resolved
					// when origin is external (e.g. a Plugin)
					externalSeeder = migration as IDataSeeder<TContext>;
				}

				try
				{
					// Call the actual Update() to execute this migration
					Update(migrationId);
					result++;
				}
				catch (AutomaticMigrationsDisabledException)
				{
					if (context is SmartObjectContext)
					{
						throw;
					}

					// DbContexts in plugin assemblies tend to produce
					// this error, but obviously without any negative side-effect.
					// Therefore catch and forget!
					// TODO: (MC) investigate this and implement a cleaner solution
				}
				catch (Exception ex)
				{
					result = 0;
					throw new DbMigrationException(lastSuccessfulMigration, migrationId, ex.InnerException ?? ex, false);
				}

				if (coreSeeder != null)
					coreSeeders.Add(new SeederEntry { 
						DataSeeder = coreSeeder, 
						MigrationId = migrationId,
 						PreviousMigrationId = lastSuccessfulMigration,
					});

				if (externalSeeder != null)
					externalSeeders.Add(new SeederEntry { 
						DataSeeder = externalSeeder, 
						MigrationId = migrationId,
						PreviousMigrationId = lastSuccessfulMigration,
					});

				lastSuccessfulMigration = migrationId;
			}

			// Apply core data seeders first
			SmartObjectContext coreContext = null;
			if (coreSeeders.Any())
			{
				coreContext = isCoreMigration ? context as SmartObjectContext : new SmartObjectContext();
				RunSeeders<SmartObjectContext>(coreSeeders, coreContext);
			}

			// Apply external data seeders
			RunSeeders<TContext>(externalSeeders, context);

			Logger.Info("Database migration successful: {0} >> {1}".FormatInvariant(initialMigration, lastSuccessfulMigration));

			return result;
		}

		private void RunSeeders<T>(IEnumerable<SeederEntry> seederEntries, T ctx) where T : DbContext
		{
			foreach (var seederEntry in seederEntries)
			{
				var seeder = (IDataSeeder<T>)seederEntry.DataSeeder;

				try
				{
					seeder.Seed(ctx);
				}
				catch (Exception ex)
				{
					if (seeder.RollbackOnFailure)
					{
						Update(seederEntry.PreviousMigrationId);
						throw new DbMigrationException(seederEntry.PreviousMigrationId, seederEntry.MigrationId, ex.InnerException ?? ex, true);
					}

					Logger.WarnFormat(ex, "Seed error in migration '{0}'. The error was ignored because no rollback was requested.", seederEntry.MigrationId);
				}
			}
		}

		private void LogError(string initialMigration, string targetMigration, Exception exception)
		{
			Logger.ErrorFormat(exception, "Database migration error: {0} >> {1}", initialMigration, targetMigration);
		}

		private class SeederEntry
		{
			public string PreviousMigrationId { get; set; }
			public string MigrationId { get; set; }
			public object DataSeeder { get; set; }
		}
	}

}
