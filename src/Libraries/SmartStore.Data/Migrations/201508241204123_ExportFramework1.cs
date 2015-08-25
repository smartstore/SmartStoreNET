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
            AddColumn("dbo.ExportDeployment", "MultipartForm", c => c.Boolean(nullable: false));
            AddColumn("dbo.ExportProfile", "EmailAccountId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ExportProfile", "EmailAccountId");
            DropColumn("dbo.ExportDeployment", "MultipartForm");
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

			builder.AddOrUpdate("Admin.Configuration.Export.FolderName.Validate",
				"Please enter a valid folder name.",
				"Bitte einen gültigen Ordnernamen eingeben.");


			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.CreateZip",
				"Create ZIP archive",
				"ZIP-Archiv erstellen",
				"Specifies whether to combine the export files in a ZIP archive and only to deploy the archive.",
				"Legt fest, ob die Exportdateien in einem ZIP-Archiv zusammengefasst und nur das Archiv bereitgestellt werden soll.");

			builder.AddOrUpdate("Admin.Configuration.Export.Projection.AttributeCombinationAsProduct",
				"Export attribute combinations",
				"Attributkombinationen exportieren",
				"Specifies whether to export all active attribute combinations as a standalone product in addition to each product.",
				"Legt fest, ob zusätzlich zu jedem Produkt alle seine aktiven Attributkombinationen als eigenständiges Produkt exportiert werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Deployment.MultipartForm",
				"Multipart form",
				"Multipart-Form",
				"Specifies whether to transmit the export files as multipart form data via HTTP.",
				"Legt fest, ob die Exportdateien als Multipart-Form-Data per HTTP übertragen werden sollen.");
		}
    }
}
