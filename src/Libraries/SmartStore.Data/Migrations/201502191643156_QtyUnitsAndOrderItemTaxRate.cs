namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class QtyUnitsAndOrderItemTaxRate : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuantityUnit",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Description = c.String(maxLength: 50),
                        DisplayLocale = c.String(maxLength: 50),
                        DisplayOrder = c.Int(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Product", "QuantityUnitId", c => c.Int());
            AddColumn("dbo.Order", "OrderShippingTaxRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("dbo.Order", "PaymentMethodAdditionalFeeTaxRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("dbo.OrderItem", "TaxRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("dbo.ProductVariantAttributeCombination", "QuantityUnitId", c => c.Int());
            AddColumn("dbo.Topic", "TitleTag", c => c.String());
            CreateIndex("dbo.Product", "QuantityUnitId");
            CreateIndex("dbo.ProductVariantAttributeCombination", "QuantityUnitId");
            AddForeignKey("dbo.ProductVariantAttributeCombination", "QuantityUnitId", "dbo.QuantityUnit", "Id");
            AddForeignKey("dbo.Product", "QuantityUnitId", "dbo.QuantityUnit", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Product", "QuantityUnitId", "dbo.QuantityUnit");
            DropForeignKey("dbo.ProductVariantAttributeCombination", "QuantityUnitId", "dbo.QuantityUnit");
            DropIndex("dbo.ProductVariantAttributeCombination", new[] { "QuantityUnitId" });
            DropIndex("dbo.Product", new[] { "QuantityUnitId" });
            DropColumn("dbo.Topic", "TitleTag");
            DropColumn("dbo.ProductVariantAttributeCombination", "QuantityUnitId");
            DropColumn("dbo.OrderItem", "TaxRate");
            DropColumn("dbo.Order", "PaymentMethodAdditionalFeeTaxRate");
            DropColumn("dbo.Order", "OrderShippingTaxRate");
            DropColumn("dbo.Product", "QuantityUnitId");
            DropTable("dbo.QuantityUnit");
        }

		public void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
		}

		public bool RollbackOnFailure
		{
			get { return false; }
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			// QtyUnits
			builder.AddOrUpdate("Admin.Configuration.QuantityUnit.AddNew",
				"Add new quantity unit",
				"Verpackungseinheit hinzufügen");
			builder.AddOrUpdate("Admin.Configuration.QuantityUnit.EditQuantityUnitDetails",
				"Edit quantity unit",
				"Verpackungseinheit bearbeiten");
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


			// OrderItemTaxRate
			builder.AddOrUpdate("Admin.Orders.Products.AddNew.TaxRate",
				"Tax rate",
				"Steuersatz",
				"The tax rate for the product",
				"Die Steuerrate des Produktes");

			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.MaxFilterItemsToDisplay",
				"Maximum filter items",
				"Maximale Anzahl Filtereinträge",
				"Determines the maximum amount of filter items to display",
				"Bestimmt die maximale Anzahl angezeigter Filtereinträge");
			builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.ExpandAllFilterCriteria",
				"Expand all filter groups",
				"Alle Filtergruppen aufklappen",
				"Determines whether all filter groups should be displayed expanded",
				"Legt fest, ob alle Filtergruppen aufgeklappt angezeigt werden sollen");

			builder.AddOrUpdate("Admin.Common.Export.Wait",
				"Please wait while the export is being executed",
				"Bitte haben Sie einen Augenblick Geduld, während der Export durchgeführt wird");

			builder.AddOrUpdate("Admin.ContentManagement.Topics.Fields.TitleTag",
				"Title tag",
				"Titel-Tag",
				"Determines the title tag of the topic",
				"Legt das Tag fest, welches für die Überschrift des Topics ausgegeben wird");
		}
	}
}
