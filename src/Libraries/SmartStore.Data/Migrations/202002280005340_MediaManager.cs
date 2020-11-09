namespace SmartStore.Data.Migrations
{
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class MediaManager : DbMigration, IDataSeeder<SmartObjectContext>
    {
        public bool RollbackOnFailure => true;

        public void Seed(SmartObjectContext context)
        {
            // Empty, but we need the events in SmartStore.Services
        }

        public override void Up()
        {
            DropIndex("dbo.MediaFile", "IX_UpdatedOn_IsTransient");
            CreateTable(
                "dbo.MediaFolder",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    ParentId = c.Int(),
                    Name = c.String(nullable: false, maxLength: 255),
                    Slug = c.String(maxLength: 255),
                    CanDetectTracks = c.Boolean(nullable: false),
                    Metadata = c.String(),
                    FilesCount = c.Int(nullable: false),
                    ResKey = c.String(maxLength: 255),
                    IncludePath = c.Boolean(),
                    Order = c.Int(),
                    Discriminator = c.String(nullable: false, maxLength: 128),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MediaFolder", t => t.ParentId)
                .Index(t => new { t.ParentId, t.Name }, unique: true, name: "IX_NameParentId");

            CreateTable(
                "dbo.MediaTag",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Name = c.String(nullable: false, maxLength: 100),
                })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, name: "IX_MediaTag_Name");

            CreateTable(
                "dbo.MediaTrack",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    MediaFileId = c.Int(nullable: false),
                    Album = c.String(nullable: false, maxLength: 50),
                    EntityId = c.Int(nullable: false),
                    EntityName = c.String(nullable: false, maxLength: 255),
                })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MediaFile", t => t.MediaFileId, cascadeDelete: true)
                .Index(t => new { t.MediaFileId, t.EntityId, t.EntityName }, unique: true, name: "IX_MediaTrack_Composite")
                .Index(t => t.Album);

            CreateTable(
                "dbo.MediaFile_Tag_Mapping",
                c => new
                {
                    MediaFile_Id = c.Int(nullable: false),
                    MediaTag_Id = c.Int(nullable: false),
                })
                .PrimaryKey(t => new { t.MediaFile_Id, t.MediaTag_Id })
                .ForeignKey("dbo.MediaFile", t => t.MediaFile_Id, cascadeDelete: true)
                .ForeignKey("dbo.MediaTag", t => t.MediaTag_Id, cascadeDelete: true)
                .Index(t => t.MediaFile_Id)
                .Index(t => t.MediaTag_Id);

            AddColumn("dbo.MediaFile", "FolderId", c => c.Int());
            AddColumn("dbo.MediaFile", "Alt", c => c.String(maxLength: 400));
            AddColumn("dbo.MediaFile", "Title", c => c.String(maxLength: 400));
            AddColumn("dbo.MediaFile", "Extension", c => c.String(maxLength: 50));
            AddColumn("dbo.MediaFile", "MediaType", c => c.String(nullable: false, maxLength: 20));
            AddColumn("dbo.MediaFile", "Size", c => c.Int(nullable: false));
            AddColumn("dbo.MediaFile", "PixelSize", c => c.Int());
            AddColumn("dbo.MediaFile", "Metadata", c => c.String());
            AddColumn("dbo.MediaFile", "CreatedOnUtc", c => c.DateTime(nullable: false));
            AddColumn("dbo.MediaFile", "Deleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.MediaFile", "Hidden", c => c.Boolean(nullable: false));
            AddColumn("dbo.MediaFile", "Version", c => c.Int(nullable: false));
            AddColumn("dbo.Download", "MediaFileId", c => c.Int());
            AlterColumn("dbo.MediaFile", "MimeType", c => c.String(nullable: false, maxLength: 100));
            CreateIndex("dbo.BlogPost", "MediaFileId");
            CreateIndex("dbo.BlogPost", "PreviewMediaFileId");
            CreateIndex("dbo.MediaFile", "FolderId");
            CreateIndex("dbo.MediaFile", "Name");
            CreateIndex("dbo.MediaFile", "Alt");
            CreateIndex("dbo.MediaFile", "Extension");
            CreateIndex("dbo.MediaFile", "MimeType");
            CreateIndex("dbo.MediaFile", "MediaType");
            CreateIndex("dbo.MediaFile", "Size");
            CreateIndex("dbo.MediaFile", "PixelSize");
            CreateIndex("dbo.MediaFile", "IsNew");
            CreateIndex("dbo.MediaFile", new[] { "CreatedOnUtc", "IsTransient" }, name: "IX_CreatedOn_IsTransient");
            CreateIndex("dbo.MediaFile", "Deleted");
            CreateIndex("dbo.Download", "MediaFileId");
            CreateIndex("dbo.News", "MediaFileId");
            CreateIndex("dbo.News", "PreviewMediaFileId");
            AddForeignKey("dbo.MediaFile", "FolderId", "dbo.MediaFolder", "Id");
            AddForeignKey("dbo.Download", "MediaFileId", "dbo.MediaFile", "Id");
            AddForeignKey("dbo.BlogPost", "MediaFileId", "dbo.MediaFile", "Id");
            AddForeignKey("dbo.BlogPost", "PreviewMediaFileId", "dbo.MediaFile", "Id");
            AddForeignKey("dbo.News", "MediaFileId", "dbo.MediaFile", "Id");
            AddForeignKey("dbo.News", "PreviewMediaFileId", "dbo.MediaFile", "Id");
        }

        public override void Down()
        {
            DropForeignKey("dbo.News", "PreviewMediaFileId", "dbo.MediaFile");
            DropForeignKey("dbo.News", "MediaFileId", "dbo.MediaFile");
            DropForeignKey("dbo.BlogPost", "PreviewMediaFileId", "dbo.MediaFile");
            DropForeignKey("dbo.BlogPost", "MediaFileId", "dbo.MediaFile");
            DropForeignKey("dbo.MediaTrack", "MediaFileId", "dbo.MediaFile");
            DropForeignKey("dbo.MediaFile_Tag_Mapping", "MediaTag_Id", "dbo.MediaTag");
            DropForeignKey("dbo.MediaFile_Tag_Mapping", "MediaFile_Id", "dbo.MediaFile");
            DropForeignKey("dbo.Download", "MediaFileId", "dbo.MediaFile");
            DropForeignKey("dbo.MediaFile", "FolderId", "dbo.MediaFolder");
            DropForeignKey("dbo.MediaFolder", "ParentId", "dbo.MediaFolder");
            DropIndex("dbo.MediaFile_Tag_Mapping", new[] { "MediaTag_Id" });
            DropIndex("dbo.MediaFile_Tag_Mapping", new[] { "MediaFile_Id" });
            DropIndex("dbo.News", new[] { "PreviewMediaFileId" });
            DropIndex("dbo.News", new[] { "MediaFileId" });
            DropIndex("dbo.MediaTrack", new[] { "Album" });
            DropIndex("dbo.MediaTrack", "IX_MediaTrack_Composite");
            DropIndex("dbo.MediaTag", "IX_MediaTag_Name");
            DropIndex("dbo.Download", new[] { "MediaFileId" });
            DropIndex("dbo.MediaFolder", "IX_NameParentId");
            DropIndex("dbo.MediaFile", new[] { "Deleted" });
            DropIndex("dbo.MediaFile", "IX_CreatedOn_IsTransient");
            DropIndex("dbo.MediaFile", new[] { "IsNew" });
            DropIndex("dbo.MediaFile", new[] { "PixelSize" });
            DropIndex("dbo.MediaFile", new[] { "Size" });
            DropIndex("dbo.MediaFile", new[] { "MediaType" });
            DropIndex("dbo.MediaFile", new[] { "MimeType" });
            DropIndex("dbo.MediaFile", new[] { "Extension" });
            DropIndex("dbo.MediaFile", new[] { "Alt" });
            DropIndex("dbo.MediaFile", new[] { "Name" });
            DropIndex("dbo.MediaFile", new[] { "FolderId" });
            DropIndex("dbo.BlogPost", new[] { "PreviewMediaFileId" });
            DropIndex("dbo.BlogPost", new[] { "MediaFileId" });
            AlterColumn("dbo.MediaFile", "MimeType", c => c.String(nullable: false, maxLength: 40));
            DropColumn("dbo.Download", "MediaFileId");
            DropColumn("dbo.MediaFile", "Version");
            DropColumn("dbo.MediaFile", "Hidden");
            DropColumn("dbo.MediaFile", "Deleted");
            DropColumn("dbo.MediaFile", "CreatedOnUtc");
            DropColumn("dbo.MediaFile", "Metadata");
            DropColumn("dbo.MediaFile", "PixelSize");
            DropColumn("dbo.MediaFile", "Size");
            DropColumn("dbo.MediaFile", "MediaType");
            DropColumn("dbo.MediaFile", "Extension");
            DropColumn("dbo.MediaFile", "Title");
            DropColumn("dbo.MediaFile", "Alt");
            DropColumn("dbo.MediaFile", "FolderId");
            DropTable("dbo.MediaFile_Tag_Mapping");
            DropTable("dbo.MediaTrack");
            DropTable("dbo.MediaTag");
            DropTable("dbo.MediaFolder");
            CreateIndex("dbo.MediaFile", new[] { "UpdatedOnUtc", "IsTransient" }, name: "IX_UpdatedOn_IsTransient");
        }
    }
}
