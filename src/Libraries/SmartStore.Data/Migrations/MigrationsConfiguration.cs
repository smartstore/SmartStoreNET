namespace SmartStore.Data.Migrations
{
	using System;
	using System.Data.Entity.Migrations;
	using Setup;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Catalog;
	using SmartStore.Core.Domain.Common;
	using SmartStore.Utilities;

	public sealed class MigrationsConfiguration : DbMigrationsConfiguration<SmartObjectContext>
	{
		public MigrationsConfiguration()
		{
			AutomaticMigrationsEnabled = false;
			AutomaticMigrationDataLossAllowed = true;
			ContextKey = "SmartStore.Core";

            if (DataSettings.Current.IsSqlServer)
            {
                var commandTimeout = CommonHelper.GetAppSetting<int?>("sm:EfMigrationsCommandTimeout");
                if (commandTimeout.HasValue)
                {
                    CommandTimeout = commandTimeout.Value;
                }

                CommandTimeout = 9999999;
            }
		}

		public void SeedDatabase(SmartObjectContext context)
		{
			using (var scope = new DbContextScope(context, hooksEnabled: false))
			{
				Seed(context);
				scope.Commit();
			}		
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
            builder.AddOrUpdate("Admin.Configuration.Settings.CustomerUser.VatIdValidCustomerRoleId",
                "Customer role for valid Vat-ID",
                "Kundengruppe bei valider Vat-ID",
                "Defines the customer role that is assigned to customers whose Vat ID is valid.",
                "Legt eine Kundengruppe fest, die Kunden zugeordnet wird, deren Vat-ID gültig ist.");

            builder.AddOrUpdate("Admin.Catalog.Products.Fields.Visibility",
                "Visibility",
                "Sichtbarkeit",
                "Limits the visibility of the product. In the case of \"Not visible\", the product only appears as an associated product on the parent product detail page, but without a link to an individual page.",
                "Schränkt die Sichtbarkeit des Produktes ein. Bei \"Nicht sichtbar\" erscheint das Produkt nur noch als verknüpftes Produkt auf der übergeordneten Produktdetailseite, jedoch ohne Verlinkung auf eine eigenständige Seite.");

            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.Full", "Fully visible", "Uneingeschränkt sichtbar");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.SearchResults", "In search results", "In Suchergebnissen");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.ProductPage", "On product detail pages", "Auf Produktdetailseiten");
            builder.AddOrUpdate("Enums.SmartStore.Core.Domain.Catalog.ProductVisibility.Hidden", "Not visible", "Nicht sichtbar");

            builder.Delete(
                "Admin.Catalog.Products.Fields.VisibleIndividually",
                "Admin.Catalog.Products.Fields.VisibleIndividually.Hint");
        }
    }
}
