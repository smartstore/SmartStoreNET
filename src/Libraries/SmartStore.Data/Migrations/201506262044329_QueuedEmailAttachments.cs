namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QueuedEmailAttachments : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QueuedEmailAttachment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QueuedEmailId = c.Int(nullable: false),
                        StorageLocation = c.Int(nullable: false),
                        Path = c.String(maxLength: 1000),
                        FileId = c.Int(),
                        Data = c.Binary(),
                        Name = c.String(nullable: false, maxLength: 200),
                        MimeType = c.String(nullable: false, maxLength: 200),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Download", t => t.FileId, cascadeDelete: true)
                .ForeignKey("dbo.QueuedEmail", t => t.QueuedEmailId, cascadeDelete: true)
                .Index(t => t.QueuedEmailId)
                .Index(t => t.FileId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QueuedEmailAttachment", "QueuedEmailId", "dbo.QueuedEmail");
            DropForeignKey("dbo.QueuedEmailAttachment", "FileId", "dbo.Download");
            DropIndex("dbo.QueuedEmailAttachment", new[] { "FileId" });
            DropIndex("dbo.QueuedEmailAttachment", new[] { "QueuedEmailId" });
            DropTable("dbo.QueuedEmailAttachment");
        }
    }
}
