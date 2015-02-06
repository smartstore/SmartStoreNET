namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Domain.Directory;
    using SmartStore.Data.Setup;

    public partial class QuantityUnits : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuantityUnit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(nullable: true, maxLength: 50),
                        DisplayLocale = c.String(maxLength: 50),
                        DisplayOrder = c.Int(nullable: false),
                        BizQuantityUnitID = c.Int(nullable: true),
                        IsDefault = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Product", "QuantityUnitId", c => c.Int());
            AddColumn("dbo.ProductVariantAttributeCombination", "QuantityUnitId", c => c.Int());
            CreateIndex("dbo.Product", "QuantityUnitId");
            CreateIndex("dbo.ProductVariantAttributeCombination", "QuantityUnitId");
            AddForeignKey("dbo.Product", "QuantityUnitId", "dbo.QuantityUnit", "Id");
            AddForeignKey("dbo.ProductVariantAttributeCombination", "QuantityUnitId", "dbo.QuantityUnit", "Id");

        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductVariantAttributeCombination", "QuantityUnitId", "dbo.QuantityUnit");
            DropForeignKey("dbo.Product", "QuantityUnitId", "dbo.QuantityUnit");
            DropIndex("dbo.ProductVariantAttributeCombination", new[] { "QuantityUnitId" });
            DropIndex("dbo.Product", new[] { "QuantityUnitId" });
            DropColumn("dbo.ProductVariantAttributeCombination", "QuantityUnitId");
            DropColumn("dbo.Product", "QuantityUnitId");
            DropTable("dbo.QuantityUnit");
        }

        public bool RollbackOnFailure
        {
            get { return false; }
        }

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);

            context.Entry(new QuantityUnit { Name = "Stück", Description = "Stück", DisplayOrder = 1 });
            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Configuration.AddNew",
                "Add new",
                "Hinzufügen");
            builder.AddOrUpdate("Admin.Configuration.BackToList",
                "Back",
                "Zurück");
            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.EditQuantityUnitDetails",
                "Edit details",
                "Details bearbeiten");
            builder.AddOrUpdate("Admin.Configuration.QuantityUnit",
                "Quantity units",
                "Verpackungseinheiten");

            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.Fields.Name",
                "Name",
                "Name");
            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.Fields.Name.Hint",
                "Set the name of quantity unit",
                "Legt den Namen der Verpackungseinheit fest");
            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.Fields.IsDefault",
                "Default quantity unit",
                "Standard-Verpackungseinheit");
            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.Fields.IsDefault.Hint",
                "Sets the default quantity unit",
                "Legt die Standard-Verpackungseinheit fest");


            builder.AddOrUpdate("Admin.Configuration.QuantityUnit.Added",
                "Quantity unit successfully added",
                "Verpackungseinheit wurde erfolgreich zugefügt");
            builder.AddOrUpdate("Admin.Configuration.Quantityunits.Updated",
                "Quantity unit successfully updated",
                "Verpackungseinheit wurde erfolgreich aktualisiert");
            builder.AddOrUpdate("Admin.Catalog.Products.Fields.QuantityUnit",
                "Quantity unit",
                "Verpackungseinheit");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ShowDefaultQuantityUnit",
                "Show default quantity unit",
                "Zeige die Standard-Verpackungseinheit",
				"Show default quantity unit if the product has no quantity unit set.",
				"Zeige die Standard-Verpackungseinheit, falls für das Produkt keine Verpackungseinheit festgelegt ist.");
            
        }
    }
}
