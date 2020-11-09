namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class MediaManager3 : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            // Empty, but we need the events in SmartStore.Services
        }

        public override void Up()
        {
        }

        public override void Down()
        {
        }
    }
}
