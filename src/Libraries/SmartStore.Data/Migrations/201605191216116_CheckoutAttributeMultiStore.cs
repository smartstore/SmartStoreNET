namespace SmartStore.Data.Migrations
{
	using System.Data.Entity.Migrations;

	public partial class CheckoutAttributeMultiStore : DbMigration
	{
        public override void Up()
        {
            AddColumn("dbo.CheckoutAttribute", "LimitedToStores", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.CheckoutAttribute", "LimitedToStores");
        }
    }
}
