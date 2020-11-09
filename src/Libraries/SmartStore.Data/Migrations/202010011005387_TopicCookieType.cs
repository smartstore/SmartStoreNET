namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Core.Data;
    using SmartStore.Data.Setup;

    public partial class TopicCookieType : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Topic", "CookieType", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Topic", "CookieType");
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
            builder.AddOrUpdate("Admin.ContentManagement.Topics.Fields.CookieType",
                "Cookie type",
                "Art des Cookies",
                "Specifies whether this widget is displayed according to the customer's settings in the cookie manager. " +
                " This option should be used if you add a third-party script that sets cookies.",
                "Bestimmt, ob dieses Widget in Abhängigkeit zur Kundeneinstellung im Cookie-Manager ausgegeben wird. " +
                "Diese Option sollte verwendet werden, wenn Sie ein Script für einen Drittanbieter zufügen, das Cookies setzt.");
        }
    }
}
