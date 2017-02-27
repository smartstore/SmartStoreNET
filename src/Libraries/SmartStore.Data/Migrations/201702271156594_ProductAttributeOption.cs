namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProductAttributeOption : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductAttributeOption",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductAttributeId = c.Int(nullable: false),
                        Alias = c.String(maxLength: 100),
                        Name = c.String(maxLength: 4000),
                        PictureId = c.Int(nullable: false),
                        ColorSquaresRgb = c.String(maxLength: 100),
                        PriceAdjustment = c.Decimal(nullable: false, precision: 18, scale: 4),
                        WeightAdjustment = c.Decimal(nullable: false, precision: 18, scale: 4),
                        IsPreSelected = c.Boolean(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        ValueTypeId = c.Int(nullable: false),
                        LinkedProductId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProductAttribute", t => t.ProductAttributeId, cascadeDelete: true)
                .Index(t => t.ProductAttributeId);
            
            AddColumn("dbo.SpecificationAttributeOption", "RangeFilterId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductAttributeOption", "ProductAttributeId", "dbo.ProductAttribute");
            DropIndex("dbo.ProductAttributeOption", new[] { "ProductAttributeId" });
            DropColumn("dbo.SpecificationAttributeOption", "RangeFilterId");
            DropTable("dbo.ProductAttributeOption");
        }
    }
}
