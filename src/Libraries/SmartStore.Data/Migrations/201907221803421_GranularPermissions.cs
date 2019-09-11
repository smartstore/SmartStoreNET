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
            builder.AddOrUpdate("Common.Read", "Read", "Lesen");
            builder.AddOrUpdate("Common.Create", "Create", "Erstellen");
            builder.AddOrUpdate("Common.Notify", "Notify", "Benachrichtigen");

            builder.AddOrUpdate("Common.Allow", "Allow", "Erlaubt");
            builder.AddOrUpdate("Common.Deny", "Deny", "Verweigert");
            builder.AddOrUpdate("Common.Inherited", "Inherited", "Geerbt");

            builder.AddOrUpdate("Permissions.DisplayName.DisplayPrice", "Display prices", "Preise anzeigen");
            builder.AddOrUpdate("Permissions.DisplayName.AccessShop", "Access shop", "Zugang zum Shop");
            builder.AddOrUpdate("Permissions.DisplayName.AccessShoppingCart", "Access shoppping cart", "Auf Warenkorb zugreifen");
            builder.AddOrUpdate("Permissions.DisplayName.AccessWishlist", "Access wishlist", "Auf Wunschliste zugreifen");

            builder.AddOrUpdate("Common.ExpandCollapseAll", "Expand\\collapse all", "Alle auf\\zuklappen");

            builder.AddOrUpdate("Admin.Customers.PermissionViewNote",
                "The view shows the permissions that apply to this customer based on the customer roles assigned to him. To change permissions, switch to the relevant <a class=\"alert-link\" href=\"{0}\">customer role</a>.",
                "Die Ansicht zeigt die Rechte, die für diesen Kunden auf Basis der ihm zugeordneten Kundengruppen gelten. Um Rechte zu ändern, wechseln Sie bitte zur betreffenden <a class=\"alert-link\" href=\"{0}\">Kundengruppe</a>.");
        }
    }
}
