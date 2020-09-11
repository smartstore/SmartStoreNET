namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using System.Web.Hosting;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Catalog;
    using SmartStore.Data.Setup;
    using SmartStore.Data.Utilities;

    public partial class DownloadVersions : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            int entityId = 0;

            if (DataSettings.DatabaseIsInstalled() && HostingEnvironment.IsHosted)
            {
                var ctx = new SmartObjectContext();
                entityId = ctx.Set<Product>().Select(x => x.Id).FirstOrDefault();
            }

            AddColumn("dbo.Download", "EntityId", c => c.Int(nullable: false, defaultValue: entityId));
            AddColumn("dbo.Download", "EntityName", c => c.String(nullable: false, maxLength: 100));
            AddColumn("dbo.Download", "FileVersion", c => c.String(maxLength: 30));
            AddColumn("dbo.Download", "Changelog", c => c.String());
            CreateIndex("dbo.Download", new[] { "EntityId", "EntityName" });
        }

        public override void Down()
        {
            DropIndex("dbo.Download", new[] { "EntityId", "EntityName" });
            DropColumn("dbo.Download", "Changelog");
            DropColumn("dbo.Download", "FileVersion");
            DropColumn("dbo.Download", "EntityName");
            DropColumn("dbo.Download", "EntityId");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (!HostingEnvironment.IsHosted)
                return;

            DataMigrator.SetDownloadProductId(context);
        }
    }
}
