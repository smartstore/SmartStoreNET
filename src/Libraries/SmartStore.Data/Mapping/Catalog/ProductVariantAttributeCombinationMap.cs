using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductVariantAttributeCombinationMap : EntityTypeConfiguration<ProductVariantAttributeCombination>
    {
        public ProductVariantAttributeCombinationMap()
        {
            this.ToTable("ProductVariantAttributeCombination");
            this.HasKey(pvac => pvac.Id);
            this.Property(pvac => pvac.AttributesXml);

            // codehint: sm-add
            this.Property(pv => pv.Sku).HasMaxLength(400);
            this.Property(pv => pv.ManufacturerPartNumber).HasMaxLength(400);
            this.Property(pv => pv.Gtin).HasMaxLength(400);
            this.Property(pv => pv.AssignedPictureIds).HasMaxLength(1000);
            this.Property(pv => pv.Length).HasPrecision(18, 4);
            this.Property(pv => pv.Width).HasPrecision(18, 4);
            this.Property(pv => pv.Height).HasPrecision(18, 4);
            this.Property(pv => pv.BasePriceAmount).HasPrecision(18, 4);

            this.HasRequired(pvac => pvac.ProductVariant)
                .WithMany(pv => pv.ProductVariantAttributeCombinations)
                .HasForeignKey(pvac => pvac.ProductVariantId);

            this.HasOptional(pv => pv.DeliveryTime)
                .WithMany()
                .HasForeignKey(pv => pv.DeliveryTimeId)
                .WillCascadeOnDelete(false);
        }
    }
}