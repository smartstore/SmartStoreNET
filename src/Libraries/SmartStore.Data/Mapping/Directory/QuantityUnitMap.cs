using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Data.Mapping.Directory
{
    public partial class QuantityUnitMap : EntityTypeConfiguration<QuantityUnit>
    {
        public QuantityUnitMap()
        {
            ToTable("QuantityUnit");
            HasKey(c => c.Id);
            Property(c => c.Name).IsRequired().HasMaxLength(50);
            Property(c => c.NamePlural).IsRequired().HasMaxLength(50);
            Property(c => c.Description).HasMaxLength(50);
            Property(c => c.DisplayLocale).HasMaxLength(50);
            Property(c => c.DisplayOrder);
        }
    }
}