namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class AddNewPermissions : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            /// This migration is obsolete and has been removed. Permissions are added via <see cref="InstallPermissionsStarter"/>.
        }
    }
}
