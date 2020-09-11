namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class AddCustomerRoleOrderAmount : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CustomerRole", "MinOrderAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
            AddColumn("dbo.CustomerRole", "MaxOrderAmount", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }

        public override void Down()
        {
            DropColumn("dbo.CustomerRole", "MinOrderAmount");
            DropColumn("dbo.CustomerRole", "MaxOrderAmount");
        }
    }
}
