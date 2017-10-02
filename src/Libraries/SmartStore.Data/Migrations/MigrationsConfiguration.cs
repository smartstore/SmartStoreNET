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
                "Maximale Anzahl von Elementen in der Katalognavigation",
                "Defines the maximum number of top level items to display in main catalog menu.",
                "Legt die maximale Anzahl von Menu-Einträgen der obersten Hierarchie fest, die in der Katalognavigation angezeigt werden.");

            builder.AddOrUpdate("CatalogMenu.MoreLink", "More", "Mehr");
        }
	}
}
