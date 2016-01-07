namespace SmartStore.GoogleMerchantCenter.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IsBundle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.GoogleProduct", "Multipack", c => c.Int(nullable: false));
            AddColumn("dbo.GoogleProduct", "IsBundle", c => c.Boolean());
            AddColumn("dbo.GoogleProduct", "IsAdult", c => c.Boolean());
            AddColumn("dbo.GoogleProduct", "EnergyEfficiencyClass", c => c.String(maxLength: 50));
            AddColumn("dbo.GoogleProduct", "CustomLabel0", c => c.String(maxLength: 100));
            AddColumn("dbo.GoogleProduct", "CustomLabel1", c => c.String(maxLength: 100));
            AddColumn("dbo.GoogleProduct", "CustomLabel2", c => c.String(maxLength: 100));
            AddColumn("dbo.GoogleProduct", "CustomLabel3", c => c.String(maxLength: 100));
            AddColumn("dbo.GoogleProduct", "CustomLabel4", c => c.String(maxLength: 100));
        }
        
        public override void Down()
        {
            DropColumn("dbo.GoogleProduct", "CustomLabel4");
            DropColumn("dbo.GoogleProduct", "CustomLabel3");
            DropColumn("dbo.GoogleProduct", "CustomLabel2");
            DropColumn("dbo.GoogleProduct", "CustomLabel1");
            DropColumn("dbo.GoogleProduct", "CustomLabel0");
            DropColumn("dbo.GoogleProduct", "EnergyEfficiencyClass");
            DropColumn("dbo.GoogleProduct", "IsAdult");
            DropColumn("dbo.GoogleProduct", "IsBundle");
            DropColumn("dbo.GoogleProduct", "Multipack");
        }
    }
}
