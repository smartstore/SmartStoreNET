using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SmartStore.Data.Migrations;

namespace SmartStore.Data.Setup
{
	internal static class MigratorUtils
	{
		private static readonly Regex _migrationIdPattern = new Regex(@"\d{15}_.+");
		private const string _migrationTypeFormat = "{0}.{1}, {2}";
		private const string _automaticMigration = "AutomaticMigration";

		/// <summary>
		/// Creates a full type instance for the migration id by using the current migrations namespace
		/// ie: SmartStore.Data.Migrations.34589734533_Initial
		/// </summary>
		/// <param name="migrator">The migrator context</param>
		/// <param name="migrationId">The migration id from the migrations list of the migrator</param>
		/// <returns>The full DbMigration instance</returns>
		public static DbMigration CreateMigrationInstanceByMigrationId(string migrationId, DbMigrationsConfiguration config)
		{
			string migrationTypeName =
				string.Format(_migrationTypeFormat,
							  config.MigrationsNamespace,
							  GetMigrationClassName(migrationId),
							  config.MigrationsAssembly.FullName);

			return CreateTypeInstance<DbMigration>(migrationTypeName);
		}

		/// <summary>
		/// Checks if the migration id is valid
		/// </summary>
		/// <param name="migrationId">The migration id from the migrations list of the migrator</param>
		/// <returns>true if valid, otherwise false</returns>
		/// <remarks>
		/// This snippet has been copied from the EntityFramework source (http://entityframework.codeplex.com/)
		/// </remarks>
		public static bool IsValidMigrationId(string migrationId)
		{
			if (string.IsNullOrWhiteSpace(migrationId))
				return false;

			return _migrationIdPattern.IsMatch(migrationId) || migrationId == DbMigrator.InitialDatabase;
		}

		/// <summary>
		/// Checks if the the migration id belongs to an automatic migration
		/// </summary>
		/// <param name="migrationId">The migration id from the migrations list of the migrator</param>
		/// <returns>true if automatic, otherwise false</returns>
		/// <remarks>
		/// This snippet has been copied from the EntityFramework source (http://entityframework.codeplex.com/)
		/// </remarks>
		public static bool IsAutomaticMigration(string migrationId)
		{
			if (string.IsNullOrWhiteSpace(migrationId))
				return false;

			return migrationId.EndsWith(_automaticMigration, StringComparison.Ordinal);
		}

		/// <summary>
		/// Gets the ClassName from a migration id
		/// </summary>
		/// <param name="migrationId">The migration id from the migrations list of the migrator</param>
		/// <returns>The class name for this migration id</returns>
		/// <remarks>
		/// This snippet has been copied from the EntityFramework source (http://entityframework.codeplex.com/)
		/// </remarks>
		public static string GetMigrationClassName(string migrationId)
		{
			if (string.IsNullOrWhiteSpace(migrationId))
				return string.Empty;

			return migrationId.Substring(16);
		}

		/// <summary>
		/// Creates a new instance of a typename
		/// </summary>
		/// <typeparam name="TType">The type of the return instance</typeparam>
		/// <param name="typeName">The full name (including assembly and namespaces) of the type to create</param>
		/// <returns>
		/// A new instance of the type if it is (or boxable to) <typeparamref name="TType"/>, 
		/// otherwise the default of <typeparamref name="TType"/>
		/// </returns>
		private static TType CreateTypeInstance<TType>(string typeName) where TType : class
		{
			Type classType = Type.GetType(typeName, false);

			if (classType == null)
				return default(TType);

			object newType = Activator.CreateInstance(classType);

			return newType as TType;
		}


		public static void ExecutePendingResourceMigrations(string resPath, SmartObjectContext dbContext)
		{
			Guard.NotNull(dbContext, nameof(dbContext));
			
			string headPath = Path.Combine(resPath, "head.txt");
			if (!File.Exists(headPath))
				return;

			string resHead = File.ReadAllText(headPath).Trim();
			if (!MigratorUtils.IsValidMigrationId(resHead))
				return;

			var migrator = new DbMigrator(new MigrationsConfiguration());
			var migrations = GetPendingResourceMigrations(migrator, resHead);

			foreach (var id in migrations)
			{
				if (IsAutomaticMigration(id))
					continue;

				if (!IsValidMigrationId(id))
					continue;

				// Resolve and instantiate the DbMigration instance from the assembly
				var migration = CreateMigrationInstanceByMigrationId(id, migrator.Configuration);

				var provider = migration as ILocaleResourcesProvider;
				if (provider == null)
					continue;

				var builder = new LocaleResourcesBuilder();
				provider.MigrateLocaleResources(builder);

				var resEntries = builder.Build();
				var resMigrator = new LocaleResourcesMigrator(dbContext);
				resMigrator.Migrate(resEntries);
			}
		}

		private static IEnumerable<string> GetPendingResourceMigrations(DbMigrator migrator, string resHead)
		{
			var local = migrator.GetLocalMigrations();
			var atHead = false;

			if (local.Last().IsCaseInsensitiveEqual(resHead))
				yield break;

			foreach (var id in local)
			{
				if (!atHead)
				{
					if (!id.IsCaseInsensitiveEqual(resHead))
					{
						continue;
					}
					else
					{
						atHead = true;
						continue;
					}
				}

				yield return id;
			}
		}

	}
}
