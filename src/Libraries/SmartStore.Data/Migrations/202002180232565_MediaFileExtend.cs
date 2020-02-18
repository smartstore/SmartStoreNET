namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using SmartStore.Data.Setup;

    public partial class MediaFileExtend : DbMigration, IDataSeeder<SmartObjectContext>
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
                "dbo.MediaTags",
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
                .ForeignKey("dbo.MediaTags", t => t.MediaTag_Id, cascadeDelete: true)
                .Index(t => t.MediaFile_Id)
                .Index(t => t.MediaTag_Id);
            
            AddColumn("dbo.MediaFile", "Alt", c => c.String(maxLength: 400));
            AddColumn("dbo.MediaFile", "Title", c => c.String(maxLength: 400));
            AddColumn("dbo.MediaFile", "Extension", c => c.String(maxLength: 50));
            AddColumn("dbo.MediaFile", "MediaType", c => c.String(nullable: false, maxLength: 20, defaultValue: "image"));
            AddColumn("dbo.MediaFile", "Size", c => c.Int(nullable: false));
            AddColumn("dbo.MediaFile", "PixelSize", c => c.Int());
            AddColumn("dbo.MediaFile", "Metadata", c => c.String());
            AddColumn("dbo.MediaFile", "CreatedOnUtc", c => c.DateTime(nullable: false, defaultValueSql: "GETDATE()"));
            CreateIndex("dbo.MediaFile", "Name");
            CreateIndex("dbo.MediaFile", "Alt");
            CreateIndex("dbo.MediaFile", "Extension");
            CreateIndex("dbo.MediaFile", "MimeType");
            CreateIndex("dbo.MediaFile", "MediaType");
            CreateIndex("dbo.MediaFile", "Size");
            CreateIndex("dbo.MediaFile", "PixelSize");
            CreateIndex("dbo.MediaFile", new[] { "CreatedOnUtc", "IsTransient" }, name: "IX_CreatedOn_IsTransient");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MediaFile_Tag_Mapping", "MediaTag_Id", "dbo.MediaTags");
            DropForeignKey("dbo.MediaFile_Tag_Mapping", "MediaFile_Id", "dbo.MediaFile");
            DropIndex("dbo.MediaFile_Tag_Mapping", new[] { "MediaTag_Id" });
            DropIndex("dbo.MediaFile_Tag_Mapping", new[] { "MediaFile_Id" });
            DropIndex("dbo.MediaTags", "IX_MediaTag_Name");
            DropIndex("dbo.MediaFile", "IX_CreatedOn_IsTransient");
            DropIndex("dbo.MediaFile", new[] { "PixelSize" });
            DropIndex("dbo.MediaFile", new[] { "Size" });
            DropIndex("dbo.MediaFile", new[] { "MediaType" });
            DropIndex("dbo.MediaFile", new[] { "MimeType" });
            DropIndex("dbo.MediaFile", new[] { "Extension" });
            DropIndex("dbo.MediaFile", new[] { "Alt" });
            DropIndex("dbo.MediaFile", new[] { "Name" });
            DropColumn("dbo.MediaFile", "CreatedOnUtc");
            DropColumn("dbo.MediaFile", "Metadata");
            DropColumn("dbo.MediaFile", "PixelSize");
            DropColumn("dbo.MediaFile", "Size");
            DropColumn("dbo.MediaFile", "MediaType");
            DropColumn("dbo.MediaFile", "Extension");
            DropColumn("dbo.MediaFile", "Title");
            DropColumn("dbo.MediaFile", "Alt");
            DropTable("dbo.MediaFile_Tag_Mapping");
            DropTable("dbo.MediaTags");
            CreateIndex("dbo.MediaFile", new[] { "UpdatedOnUtc", "IsTransient" }, name: "IX_UpdatedOn_IsTransient");
        }
    }
}
