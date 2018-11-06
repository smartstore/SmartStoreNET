namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsSystemProductIndex : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Product", "IX_Product_Deleted_and_Published");
            RenameIndex(table: "dbo.Product", name: "Product_SystemName_IsSystemProduct", newName: "IX_Product_SystemName_IsSystemProduct");
            CreateIndex("dbo.Product", new[] { "Published", "Deleted", "IsSystemProduct" }, name: "IX_Product_Published_Deleted_IsSystemProduct");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Product", "IX_Product_Published_Deleted_IsSystemProduct");
            RenameIndex(table: "dbo.Product", name: "IX_Product_SystemName_IsSystemProduct", newName: "Product_SystemName_IsSystemProduct");
            CreateIndex("dbo.Product", new[] { "Published", "Deleted" }, name: "IX_Product_Deleted_and_Published");
        }
    }
}
