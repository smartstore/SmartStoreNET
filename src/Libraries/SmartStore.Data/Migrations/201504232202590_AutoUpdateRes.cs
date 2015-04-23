namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class AutoUpdateRes : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
			builder.AddOrUpdate("Admin.CheckUpdate.UpdateNow",
				"Update now",
				"Jetzt aktualisieren");
			builder.AddOrUpdate("Admin.CheckUpdate.AutoUpdatePossible",
				"AutoUpdate possible",
				"AutoUpdate möglich");
			builder.AddOrUpdate("Admin.CheckUpdate.AutoUpdateFailure",
				"Unknown error during package download. Please try again later.",
				"Unbekannter Fehler beim Paket-Download. Bitte versuchen Sie es später erneut.");

			builder.AddOrUpdate("Admin.CheckUpdate.AutoUpdatePossibleInfo",
				@"<p>This update can be installed automatically. For this SmartStore.NET downloads an installation package
					to your webserver, executes it and restarts the application. Before the installation
					your shop directory is backed up, except the folders <i>App_Data</i> and <i>Media</i>, as well as the
					SQL Server database file.
				</p>
				<p>
					Click the <b>Update now</b> button to download and install the package. As an alternative to this, you can
					download the package to your local PC further below and perform the installation at a later time manually.
				</p>",
				@"<p>Dieses Update kann automatisch installiert werden. Hierfür lädt SmartStore.NET ein Installationspaket
					auf Ihren Webserver herunter, führt die Installation durch und startet die Anwendung neu. Vor der Installation wird der
					Verzeichnisinhalt Ihres Shops gesichert, mit Ausnahme der Ordner <i>App_Data</i> und <i>Media</i> sowie der
					SQL Server Datenbank.
				</p>
				<p>
					Klicken Sie die Schaltfläche <b>Jetzt aktualisieren</b>, um das Paket downzuloaden und zu installieren. Alternativ hierzu können
					Sie weiter unten das Paket auf Ihren lokalen PC downloaden und die Installation zu einem späteren Zeitpunkt manuell durchführen.
				</p>");
		}
    }
}
