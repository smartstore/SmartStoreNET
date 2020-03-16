namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MediaManager2 : DbMigration
    {
        public override void Up()
        {
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
            AddColumn("dbo.MediaFile", "PictureBinary", c => c.Binary());
            DropIndex("dbo.MediaTrack", "IX_MediaTrack_Composite");
            DropIndex("dbo.Setting", "IX_Setting_StoreId");
            DropIndex("dbo.Setting", "IX_Setting_Name");
            AlterColumn("dbo.Setting", "Name", c => c.String(nullable: false, maxLength: 200));
            DropColumn("dbo.MediaTrack", "Property");
            CreateIndex("dbo.MediaTrack", new[] { "MediaFileId", "EntityId", "EntityName" }, unique: true, name: "IX_MediaTrack_Composite");
        }
    }
}
