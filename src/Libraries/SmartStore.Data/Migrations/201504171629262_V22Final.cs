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

			builder.Delete(
				"Reviews.Date",
				"RewardPoints.Fields.Date",
				"Admin.Customers.Customers.RewardPoints.Fields.Date",
				"DownloadableProducts.Fields.Date",
				"Order.ShipmentStatusEvents.Date",
				"PrivateMessages.Inbox.DateColumn",
				"PrivateMessages.Sent.DateColumn");
			
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

			builder.AddOrUpdate("Common.Next",
				"Next",
				"Weiter");
			builder.AddOrUpdate("Admin.Common.BackToConfiguration",
				"Back to configuration",
				"Zurück zur Konfiguration");
			builder.AddOrUpdate("Admin.Common.UploadFileSucceeded",
				"The file has been successfully uploaded.",
				"Die Datei wurde erfolgreich hochgeladen.");
			builder.AddOrUpdate("Admin.Common.UploadFileFailed",
				"The upload has failed.",
				"Der Upload ist leider fehlgeschlagen.");
			builder.AddOrUpdate("Admin.Common.ImportAll",
				"Import all",
				"Alle importieren");
			builder.AddOrUpdate("Admin.Common.ImportSelected",
				"Import selected",
				"Ausgewählte importieren");
			builder.AddOrUpdate("Admin.Common.UnknownError",
				"An unknown error has occurred.",
				"Es ist ein unbekannter Fehler aufgetreten.");
			builder.AddOrUpdate("Plugins.Feed.FreeShippingThreshold",
				"Free shipping threshold",
				"Kostenloser Versand ab",
				"Amount as from shipping is free.",
				"Betrag, ab dem keine Versandkosten anfallen.");
		}
	}
}
