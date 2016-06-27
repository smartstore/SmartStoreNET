namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Setup;

	public partial class ImportExtraData : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.ImportProfile", "ExtraData", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ImportProfile", "ExtraData");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

            context.MigrateSettings(x =>
            {
                x.Add("MediaSettings.VariantValueThumbPictureSize", "20");
                x.Delete("MediaSettings.AutoCompleteSearchThumbPictureSize");
            });
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.DataExchange.Import.NumberOfPictures",
				"Number of pictures",
				"Anzahl der Bilder",
				"Specifies the number of images per object to be imported.",
				"Legt die Anzahl der zu importierenden Bilder pro Objekt fest.");

			builder.Update("Admin.Configuration.Settings.Catalog.DefaultPageSizeOptions")
				.Value("en", "Number of displayed products per page");

			builder.AddOrUpdate("Admin.Validation.ValueGreaterZero",
				"The value must be greater than 0.",
				"Der Wert muss größer 0 sein.");

			builder.AddOrUpdate("Admin.Configuration.Settings.Order.OrderListPageSize",
				"Number of displayed orders per page",
				"Anzahl der Aufträge pro Seite",
				"Specifies the number of displayed orders per page.",
				"Legt die Anzahl der dargestellten Aufträge pro Seite fest.");

			builder.AddOrUpdate("Admin.System.ScheduleTasks.RunningError",
				"Error while running scheduled task \"{0}\"",
				"Fehler beim Ausführen der Aufgabe \"{0}\"");

			builder.AddOrUpdate("Admin.System.ScheduleTasks.Cancellation",
				"The scheduled task \"{0}\" has been canceled",
				"Die geplante Aufgabe \"{0}\" wurde abgebrochen");

			builder.AddOrUpdate("Admin.Common.HttpStatus",
				"HTTP status {0} ({1}).",
				"HTTP-Status {0} ({1}).");

			builder.AddOrUpdate("Admin.System.Warnings.SitemapReachable.MethodNotAllowed",
				"The reachability of the sitemap could not be validated.",
				"Die Erreichbarkeit der Sitemap konnte nicht überprüft werden.");
		}
	}
}
