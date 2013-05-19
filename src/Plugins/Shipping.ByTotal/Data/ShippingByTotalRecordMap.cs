using System.Data.Entity.ModelConfiguration;
using SmartStore.Plugin.Shipping.ByTotal.Domain;

namespace SmartStore.Plugin.Shipping.ByTotal.Data
{
    public class ShippingByTotalRecordMap : EntityTypeConfiguration<ShippingByTotalRecord>
    {
        public ShippingByTotalRecordMap()
        {
            this.ToTable("ShippingByTotal");
            this.HasKey(x => x.Id);

            this.Property(x => x.Zip).IsOptional().HasMaxLength(400);
        }
    }
}
