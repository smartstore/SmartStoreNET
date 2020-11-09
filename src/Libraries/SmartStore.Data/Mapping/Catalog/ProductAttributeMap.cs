using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductAttributeMap : EntityTypeConfiguration<ProductAttribute>
    {
        public ProductAttributeMap()
        {
            ToTable("ProductAttribute");
            HasKey(pa => pa.Id);
            Property(pa => pa.Alias).HasMaxLength(100);
            Property(pa => pa.Name).IsRequired();
            Property(pa => pa.ExportMappings).IsMaxLength();
        }
    }
}