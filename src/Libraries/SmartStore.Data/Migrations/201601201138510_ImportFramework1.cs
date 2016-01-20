namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Setup;

	public partial class ImportFramework1 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.ImportProfile", "ResultInfo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ImportProfile", "ResultInfo");
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
			builder.AddOrUpdate("Admin.DataExchange.Import.MultipleFilesSameFileTypeNote",
				"For multiple import files please make sure that they are of the same file type and that the content follows the same pattern (e.g. same column headings).",
				"Bei mehreren Importdateien ist darauf zu achten, dass diese vom selben Dateityp sind und deren Inhalt demselben Schema folgt (z.B. gleiche Spaltenüberschriften).");

			builder.AddOrUpdate("Admin.DataExchange.Import.ProfileEntitySelectNote",
				"Please select the entity that you want to import.",
				"Wählen Sie bitte die Entität aus, die Sie importieren möchten.");

			builder.AddOrUpdate("Admin.DataExchange.Import.ProfileCreationNote",
				"Please upload an import file, enter a meaningful name for the import profile and save.",
				"Laden Sie bitte eine Importdatei hoch, legen Sie einen aussagekräftigen Namen für das Importprofil fest und speichern Sie.");

			builder.AddOrUpdate("Admin.DataExchange.Import.AddAnotherFile",
				"Add import file...",
				"Importdatei hinzufügen...");

			builder.AddOrUpdate("Admin.DataExchange.Import.RunNowNote",
				"The task is now running in the background. You will receive an email as soon as it is completed. The progress can be tracked in the import profile list.",
				"Die Aufgabe wird jetzt im Hintergrund ausgeführt. Sie erhalten eine E-Mail, sobald sie abgeschlossen ist. Den Fortschritt können Sie in der Importprofilliste verfolgen.");

			builder.AddOrUpdate("Admin.DataExchange.Export.RunNowNote",
				"The task is now running in the background. You will receive an email as soon as it is completed. The progress can be tracked in the export profile list.",
				"Die Aufgabe wird jetzt im Hintergrund ausgeführt. Sie erhalten eine E-Mail, sobald sie abgeschlossen ist. Den Fortschritt können Sie in der Exportprofilliste verfolgen.");

			builder.AddOrUpdate("Admin.DataExchange.Import.DefaultProfileNames",
				"My product import;My category import;My customer import;My newsletter subscription import",
				"Mein Produktimport;Mein Warengruppenimport;Mein Kundenimport;Mein Newsletter-Abonnement-Import");
		}
	}
}
