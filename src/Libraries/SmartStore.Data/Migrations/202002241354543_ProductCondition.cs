namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ProductCondition : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "Condition", c => c.Int(nullable: false));
            CreateIndex("dbo.ProductAttribute", "DisplayOrder");
        }

        public override void Down()
        {
            DropIndex("dbo.ProductAttribute", new[] { "DisplayOrder" });
            DropColumn("dbo.Product", "Condition");
        }
    }
}
