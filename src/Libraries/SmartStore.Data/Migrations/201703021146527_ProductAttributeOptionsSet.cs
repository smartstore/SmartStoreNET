namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProductAttributeOptionsSet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ProductAttributeOptionsSet",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 400),
                        ProductAttributeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ProductAttribute", t => t.ProductAttributeId, cascadeDelete: true)
                .Index(t => t.ProductAttributeId);
            
            CreateTable(
                "dbo.ProductAttributeOption",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ProductAttributeOptionsSetId = c.Int(nullable: false),
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
                .ForeignKey("dbo.ProductAttributeOptionsSet", t => t.ProductAttributeOptionsSetId, cascadeDelete: true)
                .Index(t => t.ProductAttributeOptionsSetId);
            
            AddColumn("dbo.SpecificationAttributeOption", "RangeFilterId", c => c.Int(nullable: false));
            CreateIndex("dbo.Customer", "LastIpAddress", name: "IX_Customer_LastIpAddress");
            CreateIndex("dbo.Customer", "CreatedOnUtc", name: "IX_Customer_CreatedOn");
            CreateIndex("dbo.Customer", "LastActivityDateUtc", name: "IX_Customer_LastActivity");
            CreateIndex("dbo.GenericAttribute", "Key", name: "IX_GenericAttribute_Key");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ProductAttributeOption", "ProductAttributeOptionsSetId", "dbo.ProductAttributeOptionsSet");
            DropForeignKey("dbo.ProductAttributeOptionsSet", "ProductAttributeId", "dbo.ProductAttribute");
            DropIndex("dbo.GenericAttribute", "IX_GenericAttribute_Key");
            DropIndex("dbo.ProductAttributeOption", new[] { "ProductAttributeOptionsSetId" });
            DropIndex("dbo.ProductAttributeOptionsSet", new[] { "ProductAttributeId" });
            DropIndex("dbo.Customer", "IX_Customer_LastActivity");
            DropIndex("dbo.Customer", "IX_Customer_CreatedOn");
            DropIndex("dbo.Customer", "IX_Customer_LastIpAddress");
            DropColumn("dbo.SpecificationAttributeOption", "RangeFilterId");
            DropTable("dbo.ProductAttributeOption");
            DropTable("dbo.ProductAttributeOptionsSet");
        }
    }
}
