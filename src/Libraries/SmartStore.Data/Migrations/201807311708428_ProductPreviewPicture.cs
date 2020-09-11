namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ProductPreviewPicture : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Product", "HasPreviewPicture", c => c.Boolean(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Product", "HasPreviewPicture");
        }
    }
}
