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
        }

        public void MigrateSettings(SmartObjectContext context)
        {

        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Rules.FilterDescriptor.CartItemQuantity", 
                "Product quantity is in range", 
                "Produktmenge liegt in folgendem Bereich");

            builder.AddOrUpdate("Newsletter.SubscriptionFailed",
                "The subscription or unsubscription has failed.",
                "Die Abonnierung bzw. Abbestellung ist fehlgeschlagen.");

            builder.AddOrUpdate("Common.UnsupportedBrowser",
                "You are using an unsupported browser! Please consider switching to a modern browser such as Google Chrome, Firefox or Opera to fully enjoy your shopping experience.",
                "Sie verwenden einen nicht unterstützten Browser! Bitte ziehen Sie in Betracht, zu einem modernen Browser wie Google Chrome, Firefox oder Opera zu wechseln, um Ihr Einkaufserlebnis in vollen Zügen genießen zu können.");

            builder.Delete("Admin.Configuration.Settings.Order.ApplyToSubtotal");
            builder.Delete("Checkout.MaxOrderTotalAmount");
            builder.Delete("Checkout.MinOrderTotalAmount");

            builder.AddOrUpdate("Checkout.MaxOrderSubtotalAmount",
                "The maximum order total allowed is {0}.",
                "Der zulässige Höchstbestellwert beträgt {0}.");

            builder.AddOrUpdate("Checkout.MinOrderSubtotalAmount",
                "The minimum order total allowed is {0}.",
                "Der zulässige Mindestbestellwert beträgt {0}.");

            builder.Delete("Admin.Configuration.Settings.Order.OrderTotalRestrictionType");

            builder.AddOrUpdate("Admin.Configuration.Settings.Order.MultipleOrderTotalRestrictionsExpandRange",
                "Customer groups extend the value range",
                "Kundengruppen erweitern den Wertebereich",
                "Determines whether multiple order total restrictions through customer group assignments extend the allowed order value range.",
                "Bestimmt, ob mehrfache Bestellwertbeschränkungen durch Kundengruppenzuordnungen den erlaubten Bestellwertbereich erweitern.");
        }
    }
}
