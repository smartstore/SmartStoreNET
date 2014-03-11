namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPictureProp : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Category", "PictureId", c => c.Int(nullable: true));
			AlterColumn("dbo.Manufacturer", "PictureId", c => c.Int(nullable: true));
			AlterColumn("dbo.Product", "SampleDownloadId", c => c.Int(nullable: true));
            CreateIndex("dbo.Category", "PictureId");
            CreateIndex("dbo.Manufacturer", "PictureId");
			CreateIndex("dbo.Product", "SampleDownloadId");
            AddForeignKey("dbo.Category", "PictureId", "dbo.Picture", "Id");
            AddForeignKey("dbo.Manufacturer", "PictureId", "dbo.Picture", "Id");
			AddForeignKey("dbo.Product", "SampleDownloadId", "dbo.Download", "Id");
        }
        
        public override void Down()
        {
			DropForeignKey("dbo.Product", "SampleDownloadId", "dbo.Download");
			DropForeignKey("dbo.Manufacturer", "PictureId", "dbo.Picture");
            DropForeignKey("dbo.Category", "PictureId", "dbo.Picture");
			DropIndex("dbo.Product", new[] { "SampleDownloadId" });
			DropIndex("dbo.Manufacturer", new[] { "PictureId" });
            DropIndex("dbo.Category", new[] { "PictureId" });
			AlterColumn("dbo.Product", "SampleDownloadId", c => c.Int(nullable: false));
            AlterColumn("dbo.Manufacturer", "PictureId", c => c.Int(nullable: false));
            AlterColumn("dbo.Category", "PictureId", c => c.Int(nullable: false));
        }
    }
}
