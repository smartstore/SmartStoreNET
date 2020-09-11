namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ShipmentTrackingUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Shipment", "TrackingUrl", c => c.String(maxLength: 2000));
        }

        public override void Down()
        {
            DropColumn("dbo.Shipment", "TrackingUrl");
        }
    }
}
