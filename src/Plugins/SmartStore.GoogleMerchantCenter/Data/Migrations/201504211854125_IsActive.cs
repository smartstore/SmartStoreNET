namespace SmartStore.GoogleMerchantCenter.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class IsActive : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GoogleProduct", "Export", c => c.Boolean(nullable: false, defaultValue: true));
        }

        public override void Down()
        {
            DropColumn("dbo.GoogleProduct", "Export");
        }
    }
}
