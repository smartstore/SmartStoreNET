using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductVariantAttributeCombinationMap : EntityTypeConfiguration<ProductVariantAttributeCombination>
    {
        public ProductVariantAttributeCombinationMap()
        {
            ToTable("ProductVariantAttributeCombination");
            HasKey(pvac => pvac.Id);

            Property(pvac => pvac.Sku).HasMaxLength(400);
            Property(pvac => pvac.ManufacturerPartNumber).HasMaxLength(400);
            Property(pvac => pvac.Gtin).HasMaxLength(400);
            Property(pvac => pvac.Price).HasPrecision(18, 4);
            Property(pvac => pvac.AssignedMediaFileIds).HasMaxLength(1000).HasColumnName("AssignedMediaFileIds");
            Property(pvac => pvac.Length).HasPrecision(18, 4);
            Property(pvac => pvac.Width).HasPrecision(18, 4);
            Property(pvac => pvac.Height).HasPrecision(18, 4);
            Property(pvac => pvac.BasePriceAmount).HasPrecision(18, 4);

            HasRequired(pvac => pvac.Product)
                .WithMany(pvac => pvac.ProductVariantAttributeCombinations)
                .HasForeignKey(pvac => pvac.ProductId);

            HasOptional(pvac => pvac.DeliveryTime)
                .WithMany()
                .HasForeignKey(pvac => pvac.DeliveryTimeId)
                .WillCascadeOnDelete(false);
            HasOptional(pvac => pvac.QuantityUnit)
                .WithMany()
                .HasForeignKey(pvac => pvac.QuantityUnitId)
                .WillCascadeOnDelete(false);

        }
    }
}