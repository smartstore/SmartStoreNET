namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Catalog;
    using SmartStore.Core.Domain.Configuration;
    using SmartStore.Core.Domain.Directory;
    using SmartStore.Core.Domain.Orders;
    using SmartStore.Data.Setup;

    public partial class DeliveryTimeMinMaxDays : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.DeliveryTime", "MinDays", c => c.Int());
            AddColumn("dbo.DeliveryTime", "MaxDays", c => c.Int());
        }

        public override void Down()
        {
            DropColumn("dbo.DeliveryTime", "MaxDays");
            DropColumn("dbo.DeliveryTime", "MinDays");
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            if (!DataSettings.DatabaseIsInstalled())
            {
                return;
            }

            // Migrate old to new delivery times catalog settings.
            var settings = context.Set<Setting>();
            var showInLists = settings.FirstOrDefault(x => x.Name == "CatalogSettings.ShowDeliveryTimesInProductLists");
            var showInDetail = settings.FirstOrDefault(x => x.Name == "CatalogSettings.ShowDeliveryTimesInProductDetail");
            var showInShoppingCart = settings.FirstOrDefault(x => x.Name == "ShoppingCartSettings.ShowDeliveryTimes");

            context.MigrateSettings(x =>
            {
                if (showInLists != null && showInLists.Value.IsCaseInsensitiveEqual("False"))
                {
                    var key = string.Concat(nameof(CatalogSettings), ".", nameof(CatalogSettings.DeliveryTimesInLists));
                    x.Add(key, DeliveryTimesPresentation.None.ToString());
                }
                if (showInDetail != null && showInDetail.Value.IsCaseInsensitiveEqual("False"))
                {
                    var key = string.Concat(nameof(CatalogSettings), ".", nameof(CatalogSettings.DeliveryTimesInProductDetail));
                    x.Add(key, DeliveryTimesPresentation.None.ToString());
                }
                if (showInShoppingCart != null && showInShoppingCart.Value.IsCaseInsensitiveEqual("False"))
                {
                    var key = string.Concat(nameof(ShoppingCartSettings), ".", nameof(ShoppingCartSettings.DeliveryTimesInShoppingCart));
                    x.Add(key, DeliveryTimesPresentation.None.ToString());
                }
            });

            context.SaveChanges();

            if (showInLists != null)
            {
                settings.Remove(showInLists);
            }
            if (showInDetail != null)
            {
                settings.Remove(showInDetail);
            }
            if (showInShoppingCart != null)
            {
                settings.Remove(showInShoppingCart);
            }

            context.SaveChanges();
        }
    }
}
