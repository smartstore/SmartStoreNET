using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
	public partial class SpecificationAttributeOptionMap : EntityTypeConfiguration<SpecificationAttributeOption>
    {
        public SpecificationAttributeOptionMap()
        {
            this.ToTable("SpecificationAttributeOption");
            this.HasKey(sao => sao.Id);
            this.Property(sao => sao.Name).IsRequired();
			this.Property(sao => sao.Alias).HasMaxLength(30);

			Property(soa => soa.NumberValue).HasPrecision(18, 4);

			this.HasRequired(sao => sao.SpecificationAttribute)
                .WithMany(sa => sa.SpecificationAttributeOptions)
                .HasForeignKey(sao => sao.SpecificationAttributeId);
        }
    }
}