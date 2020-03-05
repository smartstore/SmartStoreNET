namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MediaManager2 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Download", "MediaStorageId", "dbo.MediaStorage");
            DropForeignKey("dbo.QueuedEmailAttachment", "FileId", "dbo.Download");
            DropForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage");
            DropIndex("dbo.MediaFile", new[] { "IsNew" });
            DropIndex("dbo.Download", new[] { "MediaStorageId" });
            DropIndex("dbo.QueuedEmailAttachment", new[] { "FileId" });
            AddColumn("dbo.QueuedEmailAttachment", "MediaFileId", c => c.Int());
            CreateIndex("dbo.QueuedEmailAttachment", "MediaFileId");
            AddForeignKey("dbo.QueuedEmailAttachment", "MediaFileId", "dbo.MediaFile", "Id");
            AddForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage", "Id", cascadeDelete: true);
            DropColumn("dbo.MediaFile", "IsNew");
            DropColumn("dbo.Download", "DownloadBinary");
            DropColumn("dbo.Download", "ContentType");
            DropColumn("dbo.Download", "Filename");
            DropColumn("dbo.Download", "Extension");
            DropColumn("dbo.Download", "IsNew");
            DropColumn("dbo.Download", "MediaStorageId");
            DropColumn("dbo.QueuedEmailAttachment", "FileId");
            DropColumn("dbo.QueuedEmailAttachment", "Data");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QueuedEmailAttachment", "Data", c => c.Binary());
            AddColumn("dbo.QueuedEmailAttachment", "FileId", c => c.Int());
            AddColumn("dbo.Download", "MediaStorageId", c => c.Int());
            AddColumn("dbo.Download", "IsNew", c => c.Boolean(nullable: false));
            AddColumn("dbo.Download", "Extension", c => c.String());
            AddColumn("dbo.Download", "Filename", c => c.String());
            AddColumn("dbo.Download", "ContentType", c => c.String());
            AddColumn("dbo.Download", "DownloadBinary", c => c.Binary());
            AddColumn("dbo.MediaFile", "IsNew", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage");
            DropForeignKey("dbo.QueuedEmailAttachment", "MediaFileId", "dbo.MediaFile");
            DropIndex("dbo.QueuedEmailAttachment", new[] { "MediaFileId" });
            DropColumn("dbo.QueuedEmailAttachment", "MediaFileId");
            CreateIndex("dbo.QueuedEmailAttachment", "FileId");
            CreateIndex("dbo.Download", "MediaStorageId");
            CreateIndex("dbo.MediaFile", "IsNew");
            AddForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage", "Id");
            AddForeignKey("dbo.QueuedEmailAttachment", "FileId", "dbo.Download", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Download", "MediaStorageId", "dbo.MediaStorage", "Id");
        }
    }
}
