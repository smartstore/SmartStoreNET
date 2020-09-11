namespace SmartStore.GoogleMerchantCenter.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class IsTouched : DbMigration
    {
        public override void Up()
        {
            var utcNow = DateTime.UtcNow;

            AddColumn("dbo.GoogleProduct", "IsTouched", c => c.Boolean(nullable: false));
            AddColumn("dbo.GoogleProduct", "CreatedOnUtc", c => c.DateTime(nullable: false, defaultValue: utcNow));
            AddColumn("dbo.GoogleProduct", "UpdatedOnUtc", c => c.DateTime(nullable: false, defaultValue: utcNow));
        }

        public override void Down()
        {
            DropColumn("dbo.GoogleProduct", "IsTouched");
            DropColumn("dbo.GoogleProduct", "CreatedOnUtc");
            DropColumn("dbo.GoogleProduct", "UpdatedOnUtc");
        }
    }
}
