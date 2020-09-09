namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DeliveryTimeMinMaxDays : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DeliveryTime", "MinDays", c => c.Int());
            AddColumn("dbo.DeliveryTime", "MaxDays", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DeliveryTime", "MaxDays");
            DropColumn("dbo.DeliveryTime", "MinDays");
        }
    }
}
