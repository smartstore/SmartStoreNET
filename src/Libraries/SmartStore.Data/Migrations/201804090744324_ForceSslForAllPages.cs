namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Domain.Configuration;
    using SmartStore.Core.Domain.Stores;
    using SmartStore.Data.Setup;

    public partial class ForceSslForAllPages : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            DropIndex("dbo.Product", "Product_SystemName_IsSystemProduct");
            AddColumn("dbo.Store", "ForceSslForAllPages", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Product", "SystemName", c => c.String(maxLength: 400));
            CreateIndex("dbo.Product", new[] { "SystemName", "IsSystemProduct" }, name: "Product_SystemName_IsSystemProduct");
            DropColumn("dbo.Order", "RefundedCreditBalance");
        }

        public override void Down()
        {
            AddColumn("dbo.Order", "RefundedCreditBalance", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            DropIndex("dbo.Product", "Product_SystemName_IsSystemProduct");
            AlterColumn("dbo.Product", "SystemName", c => c.String(maxLength: 500));
            DropColumn("dbo.Store", "ForceSslForAllPages");
            CreateIndex("dbo.Product", new[] { "SystemName", "IsSystemProduct" }, name: "Product_SystemName_IsSystemProduct");
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);

            try
            {
                var stores = context.Set<Store>().ToList();
                var settings = context.Set<Setting>();
                // Do not use a dictionary because of duplicates.
                var sslSettings = settings
                    .Where(x => x.Name == "SecuritySettings.ForceSslForAllPages")
                    .ToList();
                var defaultSetting = sslSettings.FirstOrDefault(x => x.StoreId == 0);

                foreach (var store in stores)
                {
                    var setting = sslSettings.FirstOrDefault(x => x.StoreId == store.Id);
                    if (setting != null)
                    {
                        store.ForceSslForAllPages = setting.Value.ToBool(true);
                    }
                    else if (defaultSetting != null)
                    {
                        store.ForceSslForAllPages = defaultSetting.Value.ToBool(true);
                    }
                    else
                    {
                        store.ForceSslForAllPages = false;
                    }
                }

                // Remove settings because they are not used anymore.
                settings.RemoveRange(sslSettings);
            }
            catch { }

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.Delete("Admin.Configuration.Settings.GeneralCommon.ForceSslForAllPages");

            builder.AddOrUpdate("Admin.Configuration.Stores.Fields.ForceSslForAllPages",
                "Always use SSL",
                "Immer SSL verwenden",
                "Specifies whether to SSL secure all request.",
                "Legt fest, dass alle Anfragen SSL gesichert werden sollen.");
        }
    }
}
