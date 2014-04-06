namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AttributeCombinationPrice : DbMigration
    {
        public override void Up()
        {
			AddColumn("dbo.ProductVariantAttributeCombination", "Price", x => x.Decimal(nullable: true, precision: 18, scale: 4));
        }
        
        public override void Down()
        {
			DropColumn("dbo.ProductVariantAttributeCombination", "Price");
        }
    }
}
