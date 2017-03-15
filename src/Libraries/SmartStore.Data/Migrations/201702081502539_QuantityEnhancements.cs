namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuantityEnhancements : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "QuantityStep", c => c.Int(nullable: false));
            AddColumn("dbo.Product", "QuantiyControlType", c => c.Int(nullable: false));
            AddColumn("dbo.Product", "HideQuantityControl", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "HideQuantityControl");
            DropColumn("dbo.Product", "QuantiyControlType");
            DropColumn("dbo.Product", "QuantityStep");
        }
    }
}
