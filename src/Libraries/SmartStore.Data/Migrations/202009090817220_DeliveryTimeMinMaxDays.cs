namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Catalog;
    using SmartStore.Core.Domain.Configuration;
    using SmartStore.Core.Domain.Directory;
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
            var prefix = nameof(CatalogSettings) + ".";
            var showInLists = settings.FirstOrDefault(x => x.Name == "CatalogSettings.ShowDeliveryTimesInProductLists");
            var showInDetail = settings.FirstOrDefault(x => x.Name == "CatalogSettings.ShowDeliveryTimesInProductDetail");

            context.MigrateSettings(x =>
            {
                if (showInLists != null && showInLists.Value.IsCaseInsensitiveEqual("False"))
                {
                    x.Add(prefix + nameof(CatalogSettings.DeliveryTimesInLists), DeliveryTimesPresentation.None.ToString());
                }
                if (showInDetail != null && showInDetail.Value.IsCaseInsensitiveEqual("False"))
                {
                    x.Add(prefix + nameof(CatalogSettings.DeliveryTimesInProductDetail), DeliveryTimesPresentation.None.ToString());
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

            context.SaveChanges();
        }
    }
}
