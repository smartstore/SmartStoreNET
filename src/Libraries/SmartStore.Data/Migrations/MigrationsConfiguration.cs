namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using Setup;

	public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			Seed(context);
		}

		protected override void Seed(SmartObjectContext context)
		{
			context.MigrateLocaleResources(MigrateLocaleResources);
			MigrateSettings(context);
        }

		public void MigrateSettings(SmartObjectContext context)
		{
		}

		public void MigrateLocaleResources(LocaleResourcesBuilder builder)
		{
			builder.AddOrUpdate("Admin.Orders.Shipment", "Shipment", "Lieferung");
			builder.AddOrUpdate("Admin.Order", "Order", "Auftrag");

			builder.AddOrUpdate("Admin.Order.ViaShippingMethod", "via {0}", "via {0}");
			builder.AddOrUpdate("Admin.Order.WithPaymentMethod", "with {0}", "per {0}");
			builder.AddOrUpdate("Admin.Order.FromStore", "from {0}", "von {0}");

            builder.AddOrUpdate("Admin.Configuration.Settings.Catalog.MaxItemsToDisplayInCatalogMenu",
                "Max items to display in catalog menu",
                "Maximale Anzahl von Elementen im Katalogmenü",
                "Defines the maximum number of top level items to be displayed in the main catalog menu. All menu items which are exceeding this limit will be placed in a new dropdown menu item.",
                "Legt die maximale Anzahl von Menu-Einträgen der obersten Hierarchie fest, die im Katalogmenü angezeigt werden. Alle weiteren Menu-Einträge werden innerhalb eines neuen Dropdownmenus ausgegeben.");

            builder.AddOrUpdate("CatalogMenu.MoreLink", "More", "Mehr");

            builder.AddOrUpdate("Admin.CatalogSettings.Homepage", "Homepage", "Homepage");
            builder.AddOrUpdate("Admin.CatalogSettings.ProductDisplay", "Product display", "Produktdarstellung");
            builder.AddOrUpdate("Admin.CatalogSettings.Prices", "Prices", "Preise");
            builder.AddOrUpdate("Admin.CatalogSettings.CompareProducts", "Compare products", "Produktvergleich");
        }
	}
}
