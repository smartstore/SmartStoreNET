namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MenuItemMultistore : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MenuItemRecord", "LimitedToStores", c => c.Boolean(nullable: false));
            AddColumn("dbo.MenuItemRecord", "SubjectToAcl", c => c.Boolean(nullable: false));
            CreateIndex("dbo.MenuItemRecord", "LimitedToStores", name: "IX_MenuItem_LimitedToStores");
            CreateIndex("dbo.MenuItemRecord", "SubjectToAcl", name: "IX_MenuItem_SubjectToAcl");
        }
        
        public override void Down()
        {
            DropIndex("dbo.MenuItemRecord", "IX_MenuItem_SubjectToAcl");
            DropIndex("dbo.MenuItemRecord", "IX_MenuItem_LimitedToStores");
            DropColumn("dbo.MenuItemRecord", "SubjectToAcl");
            DropColumn("dbo.MenuItemRecord", "LimitedToStores");
        }
    }
}
