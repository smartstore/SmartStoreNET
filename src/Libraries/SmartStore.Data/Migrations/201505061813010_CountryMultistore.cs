namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CountryMultistore : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Country", "LimitedToStores", c => c.Boolean(nullable: false));
            AddColumn("dbo.Forums_Group", "LimitedToStores", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forums_Group", "LimitedToStores");
            DropColumn("dbo.Country", "LimitedToStores");
        }
    }
}
