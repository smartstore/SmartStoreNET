using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Data.Mapping.Shipping
{
    public partial class ShipmentMap : EntityTypeConfiguration<Shipment>
    {
        public ShipmentMap()
        {
            this.ToTable("Shipment");
            this.HasKey(s => s.Id);

            this.Property(s => s.TotalWeight).HasPrecision(18, 4);

            this.HasRequired(s => s.Order)
                .WithMany(o => o.Shipments)
                .HasForeignKey(s => s.OrderId);
        }
    }
}