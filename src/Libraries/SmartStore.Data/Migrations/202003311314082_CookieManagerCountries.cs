namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;

    public partial class CookieManagerCountries : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Country", "DisplayCookieManager", c => c.Boolean(nullable: false, defaultValue: true));
        }

        public override void Down()
        {
            DropColumn("dbo.Country", "DisplayCookieManager");
        }

        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            if (!DataSettings.DatabaseIsInstalled())
                return;

            // Add resources.
            context.MigrateLocaleResources(MigrateLocaleResources);

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Configuration.Countries.Fields.DisplayCookieManager",
                "Display Cookie Manager",
                "Cookie-Manager anzeigen",
                "Specifies whether the Cookie Manager will be displayed to shop visitors from this country.",
                "Bestimmt, ob der Cookie-Manager Shop-Besuchern aus diesem Land angezeigt wird.");
        }
    }
}
