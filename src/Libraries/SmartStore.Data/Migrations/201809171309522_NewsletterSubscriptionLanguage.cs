namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class NewsletterSubscriptionLanguage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NewsLetterSubscription", "WorkingLanguageId", c => c.Int(nullable: false));
        }

        public override void Down()
        {
            DropColumn("dbo.NewsLetterSubscription", "WorkingLanguageId");
        }
    }
}
