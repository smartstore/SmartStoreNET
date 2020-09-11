namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ForumGroupAcl : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Forums_Topic", new[] { "ForumId" });
            DropIndex("dbo.Forums_Forum", new[] { "ForumGroupId" });
            AddColumn("dbo.Forums_Post", "Published", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Forums_Topic", "Published", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Forums_Group", "SubjectToAcl", c => c.Boolean(nullable: false));
            CreateIndex("dbo.Forums_Post", "CreatedOnUtc");
            CreateIndex("dbo.Forums_Post", "Published");
            CreateIndex("dbo.Forums_Topic", new[] { "ForumId", "Published" });
            CreateIndex("dbo.Forums_Topic", new[] { "TopicTypeId", "LastPostTime" });
            CreateIndex("dbo.Forums_Topic", "Subject");
            CreateIndex("dbo.Forums_Topic", "NumPosts");
            CreateIndex("dbo.Forums_Topic", "CreatedOnUtc");
            CreateIndex("dbo.Forums_Forum", new[] { "ForumGroupId", "DisplayOrder" });
            CreateIndex("dbo.Forums_Group", "DisplayOrder");
            CreateIndex("dbo.Forums_Group", "LimitedToStores");
            CreateIndex("dbo.Forums_Group", "SubjectToAcl");
        }

        public override void Down()
        {
            DropIndex("dbo.Forums_Group", new[] { "SubjectToAcl" });
            DropIndex("dbo.Forums_Group", new[] { "LimitedToStores" });
            DropIndex("dbo.Forums_Group", new[] { "DisplayOrder" });
            DropIndex("dbo.Forums_Forum", new[] { "ForumGroupId", "DisplayOrder" });
            DropIndex("dbo.Forums_Topic", new[] { "CreatedOnUtc" });
            DropIndex("dbo.Forums_Topic", new[] { "NumPosts" });
            DropIndex("dbo.Forums_Topic", new[] { "Subject" });
            DropIndex("dbo.Forums_Topic", new[] { "TopicTypeId", "LastPostTime" });
            DropIndex("dbo.Forums_Topic", new[] { "ForumId", "Published" });
            DropIndex("dbo.Forums_Post", new[] { "Published" });
            DropIndex("dbo.Forums_Post", new[] { "CreatedOnUtc" });
            DropColumn("dbo.Forums_Group", "SubjectToAcl");
            DropColumn("dbo.Forums_Topic", "Published");
            DropColumn("dbo.Forums_Post", "Published");
            CreateIndex("dbo.Forums_Forum", "ForumGroupId");
            CreateIndex("dbo.Forums_Topic", "ForumId");
        }
    }
}
