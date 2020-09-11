using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductVariantAttributeValueMap : EntityTypeConfiguration<ProductVariantAttributeValue>
    {
        public ProductVariantAttributeValueMap()
        {
            ToTable("ProductVariantAttributeValue");
            HasKey(pvav => pvav.Id);
            Property(pvav => pvav.Alias).HasMaxLength(100);
            Property(pvav => pvav.Name);
            Property(pvav => pvav.Color).HasMaxLength(100);
            Property(pvav => pvav.MediaFileId).HasColumnName("MediaFileId");
            Property(pvav => pvav.PriceAdjustment).HasPrecision(18, 4);
            Property(pvav => pvav.WeightAdjustment).HasPrecision(18, 4);

            Ignore(pvav => pvav.ValueType);

            HasRequired(pvav => pvav.ProductVariantAttribute)
                .WithMany(pva => pva.ProductVariantAttributeValues)
                .HasForeignKey(pvav => pvav.ProductVariantAttributeId);
        }
    }
}