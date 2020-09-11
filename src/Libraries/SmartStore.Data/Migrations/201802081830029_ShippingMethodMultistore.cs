namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ShippingMethodMultistore : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ShippingMethodRestrictions", "ShippingMethod_Id", "dbo.ShippingMethod");
            DropForeignKey("dbo.ShippingMethodRestrictions", "Country_Id", "dbo.Country");
            DropIndex("dbo.ShippingMethodRestrictions", new[] { "ShippingMethod_Id" });
            DropIndex("dbo.ShippingMethodRestrictions", new[] { "Country_Id" });
            AddColumn("dbo.ShippingMethod", "LimitedToStores", c => c.Boolean(nullable: false));
            AddColumn("dbo.PaymentMethod", "LimitedToStores", c => c.Boolean(nullable: false));
            DropTable("dbo.ShippingMethodRestrictions");
        }

        public override void Down()
        {
            CreateTable(
                "dbo.ShippingMethodRestrictions",
                c => new
                {
                    ShippingMethod_Id = c.Int(nullable: false),
                    Country_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.ShippingMethod_Id, t.Country_Id });

            DropColumn("dbo.PaymentMethod", "LimitedToStores");
            DropColumn("dbo.ShippingMethod", "LimitedToStores");
            CreateIndex("dbo.ShippingMethodRestrictions", "Country_Id");
            CreateIndex("dbo.ShippingMethodRestrictions", "ShippingMethod_Id");
            AddForeignKey("dbo.ShippingMethodRestrictions", "Country_Id", "dbo.Country", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ShippingMethodRestrictions", "ShippingMethod_Id", "dbo.ShippingMethod", "Id", cascadeDelete: true);
        }
    }
}
