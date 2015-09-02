namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class V222 : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AlterColumn("dbo.Customer", "SystemName", c => c.String(maxLength: 500));
            CreateIndex("dbo.Product", "Deleted");
            CreateIndex("dbo.Category", "Deleted");
            CreateIndex("dbo.Manufacturer", "Deleted");
            CreateIndex("dbo.Customer", "Deleted");
            CreateIndex("dbo.Customer", "SystemName");
            CreateIndex("dbo.Order", "Deleted");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Order", new[] { "Deleted" });
            DropIndex("dbo.Customer", new[] { "SystemName" });
            DropIndex("dbo.Customer", new[] { "Deleted" });
            DropIndex("dbo.Manufacturer", new[] { "Deleted" });
            DropIndex("dbo.Category", new[] { "Deleted" });
            DropIndex("dbo.Product", new[] { "Deleted" });
            AlterColumn("dbo.Customer", "SystemName", c => c.String());
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
            builder.AddOrUpdate("Products.Availability.IsNotActive",
                "Not in assortment",
                "Nicht im Sortiment");

            builder.AddOrUpdate("Address.Fields.Required.Hint",
                "* Input elements with asterisk are required and have to be filled out.",
                "* Eingabefelder mit Sternchen sind Pflichfelder und müssen ausgefüllt werden.");

            builder.AddOrUpdate("Admin.Products.BasePrice.Hint",
                "Please keep in mind, the base price is depending on several factors and will therefore only be calculated reliable in the front end.",
                "Bitte beachten Sie, dass der tatsächliche Grundpreis von verschiedenen Faktoren abhängig ist und erst im Shop zuverlässig berechnet werden kann.");

			builder.AddOrUpdate("Admin.Help.Forums",
                "Community",
				"Community");

			builder.AddOrUpdate("Admin.Help.Documentation",
				"Documentation",
				"Dokumentation");

			builder.AddOrUpdate("Admin.Common.About",
				"About SmartStore.NET",
				"Über SmartStore.NET");
        }
    }
}
