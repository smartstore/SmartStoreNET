using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductBundleItemMap : EntityTypeConfiguration<ProductBundleItem>
    {
        public ProductBundleItemMap()
        {
            this.ToTable("ProductBundleItem");
            this.HasKey(pbi => pbi.Id);

            this.Property(pbi => pbi.Discount).HasPrecision(18, 4).IsOptional();
            this.Property(pbi => pbi.Name).HasMaxLength(400);
            this.Property(pbi => pbi.ShortDescription).IsMaxLength();

            this.HasRequired(pbi => pbi.Product)
                .WithMany()
                .HasForeignKey(pbi => pbi.ProductId)
                .WillCascadeOnDelete(false);        // SQL Server does not support multiple cascade deletes

            this.HasRequired(pbi => pbi.BundleProduct)
                .WithMany(p => p.ProductBundleItems)
                .HasForeignKey(pbi => pbi.BundleProductId)
                .WillCascadeOnDelete(true);
        }
    }
}
