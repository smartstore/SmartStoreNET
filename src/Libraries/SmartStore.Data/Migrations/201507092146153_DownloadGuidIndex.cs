namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DownloadGuidIndex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Download", "DownloadGuid");
        }
        
        public override void Down()
        {
			DropIndex("dbo.Download", new[] { "DownloadGuid" });
        }
    }
}
