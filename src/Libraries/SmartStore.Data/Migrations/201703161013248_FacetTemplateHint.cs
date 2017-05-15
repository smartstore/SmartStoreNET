namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FacetTemplateHint : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpecificationAttributeOption", "NumberValue", c => c.Decimal(nullable: false, precision: 18, scale: 4));
            AddColumn("dbo.SpecificationAttribute", "FacetTemplateHint", c => c.Int(nullable: false));
            AddColumn("dbo.ProductAttribute", "FacetTemplateHint", c => c.Int(nullable: false));
            DropColumn("dbo.SpecificationAttributeOption", "RangeFilterId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.SpecificationAttributeOption", "RangeFilterId", c => c.Int(nullable: false));
            DropColumn("dbo.ProductAttribute", "FacetTemplateHint");
            DropColumn("dbo.SpecificationAttribute", "FacetTemplateHint");
            DropColumn("dbo.SpecificationAttributeOption", "NumberValue");
        }
    }
}
