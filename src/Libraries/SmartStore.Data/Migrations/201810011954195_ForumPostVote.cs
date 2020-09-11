namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class ForumPostVote : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ForumPostVote",
                c => new
                {
                    Id = c.Int(nullable: false),
                    ForumPostId = c.Int(nullable: false),
                    Vote = c.Boolean(nullable: false),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CustomerContent", t => t.Id)
                .ForeignKey("dbo.Forums_Post", t => t.ForumPostId, cascadeDelete: true)
                .Index(t => t.Id)
                .Index(t => t.ForumPostId);

        }

        public override void Down()
        {
            DropForeignKey("dbo.ForumPostVote", "ForumPostId", "dbo.Forums_Post");
            DropForeignKey("dbo.ForumPostVote", "Id", "dbo.CustomerContent");
            DropIndex("dbo.ForumPostVote", new[] { "ForumPostId" });
            DropIndex("dbo.ForumPostVote", new[] { "Id" });
            DropTable("dbo.ForumPostVote");
        }
    }
}
