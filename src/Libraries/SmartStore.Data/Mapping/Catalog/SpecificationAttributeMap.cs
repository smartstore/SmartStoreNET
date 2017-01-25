using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class SpecificationAttributeMap : EntityTypeConfiguration<SpecificationAttribute>
    {
        public SpecificationAttributeMap()
        {
            this.ToTable("SpecificationAttribute");
            this.HasKey(sa => sa.Id);
            this.Property(sa => sa.Name).IsRequired();
			this.Property(sa => sa.Alias).HasMaxLength(30);
		}
    }
}