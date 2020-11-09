namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class V321Resources : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
        }

        public override void Down()
        {
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            context.MigrateLocaleResources(MigrateLocaleResources);
            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.Configuration.Languages.NoAvailableLanguagesFound",
                "There were no other available languages found for version {0}. On <a href=\"https://translate.smartstore.com/\" target=\"_blank\">translate.smartstore.com</a> you will find more details about available resources.",
                "Es wurden keine weiteren verfügbaren Sprachen für Version {0} gefunden. Auf <a href=\"https://translate.smartstore.com/\" target=\"_blank\">translate.smartstore.com</a> finden Sie weitere Details zu verfügbaren Ressourcen.");
        }
    }
}
