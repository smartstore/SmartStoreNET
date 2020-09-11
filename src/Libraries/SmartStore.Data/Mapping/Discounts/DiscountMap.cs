using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Discounts;

namespace SmartStore.Data.Mapping.Discounts
{
    public partial class DiscountMap : EntityTypeConfiguration<Discount>
    {
        public DiscountMap()
        {
            ToTable("Discount");
            HasKey(d => d.Id);
            Property(d => d.Name).IsRequired().HasMaxLength(200);
            Property(d => d.CouponCode).HasMaxLength(100);
            Property(d => d.DiscountPercentage).HasPrecision(18, 4);
            Property(d => d.DiscountAmount).HasPrecision(18, 4);

            Ignore(d => d.DiscountType);
            Ignore(d => d.DiscountLimitation);

            HasMany(d => d.RuleSets)
                .WithMany(rs => rs.Discounts)
                .Map(m => m.ToTable("RuleSet_Discount_Mapping"));

            HasMany(dr => dr.AppliedToCategories)
                .WithMany(c => c.AppliedDiscounts)
                .Map(m => m.ToTable("Discount_AppliedToCategories"));

            HasMany(dr => dr.AppliedToManufacturers)
                .WithMany(x => x.AppliedDiscounts)
                .Map(m => m.ToTable("Discount_AppliedToManufacturers"));

            HasMany(dr => dr.AppliedToProducts)
                .WithMany(p => p.AppliedDiscounts)
                .Map(m => m.ToTable("Discount_AppliedToProducts"));
        }
    }
}