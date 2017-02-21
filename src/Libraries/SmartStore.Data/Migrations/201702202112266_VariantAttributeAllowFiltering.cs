namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VariantAttributeAllowFiltering : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductAttribute", "AllowFiltering", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.ProductAttribute", "DisplayOrder", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductAttribute", "DisplayOrder");
            DropColumn("dbo.ProductAttribute", "AllowFiltering");
        }
    }
}
