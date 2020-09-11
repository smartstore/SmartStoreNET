using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Discounts;

namespace SmartStore.Data.Mapping.Discounts
{
    public partial class DiscountUsageHistoryMap : EntityTypeConfiguration<DiscountUsageHistory>
    {
        public DiscountUsageHistoryMap()
        {
            this.ToTable("DiscountUsageHistory");
            this.HasKey(duh => duh.Id);

            this.HasRequired(duh => duh.Discount)
                .WithMany()
                .HasForeignKey(duh => duh.DiscountId);

            this.HasRequired(duh => duh.Order)
                .WithMany(o => o.DiscountUsageHistory)
                .HasForeignKey(duh => duh.OrderId);
        }
    }
}