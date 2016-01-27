namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Setup;

	public partial class ImportFramework1 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            AddColumn("dbo.ImportProfile", "UpdateOnly", c => c.Boolean(nullable: false));
            AddColumn("dbo.ImportProfile", "KeyFieldNames", c => c.String(maxLength: 1000));
            AddColumn("dbo.ImportProfile", "ResultInfo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ImportProfile", "ResultInfo");
            DropColumn("dbo.ImportProfile", "KeyFieldNames");
            DropColumn("dbo.ImportProfile", "UpdateOnly");
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
				"Please select an object that you want to import.",
				"Wählen Sie bitte ein Objekt aus, das Sie importieren möchten.");

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

			builder.AddOrUpdate("Admin.DataExchange.Import.LastImportResult",
				"Last import result",
				"Letztes Importergebnis");

			builder.AddOrUpdate("Admin.Common.TotalRows", "Total rows", "Zeilen insgesamt");
			builder.AddOrUpdate("Admin.Common.Processed", "Processed", "Verarbeitet");
			builder.AddOrUpdate("Admin.Common.NewRecords", "New records", "Neue Datensätze");
			builder.AddOrUpdate("Admin.Common.Updated", "Updated", "Aktualisiert");
			builder.AddOrUpdate("Admin.Common.Warnings", "Warnings", "Warnungen");
			builder.AddOrUpdate("Admin.Common.Errors", "Errors", "Fehler");

			builder.AddOrUpdate("Admin.DataExchange.Import.CompletedEmail.Body",
				"This is an automatic notification of store \"{0}\" about a recent data import.",
				"Dies ist eine automatische Benachrichtung von Shop \"{0}\" über einen erfolgten Datenimport.");

			builder.AddOrUpdate("Admin.DataExchange.Import.CompletedEmail.Subject",
				"Import of profile \"{0}\" has been finished",
				"Import von Profil \"{0}\" ist abgeschlossen");

			builder.AddOrUpdate("Admin.DataExchange.Import.ColumnMapping",
				"Assignment of import fields",
				"Zuordnung der Importfelder");

			builder.AddOrUpdate("Admin.DataExchange.Import.SelectTargetProperty",
				"Create new assignment here",
				"Hier neue Zuordnung vornehmen");

			builder.AddOrUpdate("Admin.DataExchange.Import.UpdateOnly",
				"Only update",
				"Nur aktualisieren",
				"If this option is enabled, only existing data is updated but no new records are added.",
				"Ist diese Option aktiviert, werden nur vorhandene Daten aktualisiert, aber keine neue Datensätze hinzugefügt.");

			builder.AddOrUpdate("Admin.DataExchange.Import.KeyFieldNames",
				"Key fields",
				"Schlüsselfelder",
				"Existing records can be identified on the basis of key fields. The key fields are processed in the order that is defined here.",
				"Anhand von Schlüsselfeldern können vorhandene Datensätze identifiziert werden. Die Schlüsselfelder werden in der hier festgelegten Reihenfolge verarbeitet.");

			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.Validate.OneMappingRequired",
				"At least one field assignment is required.",
				"Es ist mindestens eine Feldzuordnung erforderlich.");

			builder.AddOrUpdate("Admin.DataExchange.ColumnMapping.Validate.MultipleMappedIgnored",
				"The following object properties were multiple assigned and thus ignored: {0}",
				"Die folgenden Objekteigenschaft wurden mehrfach zugeodnet und deshalb ignoriert: {0}");
		}
	}
}
