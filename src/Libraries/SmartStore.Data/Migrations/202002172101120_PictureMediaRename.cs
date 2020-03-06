namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PictureMediaRename : DbMigration
    {
        private void RenameTableEx(string name, string newName)
        {
            //Sql($"EXEC sp_rename '{name}', '{newName}';");
            RenameTable(name: name, newName: newName);
        }

        private void RenameColumnEx(string table, string name, string newName)
        {
            //Sql($"EXEC sp_rename '{table}.{name}', '{newName}', 'COLUMN';");
            RenameColumn(table, name, newName);
        }

        private void RenameIndexEx(string table, string name, string newName)
        {
            //Sql($"EXEC sp_rename N'{table}.{name}', N'{newName}', N'INDEX';");
            RenameIndex(table, name, newName);
        }

        public override void Up()
        {
            RenameTableEx(name: "dbo.Picture", newName: "MediaFile");
            RenameTableEx(name: "dbo.Product_Picture_Mapping", newName: "Product_MediaFile_Mapping");
            RenameColumnEx(table: "dbo.BlogPost", name: "PictureId", newName: "MediaFileId");
            RenameColumnEx(table: "dbo.BlogPost", name: "PreviewPictureId", newName: "PreviewMediaFileId");
            RenameColumnEx(table: "dbo.Category", name: "PictureId", newName: "MediaFileId");
            RenameColumnEx(table: "dbo.MediaFile", name: "SeoFilename", newName: "Name");
            RenameColumnEx(table: "dbo.Product_MediaFile_Mapping", name: "PictureId", newName: "MediaFileId");
            RenameColumnEx(table: "dbo.Manufacturer", name: "PictureId", newName: "MediaFileId");
            RenameColumnEx(table: "dbo.SpecificationAttributeOption", name: "PictureId", newName: "MediaFileId");
            RenameColumnEx(table: "dbo.ProductVariantAttributeCombination", name: "AssignedPictureIds", newName: "AssignedMediaFileIds");
            RenameColumnEx(table: "dbo.ProductAttributeOption", name: "PictureId", newName: "MediaFileId");
            RenameColumnEx(table: "dbo.ProductVariantAttributeValue", name: "PictureId", newName: "MediaFileId");
            RenameColumnEx(table: "dbo.News", name: "PictureId", newName: "MediaFileId");
            RenameColumnEx(table: "dbo.News", name: "PreviewPictureId", newName: "PreviewMediaFileId");
            RenameColumnEx(table: "dbo.Store", name: "LogoPictureId", newName: "LogoMediaFileId");
            RenameIndexEx(table: "dbo.Category", name: "IX_PictureId", newName: "IX_MediaFileId");
            //RenameIndexEx(table: "dbo.Product_MediaFile_Mapping", name: "IX_PictureId", newName: "IX_MediaFileId");
            RenameIndexEx(table: "dbo.Manufacturer", name: "IX_PictureId", newName: "IX_MediaFileId");
        }
        
        public override void Down()
        {
            RenameIndexEx(table: "dbo.Manufacturer", name: "IX_MediaFileId", newName: "IX_PictureId");
            RenameIndexEx(table: "dbo.Product_MediaFile_Mapping", name: "IX_MediaFileId", newName: "IX_PictureId");
            RenameIndexEx(table: "dbo.Category", name: "IX_MediaFileId", newName: "IX_PictureId");
            RenameColumnEx(table: "dbo.Store", name: "LogoMediaFileId", newName: "LogoPictureId");
            RenameColumnEx(table: "dbo.News", name: "PreviewMediaFileId", newName: "PreviewPictureId");
            RenameColumnEx(table: "dbo.News", name: "MediaFileId", newName: "PictureId");
            RenameColumnEx(table: "dbo.ProductVariantAttributeValue", name: "MediaFileId", newName: "PictureId");
            RenameColumnEx(table: "dbo.ProductAttributeOption", name: "MediaFileId", newName: "PictureId");
            RenameColumnEx(table: "dbo.ProductVariantAttributeCombination", name: "AssignedMediaFileIds", newName: "AssignedPictureIds");
            RenameColumnEx(table: "dbo.SpecificationAttributeOption", name: "MediaFileId", newName: "PictureId");
            RenameColumnEx(table: "dbo.Manufacturer", name: "MediaFileId", newName: "PictureId");
            RenameColumnEx(table: "dbo.Product_MediaFile_Mapping", name: "MediaFileId", newName: "PictureId");
            RenameColumnEx(table: "dbo.MediaFile", name: "Name", newName: "SeoFilename");
            RenameColumnEx(table: "dbo.Category", name: "MediaFileId", newName: "PictureId");
            RenameColumnEx(table: "dbo.BlogPost", name: "PreviewMediaFileId", newName: "PreviewPictureId");
            RenameColumnEx(table: "dbo.BlogPost", name: "MediaFileId", newName: "PictureId");
            //RenameTableEx(name: "dbo.Product_MediaFile_Mapping", newName: "Product_Picture_Mapping");
            RenameTableEx(name: "dbo.MediaFile", newName: "Picture");
        }
    }
}
