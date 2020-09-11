namespace SmartStore.ShippingByWeight.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            //if (DbMigrationContext.Current.SuppressInitialCreate<ShippingByWeightObjectContext>())
            //	return;

            CreateTable(
                "dbo.ShippingByWeight",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    StoreId = c.Int(nullable: false),
                    CountryId = c.Int(nullable: false),
                    ShippingMethodId = c.Int(nullable: false),
                    From = c.Decimal(nullable: false, precision: 18, scale: 2),
                    To = c.Decimal(nullable: false, precision: 18, scale: 2),
                    UsePercentage = c.Boolean(nullable: false),
                    ShippingChargePercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                    ShippingChargeAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                })
                .PrimaryKey(t => t.Id);

        }

        public override void Down()
        {
            DropTable("dbo.ShippingByWeight");
        }
    }
}
