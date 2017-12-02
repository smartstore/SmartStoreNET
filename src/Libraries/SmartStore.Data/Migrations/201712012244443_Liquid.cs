namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Liquid : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MessageTemplate", "To", c => c.String(nullable: false, maxLength: 500, defaultValue: " "));
            AddColumn("dbo.MessageTemplate", "ReplyTo", c => c.String(maxLength: 500));
            AddColumn("dbo.MessageTemplate", "LastModelTree", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MessageTemplate", "LastModelTree");
            DropColumn("dbo.MessageTemplate", "ReplyTo");
            DropColumn("dbo.MessageTemplate", "To");
        }
    }
}
