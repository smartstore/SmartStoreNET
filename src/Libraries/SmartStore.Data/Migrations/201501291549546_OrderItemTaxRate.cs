namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class OrderItemTaxRate : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Order", "OrderShippingTaxRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("dbo.Order", "PaymentMethodAdditionalFeeTaxRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("dbo.OrderItem", "TaxRate", c => c.Decimal(nullable: false, precision: 18, scale: 4));
        }
        
        public override void Down()
        {
            DropColumn("dbo.OrderItem", "TaxRate");
            DropColumn("dbo.Order", "PaymentMethodAdditionalFeeTaxRate");
            DropColumn("dbo.Order", "OrderShippingTaxRate");
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
		}
    }
}
