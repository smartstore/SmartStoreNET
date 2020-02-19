namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MediaFileExtend : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.MediaFile", "IX_UpdatedOn_IsTransient");
            CreateTable(
                "dbo.MediaFolder",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Slug = c.String(maxLength: 100),
                        ParentId = c.Int(),
                        Metadata = c.String(),
                        FilesCount = c.Int(nullable: false),
                        ResKey = c.String(maxLength: 255),
                        IncludePath = c.Boolean(),
                        Order = c.Int(),
                        Icon = c.String(maxLength: 100),
                        Color = c.String(maxLength: 100),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.MediaFolder", t => t.ParentId)
                .Index(t => t.ParentId);
            
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
            AddColumn("dbo.MediaFile", "Version", c => c.Int(nullable: false));
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
            AddForeignKey("dbo.MediaFile", "FolderId", "dbo.MediaFolder", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MediaFile_Tag_Mapping", "MediaTag_Id", "dbo.MediaTag");
            DropForeignKey("dbo.MediaFile_Tag_Mapping", "MediaFile_Id", "dbo.MediaFile");
            DropForeignKey("dbo.MediaFile", "FolderId", "dbo.MediaFolder");
            DropForeignKey("dbo.MediaFolder", "ParentId", "dbo.MediaFolder");
            DropIndex("dbo.MediaFile_Tag_Mapping", new[] { "MediaTag_Id" });
            DropIndex("dbo.MediaFile_Tag_Mapping", new[] { "MediaFile_Id" });
            DropIndex("dbo.MediaTag", "IX_MediaTag_Name");
            DropIndex("dbo.MediaFolder", new[] { "ParentId" });
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
            DropColumn("dbo.MediaFile", "Version");
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
            DropTable("dbo.MediaTag");
            DropTable("dbo.MediaFolder");
            CreateIndex("dbo.MediaFile", new[] { "UpdatedOnUtc", "IsTransient" }, name: "IX_UpdatedOn_IsTransient");
        }
    }
}
