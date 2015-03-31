using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Directory;

namespace SmartStore.Data.Mapping.Directory
{
    public partial class DeliveryTimeMap : EntityTypeConfiguration<DeliveryTime>
    {
        public DeliveryTimeMap()
        {
            this.ToTable("DeliveryTime");
            this.HasKey(c => c.Id);
            this.Property(c => c.Name).IsRequired().HasMaxLength(50);
            this.Property(c => c.ColorHexValue).IsRequired().HasMaxLength(50);
            this.Property(c => c.DisplayLocale).HasMaxLength(50);
            this.Property(c => c.DisplayOrder);
        }
    }
}