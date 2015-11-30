using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Shipping;

namespace SmartStore.Data.Mapping.Shipping
{
    public class ShippingMethodMap : EntityTypeConfiguration<ShippingMethod>
    {
        public ShippingMethodMap()
        {
            this.ToTable("ShippingMethod");
            this.HasKey(sm => sm.Id);

            this.Property(sm => sm.Name).IsRequired().HasMaxLength(400);
			this.Property(sm => sm.ExcludedCustomerRoleIds).HasMaxLength(500);

            this.HasMany(sm => sm.RestrictedCountries)
                .WithMany(c => c.RestrictedShippingMethods)
                .Map(m => m.ToTable("ShippingMethodRestrictions"));

			this.Ignore(sm => sm.CountryExclusionContext);
        }
    }
}
