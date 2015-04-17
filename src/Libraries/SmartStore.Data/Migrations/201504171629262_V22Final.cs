namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;
	using System.Linq;

	public partial class V22Final : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Common.Date",
				"Date",
				"Datum");
			builder.AddOrUpdate("Common.MoreInfo",
				"More info",
				"Mehr Info");
			builder.AddOrUpdate("Common.Download",
				"Download",
				"Download");
			
			builder.AddOrUpdate("Admin.CheckUpdate",
				"Check for update",
				"Auf Aktualisierung prüfen");
			builder.AddOrUpdate("Admin.CheckUpdate.UpdateAvailable",
				"Update available",
				"Update verfügbar");
			builder.AddOrUpdate("Admin.CheckUpdate.IsUpToDate",
				"SmartStore.NET is up to date",
				"SmartStore.NET ist auf dem neuesten Stand");
			builder.AddOrUpdate("Admin.CheckUpdate.YourVersion",
				"Your version",
				"Ihre Version");
			builder.AddOrUpdate("Admin.CheckUpdate.CurrentVersion",
				"Current version",
				"Aktuelle Version");
			builder.AddOrUpdate("Admin.CheckUpdate.ReleaseNotes",
				"Release Notes",
				"Release Notes");
			builder.AddOrUpdate("Admin.CheckUpdate.DontNotifyAnymore",
				"Don't notify anymore",
				"Nicht mehr benachrichtigen");
		}
	}
}
