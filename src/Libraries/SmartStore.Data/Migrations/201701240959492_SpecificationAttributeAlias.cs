namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SpecificationAttributeAlias : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.SpecificationAttributeOption", "Alias", c => c.String(maxLength: 100));
            AddColumn("dbo.SpecificationAttribute", "Alias", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.SpecificationAttribute", "Alias");
            DropColumn("dbo.SpecificationAttributeOption", "Alias");
        }
    }
}
