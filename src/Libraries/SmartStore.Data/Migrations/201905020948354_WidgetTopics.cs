namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using System.Linq;
    using SmartStore.Core.Data;
    using SmartStore.Core.Domain.Topics;
    using SmartStore.Data.Setup;

    public partial class WidgetTopics : DbMigration, IDataSeeder<SmartObjectContext>
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
            using (var scope = new DbContextScope(ctx: context, validateOnSave: false, hooksEnabled: false))
            {
                var widgetTopics = new[]
                {
                    "CheckoutAsGuestOrRegister",
                    "ContactUs",
                    "ForumWelcomeMessage",
                    "HomePageText",
                    "LoginRegistrationInfo"
                };

                var topics = context.Set<Topic>().Where(x => widgetTopics.Contains(x.SystemName)).ToList();
                topics.Each(x =>
                {
                    x.RenderAsWidget = true;
                    x.WidgetWrapContent = false;
                });

                context.SaveChanges();
            }
        }
    }
}
