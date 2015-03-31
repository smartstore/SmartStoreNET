namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NewIndexesV22 : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Product_ProductAttribute_Mapping", new[] { "ProductId" });
            DropIndex("dbo.ProductVariantAttributeValue", new[] { "ProductVariantAttributeId" });
            CreateIndex("dbo.Product_ProductAttribute_Mapping", new[] { "ProductId", "DisplayOrder" }, name: "IX_Product_ProductAttribute_Mapping_ProductId_DisplayOrder");
            CreateIndex("dbo.ProductVariantAttributeValue", new[] { "ProductVariantAttributeId", "DisplayOrder" }, name: "IX_ProductVariantAttributeValue_ProductVariantAttributeId_DisplayOrder");
            CreateIndex("dbo.NewsLetterSubscription", new[] { "Email", "StoreId" }, name: "IX_NewsletterSubscription_Email_StoreId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.NewsLetterSubscription", "IX_NewsletterSubscription_Email_StoreId");
            DropIndex("dbo.ProductVariantAttributeValue", "IX_ProductVariantAttributeValue_ProductVariantAttributeId_DisplayOrder");
            DropIndex("dbo.Product_ProductAttribute_Mapping", "IX_Product_ProductAttribute_Mapping_ProductId_DisplayOrder");
            CreateIndex("dbo.ProductVariantAttributeValue", "ProductVariantAttributeId");
            CreateIndex("dbo.Product_ProductAttribute_Mapping", "ProductId");
        }
    }
}
