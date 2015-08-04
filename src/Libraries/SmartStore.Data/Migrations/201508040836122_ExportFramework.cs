namespace SmartStore.Data.Migrations
{
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
                        FolderName = c.String(nullable: false, maxLength: 100),
                        ProviderSystemName = c.String(nullable: false, maxLength: 4000),
                        Enabled = c.Boolean(nullable: false),
                        SchedulingTaskId = c.Int(nullable: false),
                        ProfileGuid = c.Guid(nullable: false),
                        Filtering = c.String(),
                        Offset = c.Int(nullable: false),
                        Limit = c.Int(nullable: false),
                        BatchSize = c.Int(nullable: false),
                        PerStore = c.Boolean(nullable: false),
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
			builder.AddOrUpdate("Common.Filter", "Filter", "Filter");
			builder.AddOrUpdate("Common.Projection", "Projection", "Projektion");
			builder.AddOrUpdate("Common.Publishing", "Publishing", "Veröffentlichung");
			builder.AddOrUpdate("Common.Website", "Website", "Web-Seite");


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


			builder.AddOrUpdate("Admin.Configuration.Export.Offset",
				"Offset",
				"Abstand",
				"Specifies the number of records to be skipped.",
				"Legt die Anzahl der zu überspringenden Datensätze fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.Limit",
				"Limit",
				"Begrenzung",
				"Specifies how many records to be loaded per database round-trip.",
				"Legt die Anzahl der Datensätze fest, die pro Datenbankaufruf geladen werden sollen.");

			builder.AddOrUpdate("Admin.Configuration.Export.BatchSize",
				"Batch size",
				"Stapelgröße",
				"Specifies the maximum number of records of one processed batch.",
				"Legt die maximale Anzahl der Datensätze eines Vearbeitungsstapels fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.PerStore",
				"Per store",
				"Per Shop",
				"Specifies whether to start a separate run-through for each store.",
				"Legt fest, ob für jeden Shop ein separater Verarbeitungsdurchlauf erfolgen soll.");


			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Product", "Product", "Produkt");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Category", "Category", "Warengruppe");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Manufacturer", "Manufacturer", "Hersteller");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Customer", "Customer", "Kunde");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.ExportEntityType.Order", "Order", "Auftrag");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.StoreId",
				"Store",
				"Shop",
				"Filter by store.",
				"Nach Shop filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.CreatedFrom",
				"Created from",
				"Erstellt von",
				"Filter by created date.",
				"Nach dem Erstellungsdatum filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.CreatedTo",
				"Created to",
				"Erstellt bis",
				"Filter by created date.",
				"Nach dem Erstellungsdatum filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.PriceMinimum",
				"Price from",
				"Preis von",
				"Filter by price.",
				"Nach dem Preis filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.PriceMaximum",
				"Price to",
				"Preis bis",
				"Filter by price.",
				"Nach dem Preis filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.AvailabilityMinimum",
				"Availability from",
				"Verfügbar von",
				"Filter by availability quantity.",
				"Nach der Verfügbarkeitsmenge filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.AvailabilityMaximum",
				"Availability to",
				"Verfügbar bis",
				"Filter by availability quantity.",
				"Nach der Verfügbarkeitsmenge filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.IsPublished",
				"Published",
				"Veröffentlicht",
				"Filter by publishing.",
				"Nach Veröffentlichung filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.CategoryIds",
				"Categories",
				"Warengruppen",
				"Filter by categtories.",
				"Nach Warengruppen filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.WithoutCategories",
				"Without category mapping",
				"Ohne Warengruppenzuordnung",
				"Filter by missing category mapping.",
				"Nach fehlender Warengruppenzuordnung filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.ManufacturerIds",
				"Manufacturers",
				"Hersteller",
				"Filter by manufacturers.",
				"Nach Hersteller filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.WithoutManufacturers",
				"Without manufacturer mapping",
				"Ohne Herstellerzuordnung",
				"Filter by missing manufacturer mapping.",
				"Nach fehlender Herstellerzuordnung filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.ProductTagIds",
				"Product tags",
				"Produkt-Tags",
				"Filter by product tags.",
				"Nach Produkt-Tags filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.FeaturedProducts",
				"Only featured products",
				"Nur empfohlene Produkte",
				"Filter by featured products. Is only applied when the filtering by categories and manufacturers.",
				"Nach empfohlenen Produkten filtern. Wird nur bei der Filterung nach Warengruppen und Hersteller angewendet.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.ProductType",
				"Product type",
				"Produkttyp",
				"Filter by product type.",
				"Nach Produkttyp filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.OrderStatus",
				"Order status",
				"Auftragsstatus",
				"Filter by order status.",
				"Nach Auftragsstaus filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.PaymentStatus",
				"Payment status",
				"Zahlungsstatus",
				"Filter by payment status.",
				"Nach Zahlungsstatus filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.ShippingStatus",
				"Shipping status",
				"Versandstatus",
				"Filter by shipping status.",
				"Nach Versandstatus filtern.");

			builder.AddOrUpdate("Admin.Configuration.Export.Filter.CustomerRoleIds",
				"Customer roles",
				"Kundengruppen",
				"Filter by customer roles.",
				"Nach Kundengruppen filtern.");
		}
    }
}
