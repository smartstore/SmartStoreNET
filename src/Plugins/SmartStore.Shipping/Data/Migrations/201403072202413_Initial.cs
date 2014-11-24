namespace SmartStore.Shipping.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
	using SmartStore.Data;
	using SmartStore.Data.Setup;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
			if (DbMigrationContext.Current.SuppressInitialCreate<ByTotalObjectContext>())
				return;
			
			CreateTable(
                "dbo.ShippingByTotal",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StoreId = c.Int(nullable: false),
                        CountryId = c.Int(),
                        StateProvinceId = c.Int(),
                        Zip = c.String(maxLength: 400),
                        ShippingMethodId = c.Int(nullable: false),
                        From = c.Decimal(nullable: false, precision: 18, scale: 2),
                        To = c.Decimal(precision: 18, scale: 2),
                        UsePercentage = c.Boolean(nullable: false),
                        ShippingChargePercentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                        ShippingChargeAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        BaseCharge = c.Decimal(nullable: false, precision: 18, scale: 2),
                        MaxCharge = c.Decimal(precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ShippingByTotal");
        }
    }
}
