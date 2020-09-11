namespace SmartStore.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Builders;
    using System.Data.Entity.Migrations.Model;
    using SmartStore.Core.Data;

    public partial class PictureMediaRename : DbMigration
    {
        private void RenameTableEx(string name, string newName)
        {
            RenameTable(name: name, newName: newName);
        }

        private void RenameColumnEx(string table, string name, string newName)
        {
            RenameColumn(table, name, newName);
        }

        private void RenameIndexEx(string table, string name, string newName)
        {
            RenameIndex(table, name, newName);
        }

        private void RenameColumnCe(
            string table,
            string name,
            string newName,
            Func<ColumnBuilder, ColumnModel> columnAction,
            string[] foreignKey = null,
            string[] newForeignKey = null,
            bool dropIndex = false,
            bool createIndex = false)
        {
            var nativeTableName = table.StartsWith("dbo.", StringComparison.OrdinalIgnoreCase)
                ? table.Substring(4)
                : table;

            AddColumn(table, newName, columnAction);

            Sql($"UPDATE [{nativeTableName}] SET [{newName}] = [{name}]");

            if (foreignKey != null)
            {
                // Attention, different methods. 'The argument 'principalTable' cannot be null, empty or contain only white space.'
                if (foreignKey.Length == 1)
                {
                    // Delete by key name.
                    DropForeignKey(table, foreignKey[0]);
                }
                else
                {
                    // Delete by EF generated key name.
                    DropForeignKey(table, foreignKey[0], foreignKey[1]);
                }
            }

            // Add new foreign key after DropForeignKey to allow using old key name.
            if (newForeignKey != null)
            {
                AddForeignKey(table, newName, newForeignKey[0], newForeignKey[1], newForeignKey[2].ToBool(), newForeignKey[3]);
            }

            if (dropIndex)
            {
                DropIndex(table, new[] { name });
            }

            if (createIndex)
            {
                CreateIndex(table, newName);
            }

            DropColumn(table, name);
        }

        public override void Up()
        {
            RenameTableEx(name: "dbo.Picture", newName: "MediaFile");
            RenameTableEx(name: "dbo.Product_Picture_Mapping", newName: "Product_MediaFile_Mapping");

            if (DataSettings.Current.IsSqlServer)
            {
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
            else
            {
                // SQL Server CE does not support direct renaming of columns.
                // The goal is an identical replica of the SQL-Server installation.
                RenameColumnCe("dbo.BlogPost", "PictureId", "MediaFileId", c => c.Int());
                RenameColumnCe("dbo.BlogPost", "PreviewPictureId", "PreviewMediaFileId", c => c.Int());

                RenameColumnCe("dbo.Category", "PictureId", "MediaFileId", c => c.Int(),
                    new[] { "PictureId", "dbo.Picture" },
                    new[] { "dbo.MediaFile", "Id", "false", "FK_dbo.Category_dbo.Picture_PictureId" },
                    true,
                    true);

                RenameColumnCe("dbo.MediaFile", "SeoFilename", "Name", c => c.String(maxLength: 300));

                RenameColumnCe("dbo.Product_MediaFile_Mapping", "PictureId", "MediaFileId", c => c.Int(nullable: false),
                    new[] { "FK_dbo.Product_Picture_Mapping_dbo.Picture_PictureId" },
                    new[] { "dbo.MediaFile", "Id", "true", "FK_dbo.Product_Picture_Mapping_dbo.Picture_PictureId" },
                    true,
                    true);

                RenameColumnCe("dbo.Manufacturer", "PictureId", "MediaFileId", c => c.Int(),
                    new[] { "PictureId", "dbo.Picture" },
                    new[] { "dbo.MediaFile", "Id", "false", "FK_dbo.Manufacturer_dbo.Picture_PictureId" },
                    true,
                    true);

                RenameColumnCe("dbo.SpecificationAttributeOption", "PictureId", "MediaFileId", c => c.Int(nullable: false));
                RenameColumnCe("dbo.ProductVariantAttributeCombination", "AssignedPictureIds", "AssignedMediaFileIds", c => c.String(maxLength: 1000));
                RenameColumnCe("dbo.ProductAttributeOption", "PictureId", "MediaFileId", c => c.Int(nullable: false));
                RenameColumnCe("dbo.ProductVariantAttributeValue", "PictureId", "MediaFileId", c => c.Int(nullable: false));
                RenameColumnCe("dbo.News", "PictureId", "MediaFileId", c => c.Int());
                RenameColumnCe("dbo.News", "PreviewPictureId", "PreviewMediaFileId", c => c.Int());
                RenameColumnCe("dbo.Store", "LogoPictureId", "LogoMediaFileId", c => c.Int(nullable: false));
            }
        }

        public override void Down()
        {
            if (DataSettings.Current.IsSqlServer)
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
            }
            else
            {
                // Come on, give me a break.
            }

            //RenameTableEx(name: "dbo.Product_MediaFile_Mapping", newName: "Product_Picture_Mapping");
            RenameTableEx(name: "dbo.MediaFile", newName: "Picture");
        }
    }
}
