namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class PictureMediaRename1 : DbMigration
    {
        public override void Up()
        {
            // A cumbersome but necessary step to prevent EF from dropping renamed tables.
        }

        public override void Down()
        {
        }
    }
}
