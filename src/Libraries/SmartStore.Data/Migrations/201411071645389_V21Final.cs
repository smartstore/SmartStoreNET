namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data.Setup;

	public partial class V21Final : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
		public override void Up()
		{
			AddColumn("dbo.Product", "IsEsd", c => c.Boolean(nullable: false));
		}

		public override void Down()
		{
			DropColumn("dbo.Product", "IsEsd");
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
			// New stuff from MH
			builder.AddOrUpdate("Products.CallForPrice.GoToForm",
				"Ask for price",
				"Preis anfragen");
			builder.AddOrUpdate("ThemeVar.Alpha.DisplayNavbar",
				"Display Megamenu",
				"Zeige Megamenu");

			// Tax & VAT stuff
			builder.Delete("ShoppingCart.Totals.TaxRateLine");
			builder.AddOrUpdate("ShoppingCart.Totals.SubTotal",
				"Subtotal",
				"Zwischensumme");
			builder.AddOrUpdate("ShoppingCart.Totals.Tax",
				"Tax",
				"MwSt");
			builder.AddOrUpdate("ShoppingCart.Totals.TaxRateLineIncl",
				"Incl. {0} % Tax",
				"inkl. {0} % MwSt");
			builder.AddOrUpdate("ShoppingCart.Totals.TaxRateLineExcl",
				"Plus {0} % Tax",
				"zzgl. {0} % MwSt");

			builder.AddOrUpdate("Admin.Catalog.Products.Fields.IsEsd",
				"Is Electronic Service",
				"Ist elektronische Leistung",
				"Specifies whether the product is an electronic service bound to EU VAT regulations for digital goods (2008/8/EG directive)",
				"Legt fest, ob das Produkt elektronisch vertrieben wird und daher gem‰ﬂ EU Richtlinie 2008/8/EG versteuert werden muss.");
			
		}


	}
}
