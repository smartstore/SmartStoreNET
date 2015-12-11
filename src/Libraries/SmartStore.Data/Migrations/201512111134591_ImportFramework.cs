namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;
	using Core.Domain.Customers;
	using Core.Domain.Security;
	using Setup;

	public partial class ImportFramework : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
	{
        public override void Up()
        {
            CreateTable(
                "dbo.ImportProfile",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        FolderName = c.String(nullable: false, maxLength: 100),
                        FileName = c.String(nullable: false, maxLength: 400),
                        EntityTypeId = c.Int(nullable: false),
                        Enabled = c.Boolean(nullable: false),
                        Skip = c.Int(nullable: false),
                        Take = c.Int(nullable: false),
                        FileTypeConfiguration = c.String(),
                        ColumnMapping = c.String(),
                        SchedulingTaskId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ScheduleTask", t => t.SchedulingTaskId)
                .Index(t => t.SchedulingTaskId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ImportProfile", "SchedulingTaskId", "dbo.ScheduleTask");
            DropIndex("dbo.ImportProfile", new[] { "SchedulingTaskId" });
            DropTable("dbo.ImportProfile");
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
				Name = "Admin area. Manage Imports",
				SystemName = "ManageImports",
				Category = "Configuration"
			}, new string[] { SystemCustomerRoleNames.Administrators });
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Common.RecordsSkip",
				"Skip",
				"Überspringen",
				"Specifies the number of records to be skipped.",
				"Legt die Anzahl der zu überspringenden Datensätze fest.");

			builder.AddOrUpdate("Admin.Common.ImportFile", "Import file", "Importdatei");
			builder.AddOrUpdate("Admin.Common.ImportFiles", "Import files", "Importdateien");

			builder.AddOrUpdate("Admin.Common.RecordsTake",
				"Limit",
				"Begrenzen",
				"Specifies the maximum number of records to be processed.",
				"Legt die maximale Anzahl der zu verarbeitenden Datensätze fest.");

			builder.AddOrUpdate("Admin.Common.FileTypeMustEqual",
				"The file must be of the type {0}.",
				"Die Datei muss vom Typ {0} sein.");

			builder.AddOrUpdate("Admin.DataExchange.Import.NoProfiles",
				"There were no import profiles found.",
				"Es wurden keine Importprofile gefunden.");

			builder.AddOrUpdate("Admin.DataExchange.Import.Name",
				"Name of profile",
				"Name des Profils",
				"Specifies the name of the import profile.",
				"Legt den Namen des Importprofils fest.");

			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.Product", "Product", "Produkt");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.Customer", "Customer", "Kunde");
			builder.AddOrUpdate("Enums.SmartStore.Core.Domain.DataExchange.ImportEntityType.NewsLetterSubscription", "Newsletter Subscribers", "Newsletter Abonnenten");

			builder.AddOrUpdate("Admin.DataExchange.Import.FileUpload",
				"Upload import file...",
				"Importdatei hochladen...");

			builder.AddOrUpdate("Admin.DataExchange.Import.MissingImportFile",
				"Please upload an import file.",
				"Bitte laden Sie eine Importdatei hoch.");


			builder.Delete(
				"Admin.DataExchange.Export.LastExecution",
				"Admin.DataExchange.Export.Offset",
				"Admin.DataExchange.Export.Limit"
			);
		}
	}
}
