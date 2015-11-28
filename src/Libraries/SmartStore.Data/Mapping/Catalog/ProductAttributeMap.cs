using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductAttributeMap : EntityTypeConfiguration<ProductAttribute>
    {
        public ProductAttributeMap()
        {
            this.ToTable("ProductAttribute");
            this.HasKey(pa => pa.Id);
            this.Property(pa => pa.Alias).HasMaxLength(100);
            this.Property(pa => pa.Name).IsRequired();
        }
    }
}