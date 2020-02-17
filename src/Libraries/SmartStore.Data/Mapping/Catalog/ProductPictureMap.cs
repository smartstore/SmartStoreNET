using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductPictureMap : EntityTypeConfiguration<ProductPicture>
    {
        public ProductPictureMap()
        {
            ToTable("Product_MediaFile_Mapping");
            HasKey(pp => pp.Id);
            Property(pp => pp.PictureId).HasColumnName("MediaFileId");
            
            HasRequired(pp => pp.Picture)
                .WithMany(p => p.ProductPictures)
                .HasForeignKey(pp => pp.PictureId);

            HasRequired(pp => pp.Product)
                .WithMany(p => p.ProductPictures)
                .HasForeignKey(pp => pp.ProductId);
        }
    }
}