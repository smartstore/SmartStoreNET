namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class V401Resources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
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
