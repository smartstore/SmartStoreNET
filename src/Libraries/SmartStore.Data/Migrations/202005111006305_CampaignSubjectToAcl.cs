namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class CampaignSubjectToAcl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Campaign", "SubjectToAcl", c => c.Boolean(nullable: false));
            CreateIndex("dbo.NewsLetterSubscription", "Active");
        }

        public override void Down()
        {
            DropIndex("dbo.NewsLetterSubscription", new[] { "Active" });
            DropColumn("dbo.Campaign", "SubjectToAcl");
        }
    }
}
