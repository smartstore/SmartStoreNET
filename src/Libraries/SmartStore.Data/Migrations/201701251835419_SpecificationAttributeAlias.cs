namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SpecificationAttributeAlias : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpecificationAttributeOption", "Alias", c => c.String(maxLength: 30));
            AddColumn("dbo.SpecificationAttribute", "Alias", c => c.String(maxLength: 30));
            AddColumn("dbo.SpecificationAttribute", "FacetSorting", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SpecificationAttribute", "FacetSorting");
            DropColumn("dbo.SpecificationAttribute", "Alias");
            DropColumn("dbo.SpecificationAttributeOption", "Alias");
        }
    }
}
