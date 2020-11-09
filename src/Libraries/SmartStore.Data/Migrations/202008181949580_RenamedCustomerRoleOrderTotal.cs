namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class RenamedCustomerRoleOrderTotal : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerRole", "OrderTotalMinimum", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            AddColumn("dbo.CustomerRole", "OrderTotalMaximum", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            DropColumn("dbo.CustomerRole", "MinOrderAmount");
            DropColumn("dbo.CustomerRole", "MaxOrderAmount");
        }

        public override void Down()
        {
            AddColumn("dbo.CustomerRole", "MinOrderAmount", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            AddColumn("dbo.CustomerRole", "MaxOrderAmount", c => c.Decimal(nullable: true, precision: 18, scale: 2));
            DropColumn("dbo.CustomerRole", "OrderTotalMaximum");
            DropColumn("dbo.CustomerRole", "OrderTotalMinimum");
        }
    }
}
