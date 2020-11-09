namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Core.Domain.Topics;
    using Setup;

    public partial class SystemTopics : DbMigration, ILocaleResourcesProvider, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            AddColumn("dbo.Topic", "IsSystemTopic", c => c.Boolean(nullable: false, defaultValue: false));
        }

        public override void Down()
        {
            DropColumn("dbo.Topic", "IsSystemTopic");
        }

        public bool RollbackOnFailure => false;

        public void Seed(SmartObjectContext context)
        {
            var systemTopics = new[] {
                "AboutUs",
                "CheckoutAsGuestOrRegister",
                "ConditionsOfUse",
                "ContactUs",
                "ForumWelcomeMessage",
                "HomePageText",
                "LoginRegistrationInfo",
                "PrivacyInfo",
                "ShippingInfo",
                "Imprint",
                "Disclaimer",
                "PaymentInfo"
            };

            var topics = context.Set<Topic>().Where(x => systemTopics.Contains(x.SystemName)).ToList();
            topics.Each(x => x.IsSystemTopic = true);

            context.MigrateLocaleResources(MigrateLocaleResources);

            context.SaveChanges();
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.ContentManagement.Topics.CannotBeDeleted",
                "This topic is required by your Shop and can therefore not be deleted.",
                "Diese Seite wird von Ihrem Shop benötigt und kann daher nicht gelöscht werden.");
        }
    }
}
