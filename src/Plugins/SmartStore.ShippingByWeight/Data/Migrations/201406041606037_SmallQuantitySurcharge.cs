namespace SmartStore.ShippingByWeight.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Data;
    using SmartStore.Data.Setup;

    public partial class SmallQuantitySurcharge : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.ShippingByWeight", "SmallQuantitySurcharge", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.ShippingByWeight", "SmallQuantityThreshold", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShippingByWeight", "SmallQuantityThreshold");
            DropColumn("dbo.ShippingByWeight", "SmallQuantitySurcharge");
        }

        public bool RollbackOnFailure
        {
            get { return false; }
        }

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Plugin.Shipping.ByWeight.SmallQuantityThreshold",
                "<br>You're charged with a surcharge of <b class=\"text-success\">{0}</b> because the total of your order hasn't reached <b class=\"text-warning\">{1}</b>.",
                "<br>Es wird Ihnen ein Mindermengenzuschlag von <b class=\"text-success\">{0}</b> berechnet, da Ihr Bestellwert unter <b class=\"text-warning\">{1}</b> liegt.");
            builder.AddOrUpdate("Plugin.Shipping.ByWeight.Fields.SmallQuantitySurcharge",
                "Small quantity surcharge",
                "Mindermengenzuschlag");
            builder.AddOrUpdate("Plugin.Shipping.ByWeight.Fields.SmallQuantitySurcharge.Hint",
                "Determines the value of the small quantity surcharge.",
                "Bestimmt den Wert des Mindermengenzuschlags.");
            builder.AddOrUpdate("Plugin.Shipping.ByWeight.Fields.SmallQuantityThreshold",
                "Threshold for small quantity surcharge",
                "Schwellwert für Mindermengenzuschlag");
            builder.AddOrUpdate("Fields.SmallQuantityThreshold.Hint",
                "Determines the threshold to which the small quantity surcharge will be applied.",
                "Bestimmt den Wert bis zu dem der Mindermengenzuschlag angewendet.");
        }
    }
}
