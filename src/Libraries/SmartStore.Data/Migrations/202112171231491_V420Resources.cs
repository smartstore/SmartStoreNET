namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;

    public partial class V420Resources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
        }


        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();

            if (DataSettings.DatabaseIsInstalled())
            {
                var logTypeMigrator = new ActivityLogTypeMigrator(context);
                logTypeMigrator.AddActivityLogType("EditOrder", "Edit an order", "Auftrag bearbeitet");
            }
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
