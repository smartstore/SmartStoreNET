namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class MenuItemMultistore : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.MenuItemRecord", "IconColor", c => c.String(maxLength: 100));
            AddColumn("dbo.MenuItemRecord", "LimitedToStores", c => c.Boolean(nullable: false));
            AddColumn("dbo.MenuItemRecord", "SubjectToAcl", c => c.Boolean(nullable: false));
            CreateIndex("dbo.MenuItemRecord", "LimitedToStores", name: "IX_MenuItem_LimitedToStores");
            CreateIndex("dbo.MenuItemRecord", "SubjectToAcl", name: "IX_MenuItem_SubjectToAcl");
        }

        public override void Down()
        {
            DropIndex("dbo.MenuItemRecord", "IX_MenuItem_SubjectToAcl");
            DropIndex("dbo.MenuItemRecord", "IX_MenuItem_LimitedToStores");
            DropColumn("dbo.MenuItemRecord", "SubjectToAcl");
            DropColumn("dbo.MenuItemRecord", "LimitedToStores");
            DropColumn("dbo.MenuItemRecord", "IconColor");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate(
                "Admin.ContentManagement.Menus.Item.IconColor",
                "Icon color",
                "Icon Farbe",
                "Specifies the color of the icon.",
                "Legt die Farbe des Icons fest.");

            builder.AddOrUpdate("Common.White", "White", "Weiﬂ");
        }
    }
}
