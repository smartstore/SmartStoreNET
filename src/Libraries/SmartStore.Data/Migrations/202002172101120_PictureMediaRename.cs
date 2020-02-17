namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PictureMediaRename : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Picture", newName: "MediaFile");
            RenameTable(name: "dbo.Product_Picture_Mapping", newName: "Product_MediaFile_Mapping");
            RenameColumn(table: "dbo.BlogPost", name: "PictureId", newName: "MediaFileId");
            RenameColumn(table: "dbo.BlogPost", name: "PreviewPictureId", newName: "PreviewMediaFileId");
            RenameColumn(table: "dbo.Category", name: "PictureId", newName: "MediaFileId");
            RenameColumn(table: "dbo.MediaFile", name: "SeoFilename", newName: "Name");
            RenameColumn(table: "dbo.Product_MediaFile_Mapping", name: "PictureId", newName: "MediaFileId");
            RenameColumn(table: "dbo.Manufacturer", name: "PictureId", newName: "MediaFileId");
            RenameColumn(table: "dbo.SpecificationAttributeOption", name: "PictureId", newName: "MediaFileId");
            RenameColumn(table: "dbo.ProductVariantAttributeCombination", name: "AssignedPictureIds", newName: "AssignedMediaFileIds");
            RenameColumn(table: "dbo.ProductAttributeOption", name: "PictureId", newName: "MediaFileId");
            RenameColumn(table: "dbo.ProductVariantAttributeValue", name: "PictureId", newName: "MediaFileId");
            RenameColumn(table: "dbo.News", name: "PictureId", newName: "MediaFileId");
            RenameColumn(table: "dbo.News", name: "PreviewPictureId", newName: "PreviewMediaFileId");
            RenameColumn(table: "dbo.Store", name: "LogoPictureId", newName: "LogoMediaFileId");
            RenameIndex(table: "dbo.Category", name: "IX_PictureId", newName: "IX_MediaFileId");
            RenameIndex(table: "dbo.Product_MediaFile_Mapping", name: "IX_PictureId", newName: "IX_MediaFileId");
            RenameIndex(table: "dbo.Manufacturer", name: "IX_PictureId", newName: "IX_MediaFileId");
        }
        
        public override void Down()
        {
            RenameIndex(table: "dbo.Manufacturer", name: "IX_MediaFileId", newName: "IX_PictureId");
            RenameIndex(table: "dbo.Product_MediaFile_Mapping", name: "IX_MediaFileId", newName: "IX_PictureId");
            RenameIndex(table: "dbo.Category", name: "IX_MediaFileId", newName: "IX_PictureId");
            RenameColumn(table: "dbo.Store", name: "LogoMediaFileId", newName: "LogoPictureId");
            RenameColumn(table: "dbo.News", name: "PreviewMediaFileId", newName: "PreviewPictureId");
            RenameColumn(table: "dbo.News", name: "MediaFileId", newName: "PictureId");
            RenameColumn(table: "dbo.ProductVariantAttributeValue", name: "MediaFileId", newName: "PictureId");
            RenameColumn(table: "dbo.ProductAttributeOption", name: "MediaFileId", newName: "PictureId");
            RenameColumn(table: "dbo.ProductVariantAttributeCombination", name: "AssignedMediaFileIds", newName: "AssignedPictureIds");
            RenameColumn(table: "dbo.SpecificationAttributeOption", name: "MediaFileId", newName: "PictureId");
            RenameColumn(table: "dbo.Manufacturer", name: "MediaFileId", newName: "PictureId");
            RenameColumn(table: "dbo.Product_MediaFile_Mapping", name: "MediaFileId", newName: "PictureId");
            RenameColumn(table: "dbo.MediaFile", name: "Name", newName: "SeoFilename");
            RenameColumn(table: "dbo.Category", name: "MediaFileId", newName: "PictureId");
            RenameColumn(table: "dbo.BlogPost", name: "PreviewMediaFileId", newName: "PreviewPictureId");
            RenameColumn(table: "dbo.BlogPost", name: "MediaFileId", newName: "PictureId");
            RenameTable(name: "dbo.Product_MediaFile_Mapping", newName: "Product_Picture_Mapping");
            RenameTable(name: "dbo.MediaFile", newName: "Picture");
        }
    }
}
