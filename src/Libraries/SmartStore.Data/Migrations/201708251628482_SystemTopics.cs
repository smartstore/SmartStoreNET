namespace SmartStore.Data.Migrations
{
    using Core.Domain.Topics;
    using Setup;
    using System;
    using System.Linq;
    using System.Data.Entity.Migrations;

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

        public bool RollbackOnFailure
        {
            get { return false; }
        }

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

            context.SaveChanges();

            context.MigrateLocaleResources(MigrateLocaleResources);
        }

        public void MigrateLocaleResources(LocaleResourcesBuilder builder)
        {
            builder.AddOrUpdate("Admin.ContentManagement.Topics.CannotBeDeleted", 
                "This topic is needed by your Shop and can therefore not be deleted.", 
                "Diese Seite wird von Ihrem Shop ben�tigt und kann daher nicht gel�scht werden.");
        }
    }
}
