namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class ExportFramework1 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.ExportDeployment", "CreateZip", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportDeployment", "HttpTransmissionTypeId", c => c.Int(nullable: false));
            AddColumn("dbo.ExportDeployment", "HttpTransmissionType", c => c.Int(nullable: false));
            AddColumn("dbo.ExportDeployment", "PassiveMode", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportDeployment", "UseSsl", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportProfile", "FileNamePattern", c => c.String(maxLength: 400));
            AddColumn("dbo.ExportProfile", "EmailAccountId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExportProfile", "EmailAccountId");
            DropColumn("dbo.ExportProfile", "FileNamePattern");
            DropColumn("dbo.ExportDeployment", "UseSsl");
            DropColumn("dbo.ExportDeployment", "PassiveMode");
            DropColumn("dbo.ExportDeployment", "HttpTransmissionType");
            DropColumn("dbo.ExportDeployment", "HttpTransmissionTypeId");
            DropColumn("dbo.ExportDeployment", "CreateZip");
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
			builder.AddOrUpdate("Admin.Common.ExportSelected", "Export selected", "Ausgewählte exportieren");
			builder.AddOrUpdate("Admin.Common.ExportAll", "Export all", "Alle exportieren");
			builder.AddOrUpdate("Admin.Common.NoDescriptionAvailable", "No description available", "Keine Beschreibung vorhanden");

			builder.AddOrUpdate("Admin.Configuration.Export.FileNamePattern",
				"Pattern for file names",
				"Muster für Dateinamen",
				"Specifies the pattern for creating file names.",
				"Legt das Muster fest, nach dem Dateinamen erzeugt werden.");

			builder.AddOrUpdate("Admin.Configuration.Export.EmailAccountId",
				"Email notification",
				"E-Mail Benachrichtigung",
				"Specifies the email account used to send a notification message of the completion of the export.",
				"Legt das E-Mail Konto fest, über welches eine Benachrichtigung über die Fertigstellung des Exports verschickt werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Export.CompletedEmailAddresses",
				"Email addresses",
				"E-Mail-Addressen",
				"Specifies the email addresses where to send the notification message.",
				"Legt die E-Mail Addressen fest, an die die Benachrichtigung geschickt werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Export.CompletedEmail.Subject",
				"Export of profile \"{0}\" has been finished",
				"Export von Profile \"{0}\" ist abgeschlossen");

			builder.AddOrUpdate("Admin.Configuration.Export.CompletedEmail.Body",
				"This is an automatic notification of store \"{0}\" about a recent data export. You can disable the sending of this message in the details of the export profile.",
				"Dies ist eine automatische Benachrichtung von Shop \"{0}\" über einen erfolgten Datenexport. Sie können den Versand dieser Mitteilung in den Details des Exportprofils deaktivieren.");

			builder.AddOrUpdate("Admin.Configuration.Export.FolderName",
				"Folder name",
				"Ordnername",
				"Specifies the name of the folder where the data will be exported.",
				"Legt den Namen des Ordners fest, in den die Daten exportiert werden.");

			builder.AddOrUpdate("Admin.Configuration.Export.FolderAndFileName.Validate",
				"Please enter a valid folder and file name.",
				"Bitte einen gültigen Ordner- und Dateinamen eingeben.");


			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.CreateZip",
				"Create ZIP archive",
				"ZIP-Archiv erstellen",
				"Specifies whether to combine the export files in a ZIP archive and only to deploy the archive.",
				"Legt fest, ob die Exportdateien in einem ZIP-Archiv zusammengefasst und nur das Archiv bereitgestellt werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.AttributeCombinationAsProduct",
				"Export attribute combinations",
				"Attributkombinationen exportieren",
				"Specifies whether to export a standalone product for each active attribute combination.",
				"Legt fest, ob für jede aktive Attributkombination ein eigenständiges Produkt exportiert werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.HttpTransmissionType",
				"HTTP transmission type",
				"HTTP Übertragungsart",
				"Specifies how to transmit the export files via HTTP.",
				"Legt fest, aus welcher Art die Exportdateien per HTTP übertragen werden sollen.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportHttpTransmissionType.SimplePost", "Simple POST", "Einfacher POST");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ExportHttpTransmissionType.MultipartFormDataPost", "Multipart form data POST", "Multipart-Form-Data POST");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.PassiveMode",
				"Passive mode",
				"Passiver Modus",
				"Specifies whether to exchange data in active or passive mode.",
				"Legt fest, ob Daten im aktiven oder passiven Modus ausgetauscht werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.UseSsl",
				"Use SSL",
				"SSL verwenden",
				"Specifies whether to use a SSL (Secure Sockets Layer) connection.",
				"Legt fest, ob einen SSL (Secure Sockets Layer) Verbindung genutzt werden soll.");


			builder.AddOrUpdate("Admin.Configuration.Export.Filter.Note",
				"Specify individual filters to limit the exported data.",
				"Legen Sie individuelle Filter fest, um die zu exportierenden Daten einzugrenzen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.Note",
				"The following information will be taken into account during the export and integrated in the process.",
				"Die folgenden Angaben werden beim Export berücksichtigt und an entsprechenden Stellen in den Vorgang eingebunden.");

			builder.AddOrUpdate("Admin.Configuration.Export.Configuration.Note",
				"The following specific information will be taken into account by the provider during the export.",
				"Die folgenden spezifischen Angaben werden durch den Provider beim Export berücksichtigt.");

			builder.AddOrUpdate("Admin.Configuration.Export.Configuration.NotRequired",
				"The export provider <b>{0}</b> requires no further configuration.",
				"Der Export-Provider <b>{0}</b> benötigt keine weitergehende Konfiguration.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.Note",
				"Click <b>Insert</b> to add one or multiple publishing profiles to specify how to further proceed with the export files.",
				"Legen Sie über <b>Hinzufügen</b> ein oder mehrere Veröffentlichungsprofile an, um festzulegen wie mit den Exportdateien weiter zu verfahren ist.");
		}
    }
}
