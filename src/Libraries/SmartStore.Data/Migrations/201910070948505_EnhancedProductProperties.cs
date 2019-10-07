namespace SmartStore.Data.Migrations
{
    using SmartStore.Data.Setup;
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EnhancedProductProperties : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "ImportCatalogId", c => c.String());
            AddColumn("dbo.Product", "EClass", c => c.String());
            AddColumn("dbo.Product", "Supplier", c => c.String());
            AddColumn("dbo.Product", "IsDangerousGood", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "IsDangerousGood");
            DropColumn("dbo.Product", "Supplier");
            DropColumn("dbo.Product", "EClass");
            DropColumn("dbo.Product", "ImportCatalogId");
        }

        public bool RollbackOnFailure
        {
            get { return false; }
        }

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Catalog.Products.Fields.ImportCatalogId",
                "Import catalog identifier",
                "Import-Katalog ID",
                "Specifies the import catalog identifier (can be used to specify the import origin).",
                "Legt die Import-Katalog ID fest (Kann zur Identifizierung der Produkt-Herkunft beim Import genutzt werden).");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.EClass",
                "EClass",
                "EClass",
                "Specifies the EClass of the product.",
                "Legt die EClass des Produktes fest.");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.Supplier",
                "Supplier",
                "Anbieter",
                "Specifies the supplier of the product.",
                "Legt den Anbieter des Produktes fest.");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.IsDangerousGood",
                "Is dangerous good",
                "Ist Gefahrgut",
                "Specifies whether this product is a dangerous good.",
                "Legt fest ob das Produkt Gefahrgut ist.");

        }
    }
}
