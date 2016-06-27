namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class ImportExcel : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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

			builder.AddOrUpdate("Common.Cancelled",
				"Cancelled",
				"Abgebrochen");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.InProgress",
				"The import is being performed in the background now. You can view the progress or the result of the last completed import in the import dialog at any time.",
				"Der Import läuft jetzt im Hintergrund . Sie können den Fortschritt bzw. das Ergebnis des letzten Importvorganges jederzeit im Import-Dialog einsehen.");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.LastResultTitle",
				"<b>Last import</b>: {0}{1}.",
				"<b>Letzter Import</b>: {0}{1}.");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.ProcessedCount",
				"{0} of {1} rows processed.",
				"{0} von {1} Zeilen verarbeitet.");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.QuickStats",
				"{0} new, {1} updated - with {2} warning(s) and {3} error(s).",
				"{0} neu, {1} aktualisiert - bei {2} Warnung(en) und {3} Fehler(n).");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.ActiveSince",
				"Active since: {0}.",
				"Aktiv seit: {0}.");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.CancelPrompt",
				"Do you really want to cancel the import? Products imported so far will not be removed.",
				"Soll der aktive Importvorgang wirklich abgebrochen werden? Bislang importierte Produkte werden nicht gelöscht.");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.Cancel",
				"Cancel import",
				"Import abbrechen");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.Cancelled",
				"Import process has been cancelled",
				"Import wurde abgebrochen");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.DownloadReport",
				"Download full report...",
				"Vollständigen Bericht runterladen...");

			builder.AddOrUpdate("Admin.Common.ImportFromExcel.NoReportAvailable",
				"No report available",
				"Kein Bericht verfügbar");
		}
	}
}
