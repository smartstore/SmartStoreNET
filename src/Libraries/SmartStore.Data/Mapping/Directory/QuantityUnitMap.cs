using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Data.Mapping.Directory
{
    public partial class QuantityUnitMap : EntityTypeConfiguration<QuantityUnit>
    {
        public QuantityUnitMap()
        {
            this.ToTable("QuantityUnit");
            this.HasKey(c => c.Id);
            this.Property(c => c.Name).IsRequired().HasMaxLength(50);
            this.Property(c => c.Description).HasMaxLength(50);
            this.Property(c => c.DisplayLocale).HasMaxLength(50);
            this.Property(c => c.DisplayOrder);
        }
    }
}