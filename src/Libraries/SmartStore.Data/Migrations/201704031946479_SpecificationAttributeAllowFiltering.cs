namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SpecificationAttributeAllowFiltering : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpecificationAttribute", "ShowOnProductPage", c => c.Boolean(nullable: false));
            AddColumn("dbo.SpecificationAttribute", "AllowFiltering", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Product_SpecificationAttribute_Mapping", "AllowFiltering", c => c.Boolean());
            AlterColumn("dbo.Product_SpecificationAttribute_Mapping", "ShowOnProductPage", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Product_SpecificationAttribute_Mapping", "ShowOnProductPage", c => c.Boolean(nullable: false));
            AlterColumn("dbo.Product_SpecificationAttribute_Mapping", "AllowFiltering", c => c.Boolean(nullable: false));
            DropColumn("dbo.SpecificationAttribute", "AllowFiltering");
            DropColumn("dbo.SpecificationAttribute", "ShowOnProductPage");
        }
    }
}
