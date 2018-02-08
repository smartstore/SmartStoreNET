namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ShippingMethodMultistore : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ShippingMethod", "LimitedToStores", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ShippingMethod", "LimitedToStores");
        }
    }
}
