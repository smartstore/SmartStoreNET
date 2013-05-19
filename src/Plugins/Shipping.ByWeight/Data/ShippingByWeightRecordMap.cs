using System.Data.Entity.ModelConfiguration;
using SmartStore.Plugin.Shipping.ByWeight.Domain;

namespace SmartStore.Plugin.Shipping.ByWeight.Data
{
    public partial class ShippingByWeightRecordMap : EntityTypeConfiguration<ShippingByWeightRecord>
    {
        public ShippingByWeightRecordMap()
        {
            this.ToTable("ShippingByWeight");
            this.HasKey(x => x.Id);
        }
    }
}