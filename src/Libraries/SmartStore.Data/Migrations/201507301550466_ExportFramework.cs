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
                        Enabled = c.Boolean(nullable: false),
                        ProviderSystemName = c.String(nullable: false, maxLength: 4000),
                        EntityType = c.String(nullable: false, maxLength: 100),
                        SchedulingHours = c.Int(nullable: false),
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
			builder.AddOrUpdate("Common.Enabled",
				"Enabled",
				"Aktiviert");

			builder.AddOrUpdate("Admin.Configuration.Export.Name",
				"Profile name",
				"Profilname",
				"Specifies the name of the export profile.",
				"Legt den Namen des Exportprofils fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.ProviderSystemName",
				"Provider name",
				"Provider-Name",
				"Specifies the name of the export provider.",
				"Legt den Namen des Export-Providers fest.");

			builder.AddOrUpdate("Admin.Configuration.Export.EntityType",
				"Entity type",
				"Entitätstyp",
				"Specifies the type of the entity to be exported.",
				"Legt den Typ der zu exportierenden Entität fest.");

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
		}
    }
}
