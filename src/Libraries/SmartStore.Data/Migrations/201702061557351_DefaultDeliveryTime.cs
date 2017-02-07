namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DefaultDeliveryTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeliveryTime", "IsDefault", c => c.Boolean(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.DeliveryTime", "IsDefault");
        }
    }
}
