using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductMediaFileMap : EntityTypeConfiguration<ProductMediaFile>
    {
        public ProductMediaFileMap()
        {
            ToTable("Product_MediaFile_Mapping");
            HasKey(pp => pp.Id);
            Property(pp => pp.MediaFileId).HasColumnName("MediaFileId");

            HasRequired(pp => pp.MediaFile)
                .WithMany(p => p.ProductMediaFiles)
                .HasForeignKey(pp => pp.MediaFileId)
                .WillCascadeOnDelete(true);

            HasRequired(pp => pp.Product)
                .WithMany(p => p.ProductPictures)
                .HasForeignKey(pp => pp.ProductId)
                .WillCascadeOnDelete(true);
        }
    }
}