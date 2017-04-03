namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SpecificationAttributeAllowFiltering : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpecificationAttribute", "ShowOnProductPage", c => c.Boolean());
            AddColumn("dbo.SpecificationAttribute", "AllowFiltering", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.SpecificationAttribute", "AllowFiltering");
            DropColumn("dbo.SpecificationAttribute", "ShowOnProductPage");
        }
    }
}
