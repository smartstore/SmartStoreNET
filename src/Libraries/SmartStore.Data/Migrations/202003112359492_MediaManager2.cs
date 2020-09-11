namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class MediaManager2 : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public override void Up()
        {
            #region Moved from "202003052100521_CustomerRoleMappings" to here

            DropIndex("dbo.MediaFile", new[] { "IsNew" });
            AddColumn("dbo.QueuedEmailAttachment", "MediaFileId", c => c.Int());
            CreateIndex("dbo.QueuedEmailAttachment", "MediaFileId");
            AddForeignKey("dbo.QueuedEmailAttachment", "MediaFileId", "dbo.MediaFile", "Id");
            DropForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage");
            AddForeignKey("dbo.QueuedEmailAttachment", "MediaStorageId", "dbo.MediaStorage", "Id", cascadeDelete: true);
            DropColumn("dbo.MediaFile", "IsNew");
            DropColumn("dbo.Download", "DownloadBinary");
            DropColumn("dbo.Download", "IsNew");

            #endregion

            DropIndex("dbo.MediaTrack", "IX_MediaTrack_Composite");
            AddColumn("dbo.MediaTrack", "Property", c => c.String(maxLength: 255));
            AlterColumn("dbo.Setting", "Name", c => c.String(nullable: false, maxLength: 400));
            CreateIndex("dbo.Setting", "Name", name: "IX_Setting_Name");
            CreateIndex("dbo.Setting", "StoreId", name: "IX_Setting_StoreId");
            CreateIndex("dbo.MediaTrack", new[] { "MediaFileId", "EntityId", "EntityName", "Property" }, unique: true, name: "IX_MediaTrack_Composite");
            DropColumn("dbo.MediaFile", "PictureBinary");
        }

        public override void Down()
        {
            #region Moved from "202003052100521_CustomerRoleMappings" to here

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

            #endregion

            AddColumn("dbo.MediaFile", "PictureBinary", c => c.Binary());
            DropIndex("dbo.MediaTrack", "IX_MediaTrack_Composite");
            DropIndex("dbo.Setting", "IX_Setting_StoreId");
            DropIndex("dbo.Setting", "IX_Setting_Name");
            AlterColumn("dbo.Setting", "Name", c => c.String(nullable: false, maxLength: 200));
            DropColumn("dbo.MediaTrack", "Property");
            CreateIndex("dbo.MediaTrack", new[] { "MediaFileId", "EntityId", "EntityName" }, unique: true, name: "IX_MediaTrack_Composite");
        }

        public void Seed(SmartObjectContext context)
        {
            // We cannot tear down obsolete DB structure during regular migration, but only 
            // AFTER a successfull data seed, 'cause during the seed we need the data.
            var sql = @"
ALTER TABLE [dbo].[Download] DROP CONSTRAINT [FK_dbo.Download_dbo.MediaStorage_MediaStorageId]
ALTER TABLE [dbo].[QueuedEmailAttachment] DROP CONSTRAINT [FK_dbo.QueuedEmailAttachment_dbo.Download_FileId]
DROP INDEX [IX_MediaStorageId] ON [dbo].[Download]
DROP INDEX [IX_FileId] ON [dbo].[QueuedEmailAttachment]
ALTER TABLE [dbo].[Download] DROP COLUMN [ContentType]
ALTER TABLE [dbo].[Download] DROP COLUMN [Filename]
ALTER TABLE [dbo].[Download] DROP COLUMN [Extension]
ALTER TABLE [dbo].[Download] DROP COLUMN [MediaStorageId]
ALTER TABLE [dbo].[QueuedEmailAttachment] DROP COLUMN [FileId]
ALTER TABLE [dbo].[QueuedEmailAttachment] DROP COLUMN [Data]
";
            context.ExecuteSqlCommand(sql);
        }

        public bool RollbackOnFailure => false;
    }
}
