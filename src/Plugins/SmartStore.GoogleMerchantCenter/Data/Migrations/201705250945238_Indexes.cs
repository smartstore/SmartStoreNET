namespace SmartStore.GoogleMerchantCenter.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Indexes : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.GoogleProduct", "ProductId");
            CreateIndex("dbo.GoogleProduct", "IsTouched");
            CreateIndex("dbo.GoogleProduct", "Export");
        }

        public override void Down()
        {
            DropIndex("dbo.GoogleProduct", new[] { "Export" });
            DropIndex("dbo.GoogleProduct", new[] { "IsTouched" });
            DropIndex("dbo.GoogleProduct", new[] { "ProductId" });
        }
    }
}
