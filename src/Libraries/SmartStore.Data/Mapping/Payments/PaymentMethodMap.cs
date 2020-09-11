using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Payments;

namespace SmartStore.Data.Mapping.Payments
{
    public partial class PaymentMethodMap : EntityTypeConfiguration<PaymentMethod>
    {
        public PaymentMethodMap()
        {
            ToTable("PaymentMethod");
            HasKey(x => x.Id);

            Property(x => x.PaymentMethodSystemName).IsRequired().HasMaxLength(4000);

            Property(x => x.FullDescription).HasMaxLength(4000);

            HasMany(pm => pm.RuleSets)
                .WithMany(rs => rs.PaymentMethods)
                .Map(m => m.ToTable("RuleSet_PaymentMethod_Mapping"));
        }
    }
}
