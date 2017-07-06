namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using Setup;

	public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			Seed(context);
		}

		protected override void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
			MigrateSettings(context);
        }

		public void MigrateSettings(SmartObjectContext context)
		{
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
            builder.AddOrUpdate("Admin.Configuration.Settings.Search.DefaultSortOrderMode",
                "Default product sort order",
                "Standardsortierreihenfolge für Produkte",
                "Specifies the default product sort order in search results.",
                "Legt die Standardsortierreihenfolge für Produkte in den Suchergebnissen fest.");
        }
    }
}
