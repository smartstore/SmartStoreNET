namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Plugins;
    using SmartStore.Data.Setup;

    public partial class SpecificationAttributeColorAndPicture : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.SpecificationAttributeOption", "PictureId", c => c.Int(nullable: false));
            AddColumn("dbo.SpecificationAttributeOption", "Color", c => c.String(maxLength: 100));
            AlterColumn("dbo.Rule", "Operator", c => c.String(nullable: false, maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Rule", "Operator", c => c.String(nullable: false, maxLength: 10));
            DropColumn("dbo.SpecificationAttributeOption", "Color");
            DropColumn("dbo.SpecificationAttributeOption", "PictureId");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            // Save newly added permissions to the database.
            var descriptor = PluginFinder.Current.GetPluginDescriptorBySystemName("SmartStore.PageBuilder");
            if (descriptor != null)
            {
                var migrator = new PermissionMigrator(context);
                migrator.AddPluginPermissions("SmartStore.PageBuilder.Services.PageBuilderPermissions, SmartStore.PageBuilder");
                // Mappings are not required because admin permissions are already granted through root permission.
            }
        }
    }
}
