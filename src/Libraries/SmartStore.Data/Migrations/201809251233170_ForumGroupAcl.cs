namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ForumGroupAcl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forums_Post", "Published", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Forums_Topic", "Published", c => c.Boolean(nullable: false, defaultValue: true));
            AddColumn("dbo.Forums_Group", "SubjectToAcl", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forums_Group", "SubjectToAcl");
            DropColumn("dbo.Forums_Topic", "Published");
            DropColumn("dbo.Forums_Post", "Published");
        }
    }
}
