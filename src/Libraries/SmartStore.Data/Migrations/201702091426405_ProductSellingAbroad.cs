namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProductSellingAbroad : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "CustomsTariffNumber", c => c.String(maxLength: 30));
            AddColumn("dbo.Product", "CountryOfOriginId", c => c.Int());
            CreateIndex("dbo.Product", "CountryOfOriginId");
            AddForeignKey("dbo.Product", "CountryOfOriginId", "dbo.Country", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Product", "CountryOfOriginId", "dbo.Country");
            DropIndex("dbo.Product", new[] { "CountryOfOriginId" });
            DropColumn("dbo.Product", "CountryOfOriginId");
            DropColumn("dbo.Product", "CustomsTariffNumber");
        }
    }
}
