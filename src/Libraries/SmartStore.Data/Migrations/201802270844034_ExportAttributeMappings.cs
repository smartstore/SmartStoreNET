namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExportAttributeMappings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductAttribute", "ExportMappings", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductAttribute", "ExportMappings");
        }
    }
}
