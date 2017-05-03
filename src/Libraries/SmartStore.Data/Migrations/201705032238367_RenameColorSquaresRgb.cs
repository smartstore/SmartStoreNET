namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RenameColorSquaresRgb : DbMigration
    {
        public override void Up()
        {
			RenameColumn("dbo.ProductAttributeOption", "ColorSquaresRgb", "Color");
			RenameColumn("dbo.ProductVariantAttributeValue", "ColorSquaresRgb", "Color");
        }
        
        public override void Down()
        {
			RenameColumn("dbo.ProductVariantAttributeValue", "Color", "ColorSquaresRgb");
			RenameColumn("dbo.ProductAttributeOption", "Color", "ColorSquaresRgb");
		}
    }
}
