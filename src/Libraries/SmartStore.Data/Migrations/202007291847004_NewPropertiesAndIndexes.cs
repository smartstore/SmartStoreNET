namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class NewPropertiesAndIndexes : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Country", "DefaultCurrencyId", c => c.Int());
            AddColumn("dbo.CheckoutAttributeValue", "MediaFileId", c => c.Int());
            AddColumn("dbo.CheckoutAttributeValue", "Color", c => c.String(maxLength: 100));
            CreateIndex("dbo.CustomerRole", "Active");
            CreateIndex("dbo.CustomerRole", "IsSystemRole");
            CreateIndex("dbo.CustomerRole", new[] { "SystemName", "IsSystemRole" }, name: "IX_CustomerRole_SystemName_IsSystemRole");
            CreateIndex("dbo.CustomerRole", "SystemName");
            CreateIndex("dbo.Country", "DefaultCurrencyId");
            CreateIndex("dbo.Customer", new[] { "Deleted", "IsSystemAccount" }, name: "IX_Customer_Deleted_IsSystemAccount");
            CreateIndex("dbo.Customer", "IsSystemAccount");
            CreateIndex("dbo.ProductVariantAttributeCombination", "Gtin");
            CreateIndex("dbo.ProductVariantAttributeCombination", "ManufacturerPartNumber");
            CreateIndex("dbo.ProductVariantAttributeCombination", "IsActive");
            CreateIndex("dbo.CheckoutAttributeValue", "MediaFileId");
            AddForeignKey("dbo.Country", "DefaultCurrencyId", "dbo.Currency", "Id");
            AddForeignKey("dbo.CheckoutAttributeValue", "MediaFileId", "dbo.MediaFile", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.CheckoutAttributeValue", "MediaFileId", "dbo.MediaFile");
            DropForeignKey("dbo.Country", "DefaultCurrencyId", "dbo.Currency");
            DropIndex("dbo.CheckoutAttributeValue", new[] { "MediaFileId" });
            DropIndex("dbo.ProductVariantAttributeCombination", new[] { "IsActive" });
            DropIndex("dbo.ProductVariantAttributeCombination", new[] { "ManufacturerPartNumber" });
            DropIndex("dbo.ProductVariantAttributeCombination", new[] { "Gtin" });
            DropIndex("dbo.Customer", new[] { "IsSystemAccount" });
            DropIndex("dbo.Customer", "IX_Customer_Deleted_IsSystemAccount");
            DropIndex("dbo.Country", new[] { "DefaultCurrencyId" });
            DropIndex("dbo.CustomerRole", new[] { "SystemName" });
            DropIndex("dbo.CustomerRole", "IX_CustomerRole_SystemName_IsSystemRole");
            DropIndex("dbo.CustomerRole", new[] { "IsSystemRole" });
            DropIndex("dbo.CustomerRole", new[] { "Active" });
            DropColumn("dbo.CheckoutAttributeValue", "Color");
            DropColumn("dbo.CheckoutAttributeValue", "MediaFileId");
            DropColumn("dbo.Country", "DefaultCurrencyId");
        }
    }
}
