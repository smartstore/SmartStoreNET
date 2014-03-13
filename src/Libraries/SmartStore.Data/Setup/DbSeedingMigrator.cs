using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using System.Text.RegularExpressions;

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

		/// <summary>
		/// Initializes a new instance of the DbMigrator class.
		/// </summary>
		/// <param name="configuration">Configuration to be used for the migration process.</param>
		public DbSeedingMigrator(DbMigrationsConfiguration configuration)
			: base(configuration)
		{ }

		/// <summary>
		/// Migrates the database to the latest version
		/// </summary>
		public void RunPendingMigrations(TContext context)
		{
			var coreSeeders = new List<IDataSeeder<SmartObjectContext>>();
			var externalSeeders = new List<IDataSeeder<TContext>>();
			var isCoreMigration = context is SmartObjectContext;

			// Apply migrations
			foreach (var migrationId in GetPendingMigrations())
			{
				if (MigratorUtils.IsAutomaticMigration(migrationId))
					continue;

				if (!MigratorUtils.IsValidMigrationId(migrationId))
					continue;

				// Resolve and instantiate the DbMigration instance from the assembly
				var migration = MigratorUtils.CreateMigrationInstanceByMigrationId(migrationId, Configuration);
				
				// Seeders for the core DbContext must be run in any case 
				// (e.g. for Resource or Setting updates even from external plugins)
				IDataSeeder<SmartObjectContext> coreSeeder = migration as IDataSeeder<SmartObjectContext>;
				IDataSeeder<TContext> externalSeeder = null;

				if (!isCoreMigration)
				{
					// Context specific seeders should only be resolved
					// when origin is external (e.g. a Plugin)
					externalSeeder = migration as IDataSeeder<TContext>;
				}

				try
				{
					// Call the actual update to execute this migration
					base.Update(migrationId);
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

				if (coreSeeder != null)
					coreSeeders.Add(coreSeeder);

				if (externalSeeder != null)
					externalSeeders.Add(externalSeeder);
			}

			// Apply core data seeders first
			if (coreSeeders.Any())
			{
				var coreContext = isCoreMigration ? context as SmartObjectContext : new SmartObjectContext();
				foreach (var seeder in coreSeeders)
				{
					seeder.Seed(coreContext);
				}
			}

			// Apply external data seeders
			foreach (var seeder in externalSeeders)
			{
				seeder.Seed(context);
			}
		}

	}
}
