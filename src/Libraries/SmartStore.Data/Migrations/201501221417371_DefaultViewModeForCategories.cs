namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DefaultViewModeForCategories : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Category", "DefaultViewMode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Category", "DefaultViewMode");
        }
    }
}
