namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;
    using SmartStore.Data.Utilities;

    public partial class GranularPermissions : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PermissionRoleMapping",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Allow = c.Boolean(nullable: false),
                        PermissionRecordId = c.Int(nullable: false),
                        CustomerRoleId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerRole", t => t.CustomerRoleId, cascadeDelete: true)
                .ForeignKey("dbo.PermissionRecord", t => t.PermissionRecordId, cascadeDelete: true)
                .Index(t => t.PermissionRecordId)
                .Index(t => t.CustomerRoleId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PermissionRoleMapping", "PermissionRecordId", "dbo.PermissionRecord");
            DropForeignKey("dbo.PermissionRoleMapping", "CustomerRoleId", "dbo.CustomerRole");
            DropIndex("dbo.PermissionRoleMapping", new[] { "CustomerRoleId" });
            DropIndex("dbo.PermissionRoleMapping", new[] { "PermissionRecordId" });
            DropTable("dbo.PermissionRoleMapping");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();

            if (DataSettings.DatabaseIsInstalled())
            {
                DataMigrator.AddGranularPermissions(context);
            }
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Customers.Customers.CustomerRole", "Customer role", "Kundengruppe");

            builder.AddOrUpdate("Common.Read", "Read", "Lesen");
            builder.AddOrUpdate("Common.Create", "Create", "Erstellen");
            builder.AddOrUpdate("Common.Notify", "Notify", "Benachrichtigen");
            builder.AddOrUpdate("Common.Export", "Export", "Export");
            builder.AddOrUpdate("Common.Import", "Import", "Import");

            builder.AddOrUpdate("Permissions.DisplayName.DisplayPrice", "Display prices", "Preise anzeigen");
        }
    }
}
