namespace SmartStore.Data.Migrations
{
	using System;
    using System.Linq;
	using System.Data.Entity.Migrations;
	using Setup;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Catalog;
	using SmartStore.Core.Domain.Common;
    using SmartStore.Core.Domain.Configuration;
    using SmartStore.Core.Domain.Media;
    using SmartStore.Core.Domain.Tasks;
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
			builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CartProductCount", "Number of products", "Anzahl der Produkte");
			builder.AddOrUpdate("Admin.Rules.FilterDescriptor.Weekday", "Weekday", "Wochentag");

			builder.AddOrUpdate("ShoppingCart.QuantityExceedsStock")
				.Value("de", "Die Bestellmenge übersteigt den Lagerbestand. Es können maximal {0} bestellt werden.");

			builder.AddOrUpdate("Account.CustomerOrders.RecurringOrders.ViewInitialOrder",
				"Order Details (ID - {0})",
				"Bestelldetails (ID - {0})");

			builder.Delete(
				"Account.CustomerOrders.RecurringOrders.InitialOrder",
				"Admin.System.Warnings.NoCustomerRolesDefined",
				"Admin.Configuration.ACL.Permission");

			builder.AddOrUpdate("Enums.SmartStore.Core.Search.IndexingStatus.Unavailable",
				"Unavailable",
				"Nicht vorhanden");
			builder.AddOrUpdate("Enums.SmartStore.Core.Search.IndexingStatus.Idle",
				"Idle",
				"Bereit");
			builder.AddOrUpdate("Enums.SmartStore.Core.Search.IndexingStatus.Rebuilding",
				"Rebuilding",
				"Reindexierend");
			builder.AddOrUpdate("Enums.SmartStore.Core.Search.IndexingStatus.Updating",
				"Updating",
				"Aktualisierend");

			builder.AddOrUpdate("Admin.Packaging.InstallSuccess",
				"Package was uploaded and unzipped successfully. Please reload the list.",
				"Paket wurde hochgeladen und erfolgreich entpackt. Bitte laden Sie die Liste neu.");
		}
	}
}
