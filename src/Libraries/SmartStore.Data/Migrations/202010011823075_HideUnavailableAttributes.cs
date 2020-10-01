namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HideUnavailableAttributes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "HideUnavailableAttributes", c => c.Boolean(nullable: false));
            AddColumn("dbo.Product_ProductAttribute_Mapping", "CustomData", c => c.String());
            CreateIndex("dbo.ProductVariantAttributeCombination", new[] { "StockQuantity", "AllowOutOfStockOrders" });
        }
        
        public override void Down()
        {
            DropIndex("dbo.ProductVariantAttributeCombination", new[] { "StockQuantity", "AllowOutOfStockOrders" });
            DropColumn("dbo.Product_ProductAttribute_Mapping", "CustomData");
            DropColumn("dbo.Product", "HideUnavailableAttributes");
        }
    }
}
