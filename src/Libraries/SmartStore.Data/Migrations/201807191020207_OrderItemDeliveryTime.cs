namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class OrderItemDeliveryTime : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.OrderItem", "DeliveryTimeId", c => c.Int());
            AddColumn("dbo.OrderItem", "DisplayDeliveryTime", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.OrderItem", "DisplayDeliveryTime");
            DropColumn("dbo.OrderItem", "DeliveryTimeId");
        }
    }
}
