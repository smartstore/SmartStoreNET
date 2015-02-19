namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BizQuantityUnitID : DbMigration
    {
        public override void Up()
        {
			AddColumn("dbo.QuantityUnit", "BizQuantityUnitID", c => c.Int(nullable: true));
        }
        
        public override void Down()
        {
			DropColumn("dbo.QuantityUnit", "BizQuantityUnitID");
        }
    }
}
