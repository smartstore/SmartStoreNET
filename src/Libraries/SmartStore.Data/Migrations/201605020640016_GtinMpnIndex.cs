namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    
    public partial class GtinMpnIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Product", "ManufacturerPartNumber");
            CreateIndex("dbo.Product", "Gtin");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Product", new[] { "Gtin" });
            DropIndex("dbo.Product", new[] { "ManufacturerPartNumber" });
        }
    }
}
