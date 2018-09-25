namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ForumGroupAcl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Forums_Group", "SubjectToAcl", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Forums_Group", "SubjectToAcl");
        }
    }
}
