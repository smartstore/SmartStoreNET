namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Setup;
    using SmartStore.Core.Data;
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

            if (DataSettings.DatabaseIsInstalled())
            {
                var logTypeMigrator = new ActivityLogTypeMigrator(context);
                logTypeMigrator.AddActivityLogType("EditOrder", "Edit an order", "Auftrag bearbeitet");
            }
        }

        public void MigrateSettings(SmartObjectContext context)
        {

        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Configuration.Settings.GeneralCommon.SocialSettings.TwitterSite.Error",
                "The Twitter username must begin with an '@'.",
                "Der Twitter-Benutzername muss mit einem '@' beginnen.");

            builder.AddOrUpdate("Admin.Configuration.Settings.ShoppingCart.AddProductsToBasketInSinglePositions",
                "Add products to cart in single positions",
                "Produkte in einzelnen Positionen in den Warenkorb legen",
                "Enable this option if you want products with different quantities to be added to the shopping cart in single position.",
                "Aktivieren Sie diese Option, wenn Produkte mit verschiedenen Mengenangaben als Einzelpositionen in den Warenkorb gelegt werden sollen.");
        }
    }
}