namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MessageTemplateAttachments : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MessageTemplate", "Attachment1FileId", c => c.Int());
            AddColumn("dbo.MessageTemplate", "Attachment2FileId", c => c.Int());
            AddColumn("dbo.MessageTemplate", "Attachment3FileId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MessageTemplate", "Attachment3FileId");
            DropColumn("dbo.MessageTemplate", "Attachment2FileId");
            DropColumn("dbo.MessageTemplate", "Attachment1FileId");
        }
    }
}
