namespace SmartStore.GoogleMerchantCenter.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            //if (DbMigrationContext.Current.SuppressInitialCreate<GoogleProductObjectContext>())
            //	return;

            CreateTable(
                "dbo.GoogleProduct",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ProductId = c.Int(nullable: false),
                    Taxonomy = c.String(maxLength: 4000),
                    Gender = c.String(maxLength: 4000),
                    AgeGroup = c.String(maxLength: 4000),
                    Color = c.String(maxLength: 4000),
                    Size = c.String(maxLength: 4000),
                    Material = c.String(maxLength: 4000),
                    Pattern = c.String(maxLength: 4000),
                    ItemGroupId = c.String(maxLength: 4000),
                })
                .PrimaryKey(t => t.Id);

        }

        public override void Down()
        {
            DropTable("dbo.GoogleProduct");
        }
    }
}
