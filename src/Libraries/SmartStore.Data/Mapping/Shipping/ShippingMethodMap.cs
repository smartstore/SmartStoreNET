using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Data.Mapping.Shipping
{
	public class ShippingMethodMap : EntityTypeConfiguration<ShippingMethod>
    {
        public ShippingMethodMap()
        {
            ToTable("ShippingMethod");
            HasKey(sm => sm.Id);

            Property(sm => sm.Name).IsRequired().HasMaxLength(400);
        }
    }
}
