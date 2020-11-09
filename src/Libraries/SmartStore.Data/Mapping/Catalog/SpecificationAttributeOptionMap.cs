using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class SpecificationAttributeOptionMap : EntityTypeConfiguration<SpecificationAttributeOption>
    {
        public SpecificationAttributeOptionMap()
        {
            ToTable("SpecificationAttributeOption");
            HasKey(sao => sao.Id);
            Property(sao => sao.Name).IsRequired();
            Property(sao => sao.Alias).HasMaxLength(30);
            Property(sao => sao.Color).HasMaxLength(100);
            Property(sao => sao.MediaFileId).HasColumnName("MediaFileId");

            Property(soa => soa.NumberValue).HasPrecision(18, 4);

            HasRequired(sao => sao.SpecificationAttribute)
                .WithMany(sa => sa.SpecificationAttributeOptions)
                .HasForeignKey(sao => sao.SpecificationAttributeId);
        }
    }
}