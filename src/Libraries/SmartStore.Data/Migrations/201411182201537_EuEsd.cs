namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EuEsd : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "IsEsd", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Product", "IsEsd");
        }
    }
}
