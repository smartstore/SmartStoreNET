namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Tasks;
	using SmartStore.Data.Setup;

	public partial class QueuedEmailAttachments : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QueuedEmailAttachment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QueuedEmailId = c.Int(nullable: false),
                        StorageLocation = c.Int(nullable: false),
                        Path = c.String(maxLength: 1000),
                        FileId = c.Int(),
                        Data = c.Binary(),
                        Name = c.String(nullable: false, maxLength: 200),
                        MimeType = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Download", t => t.FileId, cascadeDelete: true)
                .ForeignKey("dbo.QueuedEmail", t => t.QueuedEmailId, cascadeDelete: true)
                .Index(t => t.QueuedEmailId)
                .Index(t => t.FileId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QueuedEmailAttachment", "QueuedEmailId", "dbo.QueuedEmail");
            DropForeignKey("dbo.QueuedEmailAttachment", "FileId", "dbo.Download");
            DropIndex("dbo.QueuedEmailAttachment", new[] { "FileId" });
            DropIndex("dbo.QueuedEmailAttachment", new[] { "QueuedEmailId" });
            DropTable("dbo.QueuedEmailAttachment");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			context.Set<ScheduleTask>().AddOrUpdate(x => x.Type,
				new ScheduleTask
				{
					Name = "Clear email queue",
					CronExpression = "0 2 * * *",
					Type = "SmartStore.Services.Messages.QueuedMessagesClearTask, SmartStore.Services",
					Enabled = true,
					StopOnError = false,
				}
			);

			context.SaveChanges();
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.AttachOrderPdfToOrderPlacedEmail",
				"Attach order PDF to 'Order Placed' email",
				"Bei Bestelleingang PDF mitsenden",
				"Dynamically creates and attaches the order PDF to the 'Order Placed' customer notification email.",
				"Erstellt bei Bestelleingang das Auftrags-PDF-Dokument und hängt es der Kunden-Benachrichtigungs-Email an");

			builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.AttachOrderPdfToOrderCompletedEmail",
				"Attach order PDF to 'Order Completed' email",
				"Bei Abschluss einer Bestellung PDF mitsenden",
				"Dynamically creates and attaches the order PDF to the 'Order Completed' customer notification email.",
				"Erstellt bei Abschluss einer Bestellung das Auftrags-PDF-Dokument und hängt es der Kunden-Benachrichtigungs-Email an");

			builder.AddOrUpdate("Admin.System.QueuedEmails.Fields.Attachments",
				"Attachments",
				"Anhänge");

			builder.AddOrUpdate("Admin.System.QueuedEmails.CouldNotDownloadAttachment",
				"Could not download e-mail attachment: no data.",
				"E-Mail Anhang konnte nicht herunterladen: Daten nicht verfügbar.");

			builder.AddOrUpdate("Admin.System.QueuedEmails.ErrorCreatingAttachment",
				"An error occured while creating e-mail attachment",
				"Während der Erstellung des E-Mail-Anhangs ist ein Fehler aufgetreten");

			builder.AddOrUpdate("Admin.System.QueuedEmails.ErrorEmptyAttachmentResult",
				"The e-mail attachment data could not be downloaded from path '{0}'",
				"Daten für den E-Mail Anhang konnten nicht heruntergeladen werden. Pfad: {0}");

			builder.AddOrUpdate("Admin.System.QueuedEmails.ErrorNoPdfAttachment",
				"The content type of the e-mail attachment must be 'application/pdf'",
				"Der Inhaltstyp des E-Mail Anhangs muss 'application/pdf' sein");

			builder.AddOrUpdate("Admin.System.QueuedEmails.List.AttachmentsCount",
				"Number of attachments",
				"Anzahl Anhänge");

			builder.AddOrUpdate("Order.PdfInvoiceFileName",
				"order-{0}.pdf",
				"bestellung-{0}.pdf");
		}
    }
}
