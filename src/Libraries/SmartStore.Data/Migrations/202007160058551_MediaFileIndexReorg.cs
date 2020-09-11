namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class MediaFileIndexReorg : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.MediaFile", new[] { "FolderId" });
            DropIndex("dbo.MediaFile", new[] { "Name" });
            DropIndex("dbo.MediaFile", new[] { "Alt" });
            DropIndex("dbo.MediaFile", new[] { "Extension" });
            DropIndex("dbo.MediaFile", new[] { "MimeType" });
            DropIndex("dbo.MediaFile", new[] { "MediaType" });
            DropIndex("dbo.MediaFile", new[] { "Size" });
            DropIndex("dbo.MediaFile", new[] { "PixelSize" });
            DropIndex("dbo.MediaFile", "IX_CreatedOn_IsTransient");
            DropIndex("dbo.MediaFile", new[] { "Deleted" });
            CreateIndex("dbo.MediaFile", new[] { "FolderId", "Extension", "PixelSize", "Deleted" }, name: "IX_Media_Extension");
            CreateIndex("dbo.MediaFile", new[] { "FolderId", "Deleted" }, name: "IX_Media_FolderId");
            CreateIndex("dbo.MediaFile", new[] { "FolderId", "MediaType", "Extension", "PixelSize", "Deleted" }, name: "IX_Media_MediaType");
            CreateIndex("dbo.MediaFile", new[] { "FolderId", "Name", "Deleted" }, name: "IX_Media_Name");
            CreateIndex("dbo.MediaFile", new[] { "FolderId", "PixelSize", "Deleted" }, name: "IX_Media_PixelSize");
            CreateIndex("dbo.MediaFile", new[] { "FolderId", "Size", "Deleted" }, name: "IX_Media_Size");
            CreateIndex("dbo.MediaFile", new[] { "FolderId", "Deleted" }, name: "IX_Media_UpdatedOnUtc");
        }

        public override void Down()
        {
            DropIndex("dbo.MediaFile", "IX_Media_UpdatedOnUtc");
            DropIndex("dbo.MediaFile", "IX_Media_Size");
            DropIndex("dbo.MediaFile", "IX_Media_PixelSize");
            DropIndex("dbo.MediaFile", "IX_Media_Name");
            DropIndex("dbo.MediaFile", "IX_Media_MediaType");
            DropIndex("dbo.MediaFile", "IX_Media_FolderId");
            DropIndex("dbo.MediaFile", "IX_Media_Extension");
            CreateIndex("dbo.MediaFile", "Deleted");
            CreateIndex("dbo.MediaFile", new[] { "CreatedOnUtc", "IsTransient" }, name: "IX_CreatedOn_IsTransient");
            CreateIndex("dbo.MediaFile", "PixelSize");
            CreateIndex("dbo.MediaFile", "Size");
            CreateIndex("dbo.MediaFile", "MediaType");
            CreateIndex("dbo.MediaFile", "MimeType");
            CreateIndex("dbo.MediaFile", "Extension");
            CreateIndex("dbo.MediaFile", "Alt");
            CreateIndex("dbo.MediaFile", "Name");
            CreateIndex("dbo.MediaFile", "FolderId");
        }
    }
}
