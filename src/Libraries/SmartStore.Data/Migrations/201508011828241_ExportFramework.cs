namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using SmartStore.Core.Domain.Customers;
	using SmartStore.Core.Domain.Security;
	using SmartStore.Data.Setup;

	public partial class ExportFramework : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExportProfile",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        ProviderSystemName = c.String(nullable: false, maxLength: 4000),
                        Enabled = c.Boolean(nullable: false),
                        FileTypeId = c.Int(nullable: false),
                        SchedulingTaskId = c.Int(nullable: false),
                        ProfileGuid = c.Guid(nullable: false),
                        Partitioning = c.String(),
                        Filtering = c.String(),
                        LastExecutionStartUtc = c.DateTime(),
                        LastExecutionEndUtc = c.DateTime(),
                        LastExecutionMessage = c.String(maxLength: 4000),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ScheduleTask", t => t.SchedulingTaskId)
                .Index(t => t.SchedulingTaskId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ExportProfile", "SchedulingTaskId", "dbo.ScheduleTask");
            DropIndex("dbo.ExportProfile", new[] { "SchedulingTaskId" });
            DropTable("dbo.ExportProfile");
        }

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);

			var permissionMigrator = new PermissionMigrator(context);

			permissionMigrator.AddPermission(new PermissionRecord
			{
				Name = "Admin area. Manage Exports",
				SystemName = "ManageExports",
				Category = "Configuration"
			}, new string[] { SystemCustomerRoleNames.Administrators });
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Common.Enabled", "Enabled", "Aktiviert");
			builder.AddOrUpdate("Common.Provider", "Provider", "Provider");
			builder.AddOrUpdate("Common.Profile", "Profile", "Profil");
			builder.AddOrUpdate("Common.Partition", "Partition", "Aufteilung");
			builder.AddOrUpdate("Common.Image", "Image", "Bild");


			builder.AddOrUpdate("Admin.Configuration.Export.ProviderSystemName.Validate",
				"There were no export provider found for system name \"{0}\". A provider is mandatory for an export profile.",
				"Es wurde kein Export-Provider mit dem Systemnamen \"{0}\" gefunden. Ein Provider ist für ein Exportprofil zwingend erforderlich.");

			builder.AddOrUpdate("Admin.Configuration.Export.NoProfiles",
				"There were no export profiles found.",
				"Es wurden keine Exportprofile gefunden.");


			builder.AddOrUpdate("Admin.Configuration.Export.ProviderSystemName",
				"Provider",
				"Provider",
				"Specifies the export provider. It is responsible for the individual formatting of the export data.",
				"Legt den Export-Provider fest. Er ist für die individuelle Formatierung der zu exportierenden Daten zuständig.");

			builder.AddOrUpdate("Admin.Configuration.Export.EntityType",
				"Entity",
				"Entität",
				"The entity type the provider processes.",
				"Der Entitätstyp, den der Provider verarbeitet.");


			builder.AddOrUpdate("Admin.Configuration.Export.Name",
				"Name of profile",
				"Name des Profils",
				"Specifies the name of the export profile.",
				"Legt den Namen des Exportprofils fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.FileType",
				"File type",
				"Dateityp",
				"The file type of the exported data.",
				"Der Dateityp der exportierten Daten.");

			builder.AddOrUpdate("Admin.Configuration.Export.SchedulingHours",
				"Hours (interval)",
				"Stunden (Intervall)",
				"Specifies the interval in hours to which the export should execute automatically.",
				"Legt das Intervall in Stunden fest, zu dem der Export automatisch erfolgen soll.");

			builder.AddOrUpdate("Admin.Configuration.Export.LastExecution",
				"Last execution",
				"Letzte Ausführung",
				"Information about the last execution of the export.",
				"Informationen zur letzten Ausführung des Exports.");


			builder.AddOrUpdate("Admin.Configuration.Export.Segmentation.Offset",
				"Offset",
				"Abstand",
				"Specifies the number of records to be skipped.",
				"Legt die Anzahl der zu überspringenden Datensätze fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Segmentation.Limit",
				"Limit",
				"Begrenzung",
				"Specifies how many records to be loaded per database round-trip.",
				"Legt die Anzahl der Datensätze fest, die pro Datenbankaufruf geladen werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.Segmentation.BatchSize",
				"Batch size",
				"Stapelgröße",
				"Specifies the maximum number of records of one processed batch.",
				"Legt die maximale Anzahl der Datensätze eines Vearbeitungsstapels fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Segmentation.PerStore",
				"Per store",
				"Per Shop",
				"Specifies whether to start a separate run-through for each store.",
				"Legt fest, ob für jeden Shop ein separater Verarbeitungsdurchlauf erfolgen soll.");



			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportFileType.Xml", "XML (Extensible Markup Language)", "XML (Extensible Markup Language)");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportFileType.Xls", "XLS (Microsoft Excel)", "XLS (Microsoft Excel)");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportFileType.Csv", "CSV (Delimiter Separated Values)", "CSV (Trennzeichen getrennt)");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportFileType.Txt", "TXT (Plain text)", "TXT (Einfacher Text)");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportFileType.Pdf", "PDF (Portable Document Format)", "PDF (Portables Dokumentenformat)");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Product", "Product", "Produkt");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Category", "Category", "Warengruppe");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Manufacturer", "Manufacturer", "Hersteller");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Customer", "Customer", "Kunde");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Order", "Order", "Auftrag");
		}
    }
}
