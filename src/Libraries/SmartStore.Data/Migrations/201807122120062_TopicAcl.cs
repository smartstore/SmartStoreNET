namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class TopicAcl : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Customer", "IX_Customer_CustomerNumber");
            AddColumn("dbo.Topic", "ShortTitle", c => c.String(maxLength: 50));
            AddColumn("dbo.Topic", "Intro", c => c.String(maxLength: 255));
            AddColumn("dbo.Topic", "SubjectToAcl", c => c.Boolean(nullable: false));
            AddColumn("dbo.Topic", "IsPublished", c => c.Boolean(nullable: false, defaultValue: true));
            CreateIndex("dbo.Customer", "CustomerNumber", name: "IX_Customer_CustomerNumber");
        }

        public override void Down()
        {
            DropIndex("dbo.Customer", "IX_Customer_CustomerNumber");
            DropColumn("dbo.Topic", "IsPublished");
            DropColumn("dbo.Topic", "SubjectToAcl");
            DropColumn("dbo.Topic", "Intro");
            DropColumn("dbo.Topic", "ShortTitle");
            CreateIndex("dbo.Customer", "CustomerNumber", unique: true, name: "IX_Customer_CustomerNumber");
        }
    }
}
